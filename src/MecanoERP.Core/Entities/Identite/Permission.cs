namespace MecanoERP.Core.Entities.Identite;

public enum ModuleApp
{
    Dashboard,
    Clients,
    Vehicules,
    OrdresTravail,
    Inventaire,
    Facturation,
    Comptabilite,
    ConfigComptable,   // Plan GL, Journaux, Config taxes
    RendezVous,
    Employes,
    Garanties,
    Fournisseurs,
    Rapports,
    Securite,          // Gestion users/rôles (Admin seulement)
    Parametres
}

public enum ActionApp
{
    Lire,
    Creer,
    Modifier,
    Supprimer,
    Configurer,    // Actions de configuration (taxes, GL, rôles)
    Valider,       // Valider journaux, approuver factures
    Exporter
}

public class Permission
{
    public int Id { get; set; }
    public ModuleApp Module { get; set; }
    public ActionApp Action { get; set; }
    public string Description { get; set; } = string.Empty;

    // Navigation
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
