namespace MecanoERP.Core.Entities;

public class Piece
{
    public int Id { get; set; }
    public string Reference { get; set; } = string.Empty;
    public string CodeBarres { get; set; } = string.Empty;
    public string Designation { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int? FournisseurId { get; set; }
    public decimal PrixAchat { get; set; }
    public decimal PrixVente { get; set; }
    public int StockActuel { get; set; }
    public int StockMinimum { get; set; }
    public string Emplacement { get; set; } = string.Empty; // Dépôt / rangée
    public string Categorie { get; set; } = string.Empty;
    public DateTime DateCreation { get; set; } = DateTime.UtcNow;
    public bool EstActif { get; set; } = true;

    // Navigation
    public Fournisseur? Fournisseur { get; set; }
    public ICollection<LigneOT> LignesOT { get; set; } = new List<LigneOT>();
    public ICollection<LigneCommandeAchat> LignesCommande { get; set; } = new List<LigneCommandeAchat>();
}
