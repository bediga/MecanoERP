using MecanoERP.WPF.ViewModels;
using System.Windows.Controls;
namespace MecanoERP.WPF.Views;
public partial class CloturePeriodeView : UserControl
{
    public CloturePeriodeView(CloturePeriodeViewModel vm)
    { InitializeComponent(); DataContext = vm; }
}
