using MecanoERP.Core.Entities;
using MecanoERP.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MecanoERP.Infrastructure.Services;

public class VehiculeService
{
    private readonly MecanoDbContext _context;

    public VehiculeService(MecanoDbContext context) => _context = context;

    public async Task<IEnumerable<Vehicule>> GetAllAsync()
        => await _context.Vehicules
            .Include(v => v.Client)
            .OrderBy(v => v.Marque).ThenBy(v => v.Modele)
            .ToListAsync();

    public async Task<IEnumerable<Vehicule>> GetByClientAsync(int clientId)
        => await _context.Vehicules
            .Where(v => v.ClientId == clientId)
            .ToListAsync();

    public async Task<Vehicule?> GetByIdAsync(int id)
        => await _context.Vehicules
            .Include(v => v.Client)
            .Include(v => v.OrdresTravail)
            .FirstOrDefaultAsync(v => v.Id == id);

    public async Task<Vehicule> AjouterAsync(Vehicule vehicule)
    {
        _context.Vehicules.Add(vehicule);
        await _context.SaveChangesAsync();
        return vehicule;
    }

    public async Task ModifierAsync(Vehicule vehicule)
    {
        _context.Vehicules.Update(vehicule);
        await _context.SaveChangesAsync();
    }

    public async Task SupprimerAsync(int id)
    {
        var vehicule = await _context.Vehicules.FindAsync(id);
        if (vehicule is not null)
        {
            _context.Vehicules.Remove(vehicule);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<Vehicule>> RechercherAsync(string terme)
    {
        terme = terme.ToLower();
        return await _context.Vehicules
            .Include(v => v.Client)
            .Where(v => v.Immatriculation.ToLower().Contains(terme)
                     || v.Marque.ToLower().Contains(terme)
                     || v.Modele.ToLower().Contains(terme)
                     || v.VIN.ToLower().Contains(terme))
            .ToListAsync();
    }
}
