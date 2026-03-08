using MecanoERP.Core.Entities.Comptabilite;
using MecanoERP.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MecanoERP.Infrastructure.Services.Comptabilite;

public class GrandLivreService
{
    private readonly MecanoDbContext _context;
    public GrandLivreService(MecanoDbContext context) => _context = context;

    // ── Grand Livre d'un compte ──────────────────────────────────────────────
    public async Task<GrandLivreCompte> GetGrandLivreAsync(
        int compteId, DateTime debut, DateTime fin)
    {
        var compte = await _context.ComptesGL.FindAsync(compteId)
            ?? throw new Exception("Compte introuvable.");

        var lignes = await _context.LignesJournal
            .Include(l => l.Journal)
            .Include(l => l.CentreDeCoût)
            .Where(l => l.CompteGLId == compteId
                     && !l.Journal.EstBrouillon
                     && !l.Journal.EstAnnule
                     && l.Journal.Date >= debut
                     && l.Journal.Date <= fin)
            .OrderBy(l => l.Journal.Date)
            .ThenBy(l => l.Journal.Id)
            .ToListAsync();

        // Solde d'ouverture = mouvements AVANT la période
        var soldePrecedent = await _context.LignesJournal
            .Include(l => l.Journal)
            .Where(l => l.CompteGLId == compteId
                     && !l.Journal.EstBrouillon
                     && !l.Journal.EstAnnule
                     && l.Journal.Date < debut)
            .SumAsync(l => l.Debit - l.Credit);

        // Construire les lignes avec solde cumulatif
        var solde = soldePrecedent;
        var lignesGL = lignes.Select(l =>
        {
            solde += l.Debit - l.Credit;
            return new LigneGrandLivre
            {
                Date         = l.Journal.Date,
                Numero       = l.Journal.Numero,
                JournalCode  = l.Journal.JournalCode,
                Description  = l.Description.Length > 0 ? l.Description : l.Journal.Description,
                Reference    = l.Journal.ReferenceExterne,
                CentreCoût   = l.CentreDeCoût?.Code ?? "",
                Debit        = l.Debit,
                Credit       = l.Credit,
                Solde        = solde
            };
        }).ToList();

        return new GrandLivreCompte
        {
            Compte       = compte,
            DateDebut    = debut,
            DateFin      = fin,
            SoldeOuverture = soldePrecedent,
            Lignes       = lignesGL,
            TotalDebit   = lignesGL.Sum(l => l.Debit),
            TotalCredit  = lignesGL.Sum(l => l.Credit),
            SoldeFinal   = solde
        };
    }

    // ── Balance de vérification ──────────────────────────────────────────────
    public async Task<BalanceVerification> GetBalanceVerificationAsync(
        int exercice, int? periode = null)
    {
        var comptes = await _context.ComptesGL
            .Where(c => c.EstActif)
            .OrderBy(c => c.Numero)
            .ToListAsync();

        var lignesQuery = _context.LignesJournal
            .Include(l => l.Journal)
            .Where(l => !l.Journal.EstBrouillon
                     && !l.Journal.EstAnnule
                     && l.Journal.Exercice == exercice);

        if (periode.HasValue)
            lignesQuery = lignesQuery.Where(l => l.Journal.Periode == periode.Value);

        var lignes = await lignesQuery.ToListAsync();

        var lignesBalance = comptes.Select(c =>
        {
            var mvts = lignes.Where(l => l.CompteGLId == c.Id).ToList();
            var totalD = mvts.Sum(l => l.Debit);
            var totalC = mvts.Sum(l => l.Credit);
            var solde  = totalD - totalC;
            return new LigneBalance
            {
                Numero         = c.Numero,
                Nom            = c.Nom,
                TypeCompte     = c.TypeCompte.ToString(),
                TotalDebit     = totalD,
                TotalCredit    = totalC,
                SoldeDebiteur  = solde > 0 ? solde : 0,
                SoldeCrediteur = solde < 0 ? -solde : 0
            };
        }).Where(l => l.TotalDebit != 0 || l.TotalCredit != 0)
          .ToList();

        return new BalanceVerification
        {
            Exercice    = exercice,
            Periode     = periode,
            Lignes      = lignesBalance,
            TotalDebit  = lignesBalance.Sum(l => l.TotalDebit),
            TotalCredit = lignesBalance.Sum(l => l.TotalCredit),
            EstEquilibre = Math.Abs(lignesBalance.Sum(l => l.TotalDebit) - lignesBalance.Sum(l => l.TotalCredit)) < 0.01m
        };
    }

    // ── Tous les comptes GL (pour ComboBox) ──────────────────────────────────
    public async Task<IEnumerable<CompteGL>> GetComptesActifsAsync()
        => await _context.ComptesGL.Where(c => c.EstActif).OrderBy(c => c.Numero).ToListAsync();

    // ── Centres de coûts ─────────────────────────────────────────────────────
    public async Task<IEnumerable<CentreDeCoût>> GetCentresDeCoutsAsync()
        => await _context.CentresDeCoût.Where(c => c.EstActif).OrderBy(c => c.Code).ToListAsync();
}

// ── DTOs ─────────────────────────────────────────────────────────────────────
public record LigneGrandLivre
{
    public DateTime Date        { get; init; }
    public string   Numero      { get; init; } = "";
    public string   JournalCode { get; init; } = "";
    public string   Description { get; init; } = "";
    public string   Reference   { get; init; } = "";
    public string   CentreCoût  { get; init; } = "";
    public decimal  Debit       { get; init; }
    public decimal  Credit      { get; init; }
    public decimal  Solde       { get; init; }
}

public record GrandLivreCompte
{
    public CompteGL          Compte         { get; init; } = null!;
    public DateTime          DateDebut      { get; init; }
    public DateTime          DateFin        { get; init; }
    public decimal           SoldeOuverture { get; init; }
    public List<LigneGrandLivre> Lignes    { get; init; } = [];
    public decimal           TotalDebit     { get; init; }
    public decimal           TotalCredit    { get; init; }
    public decimal           SoldeFinal     { get; init; }
}

public record LigneBalance
{
    public string  Numero         { get; init; } = "";
    public string  Nom            { get; init; } = "";
    public string  TypeCompte     { get; init; } = "";
    public decimal TotalDebit     { get; init; }
    public decimal TotalCredit    { get; init; }
    public decimal SoldeDebiteur  { get; init; }
    public decimal SoldeCrediteur { get; init; }
}

public record BalanceVerification
{
    public int              Exercice     { get; init; }
    public int?             Periode      { get; init; }
    public List<LigneBalance> Lignes     { get; init; } = [];
    public decimal          TotalDebit   { get; init; }
    public decimal          TotalCredit  { get; init; }
    public bool             EstEquilibre { get; init; }
}
