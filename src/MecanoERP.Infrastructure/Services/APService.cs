using MecanoERP.Core.Entities;
using MecanoERP.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MecanoERP.Infrastructure.Services;

/// <summary>Service AP enrichi — Aging, paiements en lot, échéancier.</summary>
public class APService
{
    private readonly MecanoDbContext _ctx;
    public APService(MecanoDbContext ctx) => _ctx = ctx;

    // ── Rapport d'âge AP ─────────────────────────────────────────────────────
    public async Task<IEnumerable<LigneAgingAP>> GetAgingAPAsync()
    {
        var now      = DateTime.UtcNow.Date;
        var factures = await _ctx.FacturesFournisseurs
            .Include(f => f.Fournisseur)
            .Where(f => f.Statut != StatutFactureFournisseur.Payee
                     && f.Statut != StatutFactureFournisseur.Annulee)
            .ToListAsync();

        decimal Solde(FactureFournisseur f) => f.MontantTTC - f.MontantPaye;
        int     Jours(FactureFournisseur f) => (now - f.DateEcheance.Date).Days;

        return factures.GroupBy(f => f.Fournisseur).Select(g =>
        {
            var fournisseur = g.Key!;
            var lignes      = g.ToList();
            return new LigneAgingAP
            {
                FournisseurId  = fournisseur.Id,
                FournisseurNom = fournisseur.Nom,
                Courant        = lignes.Where(f => Jours(f) <= 0) .Sum(Solde),
                Jours0_30      = lignes.Where(f => Jours(f) is > 0 and <= 30) .Sum(Solde),
                Jours31_60     = lignes.Where(f => Jours(f) is > 30 and <= 60).Sum(Solde),
                Jours61_90     = lignes.Where(f => Jours(f) is > 60 and <= 90).Sum(Solde),
                Plus90         = lignes.Where(f => Jours(f) > 90).Sum(Solde),
                Total          = lignes.Sum(Solde)
            };
        }).Where(l => l.Total > 0).OrderByDescending(l => l.Total);
    }

    // ── KPIs AP ──────────────────────────────────────────────────────────────
    public async Task<APKpis> GetKpisAsync()
    {
        var now = DateTime.UtcNow.Date;
        var semaine = now.AddDays(7);

        var factures = await _ctx.FacturesFournisseurs
            .Where(f => f.Statut != StatutFactureFournisseur.Payee
                     && f.Statut != StatutFactureFournisseur.Annulee)
            .ToListAsync();

        decimal Solde(FactureFournisseur f) => f.MontantTTC - f.MontantPaye;

        return new APKpis
        {
            TotalAP          = factures.Sum(Solde),
            APayerAujourdhui = factures.Where(f => f.DateEcheance.Date == now).Sum(Solde),
            APayerSemaine    = factures.Where(f => f.DateEcheance.Date <= semaine).Sum(Solde),
            EnSouffrance     = factures.Where(f => f.DateEcheance.Date < now).Sum(Solde),
            NombreEnAttente  = factures.Count
        };
    }

    // ── Paiement en lot ───────────────────────────────────────────────────────
    public async Task<PayerEnLotResultat> PayerEnLotAsync(
        IEnumerable<int> factureIds, ModePaiement mode, string reference = "")
    {
        var ids      = factureIds.ToList();
        var factures = await _ctx.FacturesFournisseurs
            .Where(f => ids.Contains(f.Id))
            .ToListAsync();

        decimal totalPaye = 0;
        foreach (var facture in factures)
        {
            var montant = facture.MontantTTC - facture.MontantPaye;
            if (montant <= 0) continue;

            _ctx.PaiementsFournisseurs.Add(new PaiementFournisseur
            {
                FactureFournisseurId = facture.Id,
                Montant              = montant,
                DatePaiement         = DateTime.UtcNow,
                ModePaiement         = mode,
                Reference            = reference
            });

            facture.MontantPaye = facture.MontantTTC;
            facture.Statut      = StatutFactureFournisseur.Payee;
            totalPaye          += montant;
        }

        await _ctx.SaveChangesAsync();

        return new PayerEnLotResultat
        {
            NombreFactures = factures.Count,
            TotalPaye      = totalPaye,
            Succes = true,
            Message = $"✅ {factures.Count} facture(s) réglée(s) pour un total de {totalPaye:C}"
        };
    }

    // ── Échéancier AP ─────────────────────────────────────────────────────────
    public async Task<IEnumerable<EcheancierAP>> GetEcheancierAsync(int joursHorizon = 30)
    {
        var debut = DateTime.UtcNow.Date;
        var fin   = debut.AddDays(joursHorizon);

        var factures = await _ctx.FacturesFournisseurs
            .Include(f => f.Fournisseur)
            .Where(f => f.DateEcheance >= debut
                     && f.DateEcheance <= fin
                     && f.Statut != StatutFactureFournisseur.Payee
                     && f.Statut != StatutFactureFournisseur.Annulee)
            .OrderBy(f => f.DateEcheance)
            .ToListAsync();

        return factures.Select(f => new EcheancierAP
        {
            DateEcheance   = f.DateEcheance,
            FournisseurNom = f.Fournisseur.Nom,
            NumeroFacture  = f.Numero,
            MontantDu      = f.MontantTTC - f.MontantPaye,
            JoursRestants  = (f.DateEcheance.Date - debut).Days
        });
    }

    // ── Factures fournisseurs à approuver ────────────────────────────────────
    public async Task<IEnumerable<FactureFournisseur>> GetFacturesEnAttenteAsync()
        => await _ctx.FacturesFournisseurs
            .Include(f => f.Fournisseur)
            .Where(f => f.Statut == StatutFactureFournisseur.Recue)
            .OrderBy(f => f.DateEcheance)
            .ToListAsync();
}

// ── DTOs AP ──────────────────────────────────────────────────────────────────
public class LigneAgingAP
{
    public int     FournisseurId  { get; set; }
    public string  FournisseurNom { get; set; } = "";
    public decimal Courant        { get; set; }
    public decimal Jours0_30      { get; set; }
    public decimal Jours31_60     { get; set; }
    public decimal Jours61_90     { get; set; }
    public decimal Plus90         { get; set; }
    public decimal Total          { get; set; }
}

public class APKpis
{
    public decimal TotalAP          { get; set; }
    public decimal APayerAujourdhui { get; set; }
    public decimal APayerSemaine    { get; set; }
    public decimal EnSouffrance     { get; set; }
    public int     NombreEnAttente  { get; set; }
}

public class EcheancierAP
{
    public DateTime DateEcheance   { get; set; }
    public string   FournisseurNom { get; set; } = "";
    public string   NumeroFacture  { get; set; } = "";
    public decimal  MontantDu      { get; set; }
    public int      JoursRestants  { get; set; }
}

public class PayerEnLotResultat
{
    public bool    Succes         { get; set; }
    public int     NombreFactures { get; set; }
    public decimal TotalPaye      { get; set; }
    public string  Message        { get; set; } = "";
}
