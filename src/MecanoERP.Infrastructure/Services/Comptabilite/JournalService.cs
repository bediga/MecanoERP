using MecanoERP.Core.Entities.Comptabilite;
using MecanoERP.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MecanoERP.Infrastructure.Services.Comptabilite;

public class JournalService
{
    private readonly MecanoDbContext _context;

    public JournalService(MecanoDbContext context) => _context = context;

    public async Task<IEnumerable<JournalComptable>> GetJournauxAsync(TypeJournal? type = null,
        DateTime? debut = null, DateTime? fin = null, bool brouillonSeulement = false)
    {
        var q = _context.Journaux
            .Include(j => j.Lignes).ThenInclude(l => l.CompteGL)
            .AsQueryable();
        if (type.HasValue)       q = q.Where(j => j.TypeJournal == type);
        if (debut.HasValue)      q = q.Where(j => j.Date >= debut);
        if (fin.HasValue)        q = q.Where(j => j.Date <= fin);
        if (brouillonSeulement)  q = q.Where(j => j.EstBrouillon);
        return await q.OrderByDescending(j => j.Date).ToListAsync();
    }

    public async Task<JournalComptable?> GetByIdAsync(int id)
        => await _context.Journaux
            .Include(j => j.Lignes).ThenInclude(l => l.CompteGL)
            .FirstOrDefaultAsync(j => j.Id == id);

    /// <summary>Créer un journal en mode brouillon (non validé).</summary>
    public async Task<JournalComptable> CreerAsync(JournalComptable journal)
    {
        journal.Numero = await GenererNumeroAsync(journal.TypeJournal);
        journal.EstBrouillon = true;
        journal.EstAnnule = false;
        _context.Journaux.Add(journal);
        await _context.SaveChangesAsync();
        return journal;
    }

    /// <summary>Valide un journal (débit = crédit obligatoire). Met à jour les soldes GL.</summary>
    public async Task ValiderAsync(int journalId)
    {
        var journal = await _context.Journaux
            .Include(j => j.Lignes).ThenInclude(l => l.CompteGL)
            .FirstOrDefaultAsync(j => j.Id == journalId)
            ?? throw new Exception("Journal introuvable.");

        if (!journal.EstBrouillon)
            throw new Exception("Ce journal est déjà validé.");

        var totalDebit  = journal.Lignes.Sum(l => l.Debit);
        var totalCredit = journal.Lignes.Sum(l => l.Credit);
        if (totalDebit != totalCredit || totalDebit == 0)
            throw new Exception($"Journal déséquilibré : Débit={totalDebit:C} ≠ Crédit={totalCredit:C}. Correction requise avant validation.");

        // Mettre à jour les soldes des comptes GL
        foreach (var ligne in journal.Lignes)
        {
            var compte = await _context.ComptesGL.FindAsync(ligne.CompteGLId)!;
            if (compte is null) continue;
            compte.Solde += ligne.Debit - ligne.Credit;
        }

        journal.EstBrouillon = false;
        journal.DateValidation = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    /// <summary>Contre-passe un journal validé (crée un nouveau journal inversé).</summary>
    public async Task<JournalComptable> ContrePasserAsync(int journalId, int utilisateurId)
    {
        var original = await _context.Journaux
            .Include(j => j.Lignes)
            .FirstOrDefaultAsync(j => j.Id == journalId)
            ?? throw new Exception("Journal introuvable.");

        if (original.EstBrouillon)
            throw new Exception("Impossible de contre-passer un journal brouillon. Veuillez le supprimer.");

        var contrePassation = new JournalComptable
        {
            Description  = $"CONTRE-PASSATION de {original.Numero} — {original.Description}",
            TypeJournal  = TypeJournal.General,
            Date         = DateTime.UtcNow,
            UtilisateurId = utilisateurId,
            Lignes       = original.Lignes.Select((l, i) => new LigneJournal
            {
                CompteGLId  = l.CompteGLId,
                Description = $"CP: {l.Description}",
                Debit       = l.Credit,  // Inversé
                Credit      = l.Debit,  // Inversé
                Ordre       = i + 1
            }).ToList()
        };
        await CreerAsync(contrePassation);
        await ValiderAsync(contrePassation.Id);

        original.EstAnnule = true;
        await _context.SaveChangesAsync();
        return contrePassation;
    }

    private async Task<string> GenererNumeroAsync(TypeJournal type)
    {
        var prefix = type switch
        {
            TypeJournal.Ventes  => "JV",
            TypeJournal.Achats  => "JA",
            TypeJournal.Caisse  => "JC",
            TypeJournal.General => "JG",
            TypeJournal.Cloture => "JCL",
            _                   => "JG"
        };
        var annee = DateTime.UtcNow.Year;
        var count = await _context.Journaux.CountAsync(j => j.TypeJournal == type) + 1;
        return $"{prefix}-{annee}-{count:D4}";
    }
}
