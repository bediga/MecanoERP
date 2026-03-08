using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MecanoERP.Core.Entities;
using MecanoERP.Infrastructure.Services;
using System.Collections.ObjectModel;

namespace MecanoERP.WPF.ViewModels;

// ══════════════════════════════════════════════════════════════════════════════
// DEVIS
// ══════════════════════════════════════════════════════════════════════════════
public partial class DevisViewModel : ObservableObject
{
    private readonly DevisService _svc;
    private readonly ClientService _clientSvc;
    private readonly VehiculeService _vehiculeSvc;
    private readonly OrdreTravailService _otSvc;

    [ObservableProperty] private ObservableCollection<Devis> _devisList = [];
    [ObservableProperty] private ObservableCollection<Client> _clients = [];
    [ObservableProperty] private ObservableCollection<Vehicule> _vehicules = [];
    [ObservableProperty] private Devis? _selection;
    [ObservableProperty] private bool _afficherFormulaire;
    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private string _messageStatut = string.Empty;

    // Formulaire
    [ObservableProperty] private int _clientId;
    [ObservableProperty] private int? _vehiculeId;
    [ObservableProperty] private DateTime _dateValidite = DateTime.Today.AddDays(30);
    [ObservableProperty] private string _notes = string.Empty;
    [ObservableProperty] private ObservableCollection<LigneDevis> _lignes = [];

    public DevisViewModel(DevisService svc, ClientService cs, VehiculeService vs, OrdreTravailService ots)
    { _svc = svc; _clientSvc = cs; _vehiculeSvc = vs; _otSvc = ots; }

    [RelayCommand]
    public async Task ChargerAsync()
    {
        IsLoading = true;
        try
        {
            DevisList = new(await _svc.GetAllAsync());
            Clients = new(await _clientSvc.GetAllAsync());
            Vehicules = new(await _vehiculeSvc.GetAllAsync());
        }
        finally { IsLoading = false; }
    }

    [RelayCommand]
    public void NouveauDevis()
    {
        Selection = null;
        ClientId = 0; VehiculeId = null; Notes = string.Empty;
        DateValidite = DateTime.Today.AddDays(30);
        Lignes = [];
        AfficherFormulaire = true;
    }

    [RelayCommand]
    public void ModifierDevis(Devis d)
    {
        Selection = d;
        ClientId = d.ClientId; VehiculeId = d.VehiculeId;
        DateValidite = d.DateValidite; Notes = d.Notes;
        Lignes = new(d.Lignes);
        AfficherFormulaire = true;
    }

    [RelayCommand]
    public async Task SauvegarderAsync()
    {
        try
        {
            if (Selection == null)
            {
                var d = new Devis { ClientId = ClientId, VehiculeId = VehiculeId, DateValidite = DateValidite, Notes = Notes };
                await _svc.CreerDevisAsync(d);
                MessageStatut = $"✔ Devis {d.Numero} créé.";
            }
            else
            {
                Selection.ClientId = ClientId; Selection.VehiculeId = VehiculeId;
                Selection.DateValidite = DateValidite; Selection.Notes = Notes;
                await _svc.ModifierAsync(Selection);
                MessageStatut = "✔ Devis mis à jour.";
            }
            AfficherFormulaire = false;
            await ChargerAsync();
        }
        catch (Exception ex) { MessageStatut = $"✖ {ex.Message}"; }
    }

    [RelayCommand]
    public async Task ConvertirEnOTAsync(Devis d)
    {
        try
        {
            IsLoading = true;
            var ot = await _svc.ConvertirEnOTAsync(d.Id, _otSvc);
            MessageStatut = $"✔ OT {ot.Numero} créé depuis devis {d.Numero}.";
            await ChargerAsync();
        }
        catch (Exception ex) { MessageStatut = $"✖ {ex.Message}"; }
        finally { IsLoading = false; }
    }

    [RelayCommand] public async Task SupprimerAsync(Devis d)
    {
        await _svc.SupprimerAsync(d.Id);
        await ChargerAsync();
    }

    [RelayCommand] public void Annuler() { AfficherFormulaire = false; }
}

// ══════════════════════════════════════════════════════════════════════════════
// ACHATS (Demandes + BC)
// ══════════════════════════════════════════════════════════════════════════════
public partial class AchatsViewModel : ObservableObject
{
    private readonly AchatService _svc;
    private readonly InventaireService _invSvc;
    private readonly EmployeService _empSvc;

