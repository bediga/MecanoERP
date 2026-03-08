using MecanoERP.Core.Entities;
using MecanoERP.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MecanoERP.Infrastructure.Services;

public class BanqueService
{
    private readonly MecanoDbContext _ctx;
    public BanqueService(MecanoDbContext ctx) => _ctx = ctx;

    public async Task<IEnumerable<CompteBancaire>> GetComptesAsync()
        => await _ctx.ComptesBancaires
            .Where(c => c.EstActif)
            .OrderBy(c => c.Nom)
            .ToListAsync();

    public async Task<CompteBancaire> AjouterCompteAsync(CompteBancaire compte)
    {
        _ctx.ComptesBancaires.Add(compte);
        await _ctx.SaveChangesAsync();
        return compte;
    }

    public async Task<IEnumerable<TransactionBancaire>> GetTransactionsAsync(int compteId, DateTime? debut = null, DateTime? fin = null)
    {
        var query = _ctx.TransactionsBancaires
            .Where(t => t.CompteBancaireId == compteId);

        if (debut.HasValue) query = query.Where(t => t.Date >= debut.Value);
        if (fin.HasValue)   query = query.Where(t => t.Date <= fin.Value);

        return await query.OrderByDescending(t => t.Date).ToListAsync();
    }

    public async Task<TransactionBancaire> AjouterTransactionAsync(TransactionBancaire tx)
    {
        _ctx.TransactionsBancaires.Add(tx);

        // Mise à jour solde compte
        var compte = await _ctx.ComptesBancaires.FindAsync(tx.CompteBancaireId);
        if (compte != null)
            compte.SoldeCourant += tx.TypeTx == TypeTransactionBancaire.Credit ? tx.Montant : -tx.Montant;

        await _ctx.SaveChangesAsync();
        return tx;
    }

    public async Task RapprocherAsync(int transactionId)
    {
        var tx = await _ctx.TransactionsBancaires.FindAsync(transactionId);
        if (tx != null) { tx.EstRapproche = true; await _ctx.SaveChangesAsync(); }
    }

    public async Task<(decimal Debits, decimal Credits, decimal SoldeCalcule)> GetSoldeAsync(int compteId)
    {
        var transactions = await _ctx.TransactionsBancaires
            .Where(t => t.CompteBancaireId == compteId)
            .ToListAsync();
        var credits = transactions.Where(t => t.TypeTx == TypeTransactionBancaire.Credit).Sum(t => t.Montant);
        var debits  = transactions.Where(t => t.TypeTx == TypeTransactionBancaire.Debit).Sum(t => t.Montant);
        var compte = await _ctx.ComptesBancaires.FindAsync(compteId);
        return (debits, credits, (compte?.SoldeOuverture ?? 0) + credits - debits);
    }
}
