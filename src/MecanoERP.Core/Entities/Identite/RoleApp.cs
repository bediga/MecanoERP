namespace MecanoERP.Core.Entities.Identite;

public enum CodeRole
{
    Admin,
    Comptable,
    Mecanicien,
    Receptionniste,
    Lecture
}

public class RoleApp
{
    public int Id { get; set; }
    public CodeRole Code { get; set; }
    public string Nom { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool EstSysteme { get; set; } = true; // rôles système non supprimables

    // Navigation
    public ICollection<UtilisateurApp> Utilisateurs { get; set; } = new List<UtilisateurApp>();
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
