using MecanoERP.Core.Entities;
using MecanoERP.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MecanoERP.Infrastructure.Services;

public class CheckInService
{
    private readonly MecanoDbContext _ctx;
    public CheckInService(MecanoDbContext ctx) => _ctx = ctx;

    public async Task<FicheEntree?> GetByOTAsync(int otId)
        => await _ctx.FichesEntree
            .Include(f => f.EmployeReception)
            .FirstOrDefaultAsync(f => f.OrdreTravailId == otId);

    public async Task<FicheEntree> CreerFicheAsync(int otId, int? employeReceptionId = null)
    {
        // Vérifier qu'il n'y a pas déjà une fiche
        var existante = await _ctx.FichesEntree.FirstOrDefaultAsync(f => f.OrdreTravailId == otId);
        if (existante != null) return existante;

        var ot = await _ctx.OrdresTravail.Include(o => o.Vehicule).FirstOrDefaultAsync(o => o.Id == otId)
            ?? throw new Exception("OT introuvable.");

        var fiche = new FicheEntree
        {
            OrdreTravailId = otId,
            EmployeReceptionId = employeReceptionId,
            DateEntree = DateTime.UtcNow,
            KilometrageEntree = ot.KilometrageEntree,
            ChecklistJson = System.Text.Json.JsonSerializer.Serialize(new Dictionary<string, bool>
            {
                ["Carrosserie OK"] = false,
                ["Pneus OK"] = false,
                ["Lumières OK"] = false,
                ["Rétroviseurs OK"] = false,
                ["Pare-brise OK"] = false,
                ["Essuie-glaces OK"] = false,
                ["Niveau huile OK"] = false,
                ["Niveau liquide refroidissement OK"] = false
            })
        };
        _ctx.FichesEntree.Add(fiche);
        await _ctx.SaveChangesAsync();
        return fiche;
    }

    public async Task MettreAJourChecklistAsync(int ficheId, Dictionary<string, bool> checklist)
    {
        var fiche = await _ctx.FichesEntree.FindAsync(ficheId)
            ?? throw new Exception("Fiche introuvable.");
        fiche.ChecklistJson = System.Text.Json.JsonSerializer.Serialize(checklist);
        await _ctx.SaveChangesAsync();
    }

    public async Task SignerAsync(int ficheId, string signatureBase64, bool clientAccepte)
    {
        var fiche = await _ctx.FichesEntree.FindAsync(ficheId)
            ?? throw new Exception("Fiche introuvable.");
        fiche.SignatureClientBase64 = signatureBase64;
        fiche.ClientAAccepte = clientAccepte;
        await _ctx.SaveChangesAsync();
    }

    public async Task AjouterObservationAsync(int ficheId, string observations, int niveauCarburant)
    {
        var fiche = await _ctx.FichesEntree.FindAsync(ficheId)
            ?? throw new Exception("Fiche introuvable.");
        fiche.Observations = observations;
        fiche.NiveauCarburant = Math.Clamp(niveauCarburant, 0, 8);
        await _ctx.SaveChangesAsync();
    }
}
