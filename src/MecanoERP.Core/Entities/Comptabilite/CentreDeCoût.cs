namespace MecanoERP.Core.Entities.Comptabilite;

/// <summary>Dimension analytique (centre de coût / département).</summary>
public class CentreDeCoût
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;   // ex: "ATELIER", "ADMIN"
    public string Nom { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool EstActif { get; set; } = true;
    public DateTime DateCreation { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<LigneJournal> Lignes { get; set; } = new List<LigneJournal>();
}
