using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MecanoERP.Core.Entities;
using MecanoERP.Infrastructure.Services;
using System.Collections.ObjectModel;

namespace MecanoERP.WPF.ViewModels;

// ══════════════════════════════════════════════════════════════════════════════
// AR — Comptes Clients
// ══════════════════════════════════════════════════════════════════════════════
public partial class GestionARViewModel : ObservableObject
{
    private readonly ARService      _ar;
    private readonly FactureService _factureService;

    // KPIs
    [ObservableProperty] private decimal _totalAR;
    [ObservableProperty] private decimal _echuesToday;
    [ObservableProperty] private decimal _enSouffrance90;
    [ObservableProperty] private decimal _tauxRecouvrement;
    [ObservableProperty] private int     _nombreEnRetard;

    // Aging
    [ObservableProperty] private ObservableCollection<LigneAgingAR> _lignesAging = [];

    // Clients en retard (relances)
    [ObservableProperty] private ObservableCollection<ClientEnRetard> _clientsEnRetard = [];
    [ObservableProperty] private ClientEnRetard?                       _clientSelectionne;

    // Encaissement rapide
    [ObservableProperty] private ObservableCollection<Facture> _factures = [];
    [ObservableProperty] private Facture?   _factureSelectionnee;
    [ObservableProperty] private decimal    _montantEncaissement;
    [ObservableProperty] private ModePaiement _modeEncaissement = ModePaiement.Virement;
    [ObservableProperty] private string     _referenceEncaissement = "";

    [ObservableProperty] private bool   _isLoading;
    [ObservableProperty] private string _message = "";

    public IEnumerable<ModePaiement> ModesPaiement
        => Enum.GetValues<ModePaiement>();

    public GestionARViewModel(ARService ar, FactureService factureService)
    {
        _ar             = ar;
        _factureService = factureService;
    }

    [RelayCommand]
    public async Task ChargerAsync()
    {
        IsLoading = true;
        try
        {
            var kpis = await _ar.GetKpisAsync();
            TotalAR            = kpis.TotalAR;
            EchuesToday        = kpis.EchuesToday;
            EnSouffrance90     = kpis.EnSouffrance90;
            TauxRecouvrement   = kpis.TauxRecouvrement;
            NombreEnRetard     = kpis.NombreEnRetard;

            LignesAging      = new(await _ar.GetAgingARAsync());
            ClientsEnRetard  = new(await _ar.GetClientsEnRetardAsync());

            var factures = await _factureService.GetFacturesAsync();
            Factures = new(factures.Where(f => f.Statut == StatutFacture.Emise
                                            || f.Statut == StatutFacture.PartiellementPayee));
        }
        finally { IsLoading = false; }
    }

    [RelayCommand]
    public async Task EncaisserAsync()
    {
        if (FactureSelectionnee is null || MontantEncaissement <= 0)
        {
            Message = "⚠ Sélectionnez une facture et saisissez un montant.";
            return;
        }
        IsLoading = true;
        try
        {
            await _ar.EncaisserAsync(FactureSelectionnee.Id, MontantEncaissement,
                ModeEncaissement, ReferenceEncaissement);
            Message = $"✅ Encaissement de {MontantEncaissement:C} enregistré.";
            MontantEncaissement    = 0;
            ReferenceEncaissement  = "";
            FactureSelectionnee    = null;
            await ChargerAsync();
        }
        catch (Exception ex) { Message = $"✖ {ex.Message}"; }
        finally { IsLoading = false; }
    }
}

// ══════════════════════════════════════════════════════════════════════════════
// AP — Comptes Fournisseurs
// ══════════════════════════════════════════════════════════════════════════════
public partial class GestionAPViewModel : ObservableObject
{
    private readonly APService _ap;

    // KPIs
    [ObservableProperty] private decimal _totalAP;
    [ObservableProperty] private decimal _aPayerAujourdhui;
    [ObservableProperty] private decimal _aPayerSemaine;
    [ObservableProperty] private decimal _enSouffrance;
    [ObservableProperty] private int     _nombreEnAttente;

    // Aging
    [ObservableProperty] private ObservableCollection<LigneAgingAP> _lignesAging = [];

    // Échéancier
    [ObservableProperty] private ObservableCollection<EcheancierAP> _echeancier = [];

    // Sélection pour paiement en lot
    [ObservableProperty] private ObservableCollection<FactureFournisseurSelectable> _facturesSelectables = [];
    [ObservableProperty] private string     _message = "";
    [ObservableProperty] private bool       _isLoading;
    [ObservableProperty] private ModePaiement _modePaiementLot = ModePaiement.Virement;

    public IEnumerable<ModePaiement> ModesPaiement => Enum.GetValues<ModePaiement>();

    private readonly FactureFournisseurService _ffSvc;

    public GestionAPViewModel(APService ap, FactureFournisseurService ffSvc)
    {
        _ap    = ap;
        _ffSvc = ffSvc;
    }

    [RelayCommand]
    public async Task ChargerAsync()
    {
        IsLoading = true;
        try
        {
            var kpis = await _ap.GetKpisAsync();
            TotalAP          = kpis.TotalAP;
            APayerAujourdhui = kpis.APayerAujourdhui;
            APayerSemaine    = kpis.APayerSemaine;
            EnSouffrance     = kpis.EnSouffrance;
            NombreEnAttente  = kpis.NombreEnAttente;

            LignesAging  = new(await _ap.GetAgingAPAsync());
            Echeancier   = new(await _ap.GetEcheancierAsync(45));

            var factures = await _ffSvc.GetAllAsync();
            FacturesSelectables = new(factures
                .Where(f => f.Statut != StatutFactureFournisseur.Payee
                         && f.Statut != StatutFactureFournisseur.Annulee)
                .Select(f => new FactureFournisseurSelectable { Facture = f }));
        }
        finally { IsLoading = false; }
    }

