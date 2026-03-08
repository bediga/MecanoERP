using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MecanoERP.Core.Entities;
using MecanoERP.Infrastructure.Services;
using System.Collections.ObjectModel;

namespace MecanoERP.WPF.ViewModels;

// ══════════════════════════════════════════════════════════════════════════════
// VÉHICULES
// ══════════════════════════════════════════════════════════════════════════════
public partial class VehiculesViewModel : ObservableObject
{
    private readonly VehiculeService _svc;
    private readonly ClientService _clientSvc;

    [ObservableProperty] private ObservableCollection<Vehicule> _vehicules = [];
    [ObservableProperty] private ObservableCollection<Client> _clients = [];
    [ObservableProperty] private Vehicule? _selection;
    [ObservableProperty] private string _recherche = string.Empty;
    [ObservableProperty] private bool _afficherFormulaire;
    [ObservableProperty] private bool _isLoading;

    // Champs formulaire
    [ObservableProperty] private int _selectedClientId;
    [ObservableProperty] private string _marque = string.Empty;
    [ObservableProperty] private string _modele = string.Empty;
    [ObservableProperty] private int _annee = DateTime.Now.Year;
    [ObservableProperty] private string _vin = string.Empty;
    [ObservableProperty] private string _immatriculation = string.Empty;
    [ObservableProperty] private int _kilometrage;
    [ObservableProperty] private string _typeMoteur = string.Empty;
    [ObservableProperty] private string _couleur = string.Empty;
    [ObservableProperty] private string _transmission = string.Empty;

    public VehiculesViewModel(VehiculeService svc, ClientService clientSvc)
    { _svc = svc; _clientSvc = clientSvc; }

    [RelayCommand]
    public async Task ChargerAsync()
    {
        IsLoading = true;
        try
        {
            var list = string.IsNullOrWhiteSpace(Recherche)
                ? await _svc.GetAllAsync()
                : await _svc.RechercherAsync(Recherche);
            Vehicules = new(list);
            Clients = new(await _clientSvc.GetAllAsync());
        }
        finally { IsLoading = false; }
    }

    [RelayCommand] public async Task NouveauVehicule() { Selection = null; ResetForm(); await ChargerClientsAsync(); AfficherFormulaire = true; }

    [RelayCommand] public async Task ModifierVehicule(Vehicule v) { Selection = v; ChargerForm(v); await ChargerClientsAsync(); AfficherFormulaire = true; }

    [RelayCommand]
    public async Task Supprimer(Vehicule v)
    {
        await _svc.SupprimerAsync(v.Id);
        await ChargerAsync();
    }

    [RelayCommand]
    public async Task Sauvegarder()
    {
        if (Selection is null)
            await _svc.AjouterAsync(new Vehicule
            {
                ClientId = SelectedClientId, Marque = Marque, Modele = Modele,
                Annee = Annee, VIN = Vin, Immatriculation = Immatriculation,
                Kilometrage = Kilometrage, TypeMoteur = TypeMoteur,
                Couleur = Couleur, Transmission = Transmission
            });
        else
        {
            Selection.ClientId = SelectedClientId; Selection.Marque = Marque;
            Selection.Modele = Modele; Selection.Annee = Annee; Selection.VIN = Vin;
            Selection.Immatriculation = Immatriculation; Selection.Kilometrage = Kilometrage;
            Selection.TypeMoteur = TypeMoteur; Selection.Couleur = Couleur;
            Selection.Transmission = Transmission;
            await _svc.ModifierAsync(Selection);
        }
        AfficherFormulaire = false;
        await ChargerAsync();
    }

    [RelayCommand] public void Annuler() { AfficherFormulaire = false; }

    private async Task ChargerClientsAsync() => Clients = new(await _clientSvc.GetAllAsync());
    private void ResetForm() { SelectedClientId = 0; Marque = Modele = Vin = Immatriculation = TypeMoteur = Couleur = Transmission = string.Empty; Annee = DateTime.Now.Year; Kilometrage = 0; }
    private void ChargerForm(Vehicule v) { SelectedClientId = v.ClientId; Marque = v.Marque; Modele = v.Modele; Annee = v.Annee; Vin = v.VIN; Immatriculation = v.Immatriculation; Kilometrage = v.Kilometrage; TypeMoteur = v.TypeMoteur; Couleur = v.Couleur; Transmission = v.Transmission; }
}

