using MecanoERP.Core.Entities.Comptabilite;

namespace MecanoERP.Core.Entities;

public enum StatutFacture
{
    Brouillon,
    Emise,
    PartiellementPayee,
    Payee,
    Annulee
}

public class Facture
{
    public int Id { get; set; }
    public string Numero { get; set; } = string.Empty;
    public int ClientId { get; set; }
    public int OrdreTravailId { get; set; }
    public DateTime DateFacture { get; set; } = DateTime.UtcNow;
    public DateTime? DateEcheance { get; set; }

    // Montants
    public decimal MontantHT { get; set; }

    // Taxes — calculées dynamiquement via ConfigurationFiscale
    public int? ConfigurationFiscaleId { get; set; }
    public decimal TauxTPS { get; set; } = 0.05m;
    public decimal TauxTVQ { get; set; } = 0.09975m;
    public decimal? TauxTVH { get; set; }                      // null si régime TPS+TVQ

    public decimal MontantTPS => MontantHT * TauxTPS;
    public decimal MontantTVQ => MontantHT * TauxTVQ;
    public decimal MontantTVH => MontantHT * (TauxTVH ?? 0m);
    public decimal MontantTTC => MontantHT + MontantTPS + MontantTVQ + MontantTVH;
    public decimal MontantPaye { get; set; }
    public decimal SoldeRestant => MontantTTC - MontantPaye;

    // Numéros d'inscription apparaissant sur la facture (selon province)
    public string NumeroTPS_Garage { get; set; } = string.Empty;
    public string NumeroTVQ_Garage { get; set; } = string.Empty;
    public string NumeroTVH_Garage { get; set; } = string.Empty;

    public StatutFacture Statut { get; set; } = StatutFacture.Brouillon;
    public string Notes { get; set; } = string.Empty;

    // Lien vers le journal comptable généré automatiquement
    public int? JournalComptableId { get; set; }

    // Navigation
    public Client Client { get; set; } = null!;
    public OrdreTravail OrdreTravail { get; set; } = null!;
    public ICollection<Paiement> Paiements { get; set; } = new List<Paiement>();
    public ICollection<Garantie> Garanties { get; set; } = new List<Garantie>();
    public JournalComptable? JournalComptable { get; set; }
}
