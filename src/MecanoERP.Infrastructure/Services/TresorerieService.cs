using MecanoERP.Core.Entities;
using MecanoERP.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MecanoERP.Infrastructure.Services;

/// <summary>Service trésorerie enrichi — rapprochement, import relevé, prévisions.</summary>
public class TresorerieService
{
    private readonly MecanoDbContext _ctx;
    public TresorerieService(MecanoDbContext ctx) => _ctx = ctx;

    // ── Comptes bancaires ──────────────────────────────────────────────────
    public async Task<IEnumerable<CompteBancaire>> GetComptesAsync()
        => await _ctx.ComptesBancaires.Where(c => c.EstActif).OrderBy(c => c.Nom).ToListAsync();

    // ── Solde de rapprochement ─────────────────────────────────────────────
    public async Task<SoldeRapprochement> GetSoldeRapprochementAsync(int compteId)
    {
        var compte = await _ctx.ComptesBancaires.FindAsync(compteId)
            ?? throw new Exception("Compte bancaire introuvable.");

        var txs = await _ctx.TransactionsBancaires
            .Where(t => t.CompteBancaireId == compteId)
            .ToListAsync();

        var credits  = txs.Where(t => t.TypeTx == TypeTransactionBancaire.Credit) .Sum(t => t.Montant);
        var debits   = txs.Where(t => t.TypeTx == TypeTransactionBancaire.Debit)  .Sum(t => t.Montant);
        var rapprochés = txs.Where(t => t.EstRapproche);

        var soldeLivres = compte.SoldeOuverture + credits - debits;
        var soldeBanque = compte.SoldeOuverture
            + rapprochés.Where(t => t.TypeTx == TypeTransactionBancaire.Credit).Sum(t => t.Montant)
            - rapprochés.Where(t => t.TypeTx == TypeTransactionBancaire.Debit) .Sum(t => t.Montant);

        return new SoldeRapprochement
        {
            CompteId          = compteId,
            CompteNom         = compte.Nom,
            SoldeLivres       = soldeLivres,
            SoldeBanque       = soldeBanque,
            Ecart             = soldeLivres - soldeBanque,
            NbNonRapproches   = txs.Count(t => !t.EstRapproche),
            EstEquilibre      = Math.Abs(soldeLivres - soldeBanque) < 0.01m
        };
    }

    // ── Transactions (filtrage) ────────────────────────────────────────────
    public async Task<IEnumerable<TransactionBancaire>> GetTransactionsAsync(
        int compteId, DateTime? debut = null, DateTime? fin = null, bool? rapprochees = null)
    {
        var q = _ctx.TransactionsBancaires.Where(t => t.CompteBancaireId == compteId);
        if (debut.HasValue)      q = q.Where(t => t.Date >= debut.Value);
        if (fin.HasValue)        q = q.Where(t => t.Date <= fin.Value);
        if (rapprochees.HasValue) q = q.Where(t => t.EstRapproche == rapprochees.Value);
        return await q.OrderByDescending(t => t.Date).ToListAsync();
    }

    // ── Rapprocher une transaction ─────────────────────────────────────────
    public async Task RapprocherAsync(int transactionId)
    {
        var tx = await _ctx.TransactionsBancaires.FindAsync(transactionId);
        if (tx is not null) { tx.EstRapproche = true; await _ctx.SaveChangesAsync(); }
    }

    // ── Rapprochement automatique par montant+date ─────────────────────────
    public async Task<int> AutoRapprocherAsync(int compteId, DateTime debut, DateTime fin)
    {
        var txs = await _ctx.TransactionsBancaires
            .Where(t => t.CompteBancaireId == compteId && !t.EstRapproche
                     && t.Date >= debut && t.Date <= fin)
            .ToListAsync();

        // Simple heuristique : uniquement si montant exact trouvé dans les paiements clients
        var paiements   = await _ctx.Paiements.Where(p => p.DatePaiement >= debut && p.DatePaiement <= fin).ToListAsync();
        var paiementsFourn = await _ctx.PaiementsFournisseurs.Where(p => p.DatePaiement >= debut && p.DatePaiement <= fin).ToListAsync();

        int count = 0;
        foreach (var tx in txs)
        {
            var match = tx.TypeTx == TypeTransactionBancaire.Credit
                ? paiements.Any(p => p.Montant == tx.Montant)
                : paiementsFourn.Any(p => p.Montant == tx.Montant);

            if (match) { tx.EstRapproche = true; count++; }
        }

        await _ctx.SaveChangesAsync();
        return count;
    }

