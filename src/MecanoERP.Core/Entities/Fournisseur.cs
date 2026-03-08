namespace MecanoERP.Core.Entities;

public class Fournisseur
{
    public int Id { get; set; }
    public string CodeFournisseur { get; set; } = string.Empty; // ex: FOUR-001

    // Identification
    public string Nom { get; set; } = string.Empty;
    public string ContactNom { get; set; } = string.Empty;
    public string ContactPoste { get; set; } = string.Empty;
    public string Telephone { get; set; } = string.Empty;
    public string TelephoneMobile { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string SiteWeb { get; set; } = string.Empty;

    // Adresse
    public string Adresse { get; set; } = string.Empty;
    public string Ville { get; set; } = string.Empty;
    public string CodePostal { get; set; } = string.Empty;
    public string Province { get; set; } = "QC";

    // Comptabilité fournisseur
    public string NumeroCompte { get; set; } = string.Empty;
    public string NumeroTPS { get; set; } = string.Empty;
    public string NumeroTVQ { get; set; } = string.Empty;
    public int DelaiPaiementJours { get; set; } = 30;
    public string Devise { get; set; } = "CAD";
    public decimal LimiteCreditCAD { get; set; }

    // Statut
    public bool EstActif { get; set; } = true;
    public string Categorie { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public DateTime DateCreation { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<Piece> Pieces { get; set; } = new List<Piece>();
    public ICollection<CommandeAchat> CommandesAchat { get; set; } = new List<CommandeAchat>();
}
