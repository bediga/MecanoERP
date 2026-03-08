using System.Windows.Controls;
namespace MecanoERP.WPF.Views;
public partial class DashboardView : UserControl { public DashboardView(ViewModels.DashboardViewModel vm) { InitializeComponent(); DataContext = vm; } }
