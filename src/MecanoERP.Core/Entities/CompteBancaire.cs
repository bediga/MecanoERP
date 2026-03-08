namespace MecanoERP.Core.Entities;

public class CompteBancaire
{
    public int Id { get; set; }
    public string Nom { get; set; } = string.Empty;
    public string Numero { get; set; } = string.Empty;
    public string Institution { get; set; } = string.Empty;
    public string Devise { get; set; } = "CAD";
    public decimal SoldeOuverture { get; set; }
    public decimal SoldeCourant { get; set; }
    public DateTime DateCreation { get; set; } = DateTime.UtcNow;
    public bool EstActif { get; set; } = true;

    // Navigation
    public ICollection<TransactionBancaire> Transactions { get; set; } = new List<TransactionBancaire>();
}
