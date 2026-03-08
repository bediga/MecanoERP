namespace MecanoERP.Core.Entities;

public enum StatutFactureFournisseur
{
    Recue,
    AValider,
    Approuvee,
    Payee,
    PartiellementPayee,
    Annulee
}

public class FactureFournisseur
{
    public int Id { get; set; }
    public string Numero { get; set; } = string.Empty;        // N° interne
    public string NumeroFournisseur { get; set; } = string.Empty; // N° sur la facture reçue
    public int FournisseurId { get; set; }
    public int? CommandeAchatId { get; set; }
    public DateTime DateFacture { get; set; } = DateTime.UtcNow;
    public DateTime DateEcheance { get; set; } = DateTime.UtcNow.AddDays(30);
    public decimal MontantHT { get; set; }
    public decimal TauxTaxe { get; set; } = 0.14975m;
    public decimal MontantPaye { get; set; }
    public StatutFactureFournisseur Statut { get; set; } = StatutFactureFournisseur.Recue;
    public string Notes { get; set; } = string.Empty;
    public DateTime DateCreation { get; set; } = DateTime.UtcNow;

    // Calculs
    public decimal MontantTaxes => MontantHT * TauxTaxe;
    public decimal MontantTTC => MontantHT + MontantTaxes;
    public decimal SoldeRestant => MontantTTC - MontantPaye;

    // Navigation
    public Fournisseur Fournisseur { get; set; } = null!;
    public CommandeAchat? CommandeAchat { get; set; }
    public ICollection<PaiementFournisseur> Paiements { get; set; } = new List<PaiementFournisseur>();
}
