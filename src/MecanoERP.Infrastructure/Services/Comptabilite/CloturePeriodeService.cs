using MecanoERP.Core.Entities.Comptabilite;
using MecanoERP.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MecanoERP.Infrastructure.Services.Comptabilite;

public class CloturePeriodeService
{
    private readonly MecanoDbContext _context;
    private readonly JournalService _journalService;

    public CloturePeriodeService(MecanoDbContext context, JournalService journalService)
    {
        _context = context;
        _journalService = journalService;
    }

    // ── Lister les périodes d'un exercice ────────────────────────────────────
    public async Task<IEnumerable<PeriodeComptable>> GetPeriodesAsync(int exercice)
        => await _context.PeriodесComptables
            .Where(p => p.Exercice == exercice)
            .OrderBy(p => p.Periode)
            .ToListAsync();

    // ── Initialiser les 12 périodes d'un exercice ────────────────────────────
    public async Task InitialiserExerciceAsync(int exercice)
    {
        if (await _context.PeriodесComptables.AnyAsync(p => p.Exercice == exercice))
            throw new Exception($"L'exercice {exercice} existe déjà.");

        for (int m = 1; m <= 12; m++)
        {
            var debut = new DateTime(exercice, m, 1);
            var fin   = debut.AddMonths(1).AddDays(-1);
            _context.PeriodесComptables.Add(new PeriodeComptable
            {
                Exercice = exercice,
                Periode  = m,
                DateDebut = debut,
                DateFin   = fin,
                Statut   = StatutPeriode.Ouverte
            });
        }
        await _context.SaveChangesAsync();
    }

    // ── Fermer une période mensuelle ─────────────────────────────────────────
    public async Task<FermeturePeriodeResultat> FermerPeriodeAsync(int exercice, int periode, int userId)
    {
        var p = await _context.PeriodесComptables
            .FirstOrDefaultAsync(x => x.Exercice == exercice && x.Periode == periode)
            ?? throw new Exception($"Période {exercice}-{periode:D2} introuvable.");

        if (p.Statut != StatutPeriode.Ouverte)
            throw new Exception($"La période {p.Libelle} n'est pas ouverte.");

        // Vérifier brouillons non validés
        var brouillons = await _context.Journaux.CountAsync(j =>
            j.Exercice == exercice && j.Periode == periode && j.EstBrouillon && !j.EstAnnule);

        if (brouillons > 0)
            return new FermeturePeriodeResultat
            {
                Succes = false,
                Message = $"⚠ {brouillons} journal(ux) en brouillon pour la période {p.Libelle}. Validez-les ou supprimez-les avant de fermer.",
                BrouillonsEnAttente = brouillons
            };

        p.Statut    = StatutPeriode.Fermee;
        p.TypeCloture = Core.Entities.Comptabilite.TypeCloture.Mensuelle;
        p.DateCloture = DateTime.UtcNow;
        p.UserClotureId = userId;
        await _context.SaveChangesAsync();

        return new FermeturePeriodeResultat { Succes = true, Message = $"✅ Période {p.Libelle} fermée." };
    }

    // ── Clôture annuelle ─────────────────────────────────────────────────────
    public async Task<FermeturePeriodeResultat> CloturerExerciceAsync(int exercice, int userId, int compteReportId)
    {
        // Vérifier toutes les périodes fermées
        var periodesOuvertes = await _context.PeriodесComptables
            .CountAsync(p => p.Exercice == exercice && p.Statut == StatutPeriode.Ouverte);

        if (periodesOuvertes > 0)
            return new FermeturePeriodeResultat
            {
                Succes = false,
                Message = $"⚠ {periodesOuvertes} période(s) encore ouvertes. Fermez toutes les périodes avant la clôture annuelle."
            };

        // Calculer bénéfice net (revenus - charges)
        var lignes = await _context.LignesJournal
            .Include(l => l.Journal)
            .Include(l => l.CompteGL)
            .Where(l => l.Journal.Exercice == exercice
                     && !l.Journal.EstBrouillon && !l.Journal.EstAnnule
                     && (l.CompteGL.TypeCompte == TypeCompteGL.Revenue
                      || l.CompteGL.TypeCompte == TypeCompteGL.Charge))
            .ToListAsync();

        var beneficeNet = lignes
            .Sum(l => l.CompteGL.TypeCompte == TypeCompteGL.Revenue
                ? -(l.Debit - l.Credit)
                :   (l.Debit - l.Credit));

        // Créer l'écriture de clôture (reset revenus/charges vers BNR)
        var comptes = lignes.GroupBy(l => l.CompteGL)
            .Select(g => new { Compte = g.Key, Solde = g.Sum(l => l.Debit - l.Credit) })
            .Where(x => x.Solde != 0)
            .ToList();

        var journalCloture = new JournalComptable
        {
            JournalCode    = "CLT",
            TypeJournal    = TypeJournal.Cloture,
            Date           = new DateTime(exercice, 12, 31),
            Exercice       = exercice,
            Periode        = 13, // Période de clôture
            Description    = $"Clôture de l'exercice {exercice}",
            UtilisateurId  = userId,
        };

        // Lignes qui remettent chaque compte à zéro
        int ordre = 1;
        foreach (var item in comptes)
        {
            var soldeDuCompte = item.Solde;
            journalCloture.Lignes.Add(new LigneJournal
            {
                CompteGLId  = item.Compte.Id,
                Description = $"Clôture {exercice} — {item.Compte.Nom}",
                Debit       = soldeDuCompte < 0 ? -soldeDuCompte : 0,
                Credit      = soldeDuCompte > 0 ?  soldeDuCompte : 0,
                Ordre       = ordre++
            });
        }

        // Contre-partie dans le compte BNR
        journalCloture.Lignes.Add(new LigneJournal
        {
            CompteGLId  = compteReportId,
            Description = $"Report à nouveau — Bénéfice net {exercice}",
            Debit       = beneficeNet < 0 ? -beneficeNet : 0,
            Credit      = beneficeNet > 0 ?  beneficeNet : 0,
            Ordre       = ordre
        });

        await _journalService.CreerAsync(journalCloture);
        await _journalService.ValiderAsync(journalCloture.Id);

        // Marquer toutes les périodes comme clôturées
        var periodesExercice = await _context.PeriodесComptables
            .Where(p => p.Exercice == exercice)
            .ToListAsync();
        foreach (var per in periodesExercice)
            per.Statut = StatutPeriode.Cloturee;
        await _context.SaveChangesAsync();

        return new FermeturePeriodeResultat
        {
            Succes = true,
            Message = $"✅ Exercice {exercice} clôturé. Bénéfice net reporté : {beneficeNet:C}"
        };
    }
}

public class FermeturePeriodeResultat
{
    public bool   Succes               { get; set; }
    public string Message              { get; set; } = "";
    public int    BrouillonsEnAttente  { get; set; }
}
