namespace MecanoERP.Core.Entities;

public class PaiementFournisseur
{
    public int Id { get; set; }
    public int FactureFournisseurId { get; set; }
    public decimal Montant { get; set; }
    public DateTime DatePaiement { get; set; } = DateTime.UtcNow;
    public ModePaiement ModePaiement { get; set; }
    public string Reference { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;

    // Navigation
    public FactureFournisseur FactureFournisseur { get; set; } = null!;
}
