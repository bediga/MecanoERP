namespace MecanoERP.Core.Entities;

public enum TypeAction
{
    Creation,
    Modification,
    Suppression,
    Connexion,
    Export,
    ValidationQualite,
    ChangementStatut
}

public class AuditLog
{
    public int Id { get; set; }
    public DateTime DateAction { get; set; } = DateTime.UtcNow;
    public string Module { get; set; } = string.Empty;           // "OT", "Facture", etc.
    public int? DocumentId { get; set; }
    public string DocumentNumero { get; set; } = string.Empty;
    public TypeAction TypeAction { get; set; }
    public int? UtilisateurId { get; set; }
    public string NomUtilisateur { get; set; } = string.Empty;
    public string AncienneValeur { get; set; } = string.Empty;
    public string NouvelleValeur { get; set; } = string.Empty;
    public string AdresseIP { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
