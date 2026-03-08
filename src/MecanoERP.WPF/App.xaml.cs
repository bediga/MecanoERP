using MecanoERP.Infrastructure.Data;
using MecanoERP.Infrastructure.Services;
using MecanoERP.Infrastructure.Services.Comptabilite;
using MecanoERP.Infrastructure.Services.Securite;
using MecanoERP.WPF.ViewModels;
using MecanoERP.WPF.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Windows;

namespace MecanoERP.WPF;

public partial class App : Application
{
    private IHost _host = null!;

    protected override async void OnStartup(StartupEventArgs e)
    {
        _host = Host.CreateDefaultBuilder()
            .ConfigureServices((_, services) =>
            {
                var cs = "Host=localhost;Port=5432;Database=Mecano;Username=gisebs;Password=Goodeeg!12";

                // Base de données
                services.AddDbContext<MecanoDbContext>(o => o.UseNpgsql(cs), ServiceLifetime.Transient);

                // Sécurité
                services.AddSingleton<SessionService>();
                services.AddTransient<AuthService>();
                services.AddTransient<AutorisationService>();

                // Services métier
                services.AddTransient<DashboardService>();
                services.AddTransient<ClientService>();
                services.AddTransient<VehiculeService>();
                services.AddTransient<OrdreTravailService>();
                services.AddTransient<InventaireService>();
                services.AddTransient<FactureService>();
                services.AddTransient<RendezVousService>();
                services.AddTransient<NotificationService>();
                services.AddTransient<FournisseurService>();
                services.AddTransient<EmployeService>();
                // Services Phase 1 — Finance, Achats & Ventes
                services.AddTransient<DevisService>();
                services.AddTransient<AchatService>();
                services.AddTransient<FactureFournisseurService>();
                services.AddTransient<AvoirService>();
                services.AddTransient<BanqueService>();
                // Services Vague A — OT avancé, Check-in, Audit, RH
                services.AddTransient<AuditService>();
                services.AddTransient<CheckInService>();
                services.AddTransient<PointageService>();
                // Services comptabilité
                services.AddTransient<PlanComptableService>();
                services.AddTransient<TaxeService>();
                services.AddTransient<JournalService>();
                services.AddTransient<RapportComptableService>();
                // Services GL avancés
                services.AddTransient<GrandLivreService>();
                services.AddTransient<EtatsFinanciersService>();
                services.AddTransient<CloturePeriodeService>();
                // Services AR / AP / Trésorerie
                services.AddTransient<ARService>();
                services.AddTransient<APService>();
                services.AddTransient<TresorerieService>();

                // ViewModels
                services.AddTransient<LoginViewModel>();
                services.AddTransient<DashboardViewModel>();
                services.AddTransient<ClientsViewModel>();
                services.AddTransient<VehiculesViewModel>();
                services.AddTransient<OrdresTravailViewModel>();
                services.AddTransient<RendezVousViewModel>();
                services.AddTransient<InventaireViewModel>();
                services.AddTransient<FacturationViewModel>();
                services.AddTransient<GarantiesViewModel>();
                services.AddTransient<ComptabiliteViewModel>();
                services.AddTransient<PlanComptableViewModel>();
                services.AddTransient<JournauxViewModel>();
                services.AddTransient<ConfigTaxesViewModel>();
                services.AddTransient<RapportsComptablesViewModel>();
                // ViewModels GL avancés
                services.AddTransient<GrandLivreViewModel>();
                services.AddTransient<BalanceVerificationViewModel>();
                services.AddTransient<EtatsFinanciersViewModel>();
                services.AddTransient<CloturePeriodeViewModel>();
                // ViewModels AR / AP / Trésorerie
                services.AddTransient<GestionARViewModel>();
                services.AddTransient<GestionAPViewModel>();
                services.AddTransient<TresorerieViewModel>();
                services.AddTransient<EmployesViewModel>();
                services.AddTransient<FournisseursViewModel>();
                services.AddTransient<UtilisateursViewModel>();
                services.AddTransient<ParametresViewModel>();
                // ViewModels Phase 1
                services.AddTransient<DevisViewModel>();
                services.AddTransient<AchatsViewModel>();
                services.AddTransient<FacturesFournisseursViewModel>();
                services.AddTransient<AvoirsViewModel>();
                services.AddTransient<BanqueViewModel>();
                // ViewModels Vague A
                services.AddTransient<CheckInViewModel>();
                services.AddTransient<AuditViewModel>();
                services.AddTransient<PointageViewModel>();

                // Fenêtres & vues
                services.AddTransient<LoginWindow>();
                services.AddTransient<Views.MainWindow>();
                services.AddTransient<DashboardView>();
                services.AddTransient<ClientsView>();
                services.AddTransient<VehiculesView>();
                services.AddTransient<OrdresTravailView>();
                services.AddTransient<RendezVousView>();
                services.AddTransient<InventaireView>();
                services.AddTransient<FacturationView>();
                services.AddTransient<GarantiesView>();
                services.AddTransient<ComptabiliteView>();
                services.AddTransient<PlanComptableView>();
                services.AddTransient<JournauxView>();
                services.AddTransient<ConfigTaxesView>();
                services.AddTransient<RapportsComptablesView>();
                // Vues GL avancées
                services.AddTransient<GrandLivreView>();
                services.AddTransient<BalanceVerificationView>();
                services.AddTransient<EtatsFinanciersView>();
                services.AddTransient<CloturePeriodeView>();
                // Vues AR / AP / Trésorerie
                services.AddTransient<GestionARView>();
                services.AddTransient<GestionAPView>();
                services.AddTransient<TresorerieView>();
                services.AddTransient<EmployesView>();
                services.AddTransient<UtilisateursView>();
                services.AddTransient<ParametresView>();
                services.AddTransient<FournisseursView>();
                // Views Phase 1
                services.AddTransient<DevisView>();
                services.AddTransient<AchatsView>();
                services.AddTransient<FacturesFournisseursView>();
                services.AddTransient<AvoirsView>();
                services.AddTransient<BanqueView>();
                // Views Vague A
                services.AddTransient<CheckInView>();
                services.AddTransient<AuditView>();
                services.AddTransient<PointageView>();
            })
            .Build();

        await _host.StartAsync();
        _host.Services.GetRequiredService<LoginWindow>().Show();
        base.OnStartup(e);
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        await _host.StopAsync();
        _host.Dispose();
        base.OnExit(e);
    }

    public static IServiceProvider Services =>
        ((App)Current)._host.Services;
}