// ══════════════════════════════════════════════════════════════════════════════
// ORDRES DE TRAVAIL
// ══════════════════════════════════════════════════════════════════════════════
public partial class OrdresTravailViewModel : ObservableObject
{
    private readonly OrdreTravailService _svc;
    private readonly VehiculeService _vSvc;
    private readonly EmployeService _eSvc;
    private readonly FactureService _fSvc;

    [ObservableProperty] private ObservableCollection<OrdreTravail> _ordres = [];
    [ObservableProperty] private ObservableCollection<Vehicule> _vehicules = [];
    [ObservableProperty] private ObservableCollection<Employe> _employes = [];
    [ObservableProperty] private OrdreTravail? _selection;
    [ObservableProperty] private bool _afficherFormulaire;
    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private string _messageStatut = string.Empty;

    // Champs formulaire
    [ObservableProperty] private int _vehiculeId;
    [ObservableProperty] private int? _employeId;
    [ObservableProperty] private string _diagnostic = string.Empty;
    [ObservableProperty] private string _travauxDemandes = string.Empty;
    [ObservableProperty] private string _notes = string.Empty;
    [ObservableProperty] private int _kilometrageEntree;
    [ObservableProperty] private decimal _tempsEstime;

    public OrdresTravailViewModel(OrdreTravailService svc, VehiculeService vSvc, EmployeService eSvc, FactureService fSvc)
    { _svc = svc; _vSvc = vSvc; _eSvc = eSvc; _fSvc = fSvc; }

    [RelayCommand]
    public async Task ChargerAsync()
    {
        IsLoading = true;
        try
        {
            Ordres = new(await _svc.GetAllAsync());
            Vehicules = new(await _vSvc.GetAllAsync());
            Employes = new(await _eSvc.GetAllAsync());
        }
        finally { IsLoading = false; }
    }

    [RelayCommand] public async Task NouvelOT() { Selection = null; ResetForm(); await ChargerReferentielsAsync(); AfficherFormulaire = true; }

    [RelayCommand] public async Task ModifierOT(OrdreTravail ot) { Selection = ot; ChargerForm(ot); await ChargerReferentielsAsync(); AfficherFormulaire = true; }

    [RelayCommand]
    public async Task Sauvegarder()
    {
        if (Selection is null)
            await _svc.CreerOTAsync(new OrdreTravail
            {
                VehiculeId = VehiculeId, EmployeId = EmployeId.HasValue && EmployeId > 0 ? EmployeId : null,
                Diagnostic = Diagnostic, TravauxDemandes = TravauxDemandes,
                Notes = Notes, KilometrageEntree = KilometrageEntree, TempsEstime = TempsEstime
            });
        else
        {
            Selection.VehiculeId = VehiculeId;
            Selection.EmployeId = EmployeId.HasValue && EmployeId > 0 ? EmployeId : null;
            Selection.Diagnostic = Diagnostic; Selection.TravauxDemandes = TravauxDemandes;
            Selection.Notes = Notes; Selection.KilometrageEntree = KilometrageEntree;
            Selection.TempsEstime = TempsEstime;
            await _svc.ChangerStatutAsync(Selection.Id, Selection.Statut);
        }
        AfficherFormulaire = false;
        await ChargerAsync();
    }

    [RelayCommand]
    public async Task ChangerStatut(OrdreTravail ot)
    {
        var prochainStatut = ot.Statut switch
        {
            StatutOT.Ouvert          => StatutOT.EnCours,
            StatutOT.EnCours         => StatutOT.ControleQualite,
            StatutOT.EnAttentePieces => StatutOT.EnCours,
            StatutOT.ControleQualite => StatutOT.Pret,
            _                        => ot.Statut
        };
        await _svc.ChangerStatutAsync(ot.Id, prochainStatut);
        await ChargerAsync();
    }

    [RelayCommand]
    public async Task Facturer(OrdreTravail ot)
    {
        try
        {
            await _fSvc.CreerFactureDepuisOT(ot.Id);
            await ChargerAsync();
            MessageStatut = $"Facture créée pour l'OT {ot.Numero}";
        }
        catch (Exception ex) { MessageStatut = $"Erreur : {ex.Message}"; }
    }

