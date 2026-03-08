namespace MecanoERP.Core.Entities.Comptabilite;

public enum TermesPaiementFournisseur
{
    Net30,
    Net60,
    Net90,
    Immediat,
    Personnalise
}

public enum TypeFournisseur
{
    Pieces,
    Services,
    Mixte,
    SousTraitant
}

/// <summary>
/// Configuration comptable et fiscale d'un fournisseur.
/// Géré exclusivement par le Comptable.
/// </summary>
public class ConfigFournisseurCompta
{
    public int FournisseurId { get; set; }   // PK + FK

    // Numéros d'inscription fiscaux du fournisseur
    public string NumeroTPS { get; set; } = string.Empty;   // Requis si inscrit TPS
    public string NumeroTVQ { get; set; } = string.Empty;
    public string NumeroTVH { get; set; } = string.Empty;
    public bool EstInscritTPS { get; set; }
    public bool EstInscritTVQ { get; set; }

    // Comptes GL assignés
    public int? CompteGLChargeId { get; set; }      // ex: 5000-Achats pièces
    public int? CompteGLCTI_Id { get; set; }         // Compte CTI récupérable

    // Termes et type
    public TermesPaiementFournisseur Termes { get; set; } = TermesPaiementFournisseur.Net30;
    public TypeFournisseur TypeFournisseur { get; set; }
    public decimal LimiteCredit { get; set; }
    public string Notes { get; set; } = string.Empty;

    // Navigation
    public Fournisseur Fournisseur { get; set; } = null!;
    public CompteGL? CompteGLCharge { get; set; }
}
