using MecanoERP.Core.Entities;
using MecanoERP.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MecanoERP.Infrastructure.Services;

public class InventaireService
{
    private readonly MecanoDbContext _context;

    public InventaireService(MecanoDbContext context) => _context = context;

    public async Task<IEnumerable<Piece>> GetPiecesStockCritiqueAsync()
        => await _context.Pieces
            .Where(p => p.StockActuel <= p.StockMinimum && p.EstActif)
            .Include(p => p.Fournisseur)
            .ToListAsync();

    public async Task AjusterStock(int pieceId, int quantite, string motif)
    {
        var piece = await _context.Pieces.FindAsync(pieceId)
            ?? throw new Exception("Pièce introuvable.");

        piece.StockActuel += quantite;

        if (piece.StockActuel <= piece.StockMinimum)
        {
            _context.Notifications.Add(new Notification
            {
                Titre = "Stock critique",
                Message = $"La pièce '{piece.Designation}' est en stock critique ({piece.StockActuel} restants).",
                Type = TypeNotification.StockCritique
            });
        }

        await _context.SaveChangesAsync();
    }

    public async Task ReceptionnerCommande(int commandeId)
    {
        var commande = await _context.CommandesAchat
            .Include(c => c.Lignes).ThenInclude(l => l.Piece)
            .FirstOrDefaultAsync(c => c.Id == commandeId)
            ?? throw new Exception("Commande introuvable.");

        foreach (var ligne in commande.Lignes)
        {
            ligne.Piece.StockActuel += ligne.Quantite;
            ligne.QuantiteRecue = ligne.Quantite;
        }

        commande.Statut = StatutCommande.Recue;
        commande.DateReception = DateTime.UtcNow;

        // Écriture comptable achat
        var total = commande.Lignes.Sum(l => l.Quantite * l.PrixUnitaire);
        _context.EcrituresComptables.Add(new EcritureComptable
        {
            Date = DateTime.UtcNow,
            Type = TypeEcriture.Achat,
            Reference = commande.Numero,
            Description = $"Réception commande {commande.Numero}",
            Debit = total,
            Compte = "6000-Achats",
            CommandeAchatId = commandeId
        });

        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Piece>> GetAllPiecesAsync()
        => await _context.Pieces
            .Include(p => p.Fournisseur)
            .Where(p => p.EstActif)
            .OrderBy(p => p.Designation)
            .ToListAsync();
}
