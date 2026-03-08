namespace MecanoERP.Core.Entities;

public enum StatutDevis
{
    Brouillon,
    Envoye,
    Accepte,
    Refuse,
    Expire,
    ConvertienOT
}

public class Devis
{
    public int Id { get; set; }
    public string Numero { get; set; } = string.Empty;
    public int ClientId { get; set; }
    public int? VehiculeId { get; set; }
    public DateTime DateDevis { get; set; } = DateTime.UtcNow;
    public DateTime DateValidite { get; set; } = DateTime.UtcNow.AddDays(30);
    public string Notes { get; set; } = string.Empty;
    public StatutDevis Statut { get; set; } = StatutDevis.Brouillon;
    public int? OrdreTravailId { get; set; }
    public DateTime DateCreation { get; set; } = DateTime.UtcNow;

    // Navigation
    public Client Client { get; set; } = null!;
    public Vehicule? Vehicule { get; set; }
    public OrdreTravail? OrdreTravail { get; set; }
    public ICollection<LigneDevis> Lignes { get; set; } = new List<LigneDevis>();

    // Calculs
    public decimal SousTotal => Lignes.Sum(l => l.Total);
    public decimal TotalTaxes => SousTotal * 0.14975m;
    public decimal TotalTTC => SousTotal + TotalTaxes;
}
