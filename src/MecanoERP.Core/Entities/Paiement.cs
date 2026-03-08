namespace MecanoERP.Core.Entities;

public enum ModePaiement
{
    Especes,
    Carte,
    Virement,
    Cheque,
    EnLigne
}

public class Paiement
{
    public int Id { get; set; }
    public int FactureId { get; set; }
    public decimal Montant { get; set; }
    public DateTime DatePaiement { get; set; } = DateTime.UtcNow;
    public ModePaiement ModePaiement { get; set; }
    public string Reference { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;

    // Navigation
    public Facture Facture { get; set; } = null!;
}
