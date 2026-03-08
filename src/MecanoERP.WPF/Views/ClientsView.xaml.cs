using System.Windows.Controls;
namespace MecanoERP.WPF.Views;
public partial class ClientsView : UserControl { public ClientsView(ViewModels.ClientsViewModel vm) { InitializeComponent(); DataContext = vm; } }
