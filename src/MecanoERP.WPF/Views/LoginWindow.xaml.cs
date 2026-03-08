using MecanoERP.WPF.ViewModels;
using MecanoERP.WPF.Views;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Input;

namespace MecanoERP.WPF.Views;

public partial class LoginWindow : Window
{
    private readonly LoginViewModel _vm;

    public LoginWindow(LoginViewModel vm)
    {
        InitializeComponent();
        DataContext = _vm = vm;

        _vm.OnLoginSuccess = () => Dispatcher.Invoke(() =>
        {
            var main = App.Services.GetRequiredService<MainWindow>();
            main.Show();
            Close();
        });
    }

    private void OnPasswordChanged(object sender, RoutedEventArgs e)
        => _vm.MotDePasse = txtPwd.Password;

    private void OnTitleBarDrag(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed) DragMove();
    }

    private void OnClose(object sender, RoutedEventArgs e) => Close();
}
