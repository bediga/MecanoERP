namespace MecanoERP.Core.Entities.Comptabilite;

public enum TypeJournal
{
    Ventes,   // VTE
    Achats,   // ACH
    Caisse,   // CA
    Banque,   // BQ
    General,  // OD — Opérations diverses
    Inventaire,      // INVT
    OuvertureBalance, // OPBL
    Cloture          // CLT
}

/// <summary>En-tête de journal comptable — équivalent OJDT dans SAP B1.</summary>
public class JournalComptable
{
    public int Id { get; set; }
    public string Numero { get; set; } = string.Empty;   // ex: "OD-2026-0001"

    // Code journal court (ex: OD, BQ, VTE, ACH, INVT, OPBL)
    public string JournalCode { get; set; } = "OD";

    // Date comptable
    public DateTime Date { get; set; } = DateTime.UtcNow;

    // Période comptable (exercice + mois)
    public int Exercice { get; set; } = DateTime.UtcNow.Year;
    public int Periode { get; set; }  = DateTime.UtcNow.Month;

    // Description et référence externe (ex: N° facture fournisseur)
    public string Description { get; set; } = string.Empty;
    public string ReferenceExterne { get; set; } = string.Empty;

    // Multi-devise
    public string Devise { get; set; } = "CAD";
    public decimal TauxChange { get; set; } = 1;

    // Statut
    public TypeJournal TypeJournal { get; set; }
    public bool EstBrouillon { get; set; } = true;
    public bool EstAnnule { get; set; }

    // Traçabilité
    public int? UtilisateurId { get; set; }
    public DateTime DateCreation { get; set; } = DateTime.UtcNow;
    public DateTime? DateValidation { get; set; }

    // Liens sources optionnels
    public int? FactureId { get; set; }
    public int? CommandeAchatId { get; set; }

    // Navigation
    public ICollection<LigneJournal> Lignes { get; set; } = new List<LigneJournal>();

    // Computed
    public decimal TotalDebit  => Lignes.Sum(l => l.Debit);
    public decimal TotalCredit => Lignes.Sum(l => l.Credit);
    public bool    EstEquilibre => TotalDebit == TotalCredit && TotalDebit > 0;
    public string  StatutLibelle => EstAnnule ? "Annulé" : EstBrouillon ? "Brouillon" : "Validé";
}
