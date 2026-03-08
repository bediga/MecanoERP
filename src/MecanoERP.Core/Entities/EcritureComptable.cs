using MecanoERP.Core.Entities.Comptabilite;

namespace MecanoERP.Core.Entities;

public enum TypeEcriture
{
    Vente,
    Achat,
    Paiement,
    Remboursement,
    Ajustement
}

/// <summary>
/// Écriture comptable individuelle (legacy).
/// Les nouvelles écritures passent par JournalComptable + LigneJournal.
/// Ce modèle est conservé pour compatibilité et comme résumé rapide.
/// </summary>
public class EcritureComptable
{
    public int Id { get; set; }
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public TypeEcriture Type { get; set; }
    public string Reference { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }

    // Compte GL structuré (nouveau)
    public int? CompteGLId { get; set; }
    
    // Compte legacy (gardé pour compatibilité)
    public string Compte { get; set; } = string.Empty;

    public int? FactureId { get; set; }
    public int? CommandeAchatId { get; set; }

    // Navigation
    public CompteGL? CompteGL { get; set; }
}
