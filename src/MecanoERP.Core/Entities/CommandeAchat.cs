namespace MecanoERP.Core.Entities;

public enum StatutCommande
{
    EnAttente,
    CommandeEnvoyee,
    PartielleReçue,
    Recue,
    Annulee
}

public class CommandeAchat
{
    public int Id { get; set; }
    public string Numero { get; set; } = string.Empty;
    public int FournisseurId { get; set; }
    public DateTime DateCommande { get; set; } = DateTime.UtcNow;
    public DateTime? DateReception { get; set; }
    public StatutCommande Statut { get; set; } = StatutCommande.EnAttente;
    public string Notes { get; set; } = string.Empty;

    // Navigation
    public Fournisseur Fournisseur { get; set; } = null!;
    public ICollection<LigneCommandeAchat> Lignes { get; set; } = new List<LigneCommandeAchat>();
}
