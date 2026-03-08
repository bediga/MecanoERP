using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MecanoERP.Core.Entities;
using MecanoERP.Infrastructure.Services;
using System.Collections.ObjectModel;
using System.Text.Json;

namespace MecanoERP.WPF.ViewModels;

// ══════════════════════════════════════════════════════════════════════════════
// CHECK-IN VÉHICULE
// ══════════════════════════════════════════════════════════════════════════════
public partial class CheckInViewModel : ObservableObject
{
    private readonly CheckInService _svc;
    private readonly OrdreTravailService _otSvc;
    private readonly EmployeService _empSvc;

    [ObservableProperty] private ObservableCollection<OrdreTravail> _ordres = [];
    [ObservableProperty] private ObservableCollection<Employe> _employes = [];
    [ObservableProperty] private OrdreTravail? _otSelectionne;
    [ObservableProperty] private FicheEntree? _ficheActive;
    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private string _messageStatut = string.Empty;

    // Formulaire fiche
    [ObservableProperty] private int _niveauCarburant = 4;
    [ObservableProperty] private string _observations = string.Empty;
    [ObservableProperty] private int? _employeReceptionId;

    // Checklist (clé → coché)
    [ObservableProperty] private ObservableCollection<ChecklistItem> _checklist = [];

    public CheckInViewModel(CheckInService svc, OrdreTravailService otSvc, EmployeService empSvc)
    { _svc = svc; _otSvc = otSvc; _empSvc = empSvc; }

    [RelayCommand]
    public async Task ChargerAsync()
    {
        IsLoading = true;
        try
        {
            var ordres = await _otSvc.GetAllAsync();
            Ordres = new(ordres.Where(o => o.Statut == StatutOT.Ouvert || o.Statut == StatutOT.EnCours));
            Employes = new(await _empSvc.GetAllAsync());
        }
        finally { IsLoading = false; }
    }

    [RelayCommand]
    public async Task OuvrirFicheAsync(OrdreTravail ot)
    {
        OtSelectionne = ot;
        IsLoading = true;
        try
        {
            FicheActive = await _svc.GetByOTAsync(ot.Id)
                       ?? await _svc.CreerFicheAsync(ot.Id, EmployeReceptionId);

            Observations = FicheActive.Observations;
            NiveauCarburant = FicheActive.NiveauCarburant;

            // Désérialiser checklist
            var dict = JsonSerializer.Deserialize<Dictionary<string, bool>>(
                string.IsNullOrEmpty(FicheActive.ChecklistJson) ? "{}" : FicheActive.ChecklistJson)
                ?? [];
            Checklist = new(dict.Select(kv => new ChecklistItem { Libelle = kv.Key, Coche = kv.Value }));
        }
        finally { IsLoading = false; }
    }

    [RelayCommand]
    public async Task SauvegarderFicheAsync()
    {
        if (FicheActive == null) return;
        try
        {
            var dict = Checklist.ToDictionary(c => c.Libelle, c => c.Coche);
            await _svc.MettreAJourChecklistAsync(FicheActive.Id, dict);
            await _svc.AjouterObservationAsync(FicheActive.Id, Observations, NiveauCarburant);
            MessageStatut = "✔ Fiche d'entrée sauvegardée.";
        }
        catch (Exception ex) { MessageStatut = $"✖ {ex.Message}"; }
    }
}

public partial class ChecklistItem : ObservableObject
{
    [ObservableProperty] private string _libelle = string.Empty;
    [ObservableProperty] private bool _coche;
}

// ══════════════════════════════════════════════════════════════════════════════
// AUDIT LOG
// ══════════════════════════════════════════════════════════════════════════════
public partial class AuditViewModel : ObservableObject
{
    private readonly AuditService _svc;

    [ObservableProperty] private ObservableCollection<AuditLog> _logs = [];
    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private string _filtreModule = string.Empty;
    [ObservableProperty] private DateTime _dateDebut = DateTime.Today.AddDays(-30);
    [ObservableProperty] private DateTime _dateFin = DateTime.Today;

