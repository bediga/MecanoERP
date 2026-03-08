using MecanoERP.Core.Entities;
using MecanoERP.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MecanoERP.Infrastructure.Services;

public class OrdreTravailService
{
    private readonly MecanoDbContext _context;

    public OrdreTravailService(MecanoDbContext context) => _context = context;

    public async Task<OrdreTravail> CreerOTAsync(OrdreTravail ot)
    {
        var count = await _context.OrdresTravail.CountAsync() + 1;
        ot.Numero = $"OT-{DateTime.Now.Year}-{count:D4}";
        ot.DateEntree = DateTime.UtcNow;
        ot.Statut = StatutOT.Ouvert;

        _context.OrdresTravail.Add(ot);
        await _context.SaveChangesAsync();
        return ot;
    }

    public async Task ChangerStatutAsync(int otId, StatutOT nouveauStatut)
    {
        var ot = await _context.OrdresTravail.FindAsync(otId)
            ?? throw new Exception("OT introuvable.");
        ot.Statut = nouveauStatut;
        // Fermer l'OT dès qu'on atteint Pret (ancien Termine supprimé)
        if (nouveauStatut == StatutOT.Pret || nouveauStatut == StatutOT.Facture)
            ot.DateSortie = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task AjouterLigneAsync(LigneOT ligne)
    {
        _context.LignesOT.Add(ligne);

        // Réduction stock si pièce
        if (ligne.PieceId.HasValue)
        {
            var piece = await _context.Pieces.FindAsync(ligne.PieceId.Value);
            if (piece != null)
                piece.StockActuel -= (int)ligne.Quantite;
        }

        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<OrdreTravail>> GetAllAsync()
        => await _context.OrdresTravail
            .Include(o => o.Vehicule).ThenInclude(v => v.Client)
            .Include(o => o.Employe)
            .Include(o => o.Lignes)
            .OrderByDescending(o => o.DateEntree)
            .ToListAsync();

    public async Task<OrdreTravail?> GetByIdAsync(int id)
        => await _context.OrdresTravail
            .Include(o => o.Vehicule).ThenInclude(v => v.Client)
            .Include(o => o.Employe)
            .Include(o => o.Lignes).ThenInclude(l => l.Piece)
            .FirstOrDefaultAsync(o => o.Id == id);
}
