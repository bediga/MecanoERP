using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MecanoERP.Infrastructure.Services.Comptabilite;
using System.Collections.ObjectModel;

namespace MecanoERP.WPF.ViewModels;

// ══════════════════════════════════════════════════════════════════════════════
// GRAND LIVRE
// ══════════════════════════════════════════════════════════════════════════════
public partial class GrandLivreViewModel : ObservableObject
{
    private readonly GrandLivreService _svc;

    [ObservableProperty] private ObservableCollection<MecanoERP.Core.Entities.Comptabilite.CompteGL> _comptes = [];
    [ObservableProperty] private MecanoERP.Core.Entities.Comptabilite.CompteGL? _compteSelectionne;
    [ObservableProperty] private DateTime _dateDebut = new(DateTime.Now.Year, 1, 1);
    [ObservableProperty] private DateTime _dateFin   = DateTime.Now;
    [ObservableProperty] private ObservableCollection<LigneGrandLivre> _lignes = [];
    [ObservableProperty] private decimal _soldeOuverture;
    [ObservableProperty] private decimal _totalDebit;
    [ObservableProperty] private decimal _totalCredit;
    [ObservableProperty] private decimal _soldeFinal;
    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private string _nomCompte = "";

    public GrandLivreViewModel(GrandLivreService svc) => _svc = svc;

    [RelayCommand]
    public async Task ChargerAsync()
    {
        IsLoading = true;
        try { Comptes = new(await _svc.GetComptesActifsAsync()); }
        finally { IsLoading = false; }
    }

    [RelayCommand]
    public async Task RechercherAsync()
    {
        if (CompteSelectionne is null) return;
        IsLoading = true;
        try
        {
            var gl = await _svc.GetGrandLivreAsync(CompteSelectionne.Id, DateDebut, DateFin);
            NomCompte      = $"{gl.Compte.Numero} — {gl.Compte.Nom}";
            SoldeOuverture = gl.SoldeOuverture;
            TotalDebit     = gl.TotalDebit;
            TotalCredit    = gl.TotalCredit;
            SoldeFinal     = gl.SoldeFinal;
            Lignes         = new(gl.Lignes);
        }
        finally { IsLoading = false; }
    }
}

// ══════════════════════════════════════════════════════════════════════════════
// BALANCE DE VÉRIFICATION
// ══════════════════════════════════════════════════════════════════════════════
public partial class BalanceVerificationViewModel : ObservableObject
{
    private readonly GrandLivreService _svc;

    [ObservableProperty] private int _exercice = DateTime.Now.Year;
    [ObservableProperty] private int? _periode;
    [ObservableProperty] private ObservableCollection<LigneBalance> _lignes = [];
    [ObservableProperty] private decimal _totalDebit;
    [ObservableProperty] private decimal _totalCredit;
    [ObservableProperty] private bool _estEquilibre;
    [ObservableProperty] private bool _isLoading;

    public List<int?> Periodes { get; } = [null, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12];

    public BalanceVerificationViewModel(GrandLivreService svc) => _svc = svc;

    [RelayCommand]
    public async Task ChargerAsync()
    {
        IsLoading = true;
        try
        {
            var balance = await _svc.GetBalanceVerificationAsync(Exercice, Periode);
            Lignes       = new(balance.Lignes);
            TotalDebit   = balance.TotalDebit;
            TotalCredit  = balance.TotalCredit;
            EstEquilibre = balance.EstEquilibre;
        }
        finally { IsLoading = false; }
    }
}

// ══════════════════════════════════════════════════════════════════════════════
// ÉTATS FINANCIERS
// ══════════════════════════════════════════════════════════════════════════════
public partial class EtatsFinanciersViewModel : ObservableObject
{
    private readonly EtatsFinanciersService _svc;

    // Bilan
    [ObservableProperty] private DateTime _dateBilan = DateTime.Now;
    [ObservableProperty] private ObservableCollection<PosteFinancier> _postesActif    = [];
    [ObservableProperty] private ObservableCollection<PosteFinancier> _postesPassif   = [];
    [ObservableProperty] private ObservableCollection<PosteFinancier> _postesCapital  = [];
    [ObservableProperty] private decimal _totalActif;
    [ObservableProperty] private decimal _totalPassif;
    [ObservableProperty] private decimal _totalCapital;
    [ObservableProperty] private bool _bilanEquilibre;

