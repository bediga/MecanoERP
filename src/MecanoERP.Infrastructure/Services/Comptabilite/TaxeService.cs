using MecanoERP.Core.Entities.Comptabilite;
using MecanoERP.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MecanoERP.Infrastructure.Services.Comptabilite;

public record ResultatTaxes(
    decimal MontantHT,
    decimal MontantTPS,
    decimal MontantTVQ,
    decimal MontantTVH,
    decimal MontantTTC,
    ProvinceCA Province,
    RegimeTaxe Regime,
    string NumeroTPS,
    string NumeroTVQ,
    string NumeroTVH);

public class TaxeService
{
    private readonly MecanoDbContext _context;

    public TaxeService(MecanoDbContext context) => _context = context;

    public async Task<ConfigurationFiscale?> GetConfigActiveAsync()
        => await _context.ConfigurationsFiscales.FirstOrDefaultAsync(c => c.EstActive);

    public async Task<IEnumerable<ConfigurationFiscale>> GetToutesAsync()
        => await _context.ConfigurationsFiscales.OrderBy(c => c.Province).ToListAsync();

    public async Task<ResultatTaxes?> CalculerAsync(decimal montantHT)
    {
        var config = await GetConfigActiveAsync();
        if (config is null) return null;
        var (tps, tvq, tvh, total) = config.CalculerTaxes(montantHT);
        return new ResultatTaxes(montantHT, tps, tvq, tvh, montantHT + total,
            config.Province, config.Regime,
            config.NumeroInscriptionTPS, config.NumeroInscriptionTVQ, config.NumeroInscriptionTVH);
    }

    public async Task SauvegarderConfigAsync(ConfigurationFiscale config)
    {
        // Une seule config active à la fois
        var existantes = await _context.ConfigurationsFiscales.ToListAsync();
        existantes.ForEach(c => c.EstActive = false);
        if (config.Id == 0)
            _context.ConfigurationsFiscales.Add(config);
        else
            _context.ConfigurationsFiscales.Update(config);
        config.EstActive = true;
        config.DateMiseAJour = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Retourne les configs pour toutes les provinces canadiennes avec les taux légaux.
    /// </summary>
    public static IEnumerable<ConfigurationFiscale> GetProvincesPredefinies()
    {
        return
        [
            new() { Province=ProvinceCA.QC, NomProvince="Québec",              Regime=RegimeTaxe.TPS_TVQ,   TauxTPS=0.05m, TauxTVQ=0.09975m },
            new() { Province=ProvinceCA.ON, NomProvince="Ontario",             Regime=RegimeTaxe.TVH,       TauxTPS=0.05m, TauxTVH=0.13m },
            new() { Province=ProvinceCA.NB, NomProvince="Nouveau-Brunswick",   Regime=RegimeTaxe.TVH,       TauxTPS=0.05m, TauxTVH=0.15m },
            new() { Province=ProvinceCA.NS, NomProvince="Nouvelle-Écosse",     Regime=RegimeTaxe.TVH,       TauxTPS=0.05m, TauxTVH=0.15m },
            new() { Province=ProvinceCA.NL, NomProvince="Terre-Neuve-et-Lab.", Regime=RegimeTaxe.TVH,       TauxTPS=0.05m, TauxTVH=0.15m },
            new() { Province=ProvinceCA.PE, NomProvince="Île-du-Prince-Édou.", Regime=RegimeTaxe.TVH,       TauxTPS=0.05m, TauxTVH=0.15m },
            new() { Province=ProvinceCA.BC, NomProvince="Colombie-Brit.",      Regime=RegimeTaxe.TVH,       TauxTPS=0.05m, TauxTVH=0.12m },
            new() { Province=ProvinceCA.AB, NomProvince="Alberta",             Regime=RegimeTaxe.TPS_Seule, TauxTPS=0.05m },
            new() { Province=ProvinceCA.SK, NomProvince="Saskatchewan",        Regime=RegimeTaxe.TPS_Seule, TauxTPS=0.05m },
            new() { Province=ProvinceCA.MB, NomProvince="Manitoba",            Regime=RegimeTaxe.TPS_Seule, TauxTPS=0.05m },
            new() { Province=ProvinceCA.NT, NomProvince="Territoires du Nord-O.", Regime=RegimeTaxe.TPS_Seule, TauxTPS=0.05m },
            new() { Province=ProvinceCA.NU, NomProvince="Nunavut",             Regime=RegimeTaxe.TPS_Seule, TauxTPS=0.05m },
            new() { Province=ProvinceCA.YT, NomProvince="Yukon",               Regime=RegimeTaxe.TPS_Seule, TauxTPS=0.05m },
        ];
    }
}
