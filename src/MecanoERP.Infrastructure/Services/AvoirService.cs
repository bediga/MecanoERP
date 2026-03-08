using MecanoERP.Core.Entities;
using MecanoERP.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MecanoERP.Infrastructure.Services;

public class AvoirService
{
    private readonly MecanoDbContext _ctx;
    public AvoirService(MecanoDbContext ctx) => _ctx = ctx;

    public async Task<IEnumerable<AvoirClient>> GetAllAsync()
        => await _ctx.AvoirsClients
            .Include(a => a.Client)
            .Include(a => a.FactureOrigine)
            .OrderByDescending(a => a.DateAvoir)
            .ToListAsync();

    public async Task<AvoirClient> CreerAvoirAsync(int clientId, int? factureOrigineId, decimal montant, string motif)
    {
        var count = await _ctx.AvoirsClients.CountAsync() + 1;
        var avoir = new AvoirClient
        {
            Numero = $"AV-{DateTime.Now.Year}-{count:D4}",
            ClientId = clientId,
            FactureOrigineId = factureOrigineId,
            Montant = montant,
            Motif = motif,
            DateAvoir = DateTime.UtcNow,
            Statut = StatutAvoir.Emis
        };
        _ctx.AvoirsClients.Add(avoir);

        // Mettre à jour solde client
        var client = await _ctx.Clients.FindAsync(clientId);
        if (client != null) client.Solde -= montant;

        await _ctx.SaveChangesAsync();
        return avoir;
    }

    public async Task ModifierAsync(AvoirClient avoir)
    {
        _ctx.AvoirsClients.Update(avoir);
        await _ctx.SaveChangesAsync();
    }
}
