using MecanoERP.WPF.ViewModels; using System.Windows.Controls;
namespace MecanoERP.WPF.Views;
public partial class GestionAPView : UserControl
{
    public GestionAPView(GestionAPViewModel vm) { InitializeComponent(); DataContext = vm; }
}