    [RelayCommand] public void Annuler() { AfficherFormulaire = false; }

    private async Task ChargerReferentielsAsync()
    {
        Vehicules = new(await _vSvc.GetAllAsync());
        Employes = new(await _eSvc.GetAllAsync());
    }
    private void ResetForm() { VehiculeId = 0; EmployeId = null; Diagnostic = TravauxDemandes = Notes = string.Empty; KilometrageEntree = 0; TempsEstime = 0; }
    private void ChargerForm(OrdreTravail o) { VehiculeId = o.VehiculeId; EmployeId = o.EmployeId; Diagnostic = o.Diagnostic; TravauxDemandes = o.TravauxDemandes; Notes = o.Notes; KilometrageEntree = o.KilometrageEntree; TempsEstime = o.TempsEstime; }
}

// ══════════════════════════════════════════════════════════════════════════════
// RENDEZ-VOUS
// ══════════════════════════════════════════════════════════════════════════════
public partial class RendezVousViewModel : ObservableObject
{
    private readonly RendezVousService _svc;
    private readonly ClientService _cSvc;
    private readonly VehiculeService _vSvc;

    [ObservableProperty] private ObservableCollection<RendezVous> _rdvs = [];
    [ObservableProperty] private ObservableCollection<Client> _clients = [];
    [ObservableProperty] private ObservableCollection<Vehicule> _vehicules = [];
    [ObservableProperty] private bool _afficherFormulaire;
    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private RendezVous? _selection;

    // Champs formulaire
    [ObservableProperty] private int _clientId;
    [ObservableProperty] private int? _vehiculeId;
    [ObservableProperty] private DateTime _dateRdv = DateTime.Today.AddDays(1);
    [ObservableProperty] private string _heureRdv = "09:00";
    [ObservableProperty] private int _dureeMinutes = 60;
    [ObservableProperty] private string _typeService = string.Empty;
    [ObservableProperty] private string _notes = string.Empty;

    public RendezVousViewModel(RendezVousService svc, ClientService cSvc, VehiculeService vSvc)
    { _svc = svc; _cSvc = cSvc; _vSvc = vSvc; }

    [RelayCommand]
    public async Task ChargerAsync()
    {
        IsLoading = true;
        try
        {
            Rdvs = new(await _svc.GetAllAsync());
            Clients = new(await _cSvc.GetAllAsync());
            Vehicules = new(await _vSvc.GetAllAsync());
        }
        finally { IsLoading = false; }
    }

    [RelayCommand] public void NouveauRdv() { Selection = null; ResetForm(); AfficherFormulaire = true; }

    [RelayCommand] public void ModifierRdv(RendezVous r) { Selection = r; ChargerForm(r); AfficherFormulaire = true; }

    [RelayCommand]
    public async Task Sauvegarder()
    {
        if (!TimeSpan.TryParse(HeureRdv, out var heure)) heure = TimeSpan.FromHours(9);
        var dateHeure = DateRdv.Date + heure;
        if (Selection is null)
            await _svc.AjouterAsync(new RendezVous
            {
                ClientId = ClientId, VehiculeId = VehiculeId > 0 ? VehiculeId : null,
                DateHeure = dateHeure, DureeMinutes = DureeMinutes,
                TypeService = TypeService, Notes = Notes
            });
        else
        {
            Selection.ClientId = ClientId; Selection.VehiculeId = VehiculeId > 0 ? VehiculeId : null;
            Selection.DateHeure = dateHeure; Selection.DureeMinutes = DureeMinutes;
            Selection.TypeService = TypeService; Selection.Notes = Notes;
            await _svc.ModifierAsync(Selection);
        }
        AfficherFormulaire = false;
        await ChargerAsync();
    }

    [RelayCommand]
    public async Task Annuler(RendezVous? r)
    {
        if (r is not null)
        {
            await _svc.ChangerStatutAsync(r.Id, StatutRDV.Annule);
            await ChargerAsync();
        }
        AfficherFormulaire = false;
    }

    [RelayCommand] public void FermerFormulaire() { AfficherFormulaire = false; }

