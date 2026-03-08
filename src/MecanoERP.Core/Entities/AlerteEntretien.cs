namespace MecanoERP.Core.Entities;

public enum TypeAlerte
{
    HuileMoteur,
    Pneus,
    Freins,
    FiltreAir,
    Courroie,
    Bougies,
    LiquidRefroidissement,
    RevisionGenerale,
    InspectionGouvt,
    Autre
}

public class AlerteEntretien
{
    public int Id { get; set; }
    public int VehiculeId { get; set; }
    public TypeAlerte TypeAlerte { get; set; }
    public DateTime? DateAlerte { get; set; }
    public int? KilometrageAlerte { get; set; }
    public bool EstResolue { get; set; }
    public DateTime? DateResolution { get; set; }
    public string Notes { get; set; } = string.Empty;
    public DateTime DateCreation { get; set; } = DateTime.UtcNow;

    // Navigation
    public Vehicule Vehicule { get; set; } = null!;
}
