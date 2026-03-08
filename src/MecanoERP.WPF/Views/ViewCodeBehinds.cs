using System.Windows.Controls;
namespace MecanoERP.WPF.Views;

public partial class VehiculesView           : UserControl { public VehiculesView           (ViewModels.VehiculesViewModel             vm) { InitializeComponent(); DataContext = vm; } }
public partial class OrdresTravailView       : UserControl { public OrdresTravailView       (ViewModels.OrdresTravailViewModel         vm) { InitializeComponent(); DataContext = vm; } }
public partial class RendezVousView          : UserControl { public RendezVousView          (ViewModels.RendezVousViewModel            vm) { InitializeComponent(); DataContext = vm; } }
public partial class InventaireView          : UserControl { public InventaireView          (ViewModels.InventaireViewModel            vm) { InitializeComponent(); DataContext = vm; } }
public partial class FacturationView         : UserControl { public FacturationView         (ViewModels.FacturationViewModel           vm) { InitializeComponent(); DataContext = vm; } }
public partial class GarantiesView           : UserControl { public GarantiesView           (ViewModels.GarantiesViewModel             vm) { InitializeComponent(); DataContext = vm; } }
public partial class ComptabiliteView        : UserControl { public ComptabiliteView        (ViewModels.ComptabiliteViewModel          vm) { InitializeComponent(); DataContext = vm; } }
public partial class PlanComptableView       : UserControl { public PlanComptableView       (ViewModels.PlanComptableViewModel         vm) { InitializeComponent(); DataContext = vm; } }
public partial class JournauxView            : UserControl { public JournauxView            (ViewModels.JournauxViewModel              vm) { InitializeComponent(); DataContext = vm; } }
public partial class ConfigTaxesView         : UserControl { public ConfigTaxesView         (ViewModels.ConfigTaxesViewModel           vm) { InitializeComponent(); DataContext = vm; } }
public partial class RapportsComptablesView  : UserControl { public RapportsComptablesView  (ViewModels.RapportsComptablesViewModel    vm) { InitializeComponent(); DataContext = vm; } }
public partial class EmployesView            : UserControl { public EmployesView            (ViewModels.EmployesViewModel              vm) { InitializeComponent(); DataContext = vm; } }
public partial class UtilisateursView        : UserControl { public UtilisateursView        (ViewModels.UtilisateursViewModel          vm) { InitializeComponent(); DataContext = vm; } }
public partial class ParametresView          : UserControl { public ParametresView          (ViewModels.ParametresViewModel            vm) { InitializeComponent(); DataContext = vm; } }
public partial class FournisseursView        : UserControl { public FournisseursView        (ViewModels.FournisseursViewModel          vm) { InitializeComponent(); DataContext = vm; } }

// Phase 1 — Finance, Achats & Ventes
public partial class DevisView               : UserControl { public DevisView               (ViewModels.DevisViewModel                vm) { InitializeComponent(); DataContext = vm; } }
public partial class AchatsView              : UserControl { public AchatsView              (ViewModels.AchatsViewModel               vm) { InitializeComponent(); DataContext = vm; } }
public partial class FacturesFournisseursView : UserControl { public FacturesFournisseursView(ViewModels.FacturesFournisseursViewModel vm) { InitializeComponent(); DataContext = vm; } }
public partial class AvoirsView              : UserControl { public AvoirsView              (ViewModels.AvoirsViewModel               vm) { InitializeComponent(); DataContext = vm; } }
public partial class BanqueView              : UserControl
{
    public BanqueView(ViewModels.BanqueViewModel vm) { InitializeComponent(); DataContext = vm; }
    private void CompteSelectionne_Selected(object sender, System.Windows.RoutedEventArgs e)
    {
        if (DataContext is ViewModels.BanqueViewModel vm && sender is System.Windows.Controls.ListBoxItem item)
            _ = vm.SelectionnerCompteCommand.ExecuteAsync(item.Content);
    }
}

// Vague A — OT avancé, Check-in, Audit, RH
public partial class CheckInView  : UserControl
{
    public CheckInView(ViewModels.CheckInViewModel vm) { InitializeComponent(); DataContext = vm; }
    private void OT_Selected(object sender, System.Windows.RoutedEventArgs e)
    {
        if (DataContext is ViewModels.CheckInViewModel vm && sender is System.Windows.Controls.ListBoxItem item)
            _ = vm.OuvrirFicheCommand.ExecuteAsync(item.Content);
    }
}
public partial class AuditView    : UserControl { public AuditView   (ViewModels.AuditViewModel    vm) { InitializeComponent(); DataContext = vm; } }
public partial class PointageView : UserControl
{
    public PointageView(ViewModels.PointageViewModel vm) { InitializeComponent(); DataContext = vm; }
    private void Technicien_Selected(object sender, System.Windows.RoutedEventArgs e)
    {
        if (DataContext is ViewModels.PointageViewModel vm && sender is System.Windows.Controls.ListBoxItem item)
            _ = vm.SelectionnerEmployeCommand.ExecuteAsync(item.Content);
    }
}
