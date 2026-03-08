namespace MecanoERP.Core.Entities;

public enum StatutOT
{
    Ouvert,
    EnCours,
    EnAttentePieces,   // Pièce(s) en rupture de stock
    ControleQualite,   // Travaux terminés, en vérification finale
    Pret,              // Validé QC, prêt pour facturation
    Facture,
    Annule
}

public class OrdreTravail
{
    public int Id { get; set; }
    public string Numero { get; set; } = string.Empty;
    public int VehiculeId { get; set; }
    public int? EmployeId { get; set; }
    public DateTime DateEntree { get; set; } = DateTime.UtcNow;
    public DateTime? DateSortie { get; set; }
    public string Diagnostic { get; set; } = string.Empty;
    public string TravauxDemandes { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public decimal TempsEstime { get; set; }
    public decimal TempsReel { get; set; }
    public int KilometrageEntree { get; set; }
    public StatutOT Statut { get; set; } = StatutOT.Ouvert;
    public bool SignatureClient { get; set; }
    public DateTime DateCreation { get; set; } = DateTime.UtcNow;

    // Navigation
    public Vehicule Vehicule { get; set; } = null!;
    public Employe? Employe { get; set; }
    public ICollection<LigneOT> Lignes { get; set; } = new List<LigneOT>();
    public ICollection<Facture> Factures { get; set; } = new List<Facture>();
}
