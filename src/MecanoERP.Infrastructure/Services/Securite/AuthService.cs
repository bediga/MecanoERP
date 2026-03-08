using MecanoERP.Core.Entities.Identite;
using MecanoERP.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace MecanoERP.Infrastructure.Services.Securite;

public class SessionUtilisateur
{
    public int Id { get; set; }
    public string NomUtilisateur { get; set; } = string.Empty;
    public string NomComplet { get; set; } = string.Empty;
    public CodeRole Role { get; set; }
    public HashSet<(ModuleApp Module, ActionApp Action)> Permissions { get; set; } = [];
    public bool EstAutorise(ModuleApp module, ActionApp action) => Permissions.Contains((module, action));
}

public class AuthService
{
    private readonly MecanoDbContext _context;

    public AuthService(MecanoDbContext context) => _context = context;

    public async Task<SessionUtilisateur?> ConnecterAsync(string nomUtilisateur, string motDePasse)
    {
        var user = await _context.Utilisateurs
            .Include(u => u.Role)
                .ThenInclude(r => r.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(u => u.NomUtilisateur == nomUtilisateur && u.EstActif);

        if (user is null) return null;

        var hash = HacherMotDePasse(motDePasse, user.Sel);
        if (hash != user.MotDePasseHash) return null;

        user.DernierConnexion = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return new SessionUtilisateur
        {
            Id = user.Id,
            NomUtilisateur = user.NomUtilisateur,
            NomComplet = user.NomComplet,
            Role = user.Role.Code,
            Permissions = user.Role.RolePermissions
                .Select(rp => (rp.Permission.Module, rp.Permission.Action))
                .ToHashSet()
        };
    }

    public async Task<UtilisateurApp> CreerUtilisateurAsync(string nom, string motDePasse, int roleId, string nomComplet, string email)
    {
        var sel = GenererSel();
        var hash = HacherMotDePasse(motDePasse, sel);
        var user = new UtilisateurApp
        {
            NomUtilisateur = nom,
            MotDePasseHash = hash,
            Sel = sel,
            NomComplet = nomComplet,
            Email = email,
            RoleId = roleId,
            EstActif = true
        };
        _context.Utilisateurs.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task ChangerMotDePasseAsync(int userId, string ancienMotDePasse, string nouveauMotDePasse)
    {
        var user = await _context.Utilisateurs.FindAsync(userId)
            ?? throw new Exception("Utilisateur introuvable.");
        if (HacherMotDePasse(ancienMotDePasse, user.Sel) != user.MotDePasseHash)
            throw new Exception("Mot de passe actuel incorrect.");
        user.Sel = GenererSel();
        user.MotDePasseHash = HacherMotDePasse(nouveauMotDePasse, user.Sel);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<UtilisateurApp>> GetUtilisateursAsync()
        => await _context.Utilisateurs.Include(u => u.Role).OrderBy(u => u.NomUtilisateur).ToListAsync();

    public async Task ToggleActifAsync(int userId)
    {
        var user = await _context.Utilisateurs.FindAsync(userId) ?? throw new Exception("Introuvable.");
        user.EstActif = !user.EstActif;
        await _context.SaveChangesAsync();
    }

    // ── Helpers ──────────────────────────────────────────────────────
    private static string GenererSel()
        => Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));

    public static string HacherMotDePasse(string motDePasse, string sel)
    {
        var donnees = Encoding.UTF8.GetBytes(motDePasse + sel);
        var hash = SHA256.HashData(donnees);
        return Convert.ToBase64String(hash);
    }
}