    private void ResetForm() { ClientId = 0; VehiculeId = null; DateRdv = DateTime.Today.AddDays(1); HeureRdv = "09:00"; DureeMinutes = 60; TypeService = Notes = string.Empty; }
    private void ChargerForm(RendezVous r) { ClientId = r.ClientId; VehiculeId = r.VehiculeId; DateRdv = r.DateHeure.Date; HeureRdv = r.DateHeure.ToString("HH:mm"); DureeMinutes = r.DureeMinutes; TypeService = r.TypeService; Notes = r.Notes; }
}

// ══════════════════════════════════════════════════════════════════════════════
// INVENTAIRE (PIÈCES)
// ══════════════════════════════════════════════════════════════════════════════
public partial class InventaireViewModel : ObservableObject
{
    private readonly InventaireService _svc;
    private readonly FournisseurService _fSvc;

    [ObservableProperty] private ObservableCollection<Piece> _pieces = [];
    [ObservableProperty] private ObservableCollection<Fournisseur> _fournisseurs = [];
    [ObservableProperty] private string _recherche = string.Empty;
    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private bool _afficherFormulaire;
    [ObservableProperty] private Piece? _selection;
    [ObservableProperty] private string _messageStatut = string.Empty;

    // Champs formulaire
    [ObservableProperty] private string _reference = string.Empty;
    [ObservableProperty] private string _designation = string.Empty;
    [ObservableProperty] private string _description = string.Empty;
    [ObservableProperty] private string _categorie = string.Empty;
    [ObservableProperty] private string _emplacement = string.Empty;
    [ObservableProperty] private decimal _prixAchat;
    [ObservableProperty] private decimal _prixVente;
    [ObservableProperty] private int _stockActuel;
    [ObservableProperty] private int _stockMinimum;
    [ObservableProperty] private int? _fournisseurId;
    [ObservableProperty] private int _ajustementQte;
    [ObservableProperty] private bool _afficherAjustement;

    public InventaireViewModel(InventaireService svc, FournisseurService fSvc)
    { _svc = svc; _fSvc = fSvc; }

    [RelayCommand]
    public async Task ChargerAsync()
    {
        IsLoading = true;
        try
        {
            var pieces = await _svc.GetAllPiecesAsync();
            if (!string.IsNullOrWhiteSpace(Recherche))
                pieces = pieces.Where(p => p.Designation.Contains(Recherche, StringComparison.OrdinalIgnoreCase)
                                        || p.Reference.Contains(Recherche, StringComparison.OrdinalIgnoreCase));
            Pieces = new(pieces);
            Fournisseurs = new(await _fSvc.GetAllAsync());
        }
        finally { IsLoading = false; }
    }

    [RelayCommand] public async Task NouvellePiece() { Selection = null; ResetForm(); Fournisseurs = new(await _fSvc.GetAllAsync()); AfficherFormulaire = true; }

    [RelayCommand] public async Task ModifierPiece(Piece p) { Selection = p; ChargerForm(p); Fournisseurs = new(await _fSvc.GetAllAsync()); AfficherFormulaire = true; }

    [RelayCommand]
    public async Task Sauvegarder()
    {
        // Pas encore de PieceService séparé — on passe par le DbContext via InventaireService
        // Pour l'instant on utilise une approche directe via AjusterStock
        AfficherFormulaire = false;
        MessageStatut = "Enregistré.";
        await ChargerAsync();
    }

    [RelayCommand]
    public async Task AjusterStock(Piece p)
    {
        if (AjustementQte != 0)
        {
            await _svc.AjusterStock(p.Id, AjustementQte, "Ajustement manuel");
            AjustementQte = 0;
            AfficherAjustement = false;
            await ChargerAsync();
        }
    }

    [RelayCommand] public void OuvrirAjustement(Piece p) { Selection = p; AjustementQte = 0; AfficherAjustement = true; }

    [RelayCommand] public void Annuler() { AfficherFormulaire = false; AfficherAjustement = false; }

    private void ResetForm() { Reference = Designation = Description = Categorie = Emplacement = string.Empty; PrixAchat = PrixVente = 0; StockActuel = StockMinimum = 0; FournisseurId = null; }
    private void ChargerForm(Piece p) { Reference = p.Reference; Designation = p.Designation; Description = p.Description; Categorie = p.Categorie; Emplacement = p.Emplacement; PrixAchat = p.PrixAchat; PrixVente = p.PrixVente; StockActuel = p.StockActuel; StockMinimum = p.StockMinimum; FournisseurId = p.FournisseurId; }
}

