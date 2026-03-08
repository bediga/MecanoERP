namespace MecanoERP.Core.Entities.Comptabilite;

public class LigneJournal
{
    public int Id { get; set; }
    public int JournalComptableId { get; set; }
    public int CompteGLId { get; set; }
    public int Ordre { get; set; }

    // Description
    public string Description { get; set; } = string.Empty;

    // Montants CAD (devise fonctionnelle)
    public decimal Debit  { get; set; }
    public decimal Credit { get; set; }

    // Multi-devise
    public string Devise { get; set; } = "CAD";
    public decimal MontantDevise { get; set; }   // Montant dans la devise d'origine
    public decimal TauxChange { get; set; } = 1; // Taux d'échange vers CAD

    // Dimensions analytiques
    public int? CentreDeCoûtId { get; set; }

    // Tiers (client ou fournisseur)
    public int?    TiersId { get; set; }
    public string? TiersType { get; set; }  // "Client" | "Fournisseur"

    // Lettrage / rapprochement
    public string? MatchingCode { get; set; }     // Code de lettrage entre lignes
    public bool    EstLettree { get; set; }

    // Échéance (pour les créances/dettes)
    public DateTime? DateEcheance { get; set; }

    // Navigation
    public JournalComptable Journal     { get; set; } = null!;
    public CompteGL         CompteGL    { get; set; } = null!;
    public CentreDeCoût?    CentreDeCoût { get; set; }
}
