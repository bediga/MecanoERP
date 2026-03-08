namespace MecanoERP.Core.Entities;

public class Client
{
    public int Id { get; set; }
    public string Nom { get; set; } = string.Empty;
    public string Prenom { get; set; } = string.Empty;
    public string Telephone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Adresse { get; set; } = string.Empty;
    public string Ville { get; set; } = string.Empty;
    public string CodePostal { get; set; } = string.Empty;
    public decimal Solde { get; set; }
    public bool ConsentementSMS { get; set; }
    public bool ConsentementEmail { get; set; }
    public string NotesInternes { get; set; } = string.Empty;
    public DateTime DateCreation { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<Vehicule> Vehicules { get; set; } = new List<Vehicule>();
    public ICollection<RendezVous> RendezVous { get; set; } = new List<RendezVous>();
    public ICollection<Facture> Factures { get; set; } = new List<Facture>();
}