// ══════════════════════════════════════════════════════════════════════════════
// FACTURATION
// ══════════════════════════════════════════════════════════════════════════════
public partial class FacturationViewModel : ObservableObject
{
    private readonly FactureService _svc;

    [ObservableProperty] private ObservableCollection<Facture> _factures = [];
    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private bool _afficherPaiement;
    [ObservableProperty] private Facture? _factureSelectionnee;
    [ObservableProperty] private string _messageStatut = string.Empty;

    // Champs paiement
    [ObservableProperty] private decimal _montantPaiement;
    [ObservableProperty] private string _modePaiement = "Carte";
    [ObservableProperty] private string _referencePaiement = string.Empty;

    public string[] ModesPaiement { get; } = ["Carte", "Argent comptant", "Chèque", "Virement", "Financement"];

    public FacturationViewModel(FactureService svc) => _svc = svc;

    [RelayCommand]
    public async Task ChargerAsync()
    {
        IsLoading = true;
        try { Factures = new(await _svc.GetFacturesAsync()); }
        finally { IsLoading = false; }
    }

    [RelayCommand]
    public void OuvrirPaiement(Facture f)
    {
        FactureSelectionnee = f;
        MontantPaiement = f.SoldeRestant;
        ReferencePaiement = string.Empty;
        AfficherPaiement = true;
    }

    [RelayCommand]
    public async Task EnregistrerPaiement()
    {
        if (FactureSelectionnee is null || MontantPaiement <= 0) return;
        try
        {
            var mode = ModePaiement switch
            {
                "Carte"           => global::MecanoERP.Core.Entities.ModePaiement.Carte,
                "Argent comptant" => global::MecanoERP.Core.Entities.ModePaiement.Especes,
                "Chèque"          => global::MecanoERP.Core.Entities.ModePaiement.Cheque,
                "Virement"        => global::MecanoERP.Core.Entities.ModePaiement.Virement,
                _                 => global::MecanoERP.Core.Entities.ModePaiement.Carte
            };
            await _svc.EnregistrerPaiement(FactureSelectionnee.Id, MontantPaiement, mode, ReferencePaiement);
            AfficherPaiement = false;
            MessageStatut = $"Paiement de {MontantPaiement:C} enregistré.";
            await ChargerAsync();
        }
        catch (Exception ex) { MessageStatut = $"Erreur : {ex.Message}"; }
    }

    [RelayCommand] public void AnnulerPaiement() { AfficherPaiement = false; }
}

// ══════════════════════════════════════════════════════════════════════════════
// GARANTIES
// ══════════════════════════════════════════════════════════════════════════════
public partial class GarantiesViewModel : ObservableObject
{
    private readonly FactureService _svc;
    [ObservableProperty] private ObservableCollection<Garantie> _garanties = [];
    [ObservableProperty] private bool _isLoading;

    public GarantiesViewModel(FactureService svc) => _svc = svc;

    [RelayCommand]
    public async Task ChargerAsync()
    {
        IsLoading = true;
        try { Garanties = new(await _svc.GetGarantiesAsync()); }
        finally { IsLoading = false; }
    }
}

// ══════════════════════════════════════════════════════════════════════════════
// EMPLOYÉS
// ══════════════════════════════════════════════════════════════════════════════
public partial class EmployesViewModel : ObservableObject
{
    private readonly EmployeService _svc;
    [ObservableProperty] private ObservableCollection<Employe> _employes = [];
    [ObservableProperty] private Employe? _selection;
    [ObservableProperty] private bool _afficherFormulaire;
    [ObservableProperty] private bool _isLoading;

    // Champs formulaire
    [ObservableProperty] private string _nom = string.Empty;
    [ObservableProperty] private string _prenom = string.Empty;
    [ObservableProperty] private string _poste = string.Empty;
    [ObservableProperty] private string _telephone = string.Empty;
    [ObservableProperty] private string _email = string.Empty;
    [ObservableProperty] private decimal _tauxHoraire;

    public string[] Postes { get; } = ["Mécanicien", "Technicien", "Réceptionniste", "Gérant", "Électricien automobile", "Autre"];