    [ObservableProperty] private ObservableCollection<DemandeAchat> _demandes = [];
    [ObservableProperty] private ObservableCollection<Piece> _pieces = [];
    [ObservableProperty] private ObservableCollection<Employe> _employes = [];
    [ObservableProperty] private DemandeAchat? _selection;
    [ObservableProperty] private bool _afficherFormulaire;
    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private string _messageStatut = string.Empty;

    // Formulaire
    [ObservableProperty] private int? _demandeurId;
    [ObservableProperty] private string _notes = string.Empty;

    public AchatsViewModel(AchatService svc, InventaireService inv, EmployeService emp)
    { _svc = svc; _invSvc = inv; _empSvc = emp; }

    [RelayCommand]
    public async Task ChargerAsync()
    {
        IsLoading = true;
        try
        {
            Demandes = new(await _svc.GetDemandesAsync());
            Pieces = new(await _invSvc.GetAllPiecesAsync());
            Employes = new(await _empSvc.GetAllAsync());
        }
        finally { IsLoading = false; }
    }

    [RelayCommand]
    public void NouvelleDemande()
    {
        Selection = null;
        DemandeurId = null; Notes = string.Empty;
        AfficherFormulaire = true;
    }

    [RelayCommand]
    public async Task SauvegarderAsync()
    {
        try
        {
            var d = new DemandeAchat { DemandeurId = DemandeurId, Notes = Notes };
            await _svc.CreerDemandeAsync(d);
            MessageStatut = $"✔ Demande {d.Numero} créée.";
            AfficherFormulaire = false;
            await ChargerAsync();
        }
        catch (Exception ex) { MessageStatut = $"✖ {ex.Message}"; }
    }

    [RelayCommand]
    public async Task ApprouverAsync(DemandeAchat d)
    {
        try
        {
            await _svc.ApprouverDemandeAsync(d.Id);
            MessageStatut = $"✔ Demande {d.Numero} approuvée.";
            await ChargerAsync();
        }
        catch (Exception ex) { MessageStatut = $"✖ {ex.Message}"; }
    }

    [RelayCommand]
    public async Task CreerBCAsync(DemandeAchat d)
    {
        try
        {
            var bc = await _svc.CreerBCDepuisDemandeAsync(d.Id);
            MessageStatut = $"✔ BC {bc.Numero} créé depuis demande {d.Numero}.";
            await ChargerAsync();
        }
        catch (Exception ex) { MessageStatut = $"✖ {ex.Message}"; }
    }

    [RelayCommand] public async Task SupprimerAsync(DemandeAchat d)
    {
        await _svc.SupprimerDemandeAsync(d.Id);
        await ChargerAsync();
    }

    [RelayCommand] public void Annuler() { AfficherFormulaire = false; }
}

// ══════════════════════════════════════════════════════════════════════════════
// FACTURES FOURNISSEURS (AP)
// ══════════════════════════════════════════════════════════════════════════════
public partial class FacturesFournisseursViewModel : ObservableObject
{
    private readonly FactureFournisseurService _svc;
    private readonly FournisseurService _fSvc;

    [ObservableProperty] private ObservableCollection<FactureFournisseur> _factures = [];
    [ObservableProperty] private ObservableCollection<Fournisseur> _fournisseurs = [];
    [ObservableProperty] private FactureFournisseur? _selection;
    [ObservableProperty] private bool _afficherFormulaire;
    [ObservableProperty] private bool _afficherPaiement;
    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private string _messageStatut = string.Empty;

    // Formulaire
    [ObservableProperty] private int _fournisseurId;
    [ObservableProperty] private string _numeroFournisseur = string.Empty;
    [ObservableProperty] private decimal _montantHT;
    [ObservableProperty] private DateTime _dateEcheance = DateTime.Today.AddDays(30);

    // Paiement
    [ObservableProperty] private decimal _montantPaiement;
    [ObservableProperty] private string _modePaiement = "Virement";
    [ObservableProperty] private string _referencePaiement = string.Empty;
    public List<string> ModesPaiement { get; } = ["Virement", "Chèque", "Carte", "Espèces"];

    public FacturesFournisseursViewModel(FactureFournisseurService svc, FournisseurService fs)
    { _svc = svc; _fSvc = fs; }

