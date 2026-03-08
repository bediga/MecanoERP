using MecanoERP.Core.Entities;
using MecanoERP.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MecanoERP.Infrastructure.Services;

public class NotificationService
{
    private readonly MecanoDbContext _context;

    public NotificationService(MecanoDbContext context) => _context = context;

    public async Task<IEnumerable<Notification>> GetNonLuesAsync()
        => await _context.Notifications
            .Where(n => !n.Lu)
            .OrderByDescending(n => n.DateCreation)
            .ToListAsync();

    public async Task<int> GetNombreNonLuesAsync()
        => await _context.Notifications.CountAsync(n => !n.Lu);

    public async Task<Notification> CreerAsync(string titre, string message, TypeNotification type, int? clientId = null, int? otId = null)
    {
        var notif = new Notification
        {
            Titre = titre,
            Message = message,
            Type = type,
            ClientId = clientId,
            OrdreTravailId = otId,
            DateCreation = DateTime.UtcNow,
            Lu = false
        };
        _context.Notifications.Add(notif);
        await _context.SaveChangesAsync();
        return notif;
    }

    public async Task MarquerLueAsync(int id)
    {
        var notif = await _context.Notifications.FindAsync(id);
        if (notif is not null)
        {
            notif.Lu = true;
            await _context.SaveChangesAsync();
        }
    }

    public async Task MarquerToutesLuesAsync()
    {
        var nonLues = await _context.Notifications.Where(n => !n.Lu).ToListAsync();
        foreach (var n in nonLues) n.Lu = true;
        await _context.SaveChangesAsync();
    }

    public async Task VerifierAlertesPiecesAsync(InventaireService inventaireService)
    {
        var alertes = await inventaireService.GetPiecesStockCritiqueAsync();
        foreach (var piece in alertes)
        {
            await CreerAsync(
                "Stock critique",
                $"{piece.Reference} — {piece.Designation} : {piece.StockActuel} restant(s) (min: {piece.StockMinimum})",
                TypeNotification.StockCritique);
        }
    }
}
