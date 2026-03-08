namespace MecanoERP.Core.Entities;

public class FicheEntree
{
    public int Id { get; set; }
    public int OrdreTravailId { get; set; }
    public int? EmployeReceptionId { get; set; }
    public DateTime DateEntree { get; set; } = DateTime.UtcNow;

    // Inspection rapide
    public int NiveauCarburant { get; set; }       // 0-8 (barres d'essence)
    public int KilometrageEntree { get; set; }
    public string Observations { get; set; } = string.Empty;

    // Checklist JSON : { "carrosserie": true, "pneus": false, ... }
    public string ChecklistJson { get; set; } = "{}";

    // Signature client (base64 PNG ou chemin fichier)
    public string SignatureClientBase64 { get; set; } = string.Empty;
    public bool ClientAAccepte { get; set; }

    // Photos avant intervention (chemins séparés par ';')
    public string PhotosChemin { get; set; } = string.Empty;

    public DateTime DateCreation { get; set; } = DateTime.UtcNow;

    // Navigation
    public OrdreTravail OrdreTravail { get; set; } = null!;
    public Employe? EmployeReception { get; set; }
}