    [RelayCommand]
    public async Task ChargerAsync()
    {
        IsLoading = true;
        try
        {
            Factures = new(await _svc.GetAllAsync());
            Fournisseurs = new(await _fSvc.GetAllAsync());
        }
        finally { IsLoading = false; }
    }

    [RelayCommand] public void NouvelleFacture() { Selection = null; FournisseurId = 0; NumeroFournisseur = ""; MontantHT = 0; DateEcheance = DateTime.Today.AddDays(30); AfficherFormulaire = true; }

    [RelayCommand]
    public async Task SauvegarderAsync()
    {
        try
        {
            var f = new FactureFournisseur { FournisseurId = FournisseurId, NumeroFournisseur = NumeroFournisseur, MontantHT = MontantHT, DateEcheance = DateEcheance };
            await _svc.AjouterAsync(f);
            MessageStatut = $"✔ Facture {f.Numero} enregistrée.";
            AfficherFormulaire = false;
            await ChargerAsync();
        }
        catch (Exception ex) { MessageStatut = $"✖ {ex.Message}"; }
    }

    [RelayCommand]
    public void OuvrirPaiement(FactureFournisseur f)
    {
        Selection = f;
        MontantPaiement = f.SoldeRestant;
        ModePaiement = "Virement";
        ReferencePaiement = "";
        AfficherPaiement = true;
    }

    [RelayCommand]
    public async Task EnregistrerPaiementAsync()
    {
        if (Selection == null) return;
        try
        {
            var mode = ModePaiement switch
            {
                "Chèque"  => Core.Entities.ModePaiement.Cheque,
                "Carte"   => Core.Entities.ModePaiement.Carte,
                "Espèces" => Core.Entities.ModePaiement.Especes,
                _         => Core.Entities.ModePaiement.Virement
            };
            await _svc.EnregistrerPaiementAsync(Selection.Id, MontantPaiement, mode, ReferencePaiement);
            MessageStatut = $"✔ Paiement de {MontantPaiement:C} enregistré.";
            AfficherPaiement = false;
            await ChargerAsync();
        }
        catch (Exception ex) { MessageStatut = $"✖ {ex.Message}"; }
    }

    [RelayCommand] public void AnnulerPaiement() { AfficherPaiement = false; }
    [RelayCommand] public void Annuler() { AfficherFormulaire = false; }
}

// ══════════════════════════════════════════════════════════════════════════════
// AVOIRS CLIENTS
// ══════════════════════════════════════════════════════════════════════════════
public partial class AvoirsViewModel : ObservableObject
{
    private readonly AvoirService _svc;
    private readonly ClientService _clientSvc;
    private readonly FactureService _factureSvc;

    [ObservableProperty] private ObservableCollection<AvoirClient> _avoirs = [];
    [ObservableProperty] private ObservableCollection<Client> _clients = [];
    [ObservableProperty] private ObservableCollection<Facture> _factures = [];
    [ObservableProperty] private bool _afficherFormulaire;
    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private string _messageStatut = string.Empty;

    // Formulaire
    [ObservableProperty] private int _clientId;
    [ObservableProperty] private int? _factureOrigineId;
    [ObservableProperty] private decimal _montant;
    [ObservableProperty] private string _motif = string.Empty;

    public AvoirsViewModel(AvoirService svc, ClientService cs, FactureService fs)
    { _svc = svc; _clientSvc = cs; _factureSvc = fs; }

    [RelayCommand]
    public async Task ChargerAsync()
    {
        IsLoading = true;
        try
        {
            Avoirs = new(await _svc.GetAllAsync());
            Clients = new(await _clientSvc.GetAllAsync());
            Factures = new(await _factureSvc.GetFacturesAsync());
        }
        finally { IsLoading = false; }
    }

    [RelayCommand] public void NouvelAvoir() { ClientId = 0; FactureOrigineId = null; Montant = 0; Motif = ""; AfficherFormulaire = true; }

    [RelayCommand]
    public async Task SauvegarderAsync()
    {
        try
        {
            var a = await _svc.CreerAvoirAsync(ClientId, FactureOrigineId, Montant, Motif);
            MessageStatut = $"✔ Avoir {a.Numero} créé ({Montant:C}).";
            AfficherFormulaire = false;
            await ChargerAsync();
        }
        catch (Exception ex) { MessageStatut = $"✖ {ex.Message}"; }
    }

    [RelayCommand] public void Annuler() { AfficherFormulaire = false; }
}