    // État des résultats
    [ObservableProperty] private DateTime _debutResultats = new(DateTime.Now.Year, 1, 1);
    [ObservableProperty] private DateTime _finResultats   = DateTime.Now;
    [ObservableProperty] private ObservableCollection<PosteFinancier> _postesRevenus  = [];
    [ObservableProperty] private ObservableCollection<PosteFinancier> _postesCharges  = [];
    [ObservableProperty] private decimal _totalRevenus;
    [ObservableProperty] private decimal _totalCharges;
    [ObservableProperty] private decimal _beneficeNet;

    [ObservableProperty] private bool _isLoading;

    public EtatsFinanciersViewModel(EtatsFinanciersService svc) => _svc = svc;

    [RelayCommand]
    public async Task ChargerBilanAsync()
    {
        IsLoading = true;
        try
        {
            var bilan   = await _svc.GetBilanAsync(DateBilan);
            PostesActif   = new(bilan.PostesActif);
            PostesPassif  = new(bilan.PostesPassif);
            PostesCapital = new(bilan.PostesCapital);
            TotalActif    = bilan.TotalActif;
            TotalPassif   = bilan.TotalPassif;
            TotalCapital  = bilan.TotalCapital;
            BilanEquilibre = bilan.EstEquilibre;
        }
        finally { IsLoading = false; }
    }

    [RelayCommand]
    public async Task ChargerResultatsAsync()
    {
        IsLoading = true;
        try
        {
            var er     = await _svc.GetEtatResultatsAsync(DebutResultats, FinResultats);
            PostesRevenus = new(er.PostesRevenus);
            PostesCharges = new(er.PostesCharges);
            TotalRevenus  = er.TotalRevenus;
            TotalCharges  = er.TotalCharges;
            BeneficeNet   = er.BeneficeNet;
        }
        finally { IsLoading = false; }
    }
}

// ══════════════════════════════════════════════════════════════════════════════
// CLÔTURE DES PÉRIODES
// ══════════════════════════════════════════════════════════════════════════════
public partial class CloturePeriodeViewModel : ObservableObject
{
    private readonly CloturePeriodeService _svc;

    [ObservableProperty] private int _exercice = DateTime.Now.Year;
    [ObservableProperty] private ObservableCollection<MecanoERP.Core.Entities.Comptabilite.PeriodeComptable> _periodes = [];
    [ObservableProperty] private MecanoERP.Core.Entities.Comptabilite.PeriodeComptable? _periodeSelectionnee;
    [ObservableProperty] private string _message = "";
    [ObservableProperty] private bool _isLoading;

    public CloturePeriodeViewModel(CloturePeriodeService svc) => _svc = svc;

    [RelayCommand]
    public async Task ChargerAsync()
    {
        IsLoading = true;
        try { Periodes = new(await _svc.GetPeriodesAsync(Exercice)); }
        finally { IsLoading = false; }
    }

    [RelayCommand]
    public async Task InitialiserExerciceAsync()
    {
        IsLoading = true;
        try
        {
            await _svc.InitialiserExerciceAsync(Exercice);
            await ChargerAsync();
            Message = $"✅ 12 périodes créées pour {Exercice}.";
        }
        catch (Exception ex) { Message = $"✖ {ex.Message}"; }
        finally { IsLoading = false; }
    }

    [RelayCommand]
    public async Task FermerPeriodeAsync()
    {
        if (PeriodeSelectionnee is null) return;
        IsLoading = true;
        try
        {
            var res = await _svc.FermerPeriodeAsync(
                PeriodeSelectionnee.Exercice,
                PeriodeSelectionnee.Periode,
                userId: 1);
            Message = res.Message;
            if (res.Succes) await ChargerAsync();
        }
        catch (Exception ex) { Message = $"✖ {ex.Message}"; }
        finally { IsLoading = false; }
    }
}
