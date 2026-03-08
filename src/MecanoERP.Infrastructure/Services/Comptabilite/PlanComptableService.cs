using MecanoERP.Core.Entities.Comptabilite;
using MecanoERP.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MecanoERP.Infrastructure.Services.Comptabilite;

public class PlanComptableService
{
    private readonly MecanoDbContext _context;

    public PlanComptableService(MecanoDbContext context) => _context = context;

    public async Task<IEnumerable<CompteGL>> GetAllAsync(bool inclureInactifs = false)
        => await _context.ComptesGL
            .Where(c => inclureInactifs || c.EstActif)
            .OrderBy(c => c.Numero)
            .ToListAsync();

    public async Task<IEnumerable<CompteGL>> GetByTypeAsync(TypeCompteGL type)
        => await _context.ComptesGL
            .Where(c => c.TypeCompte == type && c.EstActif)
            .OrderBy(c => c.Numero)
            .ToListAsync();

    public async Task<CompteGL?> GetByNumeroAsync(string numero)
        => await _context.ComptesGL.FirstOrDefaultAsync(c => c.Numero == numero);

    public async Task<CompteGL> AjouterAsync(CompteGL compte)
    {
        if (await _context.ComptesGL.AnyAsync(c => c.Numero == compte.Numero))
            throw new Exception($"Le compte numéro {compte.Numero} existe déjà.");
        compte.EstSysteme = false;
        _context.ComptesGL.Add(compte);
        await _context.SaveChangesAsync();
        return compte;
    }

    public async Task ModifierAsync(CompteGL compte)
    {
        var existant = await _context.ComptesGL.FindAsync(compte.Id)
            ?? throw new Exception("Compte introuvable.");
        if (existant.EstSysteme)
            throw new Exception("Les comptes système ne peuvent pas être modifiés.");
        existant.Nom = compte.Nom;
        existant.Description = compte.Description;
        existant.EstActif = compte.EstActif;
        await _context.SaveChangesAsync();
    }

    public async Task<decimal> CalculerSoldeAsync(int compteId, DateTime? debut = null, DateTime? fin = null)
    {
        var query = _context.LignesJournal
            .Include(l => l.Journal)
            .Where(l => l.CompteGLId == compteId && !l.Journal.EstBrouillon && !l.Journal.EstAnnule);
        if (debut.HasValue) query = query.Where(l => l.Journal.Date >= debut);
        if (fin.HasValue)   query = query.Where(l => l.Journal.Date <= fin);
        var lignes = await query.ToListAsync();
        return lignes.Sum(l => l.Debit - l.Credit);
    }

