using MecanoERP.Core.Entities.Comptabilite;
using MecanoERP.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MecanoERP.Infrastructure.Services.Comptabilite;

public class EtatsFinanciersService
{
    private readonly MecanoDbContext _context;
    public EtatsFinanciersService(MecanoDbContext context) => _context = context;

    // ── Bilan ────────────────────────────────────────────────────────────────
    public async Task<Bilan> GetBilanAsync(DateTime date)
    {
        var lignes = await _context.LignesJournal
            .Include(l => l.Journal)
            .Include(l => l.CompteGL)
            .Where(l => !l.Journal.EstBrouillon
                     && !l.Journal.EstAnnule
                     && l.Journal.Date <= date)
            .ToListAsync();

        var soldesParCompte = lignes
            .GroupBy(l => l.CompteGL)
            .Select(g => new { Compte = g.Key, Solde = g.Sum(l => l.Debit - l.Credit) })
            .ToList();

        Decimal SumType(TypeCompteGL type) =>
            soldesParCompte.Where(x => x.Compte.TypeCompte == type).Sum(x => x.Solde);

        var totalActif   = SumType(TypeCompteGL.Actif);
        var totalPassif  = -SumType(TypeCompteGL.Passif);
        var totalCapital = -SumType(TypeCompteGL.Capital);

        var postesActif = soldesParCompte
            .Where(x => x.Compte.TypeCompte == TypeCompteGL.Actif && x.Solde != 0)
            .Select(x => new PosteFinancier { Numero = x.Compte.Numero, Nom = x.Compte.Nom, Montant = x.Solde })
            .OrderBy(p => p.Numero).ToList();

        var postesPassif = soldesParCompte
            .Where(x => x.Compte.TypeCompte == TypeCompteGL.Passif && x.Solde != 0)
            .Select(x => new PosteFinancier { Numero = x.Compte.Numero, Nom = x.Compte.Nom, Montant = -x.Solde })
            .OrderBy(p => p.Numero).ToList();

        var postesCapital = soldesParCompte
            .Where(x => x.Compte.TypeCompte == TypeCompteGL.Capital && x.Solde != 0)
            .Select(x => new PosteFinancier { Numero = x.Compte.Numero, Nom = x.Compte.Nom, Montant = -x.Solde })
            .OrderBy(p => p.Numero).ToList();

        return new Bilan
        {
            DateArrete   = date,
            PostesActif  = postesActif,
            PostesPassif = postesPassif,
            PostesCapital = postesCapital,
            TotalActif   = totalActif,
            TotalPassif  = totalPassif,
            TotalCapital = totalCapital,
            EstEquilibre = Math.Abs(totalActif - (totalPassif + totalCapital)) < 0.01m
        };
    }

    // ── État des résultats ───────────────────────────────────────────────────
    public async Task<EtatResultats> GetEtatResultatsAsync(DateTime debut, DateTime fin)
    {
        var lignes = await _context.LignesJournal
            .Include(l => l.Journal)
            .Include(l => l.CompteGL)
            .Where(l => !l.Journal.EstBrouillon
                     && !l.Journal.EstAnnule
                     && l.Journal.Date >= debut
                     && l.Journal.Date <= fin)
            .ToListAsync();

        var postesRevenus = lignes
            .Where(l => l.CompteGL.TypeCompte == TypeCompteGL.Revenue)
            .GroupBy(l => l.CompteGL)
            .Select(g => new PosteFinancier
            {
                Numero  = g.Key.Numero,
                Nom     = g.Key.Nom,
                Montant = -(g.Sum(l => l.Debit - l.Credit))  // Revenus = solde créditeur
            })
            .OrderBy(p => p.Numero).ToList();

        var postesCharges = lignes
            .Where(l => l.CompteGL.TypeCompte == TypeCompteGL.Charge)
            .GroupBy(l => l.CompteGL)
            .Select(g => new PosteFinancier
            {
                Numero  = g.Key.Numero,
                Nom     = g.Key.Nom,
                Montant = g.Sum(l => l.Debit - l.Credit)   // Charges = solde débiteur
            })
            .OrderBy(p => p.Numero).ToList();

        var totalRevenus = postesRevenus.Sum(p => p.Montant);
        var totalCharges = postesCharges.Sum(p => p.Montant);

        return new EtatResultats
        {
            DateDebut      = debut,
            DateFin        = fin,
            PostesRevenus  = postesRevenus,
            PostesCharges  = postesCharges,
            TotalRevenus   = totalRevenus,
            TotalCharges   = totalCharges,
            BeneficeNet    = totalRevenus - totalCharges
        };
    }
}

// ── DTOs ─────────────────────────────────────────────────────────────────────
public class PosteFinancier
{
    public string  Numero  { get; set; } = "";
    public string  Nom     { get; set; } = "";
    public decimal Montant { get; set; }
}

public class Bilan
{
    public DateTime            DateArrete    { get; set; }
    public List<PosteFinancier> PostesActif  { get; set; } = [];
    public List<PosteFinancier> PostesPassif { get; set; } = [];
    public List<PosteFinancier> PostesCapital { get; set; } = [];
    public decimal TotalActif    { get; set; }
    public decimal TotalPassif   { get; set; }
    public decimal TotalCapital  { get; set; }
    public bool    EstEquilibre  { get; set; }
}

public class EtatResultats
{
    public DateTime             DateDebut     { get; set; }
    public DateTime             DateFin       { get; set; }
    public List<PosteFinancier> PostesRevenus { get; set; } = [];
    public List<PosteFinancier> PostesCharges { get; set; } = [];
    public decimal TotalRevenus  { get; set; }
    public decimal TotalCharges  { get; set; }
    public decimal BeneficeNet   { get; set; }
}
