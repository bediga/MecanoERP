namespace MecanoERP.Core.Entities.Comptabilite;

public enum TypeCompteGL
{
    Actif,
    Passif,
    Capital,
    Revenue,
    Charge
}

public enum SousTypeCompteGL
{
    // Actif
    CaisseEtBanque,
    ComptesClients,
    StockPieces,
    TaxeRecuperableTPS,
    TaxeRecuperableTVQ,
    AutresActifsCourants,
    ImmobilisationsBrutes,
    AmortissementsAccumules,
    AutresActifsLongTerme,
    // Passif
    ComptesFournisseurs,
    TPS_Percue,
    TVQ_Percue,
    TVH_Percue,
    AutresPassifsCourants,
    DettesBancaires,
    // Capital
    CapitalProprietaire,
    BeneficesNonRepartis,
    // Revenue
    RevenusMOeuvre,
    RevenusPieces,
    RevenusServices,
    AutresRevenus,
    // Charge
    AchatsPieces,
    Salaires,
    Loyer,
    Assurances,
    AutresCharges
}

public class CompteGL
{
    public int Id { get; set; }
    public string Numero { get; set; } = string.Empty;  // ex: "2310"
    public string Nom { get; set; } = string.Empty;
    public TypeCompteGL TypeCompte { get; set; }
    public SousTypeCompteGL SousType { get; set; }
    public int? CompteParentId { get; set; }
    public bool EstSysteme { get; set; }    // Comptes PCGR protégés
    public bool EstActif { get; set; } = true;
    public decimal Solde { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime DateCreation { get; set; } = DateTime.UtcNow;

    // Navigation
    public CompteGL? CompteParent { get; set; }
    public ICollection<CompteGL> SousComptes { get; set; } = new List<CompteGL>();
    public ICollection<LigneJournal> LignesJournal { get; set; } = new List<LigneJournal>();
}
