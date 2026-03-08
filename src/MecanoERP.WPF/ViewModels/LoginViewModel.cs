using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MecanoERP.Infrastructure.Services.Securite;

namespace MecanoERP.WPF.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly AuthService _auth;
    private readonly SessionService _session;

    [ObservableProperty] private string _nomUtilisateur = string.Empty;
    [ObservableProperty] private string _messageErreur  = string.Empty;
    [ObservableProperty] private bool   _isLoading;

    public string MotDePasse { get; set; } = string.Empty; // géré via PasswordBox dans le code-behind

    public Action? OnLoginSuccess { get; set; }

    public LoginViewModel(AuthService auth, SessionService session)
    {
        _auth    = auth;
        _session = session;
    }

    [RelayCommand]
    public async Task ConnecterAsync()
    {
        if (string.IsNullOrWhiteSpace(NomUtilisateur) || string.IsNullOrWhiteSpace(MotDePasse))
        {
            MessageErreur = "Veuillez saisir votre identifiant et mot de passe.";
            return;
        }
        IsLoading     = true;
        MessageErreur = string.Empty;
        try
        {
            var session = await _auth.ConnecterAsync(NomUtilisateur, MotDePasse);
            if (session is null)
            {
                MessageErreur = "Identifiant ou mot de passe incorrect.";
                return;
            }
            _session.OuvrirSession(session);
            OnLoginSuccess?.Invoke();
        }
        catch (Exception ex)
        {
            MessageErreur = $"Erreur : {ex.Message}";
        }
        finally { IsLoading = false; }
    }
}
