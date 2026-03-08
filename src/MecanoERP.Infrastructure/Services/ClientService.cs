using MecanoERP.Core.Entities;
using MecanoERP.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MecanoERP.Infrastructure.Services;

public class ClientService
{
    private readonly MecanoDbContext _context;

    public ClientService(MecanoDbContext context) => _context = context;

    public async Task<IEnumerable<Client>> GetAllAsync()
        => await _context.Clients
            .Include(c => c.Vehicules)
            .OrderBy(c => c.Nom).ThenBy(c => c.Prenom)
            .ToListAsync();

    public async Task<Client?> GetByIdAsync(int id)
        => await _context.Clients
            .Include(c => c.Vehicules)
            .Include(c => c.Factures)
            .FirstOrDefaultAsync(c => c.Id == id);

    public async Task<IEnumerable<Client>> RechercherAsync(string terme)
    {
        terme = terme.ToLower();
        return await _context.Clients
            .Where(c => c.Nom.ToLower().Contains(terme)
                     || c.Prenom.ToLower().Contains(terme)
                     || c.Telephone.Contains(terme)
                     || c.Email.ToLower().Contains(terme))
            .OrderBy(c => c.Nom)
            .ToListAsync();
    }

    public async Task<Client> AjouterAsync(Client client)
    {
        client.DateCreation = DateTime.UtcNow;
        _context.Clients.Add(client);
        await _context.SaveChangesAsync();
        return client;
    }

    public async Task ModifierAsync(Client client)
    {
        _context.Clients.Update(client);
        await _context.SaveChangesAsync();
    }

    public async Task SupprimerAsync(int id)
    {
        var client = await _context.Clients.FindAsync(id);
        if (client is not null)
        {
            _context.Clients.Remove(client);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<int> GetNombreClientsAsync()
        => await _context.Clients.CountAsync();
}
