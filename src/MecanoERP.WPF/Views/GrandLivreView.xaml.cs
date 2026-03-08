using MecanoERP.WPF.ViewModels;
using System.Windows.Controls;
namespace MecanoERP.WPF.Views;
public partial class GrandLivreView : UserControl
{
    private readonly GrandLivreViewModel _vm;
    public GrandLivreView(GrandLivreViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        DataContext = vm;
        Loaded += async (_, _) => await vm.ChargerAsync();
    }
}