// ══════════════════════════════════════════════════════════════════════════════
// BANQUE
// ══════════════════════════════════════════════════════════════════════════════
public partial class BanqueViewModel : ObservableObject
{
    private readonly BanqueService _svc;

    [ObservableProperty] private ObservableCollection<CompteBancaire> _comptes = [];
    [ObservableProperty] private CompteBancaire? _compteSelectionne;
    [ObservableProperty] private ObservableCollection<TransactionBancaire> _transactions = [];
    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private string _messageStatut = string.Empty;
    [ObservableProperty] private bool _afficherNouveauCompte;
    [ObservableProperty] private bool _afficherNouvelleTransaction;

    // Formulaire nouveau compte
    [ObservableProperty] private string _nomCompte = string.Empty;
    [ObservableProperty] private string _numeroCompte = string.Empty;
    [ObservableProperty] private string _institution = string.Empty;
    [ObservableProperty] private decimal _soldeOuverture;

    // Formulaire nouvelle transaction
    [ObservableProperty] private decimal _montantTx;
    [ObservableProperty] private string _descriptionTx = string.Empty;
    [ObservableProperty] private string _typeTx = "Crédit";
    [ObservableProperty] private string _referenceTx = string.Empty;
    [ObservableProperty] private decimal _debitsSolde;
    [ObservableProperty] private decimal _creditsSolde;
    [ObservableProperty] private decimal _soldeCourant;
    public List<string> TypesTransaction { get; } = ["Crédit", "Débit"];

    public BanqueViewModel(BanqueService svc) { _svc = svc; }

    [RelayCommand]
    public async Task ChargerAsync()
    {
        IsLoading = true;
        try { Comptes = new(await _svc.GetComptesAsync()); }
        finally { IsLoading = false; }
    }

    [RelayCommand]
    public async Task SelectionnerCompteAsync(CompteBancaire c)
    {
        CompteSelectionne = c;
        IsLoading = true;
        try
        {
            Transactions = new(await _svc.GetTransactionsAsync(c.Id));
            var (debits, credits, solde) = await _svc.GetSoldeAsync(c.Id);
            DebitsSolde = debits; CreditsSolde = credits; SoldeCourant = solde;
        }
        finally { IsLoading = false; }
    }

    [RelayCommand]
    public async Task SauvegarderCompteAsync()
    {
        try
        {
            var c = new CompteBancaire { Nom = NomCompte, Numero = NumeroCompte, Institution = Institution, SoldeOuverture = SoldeOuverture, SoldeCourant = SoldeOuverture };
            await _svc.AjouterCompteAsync(c);
            MessageStatut = $"✔ Compte {NomCompte} créé.";
            AfficherNouveauCompte = false;
            await ChargerAsync();
        }
        catch (Exception ex) { MessageStatut = $"✖ {ex.Message}"; }
    }

    [RelayCommand]
    public async Task AjouterTransactionAsync()
    {
        if (CompteSelectionne == null) return;
        try
        {
            var type = TypeTx == "Crédit" ? TypeTransactionBancaire.Credit : TypeTransactionBancaire.Debit;
            var tx = new TransactionBancaire { CompteBancaireId = CompteSelectionne.Id, Montant = MontantTx, Description = DescriptionTx, TypeTx = type, Reference = ReferenceTx, Date = DateTime.UtcNow };
            await _svc.AjouterTransactionAsync(tx);
            MessageStatut = $"✔ Transaction enregistrée.";
            AfficherNouvelleTransaction = false;
            await SelectionnerCompteAsync(CompteSelectionne);
        }
        catch (Exception ex) { MessageStatut = $"✖ {ex.Message}"; }
    }

    [RelayCommand]
    public async Task RapprocherAsync(TransactionBancaire tx)
    {
        await _svc.RapprocherAsync(tx.Id);
        if (CompteSelectionne != null) await SelectionnerCompteAsync(CompteSelectionne);
    }

    [RelayCommand] public void OuvrirNouveauCompte() { NomCompte = ""; NumeroCompte = ""; Institution = ""; SoldeOuverture = 0; AfficherNouveauCompte = true; }
    [RelayCommand] public void OuvrirNouvelleTransaction() { MontantTx = 0; DescriptionTx = ""; TypeTx = "Crédit"; ReferenceTx = ""; AfficherNouvelleTransaction = true; }
    [RelayCommand] public void Annuler() { AfficherNouveauCompte = false; AfficherNouvelleTransaction = false; }
}
