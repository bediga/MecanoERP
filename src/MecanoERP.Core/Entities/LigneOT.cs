namespace MecanoERP.Core.Entities;

public enum TypeLigneOT
{
    Piece,
    MainOeuvre,
    Forfait
}

public class LigneOT
{
    public int Id { get; set; }
    public int OrdreTravailId { get; set; }
    public int? PieceId { get; set; }
    public TypeLigneOT Type { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Quantite { get; set; }
    public decimal PrixUnitaire { get; set; }
    public decimal Rabais { get; set; }
    public decimal Total => (Quantite * PrixUnitaire) - Rabais;

    // Navigation
    public OrdreTravail OrdreTravail { get; set; } = null!;
    public Piece? Piece { get; set; }
}
