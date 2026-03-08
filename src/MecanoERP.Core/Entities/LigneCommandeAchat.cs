namespace MecanoERP.Core.Entities;

public class LigneCommandeAchat
{
    public int Id { get; set; }
    public int CommandeAchatId { get; set; }
    public int PieceId { get; set; }
    public int Quantite { get; set; }
    public int QuantiteRecue { get; set; }
    public decimal PrixUnitaire { get; set; }

    // Navigation
    public CommandeAchat CommandeAchat { get; set; } = null!;
    public Piece Piece { get; set; } = null!;
}
