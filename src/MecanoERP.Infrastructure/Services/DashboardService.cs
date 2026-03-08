using MecanoERP.Core.Entities;
using MecanoERP.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MecanoERP.Infrastructure.Services;

public class DashboardService
{
    private readonly MecanoDbContext _context;
    public DashboardService(MecanoDbContext context) => _context = context;

    // ── KPIs ────────────────────────────────────────────────────────────────
    public async Task<decimal> GetCAJourAsync()
    {
        var today = DateTime.UtcNow.Date;
        return await _context.Factures
            .Where(f => f.DateFacture.Date == today && f.Statut != StatutFacture.Annulee)
            .SumAsync(f => f.MontantHT);
    }

    public async Task<decimal> GetCAMoisAsync()
    {
        var debut = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        return await _context.Factures
            .Where(f => f.DateFacture >= debut && f.Statut != StatutFacture.Annulee)
            .SumAsync(f => f.MontantHT);
    }

    public async Task<int> GetOTEnCoursAsync()
        => await _context.OrdresTravail
            .CountAsync(o => o.Statut == StatutOT.Ouvert || o.Statut == StatutOT.EnCours);

    public async Task<int> GetOTPretAsync()
        => await _context.OrdresTravail.CountAsync(o => o.Statut == StatutOT.Pret);

    public async Task<int> GetOTAttentePiecesAsync()
        => await _context.OrdresTravail.CountAsync(o => o.Statut == StatutOT.EnAttentePieces);

    public async Task<int> GetStockCritiqueCountAsync()
        => await _context.Pieces.CountAsync(p => p.StockActuel <= p.StockMinimum && p.EstActif);

    public async Task<int> GetNombreClientsAsync()
        => await _context.Clients.CountAsync();

    public async Task<int> GetRdvAujourdhuiAsync()
    {
        var today = DateTime.UtcNow.Date;
        return await _context.RendezVous
            .CountAsync(r => r.DateHeure.Date == today);
    }

    public async Task<int> GetFacturesImpayeesAsync()
        => await _context.Factures.CountAsync(f => f.Statut == StatutFacture.Emise || f.Statut == StatutFacture.Brouillon);

    // ── Listes opérationnelles ───────────────────────────────────────────────
    public async Task<IEnumerable<OrdreTravail>> GetOTsActifsAsync()
        => await _context.OrdresTravail
            .Include(o => o.Vehicule).ThenInclude(v => v.Client)
            .Include(o => o.Employe)
            .Where(o => o.Statut == StatutOT.Ouvert || o.Statut == StatutOT.EnCours
                     || o.Statut == StatutOT.EnAttentePieces || o.Statut == StatutOT.ControleQualite
                     || o.Statut == StatutOT.Pret)
            .OrderByDescending(o => o.DateCreation)
            .Take(10)
            .ToListAsync();

    public async Task<IEnumerable<RendezVous>> GetRdvDuJourAsync()
    {
        var today = DateTime.UtcNow.Date;
        return await _context.RendezVous
            .Include(r => r.Client)
            .Include(r => r.Vehicule)
            .Where(r => r.DateHeure.Date == today)
            .OrderBy(r => r.DateHeure)
            .ToListAsync();
    }

    public async Task<IEnumerable<Piece>> GetPiecesStockCritiqueAsync()
        => await _context.Pieces
            .Where(p => p.StockActuel <= p.StockMinimum && p.EstActif)
            .OrderBy(p => p.StockActuel)
            .Take(8)
            .ToListAsync();

    public async Task<IEnumerable<Facture>> GetFacturesRecentsAsync()
        => await _context.Factures
            .Include(f => f.Client)
            .OrderByDescending(f => f.DateFacture)
            .Take(6)
            .ToListAsync();

    // ── CA 30 jours (graphique) ──────────────────────────────────────────────
    public async Task<IEnumerable<(DateTime Date, decimal CA)>> GetCA30JoursAsync()
    {
        var debut = DateTime.UtcNow.AddDays(-30).Date;
        var factures = await _context.Factures
            .Where(f => f.DateFacture >= debut && f.Statut != StatutFacture.Annulee)
            .ToListAsync();

        return factures
            .GroupBy(f => f.DateFacture.Date)
            .Select(g => (g.Key, g.Sum(f => f.MontantHT)))
            .OrderBy(x => x.Key);
    }

    public async Task<IEnumerable<(string Service, int Count)>> GetTopServicesAsync()
    {
        var lignes = await _context.LignesOT
            .Where(l => l.Type == TypeLigneOT.MainOeuvre)
            .ToListAsync();

        return lignes
            .GroupBy(l => l.Description)
            .Select(g => (Key: g.Key, Count: g.Count()))
            .OrderByDescending(x => x.Count)
            .Take(5);
    }
}
