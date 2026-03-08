using MecanoERP.Core.Entities.Identite;
using MecanoERP.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MecanoERP.Infrastructure.Services.Securite;

/// <summary>
/// Seed et gestion des rôles et permissions.
/// À appeler au démarrage si la DB est vide.
/// </summary>
public class AutorisationService
{
    private readonly MecanoDbContext _context;

    public AutorisationService(MecanoDbContext context) => _context = context;

    public async Task<bool> EstAutoriseAsync(int userId, ModuleApp module, ActionApp action)
    {
        var user = await _context.Utilisateurs
            .Include(u => u.Role).ThenInclude(r => r.RolePermissions).ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(u => u.Id == userId && u.EstActif);
        return user?.Role.RolePermissions
            .Any(rp => rp.Permission.Module == module && rp.Permission.Action == action) ?? false;
    }

    public async Task SeedRolesEtPermissionsAsync()
    {
        if (await _context.Roles.AnyAsync()) return;

        // ── 1. Rôles ─────────────────────────────────────────────
        var roles = new[]
        {
            new RoleApp { Code = CodeRole.Admin,          Nom = "Administrateur",    Description = "Accès complet à tout le système" },
            new RoleApp { Code = CodeRole.Comptable,      Nom = "Comptable",         Description = "Comptabilité GL, taxes, fournisseurs, rapports" },
            new RoleApp { Code = CodeRole.Mecanicien,     Nom = "Mécanicien",        Description = "OT, inventaire (lecture+saisie)" },
            new RoleApp { Code = CodeRole.Receptionniste, Nom = "Réceptionniste",    Description = "Clients, RDV, OT (création)" },
            new RoleApp { Code = CodeRole.Lecture,        Nom = "Lecture seule",     Description = "Consultation uniquement" }
        };
        _context.Roles.AddRange(roles);
        await _context.SaveChangesAsync();

        // ── 2. Toutes les permissions possibles ──────────────────
        var modules = Enum.GetValues<ModuleApp>();
        var actions = Enum.GetValues<ActionApp>();
        var perms = new List<Permission>();
        foreach (var m in modules)
            foreach (var a in actions)
                perms.Add(new Permission { Module = m, Action = a, Description = $"{m} — {a}" });
        _context.Permissions.AddRange(perms);
        await _context.SaveChangesAsync();

        var permDict = perms.ToDictionary(p => (p.Module, p.Action));

        // ── 3. Assignation permissions par rôle ─────────────────
        var roleDict = roles.ToDictionary(r => r.Code);

        // Admin = tout
        AssignerToutes(roleDict[CodeRole.Admin], permDict);

        // Comptable = tout sauf Securite.Configurer/Supprimer
        AssignerSauf(roleDict[CodeRole.Comptable], permDict, [
            (ModuleApp.Securite, ActionApp.Configurer),
            (ModuleApp.Securite, ActionApp.Supprimer),
            (ModuleApp.Parametres, ActionApp.Configurer)
        ]);

        // Mécanicien
        AssignerListe(roleDict[CodeRole.Mecanicien], permDict, [
            (ModuleApp.Dashboard,      ActionApp.Lire),
            (ModuleApp.OrdresTravail,  ActionApp.Lire),
            (ModuleApp.OrdresTravail,  ActionApp.Creer),
            (ModuleApp.OrdresTravail,  ActionApp.Modifier),
            (ModuleApp.Inventaire,     ActionApp.Lire),
            (ModuleApp.Vehicules,      ActionApp.Lire),
            (ModuleApp.Clients,        ActionApp.Lire),
        ]);

        // Réceptionniste
        AssignerListe(roleDict[CodeRole.Receptionniste], permDict, [
            (ModuleApp.Dashboard,      ActionApp.Lire),
            (ModuleApp.Clients,        ActionApp.Lire),
            (ModuleApp.Clients,        ActionApp.Creer),
            (ModuleApp.Clients,        ActionApp.Modifier),
            (ModuleApp.Vehicules,      ActionApp.Lire),
            (ModuleApp.Vehicules,      ActionApp.Creer),
            (ModuleApp.RendezVous,     ActionApp.Lire),
            (ModuleApp.RendezVous,     ActionApp.Creer),
            (ModuleApp.RendezVous,     ActionApp.Modifier),
            (ModuleApp.OrdresTravail,  ActionApp.Lire),
            (ModuleApp.OrdresTravail,  ActionApp.Creer),
            (ModuleApp.Facturation,    ActionApp.Lire),
        ]);

        // Lecture = Lire partout sauf Securite
        foreach (var m in modules.Where(m => m != ModuleApp.Securite && m != ModuleApp.ConfigComptable))
            AddPerm(roleDict[CodeRole.Lecture], permDict, m, ActionApp.Lire);

        await _context.SaveChangesAsync();

        // ── 4. Utilisateur admin par défaut ──────────────────────
        if (!await _context.Utilisateurs.AnyAsync())
        {
            var adminRole = roleDict[CodeRole.Admin];
            var sel = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32));
            _context.Utilisateurs.Add(new UtilisateurApp
            {
                NomUtilisateur = "admin",
                MotDePasseHash = AuthService.HacherMotDePasse("Admin123!", sel),
                Sel = sel,
                NomComplet = "Administrateur système",
                Email = "admin@garage.ca",
                RoleId = adminRole.Id,
                EstActif = true
            });
            await _context.SaveChangesAsync();
        }
    }

    // ── Helpers ──────────────────────────────────────────────────────
    private static void AssignerToutes(RoleApp role, Dictionary<(ModuleApp, ActionApp), Permission> d)
    {
        foreach (var (_, p) in d)
            role.RolePermissions.Add(new RolePermission { RoleId = role.Id, PermissionId = p.Id });
    }

    private static void AssignerSauf(RoleApp role, Dictionary<(ModuleApp, ActionApp), Permission> d,
        (ModuleApp, ActionApp)[] exclusions)
    {
        foreach (var (key, p) in d)
            if (!exclusions.Contains(key))
                role.RolePermissions.Add(new RolePermission { RoleId = role.Id, PermissionId = p.Id });
    }

    private static void AssignerListe(RoleApp role, Dictionary<(ModuleApp, ActionApp), Permission> d,
        (ModuleApp, ActionApp)[] liste)
    {
        foreach (var key in liste)
            if (d.TryGetValue(key, out var p))
                role.RolePermissions.Add(new RolePermission { RoleId = role.Id, PermissionId = p.Id });
    }

    private static void AddPerm(RoleApp role, Dictionary<(ModuleApp, ActionApp), Permission> d,
        ModuleApp m, ActionApp a)
    {
        if (d.TryGetValue((m, a), out var p))
            role.RolePermissions.Add(new RolePermission { RoleId = role.Id, PermissionId = p.Id });
    }
}
