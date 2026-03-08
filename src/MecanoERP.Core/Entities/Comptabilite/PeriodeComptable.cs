namespace MecanoERP.Core.Entities.Comptabilite;

public enum StatutPeriode { Ouverte, Fermee, Cloturee }
public enum TypeCloture   { Mensuelle, Annuelle }

/// <summary>Période comptable (exercice + mois). Empêche les écritures sur périodes fermées.</summary>
public class PeriodeComptable
{
    public int Id { get; set; }
    public int Exercice { get; set; }        // ex: 2026
    public int Periode { get; set; }         // 1-12
    public DateTime DateDebut { get; set; }
    public DateTime DateFin { get; set; }
    public StatutPeriode Statut { get; set; } = StatutPeriode.Ouverte;
    public TypeCloture? TypeCloture { get; set; }
    public DateTime? DateCloture { get; set; }
    public int? UserClotureId { get; set; }
    public string Notes { get; set; } = string.Empty;

    public string Libelle => $"{Exercice}-{Periode:D2}";
}
