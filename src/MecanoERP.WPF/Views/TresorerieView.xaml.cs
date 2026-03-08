using MecanoERP.WPF.ViewModels; using System.Windows.Controls;
namespace MecanoERP.WPF.Views;
public partial class TresorerieView : UserControl
{
    public TresorerieView(TresorerieViewModel vm) { InitializeComponent(); DataContext = vm; }
}
