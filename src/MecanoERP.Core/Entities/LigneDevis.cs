namespace MecanoERP.Core.Entities;

public class LigneDevis
{
    public int Id { get; set; }
    public int DevisId { get; set; }
    public int? PieceId { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Quantite { get; set; } = 1;
    public decimal PrixUnitaire { get; set; }
    public decimal Rabais { get; set; }
    public bool EstMainOeuvre { get; set; }

    // Navigation
    public Devis Devis { get; set; } = null!;
    public Piece? Piece { get; set; }

    // Calcul
    public decimal Total => (Quantite * PrixUnitaire) - Rabais;
}
