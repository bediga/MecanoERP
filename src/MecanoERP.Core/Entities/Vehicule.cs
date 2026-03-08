namespace MecanoERP.Core.Entities;

public class Vehicule
{
    public int Id { get; set; }
    public int ClientId { get; set; }
    public string Marque { get; set; } = string.Empty;
    public string Modele { get; set; } = string.Empty;
    public int Annee { get; set; }
    public string VIN { get; set; } = string.Empty;
    public string Immatriculation { get; set; } = string.Empty;
    public int Kilometrage { get; set; }
    public string TypeMoteur { get; set; } = string.Empty; // Essence, Diesel, Hybride, Électrique
    public string Couleur { get; set; } = string.Empty;
    public string Transmission { get; set; } = string.Empty;
    public DateTime DateCreation { get; set; } = DateTime.UtcNow;

    // Navigation
    public Client Client { get; set; } = null!;
    public ICollection<OrdreTravail> OrdresTravail { get; set; } = new List<OrdreTravail>();
    public ICollection<RendezVous> RendezVous { get; set; } = new List<RendezVous>();
}
