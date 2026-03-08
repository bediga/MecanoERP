using MecanoERP.Core.Entities;
using MecanoERP.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MecanoERP.Infrastructure.Services;

public class PointageService
{
    private readonly MecanoDbContext _ctx;
    public PointageService(MecanoDbContext ctx) => _ctx = ctx;

    public async Task<IEnumerable<Pointage>> GetByEmployeAsync(int employeId, DateTime? debut = null, DateTime? fin = null)
    {
        var query = _ctx.Pointages
            .Include(p => p.OrdreTravail)
            .Where(p => p.EmployeId == employeId);
        if (debut.HasValue) query = query.Where(p => p.DateDebut >= debut.Value);
        if (fin.HasValue)   query = query.Where(p => p.DateDebut <= fin.Value);
        return await query.OrderByDescending(p => p.DateDebut).ToListAsync();
    }

    public async Task<Pointage?> GetPointageOuvertAsync(int employeId)
        => await _ctx.Pointages
            .FirstOrDefaultAsync(p => p.EmployeId == employeId && p.DateFin == null);

    public async Task<Pointage> PointerEntreeAsync(int employeId, int? otId = null, string type = "Travail")
    {
        // Fermer tout pointage ouvert
        var ouvert = await GetPointageOuvertAsync(employeId);
        if (ouvert != null)
        {
            ouvert.DateFin = DateTime.UtcNow;
        }

        var pointage = new Pointage
        {
            EmployeId = employeId,
            OrdreTravailId = otId,
            DateDebut = DateTime.UtcNow,
            TypePointage = type
        };
        _ctx.Pointages.Add(pointage);
        await _ctx.SaveChangesAsync();
        return pointage;
    }

    public async Task PointerSortieAsync(int employeId)
    {
        var ouvert = await GetPointageOuvertAsync(employeId)
            ?? throw new Exception("Aucun pointage ouvert pour cet employé.");
        ouvert.DateFin = DateTime.UtcNow;
        await _ctx.SaveChangesAsync();
    }

    public async Task<(double HeuresTravaillees, double HeuresFacturables, double TauxProductivite)>
        GetProductiviteAsync(int employeId, DateTime debut, DateTime fin)
    {
        var pointages = await _ctx.Pointages
            .Where(p => p.EmployeId == employeId
                     && p.DateDebut >= debut
                     && p.DateFin != null
                     && p.DateFin <= fin)
            .ToListAsync();

        var heuresTotales    = pointages.Sum(p => p.HeuresTravaillees);
        var heuresFacturables = pointages
            .Where(p => p.TypePointage == "Travail" && p.OrdreTravailId != null)
            .Sum(p => p.HeuresTravaillees);

        var taux = heuresTotales > 0 ? heuresFacturables / heuresTotales * 100 : 0;
        return (heuresTotales, heuresFacturables, Math.Round(taux, 1));
    }

    public async Task<IEnumerable<Pointage>> GetAllAujourdhuiAsync()
        => await _ctx.Pointages
            .Include(p => p.Employe)
            .Include(p => p.OrdreTravail)
            .Where(p => p.DateDebut.Date == DateTime.UtcNow.Date)
            .OrderByDescending(p => p.DateDebut)
            .ToListAsync();
}
