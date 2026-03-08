using MecanoERP.Core.Entities;
using MecanoERP.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MecanoERP.Infrastructure.Services;

public class RendezVousService
{
    private readonly MecanoDbContext _context;

    public RendezVousService(MecanoDbContext context) => _context = context;

    public async Task<IEnumerable<RendezVous>> GetAllAsync()
        => await _context.RendezVous
            .Include(r => r.Client)
            .Include(r => r.Vehicule)
            .Include(r => r.Employe)
            .OrderBy(r => r.DateHeure)
            .ToListAsync();

    public async Task<IEnumerable<RendezVous>> GetByDateAsync(DateTime date)
        => await _context.RendezVous
            .Include(r => r.Client)
            .Include(r => r.Vehicule)
            .Include(r => r.Employe)
            .Where(r => r.DateHeure.Date == date.Date)
            .OrderBy(r => r.DateHeure)
            .ToListAsync();

    public async Task<IEnumerable<RendezVous>> GetProchainAsync(int jours = 7)
    {
        var limite = DateTime.UtcNow.AddDays(jours);
        return await _context.RendezVous
            .Include(r => r.Client)
            .Include(r => r.Vehicule)
            .Where(r => r.DateHeure >= DateTime.UtcNow && r.DateHeure <= limite
                     && r.Statut == StatutRDV.EnAttente)
            .OrderBy(r => r.DateHeure)
            .ToListAsync();
    }

    public async Task<RendezVous> AjouterAsync(RendezVous rdv)
    {
        _context.RendezVous.Add(rdv);
        await _context.SaveChangesAsync();
        return rdv;
    }

    public async Task ModifierAsync(RendezVous rdv)
    {
        _context.RendezVous.Update(rdv);
        await _context.SaveChangesAsync();
    }

    public async Task ChangerStatutAsync(int id, StatutRDV statut)
    {
        var rdv = await _context.RendezVous.FindAsync(id);
        if (rdv is not null)
        {
            rdv.Statut = statut;
            await _context.SaveChangesAsync();
        }
    }

    public async Task SupprimerAsync(int id)
    {
        var rdv = await _context.RendezVous.FindAsync(id);
        if (rdv is not null)
        {
            _context.RendezVous.Remove(rdv);
            await _context.SaveChangesAsync();
        }
    }
}
