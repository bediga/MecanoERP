using MecanoERP.WPF.ViewModels;
using System.Windows.Controls;
namespace MecanoERP.WPF.Views;
public partial class BalanceVerificationView : UserControl
{
    public BalanceVerificationView(BalanceVerificationViewModel vm)
    { InitializeComponent(); DataContext = vm; }
}
