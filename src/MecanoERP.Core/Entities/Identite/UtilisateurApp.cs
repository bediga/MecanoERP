namespace MecanoERP.Core.Entities.Identite;

public class UtilisateurApp
{
    public int Id { get; set; }
    public string NomUtilisateur { get; set; } = string.Empty;
    public string MotDePasseHash { get; set; } = string.Empty;
    public string Sel { get; set; } = string.Empty;
    public string NomComplet { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int RoleId { get; set; }
    public bool EstActif { get; set; } = true;
    public DateTime? DernierConnexion { get; set; }
    public DateTime DateCreation { get; set; } = DateTime.UtcNow;
    public string Notes { get; set; } = string.Empty;

    // Navigation
    public RoleApp Role { get; set; } = null!;
}
