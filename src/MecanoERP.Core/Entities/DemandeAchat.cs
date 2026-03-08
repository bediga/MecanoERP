namespace MecanoERP.Core.Entities;

public enum StatutDemandeAchat
{
    Brouillon,
    Soumise,
    Approuvee,
    BonDeCommandeEmis,
    Annulee
}

public class DemandeAchat
{
    public int Id { get; set; }
    public string Numero { get; set; } = string.Empty;
    public int? DemandeurId { get; set; }   // EmployeId
    public DateTime DateDemande { get; set; } = DateTime.UtcNow;
    public StatutDemandeAchat Statut { get; set; } = StatutDemandeAchat.Brouillon;
    public string Notes { get; set; } = string.Empty;
    public int? CommandeAchatId { get; set; }

    // Navigation
    public Employe? Demandeur { get; set; }
    public CommandeAchat? CommandeAchat { get; set; }
    public ICollection<LigneDemandeAchat> Lignes { get; set; } = new List<LigneDemandeAchat>();
}