    /// <summary>
    /// Seed du plan comptable PCGR canadien (18 comptes de base).
    /// Appelé une seule fois au premier démarrage.
    /// </summary>
    public async Task SeedPlanComptableAsync()
    {
        if (await _context.ComptesGL.AnyAsync()) return;

        var comptes = new List<CompteGL>
        {
            // ── ACTIF ──────────────────────────────────────────────
            new() { Numero="1000", Nom="Caisse",                          TypeCompte=TypeCompteGL.Actif, SousType=SousTypeCompteGL.CaisseEtBanque,      EstSysteme=true },
            new() { Numero="1010", Nom="Banque — compte courant",         TypeCompte=TypeCompteGL.Actif, SousType=SousTypeCompteGL.CaisseEtBanque,      EstSysteme=true },
            new() { Numero="1200", Nom="Comptes clients (A/R)",           TypeCompte=TypeCompteGL.Actif, SousType=SousTypeCompteGL.ComptesClients,       EstSysteme=true },
            new() { Numero="1300", Nom="Stocks — pièces automobiles",     TypeCompte=TypeCompteGL.Actif, SousType=SousTypeCompteGL.StockPieces,          EstSysteme=true },
            new() { Numero="1410", Nom="TPS récupérable (CTI)",           TypeCompte=TypeCompteGL.Actif, SousType=SousTypeCompteGL.TaxeRecuperableTPS,   EstSysteme=true },
            new() { Numero="1420", Nom="TVQ récupérable (RTI)",           TypeCompte=TypeCompteGL.Actif, SousType=SousTypeCompteGL.TaxeRecuperableTVQ,   EstSysteme=true },
            new() { Numero="1500", Nom="Immobilisations — équipements",   TypeCompte=TypeCompteGL.Actif, SousType=SousTypeCompteGL.ImmobilisationsBrutes, EstSysteme=true },
            new() { Numero="1510", Nom="Amortissements cumulés",          TypeCompte=TypeCompteGL.Actif, SousType=SousTypeCompteGL.AmortissementsAccumules, EstSysteme=true },

            // ── PASSIF ─────────────────────────────────────────────
            new() { Numero="2100", Nom="Comptes fournisseurs (A/P)",      TypeCompte=TypeCompteGL.Passif, SousType=SousTypeCompteGL.ComptesFournisseurs, EstSysteme=true },
            new() { Numero="2310", Nom="TPS perçue à remettre",           TypeCompte=TypeCompteGL.Passif, SousType=SousTypeCompteGL.TPS_Percue,          EstSysteme=true },
            new() { Numero="2320", Nom="TVQ perçue à remettre",           TypeCompte=TypeCompteGL.Passif, SousType=SousTypeCompteGL.TVQ_Percue,          EstSysteme=true },
            new() { Numero="2330", Nom="TVH perçue à remettre",           TypeCompte=TypeCompteGL.Passif, SousType=SousTypeCompteGL.TVH_Percue,          EstSysteme=true },
            new() { Numero="2400", Nom="Salaires à payer",                TypeCompte=TypeCompteGL.Passif, SousType=SousTypeCompteGL.AutresPassifsCourants, EstSysteme=true },

            // ── CAPITAL ────────────────────────────────────────────
            new() { Numero="3000", Nom="Capital — propriétaire",          TypeCompte=TypeCompteGL.Capital, SousType=SousTypeCompteGL.CapitalProprietaire, EstSysteme=true },
            new() { Numero="3100", Nom="Bénéfices non répartis",          TypeCompte=TypeCompteGL.Capital, SousType=SousTypeCompteGL.BeneficesNonRepartis, EstSysteme=true },

            // ── REVENUS ────────────────────────────────────────────
            new() { Numero="4000", Nom="Revenus — main-d'œuvre",          TypeCompte=TypeCompteGL.Revenue, SousType=SousTypeCompteGL.RevenusMOeuvre,     EstSysteme=true },
            new() { Numero="4100", Nom="Revenus — vente de pièces",       TypeCompte=TypeCompteGL.Revenue, SousType=SousTypeCompteGL.RevenusPieces,       EstSysteme=true },
            new() { Numero="4200", Nom="Revenus — services divers",       TypeCompte=TypeCompteGL.Revenue, SousType=SousTypeCompteGL.RevenusServices,     EstSysteme=true },

            // ── CHARGES ────────────────────────────────────────────
            new() { Numero="5000", Nom="Achats — pièces automobiles",     TypeCompte=TypeCompteGL.Charge, SousType=SousTypeCompteGL.AchatsPieces,         EstSysteme=true },
            new() { Numero="5100", Nom="Salaires et avantages sociaux",   TypeCompte=TypeCompteGL.Charge, SousType=SousTypeCompteGL.Salaires,             EstSysteme=true },
            new() { Numero="5200", Nom="Loyer et occupation",             TypeCompte=TypeCompteGL.Charge, SousType=SousTypeCompteGL.Loyer,               EstSysteme=true },
            new() { Numero="5300", Nom="Assurances",                      TypeCompte=TypeCompteGL.Charge, SousType=SousTypeCompteGL.Assurances,           EstSysteme=true },
            new() { Numero="5900", Nom="Charges diverses",                TypeCompte=TypeCompteGL.Charge, SousType=SousTypeCompteGL.AutresCharges,        EstSysteme=true },
        };

        _context.ComptesGL.AddRange(comptes);
        await _context.SaveChangesAsync();
    }
}
