using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MecanoERP.Core.Entities;
using MecanoERP.Infrastructure.Services;
using System.Collections.ObjectModel;

namespace MecanoERP.WPF.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    private readonly DashboardService _svc;

    // ── KPIs ────────────────────────────────────────────────────────────────
    [ObservableProperty] private decimal _chiffreAffaireJour;
    [ObservableProperty] private decimal _chiffreAffaireMois;
    [ObservableProperty] private int _otEnCours;
    [ObservableProperty] private int _otPret;
    [ObservableProperty] private int _otAttentePieces;
    [ObservableProperty] private int _piecesStockCritique;
    [ObservableProperty] private int _rdvAujourdhui;
    [ObservableProperty] private int _nombreClients;
    [ObservableProperty] private int _facturesImpayees;
    [ObservableProperty] private bool _isLoading;

    // ── Listes ──────────────────────────────────────────────────────────────
    [ObservableProperty] private ObservableCollection<OrdreTravail> _otsActifs = [];
    [ObservableProperty] private ObservableCollection<RendezVous> _rdvDuJour = [];
    [ObservableProperty] private ObservableCollection<Piece> _piecesAlertes = [];
    [ObservableProperty] private ObservableCollection<Facture> _facturesRecentes = [];

    // ── Date/heure ──────────────────────────────────────────────────────────
    public string DateAffichage => DateTime.Now.ToString("dddd d MMMM yyyy", new System.Globalization.CultureInfo("fr-CA"));
    public string HeureAffichage => DateTime.Now.ToString("HH:mm");

    public DashboardViewModel(DashboardService svc) => _svc = svc;

    [RelayCommand]
    public async Task ChargerAsync()
    {
        IsLoading = true;
        try
        {
            // KPIs en parallèle
            await Task.WhenAll(
                Task.Run(async () => ChiffreAffaireJour   = await _svc.GetCAJourAsync()),
                Task.Run(async () => ChiffreAffaireMois   = await _svc.GetCAMoisAsync()),
                Task.Run(async () => OtEnCours            = await _svc.GetOTEnCoursAsync()),
                Task.Run(async () => OtPret               = await _svc.GetOTPretAsync()),
                Task.Run(async () => OtAttentePieces      = await _svc.GetOTAttentePiecesAsync()),
                Task.Run(async () => PiecesStockCritique  = await _svc.GetStockCritiqueCountAsync()),
                Task.Run(async () => RdvAujourdhui        = await _svc.GetRdvAujourdhuiAsync()),
                Task.Run(async () => NombreClients        = await _svc.GetNombreClientsAsync()),
                Task.Run(async () => FacturesImpayees     = await _svc.GetFacturesImpayeesAsync())
            );

            // Listes
            OtsActifs        = new(await _svc.GetOTsActifsAsync());
            RdvDuJour        = new(await _svc.GetRdvDuJourAsync());
            PiecesAlertes    = new(await _svc.GetPiecesStockCritiqueAsync());
            FacturesRecentes = new(await _svc.GetFacturesRecentsAsync());
        }
        finally { IsLoading = false; }
    }
}
