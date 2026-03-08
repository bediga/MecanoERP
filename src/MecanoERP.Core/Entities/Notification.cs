namespace MecanoERP.Core.Entities;

public enum TypeNotification
{
    VehiculePrep,
    PieceArrivee,
    EntretienAPrevoir,
    PaiementEnRetard,
    StockCritique,
    System
}

public class Notification
{
    public int Id { get; set; }
    public string Titre { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public TypeNotification Type { get; set; }
    public bool Lu { get; set; }
    public int? ClientId { get; set; }
    public int? OrdreTravailId { get; set; }
    public DateTime DateCreation { get; set; } = DateTime.UtcNow;
}