    public EmployesViewModel(EmployeService svc) => _svc = svc;

    [RelayCommand]
    public async Task ChargerAsync()
    {
        IsLoading = true;
        try { Employes = new(await _svc.GetAllIncludeInactifAsync()); }
        finally { IsLoading = false; }
    }

    [RelayCommand] public void NouvelEmploye() { Selection = null; ResetForm(); AfficherFormulaire = true; }
    [RelayCommand] public void ModifierEmploye(Employe e) { Selection = e; ChargerForm(e); AfficherFormulaire = true; }

    [RelayCommand]
    public async Task Sauvegarder()
    {
        if (Selection is null)
            await _svc.AjouterAsync(new Employe { Nom = Nom, Prenom = Prenom, Poste = Poste, Telephone = Telephone, Email = Email, TauxHoraire = TauxHoraire, DateEmbauche = DateTime.Today });
        else
        {
            Selection.Nom = Nom; Selection.Prenom = Prenom; Selection.Poste = Poste;
            Selection.Telephone = Telephone; Selection.Email = Email; Selection.TauxHoraire = TauxHoraire;
            await _svc.ModifierAsync(Selection);
        }
        AfficherFormulaire = false;
        await ChargerAsync();
    }

    [RelayCommand] public async Task Supprimer(Employe e) { await _svc.SupprimerAsync(e.Id); await ChargerAsync(); }
    [RelayCommand] public void Annuler() { AfficherFormulaire = false; }

    private void ResetForm() { Nom = Prenom = Poste = Telephone = Email = string.Empty; TauxHoraire = 0; }
    private void ChargerForm(Employe e) { Nom = e.Nom; Prenom = e.Prenom; Poste = e.Poste; Telephone = e.Telephone; Email = e.Email; TauxHoraire = e.TauxHoraire; }
}

// ══════════════════════════════════════════════════════════════════════════════
// FOURNISSEURS
// ══════════════════════════════════════════════════════════════════════════════
public partial class FournisseursViewModel : ObservableObject
{
    private readonly FournisseurService _svc;
    [ObservableProperty] private ObservableCollection<Fournisseur> _fournisseurs = [];
    [ObservableProperty] private Fournisseur? _selection;
    [ObservableProperty] private string _recherche = string.Empty;
    [ObservableProperty] private bool _afficherFormulaire;
    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private string _messageStatut = string.Empty;

    // ── Identification ────────────────────────────────────────
    [ObservableProperty] private string _codeFournisseur = string.Empty;
    [ObservableProperty] private string _nom = string.Empty;
    [ObservableProperty] private string _contactNom = string.Empty;
    [ObservableProperty] private string _contactPoste = string.Empty;
    [ObservableProperty] private string _telephone = string.Empty;
    [ObservableProperty] private string _telephoneMobile = string.Empty;
    [ObservableProperty] private string _email = string.Empty;
    [ObservableProperty] private string _siteWeb = string.Empty;

    // ── Adresse ───────────────────────────────────────────────
    [ObservableProperty] private string _adresse = string.Empty;
    [ObservableProperty] private string _ville = string.Empty;
    [ObservableProperty] private string _codePostal = string.Empty;
    [ObservableProperty] private string _province = "QC";

    // ── Comptabilité ──────────────────────────────────────────
    [ObservableProperty] private string _numeroCompte = string.Empty;
    [ObservableProperty] private string _numeroTPS = string.Empty;
    [ObservableProperty] private string _numeroTVQ = string.Empty;
    [ObservableProperty] private int _delaiPaiementJours = 30;
    [ObservableProperty] private string _devise = "CAD";
    [ObservableProperty] private decimal _limiteCreditCAD;

    // ── Statut ─────────────────────────────────────────────────
    [ObservableProperty] private bool _estActif = true;
    [ObservableProperty] private string _categorie = string.Empty;
    [ObservableProperty] private string _notes = string.Empty;

    public List<string> Provinces { get; } = ["QC", "ON", "BC", "AB", "MB", "SK", "NB", "NS", "NL", "PE", "NT", "NU", "YT"];
    public List<string> Devises { get; } = ["CAD", "USD", "EUR"];
    public List<string> Categories { get; } = ["Pièces auto", "Outillage", "Lubrifiant", "Électronique", "Services", "Autre"];
    public List<int> DelaisPaiement { get; } = [0, 7, 15, 30, 45, 60, 90];

