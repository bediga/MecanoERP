using MecanoERP.Core.Entities;
using MecanoERP.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MecanoERP.Infrastructure.Services;

public class DevisService
{
    private readonly MecanoDbContext _ctx;
    public DevisService(MecanoDbContext ctx) => _ctx = ctx;

    public async Task<IEnumerable<Devis>> GetAllAsync()
        => await _ctx.Devis
            .Include(d => d.Client)
            .Include(d => d.Vehicule)
            .Include(d => d.Lignes)
            .OrderByDescending(d => d.DateDevis)
            .ToListAsync();

    public async Task<Devis?> GetByIdAsync(int id)
        => await _ctx.Devis
            .Include(d => d.Client)
            .Include(d => d.Vehicule)
            .Include(d => d.Lignes).ThenInclude(l => l.Piece)
            .FirstOrDefaultAsync(d => d.Id == id);

    public async Task<Devis> CreerDevisAsync(Devis devis)
    {
        var count = await _ctx.Devis.CountAsync() + 1;
        devis.Numero = $"DEV-{DateTime.Now.Year}-{count:D4}";
        devis.DateDevis = DateTime.UtcNow;
        _ctx.Devis.Add(devis);
        await _ctx.SaveChangesAsync();
        return devis;
    }

    public async Task ModifierAsync(Devis devis)
    {
        _ctx.Devis.Update(devis);
        await _ctx.SaveChangesAsync();
    }

    public async Task SupprimerAsync(int id)
    {
        var d = await _ctx.Devis.FindAsync(id);
        if (d != null) { _ctx.Devis.Remove(d); await _ctx.SaveChangesAsync(); }
    }

    public async Task<OrdreTravail> ConvertirEnOTAsync(int devisId, OrdreTravailService otSvc)
    {
        var devis = await GetByIdAsync(devisId)
            ?? throw new Exception("Devis introuvable.");

        var ot = new OrdreTravail
        {
            VehiculeId = devis.VehiculeId ?? throw new Exception("Véhicule requis pour convertir en OT."),
            Diagnostic = $"Depuis devis {devis.Numero}",
            TravauxDemandes = string.Join("\n", devis.Lignes.Select(l => $"{l.Description} x{l.Quantite}")),
            Notes = devis.Notes
        };

        var otCree = await otSvc.CreerOTAsync(ot);

        devis.Statut = StatutDevis.ConvertienOT;
        devis.OrdreTravailId = otCree.Id;
        await _ctx.SaveChangesAsync();

        return otCree;
    }
}
