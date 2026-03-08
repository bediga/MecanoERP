using MecanoERP.Core.Entities;
using MecanoERP.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MecanoERP.Infrastructure.Services;

public class FournisseurService
{
    private readonly MecanoDbContext _context;
    public FournisseurService(MecanoDbContext context) => _context = context;

    public async Task<IEnumerable<Fournisseur>> GetAllAsync()
        => await _context.Fournisseurs
            .OrderBy(f => f.CodeFournisseur)
            .ToListAsync();

    public async Task<IEnumerable<Fournisseur>> RechercherAsync(string terme)
    {
        terme = terme.ToLower();
        return await _context.Fournisseurs
            .Where(f => f.Nom.ToLower().Contains(terme)
                     || f.CodeFournisseur.ToLower().Contains(terme)
                     || f.Telephone.ToLower().Contains(terme)
                     || f.Email.ToLower().Contains(terme))
            .OrderBy(f => f.CodeFournisseur)
            .ToListAsync();
    }

    /// <summary>
    /// Ajoute un fournisseur. Code auto-généré (FOUR-001…) si absent.
    /// Lance une exception si le code existe déjà.
    /// </summary>
    public async Task<Fournisseur> AjouterAsync(Fournisseur f)
    {
        if (string.IsNullOrWhiteSpace(f.CodeFournisseur))
            f.CodeFournisseur = await GenererCodeAsync();

        f.CodeFournisseur = f.CodeFournisseur.ToUpper().Trim();

        if (await _context.Fournisseurs.AnyAsync(x => x.CodeFournisseur == f.CodeFournisseur))
            throw new Exception($"Le code fournisseur « {f.CodeFournisseur} » est déjà utilisé.");

        _context.Fournisseurs.Add(f);
        await _context.SaveChangesAsync();
        return f;
    }

    public async Task ModifierAsync(Fournisseur f)
    {
        if (string.IsNullOrWhiteSpace(f.CodeFournisseur))
            throw new Exception("Le code fournisseur est obligatoire.");

        f.CodeFournisseur = f.CodeFournisseur.ToUpper().Trim();

        if (await _context.Fournisseurs.AnyAsync(x => x.CodeFournisseur == f.CodeFournisseur && x.Id != f.Id))
            throw new Exception($"Le code « {f.CodeFournisseur} » est déjà utilisé par un autre fournisseur.");

        _context.Fournisseurs.Update(f);
        await _context.SaveChangesAsync();
    }

    public async Task SupprimerAsync(int id)
    {
        var f = await _context.Fournisseurs.FindAsync(id);
        if (f is not null) { _context.Fournisseurs.Remove(f); await _context.SaveChangesAsync(); }
    }

    private async Task<string> GenererCodeAsync()
    {
        var count = await _context.Fournisseurs.CountAsync() + 1;
        var code = $"FOUR-{count:D3}";
        while (await _context.Fournisseurs.AnyAsync(x => x.CodeFournisseur == code))
        {
            count++;
            code = $"FOUR-{count:D3}";
        }
        return code;
    }
}
