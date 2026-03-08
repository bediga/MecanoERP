using MecanoERP.Core.Entities;
using MecanoERP.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MecanoERP.Infrastructure.Services;

public class EmployeService
{
    private readonly MecanoDbContext _context;
    public EmployeService(MecanoDbContext context) => _context = context;

    public async Task<IEnumerable<Employe>> GetAllAsync()
        => await _context.Employes
            .Where(e => e.EstActif)
            .OrderBy(e => e.Nom).ThenBy(e => e.Prenom)
            .ToListAsync();

    public async Task<IEnumerable<Employe>> GetAllIncludeInactifAsync()
        => await _context.Employes
            .OrderBy(e => e.Nom).ThenBy(e => e.Prenom)
            .ToListAsync();

    public async Task<Employe> AjouterAsync(Employe e)
    {
        _context.Employes.Add(e);
        await _context.SaveChangesAsync();
        return e;
    }

    public async Task ModifierAsync(Employe e)
    {
        _context.Employes.Update(e);
        await _context.SaveChangesAsync();
    }

    public async Task SupprimerAsync(int id)
    {
        var emp = await _context.Employes.FindAsync(id);
        if (emp is not null) { emp.EstActif = false; await _context.SaveChangesAsync(); }
    }
}
