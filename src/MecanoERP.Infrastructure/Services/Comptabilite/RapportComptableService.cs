using MecanoERP.Core.Entities.Comptabilite;
using MecanoERP.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MecanoERP.Infrastructure.Services.Comptabilite;

public record LigneBalanceVerification(
    string Numero, string Nom, TypeCompteGL Type,
    decimal TotalDebit, decimal TotalCredit, decimal Solde);

public record LigneGL_Rapport(
    DateTime Date, string JournalNumero, string Description,
    decimal Debit, decimal Credit, decimal SoldeCumulatif);

public record DeclarationTaxes(
    DateTime Debut, DateTime Fin, ProvinceCA Province,
    decimal VentesHT, decimal TPS_Percue, decimal TVQ_Percue, decimal TVH_Percue,
    decimal TPS_Recuperable, decimal TVQ_Recuperable,
    decimal TPS_NetteARemettre, decimal TVQ_NetteARemettre, decimal TVH_NetteARemettre);

public class RapportComptableService
{
    private readonly MecanoDbContext _context;

    public RapportComptableService(MecanoDbContext context) => _context = context;

    /// <summary>Balance de vérification — tous les comptes avec débit/crédit cumulés.</summary>
    public async Task<IEnumerable<LigneBalanceVerification>> GetBalanceVerificationAsync(
        DateTime? debut = null, DateTime? fin = null)
    {
        var comptes = await _context.ComptesGL
            .Include(c => c.LignesJournal).ThenInclude(l => l.Journal)
            .Where(c => c.EstActif)
            .OrderBy(c => c.Numero)
            .ToListAsync();

        return comptes.Select(c =>
        {
            var lignes = c.LignesJournal
                .Where(l => !l.Journal.EstBrouillon && !l.Journal.EstAnnule
                    && (debut == null || l.Journal.Date >= debut)
                    && (fin   == null || l.Journal.Date <= fin));
            var d = lignes.Sum(l => l.Debit);
            var cr = lignes.Sum(l => l.Credit);
            return new LigneBalanceVerification(c.Numero, c.Nom, c.TypeCompte, d, cr, d - cr);
        }).Where(l => l.TotalDebit != 0 || l.TotalCredit != 0);
    }

    /// <summary>Grand Livre d'un compte.</summary>
    public async Task<IEnumerable<LigneGL_Rapport>> GetGrandLivreAsync(
        int compteId, DateTime? debut = null, DateTime? fin = null)
    {
        var lignes = await _context.LignesJournal
            .Include(l => l.Journal)
            .Where(l => l.CompteGLId == compteId
                && !l.Journal.EstBrouillon && !l.Journal.EstAnnule
                && (debut == null || l.Journal.Date >= debut)
                && (fin   == null || l.Journal.Date <= fin))
            .OrderBy(l => l.Journal.Date)
            .ToListAsync();

        decimal solde = 0;
        return lignes.Select(l =>
        {
            solde += l.Debit - l.Credit;
            return new LigneGL_Rapport(l.Journal.Date, l.Journal.Numero, l.Description, l.Debit, l.Credit, solde);
        });
    }

    /// <summary>Sommaire TPS/TVQ/TVH pour déclaration fiscale (ARC).</summary>
    public async Task<DeclarationTaxes?> GetDeclarationTaxesAsync(DateTime debut, DateTime fin)
    {
        var config = await _context.ConfigurationsFiscales.FirstOrDefaultAsync(c => c.EstActive);
        if (config is null) return null;

        // Montants via les factures émises (données source)
        var factures = await _context.Factures
            .Where(f => f.DateFacture >= debut && f.DateFacture <= fin && f.Statut != Core.Entities.StatutFacture.Annulee)
            .ToListAsync();

        var ventesHT     = factures.Sum(f => f.MontantHT);
        var tpsPercue    = factures.Sum(f => f.MontantTPS);
        var tvqPercue    = factures.Sum(f => f.MontantTVQ);
        var tvhPercue    = factures.Sum(f => f.MontantTVH);

        // CTI/RTI via comptes GL
        decimal tpsRecup = 0, tvqRecup = 0;
        if (config.CompteGLTPS_RecuperableId.HasValue)
            tpsRecup = await SoldeCompte(config.CompteGLTPS_RecuperableId.Value, debut, fin);
        if (config.CompteGLTVQ_RecuperableId.HasValue)
            tvqRecup = await SoldeCompte(config.CompteGLTVQ_RecuperableId.Value, debut, fin);

        return new DeclarationTaxes(debut, fin, config.Province,
            ventesHT, tpsPercue, tvqPercue, tvhPercue, tpsRecup, tvqRecup,
            tpsPercue - tpsRecup, tvqPercue - tvqRecup, tvhPercue);
    }

    private async Task<decimal> SoldeCompte(int id, DateTime debut, DateTime fin)
    {
        var lignes = await _context.LignesJournal
            .Include(l => l.Journal)
            .Where(l => l.CompteGLId == id && !l.Journal.EstBrouillon && !l.Journal.EstAnnule
                && l.Journal.Date >= debut && l.Journal.Date <= fin)
            .ToListAsync();
        return lignes.Sum(l => l.Credit - l.Debit);
    }
}