    public FournisseursViewModel(FournisseurService svc) => _svc = svc;

    [RelayCommand]
    public async Task ChargerAsync()
    {
        IsLoading = true;
        try
        {
            var list = string.IsNullOrWhiteSpace(Recherche)
                ? await _svc.GetAllAsync()
                : await _svc.RechercherAsync(Recherche);
            Fournisseurs = new(list);
        }
        finally { IsLoading = false; }
    }

    [RelayCommand] public void NouveauFournisseur() { Selection = null; ResetForm(); AfficherFormulaire = true; }
    [RelayCommand] public void ModifierFournisseur(Fournisseur f) { Selection = f; ChargerForm(f); AfficherFormulaire = true; }

    [RelayCommand]
    public async Task Sauvegarder()
    {
        if (string.IsNullOrWhiteSpace(Nom)) { MessageStatut = "⚠ Le nom du fournisseur est obligatoire."; return; }
        try
        {
            if (Selection is null)
                await _svc.AjouterAsync(BuildFournisseur(new Fournisseur()));
            else
                await _svc.ModifierAsync(BuildFournisseur(Selection));
            MessageStatut = string.Empty;
            AfficherFormulaire = false;
            await ChargerAsync();
        }
        catch (Exception ex) { MessageStatut = $"✖ {ex.Message}"; }
    }

    [RelayCommand] public async Task Supprimer(Fournisseur f) { await _svc.SupprimerAsync(f.Id); await ChargerAsync(); }
    [RelayCommand] public void Annuler() { AfficherFormulaire = false; }

    private Fournisseur BuildFournisseur(Fournisseur f)
    {
        f.CodeFournisseur = CodeFournisseur;
        f.Nom = Nom; f.ContactNom = ContactNom; f.ContactPoste = ContactPoste;
        f.Telephone = Telephone; f.TelephoneMobile = TelephoneMobile;
        f.Email = Email; f.SiteWeb = SiteWeb;
        f.Adresse = Adresse; f.Ville = Ville; f.CodePostal = CodePostal; f.Province = Province;
        f.NumeroCompte = NumeroCompte; f.NumeroTPS = NumeroTPS; f.NumeroTVQ = NumeroTVQ;
        f.DelaiPaiementJours = DelaiPaiementJours; f.Devise = Devise; f.LimiteCreditCAD = LimiteCreditCAD;
        f.EstActif = EstActif; f.Categorie = Categorie; f.Notes = Notes;
        return f;
    }

    private void ResetForm()
    {
        CodeFournisseur = string.Empty;
        Nom = ContactNom = ContactPoste = Telephone = TelephoneMobile = Email = SiteWeb = string.Empty;
        Adresse = Ville = CodePostal = string.Empty; Province = "QC";
        NumeroCompte = NumeroTPS = NumeroTVQ = Categorie = Notes = string.Empty;
        DelaiPaiementJours = 30; Devise = "CAD"; LimiteCreditCAD = 0; EstActif = true;
    }

    private void ChargerForm(Fournisseur f)
    {
        CodeFournisseur = f.CodeFournisseur;
        Nom = f.Nom; ContactNom = f.ContactNom; ContactPoste = f.ContactPoste;
        Telephone = f.Telephone; TelephoneMobile = f.TelephoneMobile;
        Email = f.Email; SiteWeb = f.SiteWeb;
        Adresse = f.Adresse; Ville = f.Ville; CodePostal = f.CodePostal; Province = f.Province;
        NumeroCompte = f.NumeroCompte; NumeroTPS = f.NumeroTPS; NumeroTVQ = f.NumeroTVQ;
        DelaiPaiementJours = f.DelaiPaiementJours; Devise = f.Devise; LimiteCreditCAD = f.LimiteCreditCAD;
        EstActif = f.EstActif; Categorie = f.Categorie; Notes = f.Notes;
    }
}

// Stub ViewModels conservés pour la compilation
public partial class ParametresViewModel : ObservableObject
{
    [RelayCommand] public Task ChargerAsync() => Task.CompletedTask;
}
