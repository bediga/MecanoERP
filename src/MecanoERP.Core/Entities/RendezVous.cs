namespace MecanoERP.Core.Entities;

public enum StatutRDV
{
    Confirme,
    EnAttente,
    Arrive,
    Termine,
    Annule,
    NoShow
}

public class RendezVous
{
    public int Id { get; set; }
    public int ClientId { get; set; }
    public int? VehiculeId { get; set; }
    public int? EmployeId { get; set; }
    public DateTime DateHeure { get; set; }
    public int DureeMinutes { get; set; } = 60;
    public string TypeService { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public StatutRDV Statut { get; set; } = StatutRDV.EnAttente;
    public bool RappelEnvoye { get; set; }
    public DateTime DateCreation { get; set; } = DateTime.UtcNow;

    // Navigation
    public Client Client { get; set; } = null!;
    public Vehicule? Vehicule { get; set; }
    public Employe? Employe { get; set; }
}
