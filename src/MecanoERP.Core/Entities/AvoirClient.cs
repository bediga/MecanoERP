namespace MecanoERP.Core.Entities;

public enum StatutAvoir
{
    Brouillon,
    Emis,
    Applique,
    Annule
}

public class AvoirClient
{
    public int Id { get; set; }
    public string Numero { get; set; } = string.Empty;
    public int ClientId { get; set; }
    public int? FactureOrigineId { get; set; }
    public DateTime DateAvoir { get; set; } = DateTime.UtcNow;
    public decimal Montant { get; set; }
    public string Motif { get; set; } = string.Empty;
    public StatutAvoir Statut { get; set; } = StatutAvoir.Brouillon;
    public string Notes { get; set; } = string.Empty;

    // Navigation
    public Client Client { get; set; } = null!;
    public Facture? FactureOrigine { get; set; }
}
