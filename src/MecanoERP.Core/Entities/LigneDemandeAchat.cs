namespace MecanoERP.Core.Entities;

public class LigneDemandeAchat
{
    public int Id { get; set; }
    public int DemandeAchatId { get; set; }
    public int? PieceId { get; set; }
    public string Description { get; set; } = string.Empty;
    public int Quantite { get; set; } = 1;
    public decimal PrixEstime { get; set; }

    // Navigation
    public DemandeAchat DemandeAchat { get; set; } = null!;
    public Piece? Piece { get; set; }

    public decimal Total => Quantite * PrixEstime;
}
