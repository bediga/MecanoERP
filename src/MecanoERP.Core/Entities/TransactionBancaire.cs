namespace MecanoERP.Core.Entities;

public enum TypeTransactionBancaire
{
    Credit,  // Dépôt / Encaissement
    Debit    // Paiement / Retrait
}

public class TransactionBancaire
{
    public int Id { get; set; }
    public int CompteBancaireId { get; set; }
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public string Description { get; set; } = string.Empty;
    public decimal Montant { get; set; }
    public TypeTransactionBancaire TypeTx { get; set; }
    public bool EstRapproche { get; set; }
    public int? PaiementId { get; set; }
    public int? PaiementFournisseurId { get; set; }
    public string Reference { get; set; } = string.Empty;

    // Navigation
    public CompteBancaire CompteBancaire { get; set; } = null!;
}
