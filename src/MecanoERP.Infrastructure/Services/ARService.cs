using MecanoERP.Core.Entities;
using MecanoERP.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MecanoERP.Infrastructure.Services;

/// <summary>Service AR enrichi — Aging, relances, encaissements.</summary>
public class ARService
{
    private readonly MecanoDbContext _ctx;
    public ARService(MecanoDbContext ctx) => _ctx = ctx;

    // ── Rapport d'âge AR ─────────────────────────────────────────────────────
    public async Task<IEnumerable<LigneAgingAR>> GetAgingARAsync()
    {
        var now = DateTime.UtcNow.Date;
        var factures = await _ctx.Factures
            .Include(f => f.Client)
            .Where(f => f.Statut == StatutFacture.Emise
                     || f.Statut == StatutFacture.PartiellementPayee)
            .ToListAsync();

        decimal Solde(Facture f) => (f.MontantHT * (1 + f.TauxTPS + f.TauxTVQ)) - f.MontantPaye;
        int     Jours(Facture f) => (now - f.DateEcheance.GetValueOrDefault(now).Date).Days;

        return factures.GroupBy(f => f.Client).Select(g =>
        {
            var client = g.Key;
            var lignes  = g.ToList();
            return new LigneAgingAR
            {
                ClientId   = client!.Id,
                ClientNom  = client.Prenom + " " + client.Nom,
                Courant    = lignes.Where(f => Jours(f) <= 0) .Sum(Solde),
                Jours0_30  = lignes.Where(f => Jours(f) is > 0 and <= 30) .Sum(Solde),
                Jours31_60 = lignes.Where(f => Jours(f) is > 30 and <= 60).Sum(Solde),
                Jours61_90 = lignes.Where(f => Jours(f) is > 60 and <= 90).Sum(Solde),
                Plus90     = lignes.Where(f => Jours(f) > 90).Sum(Solde),
                Total      = lignes.Sum(Solde)
            };
        }).Where(l => l.Total > 0).OrderByDescending(l => l.Total);
    }

    // ── KPIs AR ──────────────────────────────────────────────────────────────
    public async Task<ARKpis> GetKpisAsync()
    {
        var now      = DateTime.UtcNow.Date;
        var factures = await _ctx.Factures
            .Where(f => f.Statut == StatutFacture.Emise || f.Statut == StatutFacture.PartiellementPayee)
            .ToListAsync();

        decimal Solde(Facture f) => (f.MontantHT * (1 + f.TauxTPS + f.TauxTVQ)) - f.MontantPaye;
        int     Jours(Facture f) => (now - f.DateEcheance.GetValueOrDefault(now).Date).Days;

        return new ARKpis
        {
            TotalAR          = factures.Sum(Solde),
            EchuesToday      = factures.Where(f => Jours(f) == 0).Sum(Solde),
            EnSouffrance90   = factures.Where(f => Jours(f) > 90).Sum(Solde),
            NombreEnRetard   = factures.Count(f => Jours(f) > 0),
            TauxRecouvrement = await GetTauxRecouvrementAsync()
        };
    }

    private async Task<decimal> GetTauxRecouvrementAsync()
    {
        var debut    = DateTime.UtcNow.AddMonths(-3).Date;
        var total    = await _ctx.Factures.Where(f => f.DateFacture >= debut).SumAsync(f => f.MontantHT);
        var encaisse = await _ctx.Paiements.Where(p => p.DatePaiement >= debut).SumAsync(p => p.Montant);
        return total == 0 ? 100m : Math.Round(encaisse / total * 100, 1);
    }

    // ── Encaissement ─────────────────────────────────────────────────────────
    public async Task EncaisserAsync(int factureId, decimal montant, ModePaiement mode,
        string reference = "", int? compteGLId = null)
    {
        var facture = await _ctx.Factures
            .Include(f => f.Client)
            .FirstOrDefaultAsync(f => f.Id == factureId)
            ?? throw new Exception("Facture introuvable.");

        _ctx.Paiements.Add(new Paiement
        {
            FactureId    = factureId,
            Montant      = montant,
            DatePaiement = DateTime.UtcNow,
            ModePaiement = mode,
            Reference    = reference
        });

        facture.MontantPaye += montant;
        var ttc = facture.MontantHT * (1 + facture.TauxTPS + facture.TauxTVQ);
        facture.Statut = facture.MontantPaye >= ttc ? StatutFacture.Payee : StatutFacture.PartiellementPayee;

        await _ctx.SaveChangesAsync();
    }

    // ── Clients en retard ────────────────────────────────────────────────────
    public async Task<IEnumerable<ClientEnRetard>> GetClientsEnRetardAsync(int joursMinimum = 1)
    {
        var now      = DateTime.UtcNow.Date;
        var factures = await _ctx.Factures
            .Include(f => f.Client)
            .Where(f => f.Statut == StatutFacture.Emise || f.Statut == StatutFacture.PartiellementPayee)
            .ToListAsync();

        return factures
            .Where(f => f.DateEcheance.HasValue && f.DateEcheance.Value.Date < now)
            .GroupBy(f => f.Client).Select(g =>
            {
                var client = g.Key!;
                var retard = g.Max(f => (now - f.DateEcheance!.Value.Date).Days);
                var solde  = g.Sum(f => (f.MontantHT * (1 + f.TauxTPS + f.TauxTVQ)) - f.MontantPaye);
                return new ClientEnRetard
                {
                    ClientId       = client.Id,
                    Nom            = client.Prenom + " " + client.Nom,
                    Telephone      = client.Telephone,
                    Email          = client.Email,
                    NombreFactures = g.Count(),
                    SoldeEchu      = solde,
                    JoursRetard    = retard,
                    Niveau         = retard switch { <= 30 => 1, <= 60 => 2, _ => 3 }
                };
            }).Where(c => c.JoursRetard >= joursMinimum)
              .OrderByDescending(c => c.JoursRetard);
    }
}

// ── DTOs AR ──────────────────────────────────────────────────────────────────
public class LigneAgingAR
{
    public int     ClientId   { get; set; }
    public string  ClientNom  { get; set; } = "";
    public decimal Courant    { get; set; }
    public decimal Jours0_30  { get; set; }
    public decimal Jours31_60 { get; set; }
    public decimal Jours61_90 { get; set; }
    public decimal Plus90     { get; set; }
    public decimal Total      { get; set; }
}

public class ARKpis
{
    public decimal TotalAR          { get; set; }
    public decimal EchuesToday      { get; set; }
    public decimal EnSouffrance90   { get; set; }
    public int     NombreEnRetard   { get; set; }
    public decimal TauxRecouvrement { get; set; }
}

public class LimiteCreditResultat
{
    public bool    AutorisePas    { get; set; }
    public decimal SoldeActuel    { get; set; }
    public decimal MontantNouveau { get; set; }
    public decimal NouveauSolde   { get; set; }
    public decimal Limite         { get; set; }
    public decimal Disponible     { get; set; }
    public string  Message        { get; set; } = "";
}

public class ClientEnRetard
{
    public int     ClientId       { get; set; }
    public string  Nom            { get; set; } = "";
    public string  Telephone      { get; set; } = "";
    public string  Email          { get; set; } = "";
    public int     NombreFactures { get; set; }
    public decimal SoldeEchu      { get; set; }
    public int     JoursRetard    { get; set; }
    public int     Niveau         { get; set; }
}
