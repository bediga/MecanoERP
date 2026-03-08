namespace MecanoERP.Core.Entities;

public enum TypeGarantie
{
    Piece,
    MainOeuvre,
    Mixte
}

public class Garantie
{
    public int Id { get; set; }
    public int FactureId { get; set; }
    public TypeGarantie Type { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime DateDebut { get; set; }
    public DateTime DateFin { get; set; }
    public bool EstActive => DateTime.UtcNow <= DateFin;
    public string Notes { get; set; } = string.Empty;

    // Navigation
    public Facture Facture { get; set; } = null!;
}