    [RelayCommand]
    public async Task PayerSelectionAsync()
    {
        var selection = FacturesSelectables.Where(f => f.EstSelectionnee).ToList();
        if (!selection.Any()) { Message = "⚠ Sélectionnez au moins une facture."; return; }

        IsLoading = true;
        try
        {
            var res = await _ap.PayerEnLotAsync(selection.Select(f => f.Facture.Id), ModePaiementLot);
            Message = res.Message;
            await ChargerAsync();
        }
        catch (Exception ex) { Message = $"✖ {ex.Message}"; }
        finally { IsLoading = false; }
    }

    [RelayCommand] public void ToutSelectionner()
    {
        foreach (var f in FacturesSelectables) f.EstSelectionnee = true;
    }
    [RelayCommand] public void ToutDeselectionner()
    {
        foreach (var f in FacturesSelectables) f.EstSelectionnee = false;
    }
}

public partial class FactureFournisseurSelectable : ObservableObject
{
    [ObservableProperty] private bool _estSelectionnee;
    public FactureFournisseur Facture { get; set; } = null!;
    public string Libelle => $"{Facture.Numero} — {Facture.Fournisseur?.Nom} ({(Facture.MontantTTC - Facture.MontantPaye):C})";
}

// ══════════════════════════════════════════════════════════════════════════════
// TRÉSORERIE & BANQUE
// ══════════════════════════════════════════════════════════════════════════════
public partial class TresorerieViewModel : ObservableObject
{
    private readonly TresorerieService _svc;

    [ObservableProperty] private ObservableCollection<CompteBancaire>      _comptes         = [];
    [ObservableProperty] private CompteBancaire?                            _compteSelectionne;
    [ObservableProperty] private ObservableCollection<TransactionBancaire>  _transactions     = [];
    [ObservableProperty] private ObservableCollection<PrevisionTresorerie>  _previsions       = [];
    [ObservableProperty] private SoldeRapprochement?                        _soldeRapproché;

    [ObservableProperty] private DateTime _dateDebut     = new(DateTime.Now.Year, DateTime.Now.Month, 1);
    [ObservableProperty] private DateTime _dateFin       = DateTime.Now;
    [ObservableProperty] private string   _csvContent    = "";
    [ObservableProperty] private bool     _isLoading;
    [ObservableProperty] private string   _message       = "";

    // Totaux
    [ObservableProperty] private decimal _totalEncaissementsPrevu;
    [ObservableProperty] private decimal _totalDecaissementsPrevu;
    [ObservableProperty] private decimal _fluxNetPrevu;

    public TresorerieViewModel(TresorerieService svc) => _svc = svc;

    [RelayCommand]
    public async Task ChargerAsync()
    {
        IsLoading = true;
        try
        {
            Comptes = new(await _svc.GetComptesAsync());
            if (CompteSelectionne is null && Comptes.Any())
                CompteSelectionne = Comptes.First();
            if (CompteSelectionne is not null)
                await ChargerCompteAsync();

            await ChargerPrevisionsAsync();
        }
        finally { IsLoading = false; }
    }

    [RelayCommand]
    public async Task ChargerCompteAsync()
    {
        if (CompteSelectionne is null) return;
        Transactions  = new(await _svc.GetTransactionsAsync(CompteSelectionne.Id, DateDebut, DateFin));
        SoldeRapproché = await _svc.GetSoldeRapprochementAsync(CompteSelectionne.Id);
    }

    [RelayCommand]
    public async Task RapprocherAsync(TransactionBancaire tx)
    {
        await _svc.RapprocherAsync(tx.Id);
        await ChargerCompteAsync();
    }

    [RelayCommand]
    public async Task AutoRapprocherAsync()
    {
        if (CompteSelectionne is null) return;
        IsLoading = true;
        try
        {
            var n = await _svc.AutoRapprocherAsync(CompteSelectionne.Id, DateDebut, DateFin);
            Message = $"✅ {n} transaction(s) rapprochée(s) automatiquement.";
            await ChargerCompteAsync();
        }
        finally { IsLoading = false; }
    }

    [RelayCommand]
    public async Task ImporterRelevéAsync()
    {
        if (CompteSelectionne is null || string.IsNullOrWhiteSpace(CsvContent))
        { Message = "⚠ Sélectionnez un compte et collez le contenu CSV."; return; }
        IsLoading = true;
        try
        {
            var res = await _svc.ImporterRelevéAsync(CompteSelectionne.Id, CsvContent);
            Message   = res.Message;
            CsvContent = "";
            await ChargerCompteAsync();
        }
        catch (Exception ex) { Message = $"✖ {ex.Message}"; }
        finally { IsLoading = false; }
    }

    private async Task ChargerPrevisionsAsync()
    {
        var previsions = (await _svc.GetPrevisionsAsync(60)).ToList();
        Previsions = new(previsions);
        TotalEncaissementsPrevu  = previsions.Where(p => p.Type == TypeFlux.Encaissement).Sum(p => p.Montant);
        TotalDecaissementsPrevu  = previsions.Where(p => p.Type == TypeFlux.Decaissement).Sum(p => p.Montant);
        FluxNetPrevu             = TotalEncaissementsPrevu - TotalDecaissementsPrevu;
    }
}
