using MecanoERP.WPF.ViewModels;
using System.Windows.Controls;
namespace MecanoERP.WPF.Views;
public partial class EtatsFinanciersView : UserControl
{
    public EtatsFinanciersView(EtatsFinanciersViewModel vm)
    { InitializeComponent(); DataContext = vm; }
}
