namespace MecanoERP.Core.Entities;

public class Pointage
{
    public int Id { get; set; }
    public int EmployeId { get; set; }
    public int? OrdreTravailId { get; set; }
    public DateTime DateDebut { get; set; } = DateTime.UtcNow;
    public DateTime? DateFin { get; set; }
    public string TypePointage { get; set; } = "Travail"; // Travail, Pause, Formation, Congé
    public string Notes { get; set; } = string.Empty;

    // Navigation
    public Employe Employe { get; set; } = null!;
    public OrdreTravail? OrdreTravail { get; set; }

    // Calcul
    public double HeuresTravaillees =>
        DateFin.HasValue ? (DateFin.Value - DateDebut).TotalHours : 0;
}
