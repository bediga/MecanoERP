using MecanoERP.Infrastructure.Services.Securite;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Controls;

namespace MecanoERP.WPF.Views;

public partial class MainWindow : Window
{
    private readonly IServiceProvider _services;
    private Button? _activeBtn;

    public MainWindow(IServiceProvider services, SessionService session)
    {
        InitializeComponent();
        _services = services;
        lblUser.Text = session.Courant?.NomComplet ?? session.Courant?.NomUtilisateur ?? "";
        Navigate("Dashboard", btnDashboard);
    }

    private void OnNav(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn)
            Navigate(btn.Tag?.ToString() ?? "Dashboard", btn);
    }

    private void OnDeconnexion(object sender, RoutedEventArgs e)
    {
        _services.GetRequiredService<SessionService>().FermerSession();
        var login = App.Services.GetRequiredService<LoginWindow>();
        login.Show();
        Close();
    }

    private void Navigate(string tag, Button btn)
    {
        // Style précédent
        if (_activeBtn is not null)
            _activeBtn.Style = (Style)FindResource("NavBtn");
        _activeBtn = btn;
        btn.Style = (Style)FindResource("NavBtnActive");

        // Résoudre la vue
        UserControl? view = tag switch
        {
            "Dashboard"             => _services.GetRequiredService<DashboardView>(),
            "Clients"               => _services.GetRequiredService<ClientsView>(),
            "Vehicules"             => _services.GetRequiredService<VehiculesView>(),
            "OrdresTravail"         => _services.GetRequiredService<OrdresTravailView>(),
            "RendezVous"            => _services.GetRequiredService<RendezVousView>(),
            "Inventaire"            => _services.GetRequiredService<InventaireView>(),
            "Facturation"           => _services.GetRequiredService<FacturationView>(),
            "Garanties"             => _services.GetRequiredService<GarantiesView>(),
            "Comptabilite"          => _services.GetRequiredService<ComptabiliteView>(),
            "PlanComptable"         => _services.GetRequiredService<PlanComptableView>(),
            "Journaux"              => _services.GetRequiredService<JournauxView>(),
            "ConfigTaxes"           => _services.GetRequiredService<ConfigTaxesView>(),
            "Rapports"              => _services.GetRequiredService<RapportsComptablesView>(),
            "Employes"              => _services.GetRequiredService<EmployesView>(),
            "Fournisseurs"          => _services.GetRequiredService<FournisseursView>(),
            "Utilisateurs"          => _services.GetRequiredService<UtilisateursView>(),
            "Parametres"            => _services.GetRequiredService<ParametresView>(),
            // Phase 1
            "Devis"                 => _services.GetRequiredService<DevisView>(),
            "Achats"                => _services.GetRequiredService<AchatsView>(),
            "FacturesFournisseurs"  => _services.GetRequiredService<FacturesFournisseursView>(),
            "Avoirs"                => _services.GetRequiredService<AvoirsView>(),
            "Banque"                => _services.GetRequiredService<BanqueView>(),
            // Vague A
            "CheckIn"               => _services.GetRequiredService<CheckInView>(),
            "Pointage"              => _services.GetRequiredService<PointageView>(),
            "Audit"                 => _services.GetRequiredService<AuditView>(),
            // GL avancé
            "GrandLivre"            => _services.GetRequiredService<GrandLivreView>(),
            "BalanceVerification"   => _services.GetRequiredService<BalanceVerificationView>(),
            "EtatsFinanciers"       => _services.GetRequiredService<EtatsFinanciersView>(),
            "CloturePeriode"        => _services.GetRequiredService<CloturePeriodeView>(),
            // AR / AP / Trésorerie
            "GestionAR"             => _services.GetRequiredService<GestionARView>(),
            "GestionAP"             => _services.GetRequiredService<GestionAPView>(),
            "Tresorerie"            => _services.GetRequiredService<TresorerieView>(),
            _                       => _services.GetRequiredService<DashboardView>()
        };
        ContentArea.Content = view;

        // Déclencher le chargement
        if (view?.DataContext is not null)
        {
            var m = view.DataContext.GetType().GetMethod("ChargerAsync");
            if (m is not null) _ = (System.Threading.Tasks.Task?)m.Invoke(view.DataContext, null);
        }
    }
}
