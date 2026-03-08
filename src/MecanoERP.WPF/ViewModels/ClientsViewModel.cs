using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MecanoERP.Core.Entities;
using MecanoERP.Infrastructure.Services;
using System.Collections.ObjectModel;

namespace MecanoERP.WPF.ViewModels;

public partial class ClientsViewModel : ObservableObject
{
    private readonly ClientService _svc;
    [ObservableProperty] private ObservableCollection<Client> _clients = [];
    [ObservableProperty] private Client? _selection;
    [ObservableProperty] private string _recherche = string.Empty;
    [ObservableProperty] private bool _afficherFormulaire;
    [ObservableProperty] private bool _isLoading;

    // Champs formulaire
    [ObservableProperty] private string _nom = string.Empty;
    [ObservableProperty] private string _prenom = string.Empty;
    [ObservableProperty] private string _telephone = string.Empty;
    [ObservableProperty] private string _email = string.Empty;
    [ObservableProperty] private string _adresse = string.Empty;
    [ObservableProperty] private string _ville = string.Empty;

    public ClientsViewModel(ClientService svc) => _svc = svc;

    [RelayCommand] public async Task ChargerAsync()
    {
        IsLoading = true;
        try { Clients = new(await _svc.RechercherAsync(Recherche)); }
        finally { IsLoading = false; }
    }

    [RelayCommand] public void NouveauClient() { Selection = null; ResetForm(); AfficherFormulaire = true; }

    [RelayCommand] public void ModifierClient(Client c) { Selection = c; ChargerForm(c); AfficherFormulaire = true; }

    [RelayCommand] public async Task Sauvegarder()
    {
        if (Selection is null)
            await _svc.AjouterAsync(new Client { Nom = Nom, Prenom = Prenom, Telephone = Telephone, Email = Email, Adresse = Adresse, Ville = Ville });
        else
        {
            Selection.Nom = Nom; Selection.Prenom = Prenom; Selection.Telephone = Telephone;
            Selection.Email = Email; Selection.Adresse = Adresse; Selection.Ville = Ville;
            await _svc.ModifierAsync(Selection);
        }
        AfficherFormulaire = false;
        await ChargerAsync();
    }

    [RelayCommand] public async Task Supprimer(Client c) { await _svc.SupprimerAsync(c.Id); await ChargerAsync(); }

    [RelayCommand] public void Annuler() { AfficherFormulaire = false; }

    private void ResetForm() { Nom = Prenom = Telephone = Email = Adresse = Ville = string.Empty; }
    private void ChargerForm(Client c) { Nom = c.Nom; Prenom = c.Prenom; Telephone = c.Telephone ?? ""; Email = c.Email ?? ""; Adresse = c.Adresse ?? ""; Ville = c.Ville ?? ""; }
}
