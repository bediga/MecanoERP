namespace MecanoERP.Core.Entities;

public class Employe
{
    public int Id { get; set; }
    public string Nom { get; set; } = string.Empty;
    public string Prenom { get; set; } = string.Empty;
    public string Poste { get; set; } = string.Empty; // Mécanicien, Réceptionniste, Gérant
    public string Telephone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public decimal TauxHoraire { get; set; }
    public decimal TauxCommission { get; set; }
    public DateTime DateEmbauche { get; set; }
    public bool EstActif { get; set; } = true;
    public string Notes { get; set; } = string.Empty;

    // Navigation
    public ICollection<OrdreTravail> OrdresTravail { get; set; } = new List<OrdreTravail>();
}
