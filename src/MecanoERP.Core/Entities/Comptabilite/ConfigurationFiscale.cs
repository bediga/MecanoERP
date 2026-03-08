namespace MecanoERP.Core.Entities.Comptabilite;

/// <summary>
/// Régimes de taxation canadiens.
/// QC  → TPS (fédéral) + TVQ (provincial)  
/// ON/NB/NS/NL/PEI/BC → TVH combinée (fédéral+provincial)  
/// AB/SK/MB/NT/NU/YT  → TPS seule (pas de taxe provinciale)
/// </summary>
public enum RegimeTaxe
{
    TPS_TVQ,   // Québec
    TVH,       // Provinces TVH
    TPS_Seule  // Provinces sans taxe provinciale
}

public enum ProvinceCA
{
    AB, BC, MB, NB, NL, NS, NT, NU, ON, PE, QC, SK, YT
}

public class ConfigurationFiscale
{
    public int Id { get; set; }
    public ProvinceCA Province { get; set; }
    public string NomProvince { get; set; } = string.Empty;
    public RegimeTaxe Regime { get; set; }

    // Taux fédéral TPS (toujours 5%)
    public decimal TauxTPS { get; set; } = 0.05m;

    // TVQ provinciale (QC = 9,975%) — 0 si régime TVH ou TPS seule
    public decimal TauxTVQ { get; set; }

    // TVH = TPS+provincial combiné (ex: ON=13%, NB/NS/NL/PEI=15%, BC=12%)
    // Null si régime TPS_TVQ ou TPS_Seule
    public decimal? TauxTVH { get; set; }

    // Numéros d'inscription du garage
    public string NumeroInscriptionTPS { get; set; } = string.Empty;  // 9XX XXX XXX RT 0001
    public string NumeroInscriptionTVQ { get; set; } = string.Empty;  // XXXXXXXXXXXXTQ 0001
    public string NumeroInscriptionTVH { get; set; } = string.Empty;

    // Comptes GL liés (configurés par le comptable)
    public int? CompteGLTPS_PercueId { get; set; }
    public int? CompteGLTVQ_PercueId { get; set; }
    public int? CompteGLTVH_PercueId { get; set; }
    public int? CompteGLTPS_RecuperableId { get; set; }  // CTI
    public int? CompteGLTVQ_RecuperableId { get; set; }  // RTI

    public bool EstActive { get; set; } = true;
    public DateTime DateMiseAJour { get; set; } = DateTime.UtcNow;

    // Computed
    public decimal TauxEffectifTotal => Regime switch
    {
        RegimeTaxe.TPS_TVQ   => TauxTPS + TauxTVQ,
        RegimeTaxe.TVH       => TauxTVH ?? TauxTPS,
        RegimeTaxe.TPS_Seule => TauxTPS,
        _                    => TauxTPS
    };

    public (decimal MontantTPS, decimal MontantTVQ, decimal MontantTVH, decimal Total) 
        CalculerTaxes(decimal montantHT)
    {
        return Regime switch
        {
            RegimeTaxe.TPS_TVQ => (
                montantHT * TauxTPS,
                montantHT * TauxTVQ,
                0m,
                montantHT * (TauxTPS + TauxTVQ)),
            RegimeTaxe.TVH => (
                0m, 0m,
                montantHT * (TauxTVH ?? TauxTPS),
                montantHT * (TauxTVH ?? TauxTPS)),
            RegimeTaxe.TPS_Seule => (
                montantHT * TauxTPS,
                0m, 0m,
                montantHT * TauxTPS),
            _ => (0m, 0m, 0m, 0m)
        };
    }
}
