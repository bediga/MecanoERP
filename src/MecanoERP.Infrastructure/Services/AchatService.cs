using MecanoERP.Core.Entities;
using MecanoERP.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MecanoERP.Infrastructure.Services;

public class AchatService
{
    private readonly MecanoDbContext _ctx;
    public AchatService(MecanoDbContext ctx) => _ctx = ctx;

    // ── Demandes d'achat ────────────────────────────────────────────
    public async Task<IEnumerable<DemandeAchat>> GetDemandesAsync()
        => await _ctx.DemandesAchat
            .Include(d => d.Demandeur)
            .Include(d => d.Lignes).ThenInclude(l => l.Piece)
            .OrderByDescending(d => d.DateDemande)
            .ToListAsync();

    public async Task<DemandeAchat> CreerDemandeAsync(DemandeAchat demande)
    {
        var count = await _ctx.DemandesAchat.CountAsync() + 1;
        demande.Numero = $"DA-{DateTime.Now.Year}-{count:D4}";
        demande.DateDemande = DateTime.UtcNow;
        demande.Statut = StatutDemandeAchat.Brouillon;
        _ctx.DemandesAchat.Add(demande);
        await _ctx.SaveChangesAsync();
        return demande;
    }

    public async Task ApprouverDemandeAsync(int demandeId)
    {
        var d = await _ctx.DemandesAchat.FindAsync(demandeId)
            ?? throw new Exception("Demande introuvable.");
        d.Statut = StatutDemandeAchat.Approuvee;
        await _ctx.SaveChangesAsync();
    }

    public async Task SupprimerDemandeAsync(int id)
    {
        var d = await _ctx.DemandesAchat.FindAsync(id);
        if (d != null) { _ctx.DemandesAchat.Remove(d); await _ctx.SaveChangesAsync(); }
    }

    public async Task<CommandeAchat> CreerBCDepuisDemandeAsync(int demandeId)
    {
        var demande = await _ctx.DemandesAchat
            .Include(d => d.Lignes).ThenInclude(l => l.Piece)
            .FirstOrDefaultAsync(d => d.Id == demandeId)
            ?? throw new Exception("Demande introuvable.");

        if (demande.Lignes.Count == 0)
            throw new Exception("La demande n'a aucune ligne.");

        var fournisseurId = demande.Lignes
            .FirstOrDefault(l => l.Piece?.FournisseurId != null)?.Piece!.FournisseurId
            ?? throw new Exception("Aucune pièce avec fournisseur dans la demande.");

        var count = await _ctx.CommandesAchat.CountAsync() + 1;
        var bc = new CommandeAchat
        {
            Numero = $"BC-{DateTime.Now.Year}-{count:D4}",
            FournisseurId = fournisseurId,
            DateCommande = DateTime.UtcNow,
            Statut = StatutCommande.EnAttente,
            Notes = $"Généré depuis demande {demande.Numero}",
            Lignes = demande.Lignes.Select(l => new LigneCommandeAchat
            {
                PieceId = l.PieceId!.Value,
                Quantite = l.Quantite,
                PrixUnitaire = l.PrixEstime
            }).ToList()
        };

        _ctx.CommandesAchat.Add(bc);
        demande.Statut = StatutDemandeAchat.BonDeCommandeEmis;
        demande.CommandeAchatId = bc.Id;
        await _ctx.SaveChangesAsync();
        return bc;
    }
}