    public List<string> Modules { get; } =
        ["", "OT", "Facture", "Client", "Inventaire", "Achat", "Dévis", "Banque"];

    public AuditViewModel(AuditService svc) { _svc = svc; }

    [RelayCommand]
    public async Task ChargerAsync()
    {
        IsLoading = true;
        try
        {
            var module = string.IsNullOrWhiteSpace(FiltreModule) ? null : FiltreModule;
            var logs = await _svc.GetLogsAsync(module, DateDebut, DateFin.AddHours(23));
            Logs = new(logs);
        }
        finally { IsLoading = false; }
    }
}

// ══════════════════════════════════════════════════════════════════════════════
// POINTAGE TECHNICIENS
// ══════════════════════════════════════════════════════════════════════════════
public partial class PointageViewModel : ObservableObject
{
    private readonly PointageService _svc;
    private readonly EmployeService _empSvc;
    private readonly OrdreTravailService _otSvc;

    [ObservableProperty] private ObservableCollection<Employe> _employes = [];
    [ObservableProperty] private ObservableCollection<Pointage> _pointagesAujourdhui = [];
    [ObservableProperty] private ObservableCollection<OrdreTravail> _ordresEnCours = [];
    [ObservableProperty] private Employe? _employeSelectionne;
    [ObservableProperty] private Pointage? _pointageOuvert;
    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private string _messageStatut = string.Empty;
    [ObservableProperty] private double _heuresTravaillees;
    [ObservableProperty] private double _heuresFacturables;
    [ObservableProperty] private double _tauxProductivite;

    // Formulaire pointage
    [ObservableProperty] private int? _otId;
    [ObservableProperty] private string _typePointage = "Travail";
    public List<string> TypesPointage { get; } = ["Travail", "Pause", "Formation", "Congé"];

    public PointageViewModel(PointageService svc, EmployeService emp, OrdreTravailService ots)
    { _svc = svc; _empSvc = emp; _otSvc = ots; }

    [RelayCommand]
    public async Task ChargerAsync()
    {
        IsLoading = true;
        try
        {
            Employes = new(await _empSvc.GetAllAsync());
            PointagesAujourdhui = new(await _svc.GetAllAujourdhuiAsync());
            var ordres = await _otSvc.GetAllAsync();
            OrdresEnCours = new(ordres.Where(o => o.Statut == StatutOT.EnCours));
        }
        finally { IsLoading = false; }
    }

    [RelayCommand]
    public async Task SelectionnerEmployeAsync(Employe e)
    {
        EmployeSelectionne = e;
        PointageOuvert = await _svc.GetPointageOuvertAsync(e.Id);
        var (h, hf, taux) = await _svc.GetProductiviteAsync(e.Id, DateTime.Today, DateTime.Today.AddHours(23));
        HeuresTravaillees = Math.Round(h, 2);
        HeuresFacturables = Math.Round(hf, 2);
        TauxProductivite = taux;
    }

    [RelayCommand]
    public async Task PointerEntreeAsync()
    {
        if (EmployeSelectionne == null) return;
        try
        {
            await _svc.PointerEntreeAsync(EmployeSelectionne.Id, OtId, TypePointage);
            MessageStatut = $"✔ Entrée pointée pour {EmployeSelectionne.Nom}.";
            await SelectionnerEmployeAsync(EmployeSelectionne);
            await ChargerAsync();
        }
        catch (Exception ex) { MessageStatut = $"✖ {ex.Message}"; }
    }

    [RelayCommand]
    public async Task PointerSortieAsync()
    {
        if (EmployeSelectionne == null) return;
        try
        {
            await _svc.PointerSortieAsync(EmployeSelectionne.Id);
            MessageStatut = $"✔ Sortie pointée pour {EmployeSelectionne.Nom}.";
            PointageOuvert = null;
            await SelectionnerEmployeAsync(EmployeSelectionne);
            await ChargerAsync();
        }
        catch (Exception ex) { MessageStatut = $"✖ {ex.Message}"; }
    }
}
