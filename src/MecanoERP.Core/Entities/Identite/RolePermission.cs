namespace MecanoERP.Core.Entities.Identite;

public class RolePermission
{
    public int RoleId { get; set; }
    public int PermissionId { get; set; }

    // Navigation
    public RoleApp Role { get; set; } = null!;
    public Permission Permission { get; set; } = null!;
}