    // ── Import relevé CSV (format générique : date,description,montant,type) ─
    public async Task<ImportRelevéResultat> ImporterRelevéAsync(int compteId, string csvContent)
    {
        var lignes   = csvContent.Split('\n').Skip(1); // skip header
        int importées = 0, erreurs = 0;

        foreach (var ligne in lignes)
        {
            if (string.IsNullOrWhiteSpace(ligne)) continue;
            try
            {
                var champs = ligne.Split(',');
                var date    = DateTime.Parse(champs[0].Trim());
                var descr   = champs[1].Trim().Trim('"');
                var montant = decimal.Parse(champs[2].Trim());
                var type    = montant >= 0 ? TypeTransactionBancaire.Credit : TypeTransactionBancaire.Debit;

                _ctx.TransactionsBancaires.Add(new TransactionBancaire
                {
                    CompteBancaireId = compteId,
                    Date             = date,
                    Description      = descr,
                    Montant          = Math.Abs(montant),
                    TypeTx           = type,
                    EstRapproche     = false
                });
                importées++;
            }
            catch { erreurs++; }
        }

        if (importées > 0) await _ctx.SaveChangesAsync();
        return new ImportRelevéResultat
        {
            LignesImportées = importées,
            LignesErreur    = erreurs,
            Message = $"✅ {importées} transaction(s) importée(s), {erreurs} erreur(s)."
        };
    }

    // ── Prévisions de trésorerie ───────────────────────────────────────────
    public async Task<IEnumerable<PrevisionTresorerie>> GetPrevisionsAsync(int joursHorizon = 30)
    {
        var debut = DateTime.UtcNow.Date;
        var fin   = debut.AddDays(joursHorizon);
        var previsions = new List<PrevisionTresorerie>();

        // Encaissements attendus (factures AR, non payées, échues dans la période)
        var facturesAR = await _ctx.Factures
            .Include(f => f.Client)
            .Where(f => f.DateEcheance >= debut && f.DateEcheance <= fin
                     && (f.Statut == StatutFacture.Emise || f.Statut == StatutFacture.PartiellementPayee))
            .ToListAsync();

        previsions.AddRange(facturesAR.Select(f => new PrevisionTresorerie
        {
            Date        = f.DateEcheance.GetValueOrDefault(DateTime.UtcNow),
            Type        = TypeFlux.Encaissement,
            Montant     = (f.MontantHT * (1 + f.TauxTPS + f.TauxTVQ)) - f.MontantPaye,
            Source      = $"Facture {f.Numero} — {f.Client?.Prenom} {f.Client?.Nom}",
            EstRéalisé  = false
        }));

        // Décaissements attendus (factures AP)
        var facturesAP = await _ctx.FacturesFournisseurs
            .Include(f => f.Fournisseur)
            .Where(f => f.DateEcheance >= debut && f.DateEcheance <= fin
                     && f.Statut != StatutFactureFournisseur.Payee
                     && f.Statut != StatutFactureFournisseur.Annulee)
            .ToListAsync();

        previsions.AddRange(facturesAP.Select(f => new PrevisionTresorerie
        {
            Date       = f.DateEcheance,
            Type       = TypeFlux.Decaissement,
            Montant    = f.MontantTTC - f.MontantPaye,
            Source     = $"Facture fourn. {f.Numero} — {f.Fournisseur?.Nom}",
            EstRéalisé = false
        }));

        return previsions.OrderBy(p => p.Date);
    }
}

// ── DTOs Trésorerie ──────────────────────────────────────────────────────────
public enum TypeFlux { Encaissement, Decaissement }

public class SoldeRapprochement
{
    public int     CompteId        { get; set; }
    public string  CompteNom       { get; set; } = "";
    public decimal SoldeLivres     { get; set; }
    public decimal SoldeBanque     { get; set; }
    public decimal Ecart           { get; set; }
    public int     NbNonRapproches { get; set; }
    public bool    EstEquilibre    { get; set; }
}

public class PrevisionTresorerie
{
    public DateTime Date       { get; set; }
    public TypeFlux Type       { get; set; }
    public decimal  Montant    { get; set; }
    public string   Source     { get; set; } = "";
    public bool     EstRéalisé { get; set; }
}

public class ImportRelevéResultat
{
    public int    LignesImportées { get; set; }
    public int    LignesErreur   { get; set; }
    public string Message        { get; set; } = "";
}
