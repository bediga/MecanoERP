using MecanoERP.WPF.ViewModels; using System.Windows.Controls;
namespace MecanoERP.WPF.Views;
public partial class GestionARView : UserControl
{
    public GestionARView(GestionARViewModel vm) { InitializeComponent(); DataContext = vm; }
}
