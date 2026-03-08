using MecanoERP.Core.Entities;
using MecanoERP.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MecanoERP.Infrastructure.Services;

public class FactureFournisseurService
{
    private readonly MecanoDbContext _ctx;
    public FactureFournisseurService(MecanoDbContext ctx) => _ctx = ctx;

    public async Task<IEnumerable<FactureFournisseur>> GetAllAsync()
        => await _ctx.FacturesFournisseurs
            .Include(f => f.Fournisseur)
            .Include(f => f.Paiements)
            .OrderByDescending(f => f.DateFacture)
            .ToListAsync();

    public async Task<FactureFournisseur> AjouterAsync(FactureFournisseur facture)
    {
        var count = await _ctx.FacturesFournisseurs.CountAsync() + 1;
        facture.Numero = $"AP-{DateTime.Now.Year}-{count:D4}";
        facture.DateCreation = DateTime.UtcNow;
        _ctx.FacturesFournisseurs.Add(facture);
        await _ctx.SaveChangesAsync();
        return facture;
    }

    public async Task ModifierAsync(FactureFournisseur facture)
    {
        _ctx.FacturesFournisseurs.Update(facture);
        await _ctx.SaveChangesAsync();
    }

    public async Task EnregistrerPaiementAsync(int factureId, decimal montant, ModePaiement mode, string reference = "")
    {
        var facture = await _ctx.FacturesFournisseurs.FindAsync(factureId)
            ?? throw new Exception("Facture fournisseur introuvable.");

        var paiement = new PaiementFournisseur
        {
            FactureFournisseurId = factureId,
            Montant = montant,
            DatePaiement = DateTime.UtcNow,
            ModePaiement = mode,
            Reference = reference
        };
        _ctx.PaiementsFournisseurs.Add(paiement);
        facture.MontantPaye += montant;
        facture.Statut = facture.MontantPaye >= facture.MontantTTC
            ? StatutFactureFournisseur.Payee
            : StatutFactureFournisseur.PartiellementPayee;

        await _ctx.SaveChangesAsync();
    }

    public async Task<IEnumerable<FactureFournisseur>> GetAgingAPAsync()
    {
        var limite = DateTime.UtcNow;
        return await _ctx.FacturesFournisseurs
            .Include(f => f.Fournisseur)
            .Where(f => f.Statut != StatutFactureFournisseur.Payee
                     && f.Statut != StatutFactureFournisseur.Annulee
                     && f.DateEcheance < limite)
            .OrderBy(f => f.DateEcheance)
            .ToListAsync();
    }
}
