using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MecanoERP.Core.Entities;
using MecanoERP.Core.Entities.Comptabilite;
using MecanoERP.Core.Entities.Identite;
using MecanoERP.Infrastructure.Services;
using MecanoERP.Infrastructure.Services.Comptabilite;
using MecanoERP.Infrastructure.Services.Securite;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;

namespace MecanoERP.WPF.ViewModels;

// ══════════════════════════════════════════════════════════════════════════════
// PLAN COMPTABLE
// ══════════════════════════════════════════════════════════════════════════════
public partial class PlanComptableViewModel : ObservableObject
{
    private readonly PlanComptableService _svc;

    [ObservableProperty] private ObservableCollection<CompteGL> _comptes = [];
    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private bool _afficherFormulaire;
    [ObservableProperty] private CompteGL? _selection;

    [ObservableProperty] private string _numero = string.Empty;
    [ObservableProperty] private string _nomCompte = string.Empty;
    [ObservableProperty] private TypeCompteGL _typeCompte = TypeCompteGL.Actif;
    [ObservableProperty] private bool _estActif = true;

    public IEnumerable<TypeCompteGL> TypesCompte { get; } = Enum.GetValues<TypeCompteGL>();

    public PlanComptableViewModel(PlanComptableService svc) => _svc = svc;

    [RelayCommand]
    public async Task ChargerAsync()
    {
        IsLoading = true;
        try { Comptes = new(await _svc.GetAllAsync(inclureInactifs: true)); }
        finally { IsLoading = false; }
    }

    [RelayCommand]
    public void NouveauCompte()
    {
        Selection = null; Numero = NomCompte = string.Empty; TypeCompte = TypeCompteGL.Actif; EstActif = true;
        AfficherFormulaire = true;
    }

    [RelayCommand]
    public void ModifierCompte(CompteGL c)
    {
        Selection = c; Numero = c.Numero; NomCompte = c.Nom; TypeCompte = c.TypeCompte; EstActif = c.EstActif;
        AfficherFormulaire = true;
    }

    [RelayCommand]
    public async Task SauvegarderCompteAsync()
    {
        if (Selection == null)
            await _svc.AjouterAsync(new CompteGL { Numero = Numero, Nom = NomCompte, TypeCompte = TypeCompte, EstActif = EstActif });
        else
        {
            Selection.Nom = NomCompte; Selection.TypeCompte = TypeCompte; Selection.EstActif = EstActif;
            await _svc.ModifierAsync(Selection);
        }
        AfficherFormulaire = false;
        await ChargerAsync();
    }

    [RelayCommand] public void Annuler() { AfficherFormulaire = false; }
}

// ══════════════════════════════════════════════════════════════════════════════
// JOURNAUX COMPTABLES
// ══════════════════════════════════════════════════════════════════════════════
public partial class JournauxViewModel : ObservableObject
{
    private readonly JournalService _svc;
    private readonly PlanComptableService _planSvc;

    [ObservableProperty] private ObservableCollection<JournalComptable> _journaux = [];
    [ObservableProperty] private ObservableCollection<CompteGL> _comptes = [];
    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private bool _afficherFormulaire;
    [ObservableProperty] private string _messageStatut = string.Empty;

    [ObservableProperty] private DateTime _dateDebut = DateTime.Today.AddMonths(-1);
    [ObservableProperty] private DateTime _dateFin = DateTime.Today;

    // Formulaire
    [ObservableProperty] private TypeJournal _typeJournal = TypeJournal.General;
    [ObservableProperty] private DateTime _dateEcriture = DateTime.Today;
    [ObservableProperty] private string _description = string.Empty;
    [ObservableProperty] private int? _compteDebitId;
    [ObservableProperty] private int? _compteCredit2Id;
    [ObservableProperty] private decimal _montantDebit;
    [ObservableProperty] private decimal _montantCredit;

    public IEnumerable<TypeJournal> TypesJournal { get; } = Enum.GetValues<TypeJournal>();

    public JournauxViewModel(JournalService svc, PlanComptableService planSvc)
    { _svc = svc; _planSvc = planSvc; }

    [RelayCommand]
    public async Task ChargerAsync()
    {
        IsLoading = true;
        try
        {
            Journaux = new(await _svc.GetJournauxAsync(debut: DateDebut, fin: DateFin));
            if (!Comptes.Any())
                Comptes = new(await _planSvc.GetAllAsync());
        }
        finally { IsLoading = false; }
    }

    [RelayCommand]
    public void NouvelleEcriture()
    {
        TypeJournal = TypeJournal.General; DateEcriture = DateTime.Today;
        Description = string.Empty; MontantDebit = MontantCredit = 0;
        CompteDebitId = CompteCredit2Id = null; MessageStatut = string.Empty;
        AfficherFormulaire = true;
    }

    [RelayCommand]
    public async Task SauvegarderJournalAsync()
    {
        if (CompteDebitId == null || CompteCredit2Id == null || MontantDebit <= 0)
        { MessageStatut = "⚠ Remplissez les comptes et le montant."; return; }
        try
        {
            var journal = new JournalComptable
            {
                TypeJournal = TypeJournal,
                Date = DateEcriture,
                Description = Description,
                Lignes = new List<LigneJournal>
                {
                    new() { CompteGLId = CompteDebitId.Value,  Debit = MontantDebit,  Credit = 0, Description = Description, Ordre = 1 },
                    new() { CompteGLId = CompteCredit2Id.Value, Debit = 0, Credit = MontantCredit > 0 ? MontantCredit : MontantDebit, Description = Description, Ordre = 2 }
                }
            };
            var created = await _svc.CreerAsync(journal);
            await _svc.ValiderAsync(created.Id);
            MessageStatut = "✔ Écriture enregistrée et validée.";
            AfficherFormulaire = false;
            await ChargerAsync();
        }
        catch (Exception ex) { MessageStatut = $"✖ {ex.Message}"; }
    }

    [RelayCommand] public void Annuler() { AfficherFormulaire = false; }
}

// ══════════════════════════════════════════════════════════════════════════════
// RAPPORTS COMPTABLES
// ══════════════════════════════════════════════════════════════════════════════
public record LigneRapport(string Description, string Quantite, decimal Montant, string PourcentageStr);

public partial class RapportsComptablesViewModel : ObservableObject
{
    private readonly RapportComptableService _svc;
    private readonly FactureService _factureSvc;
    private readonly FactureFournisseurService _apSvc;
    private readonly BanqueService _banqueSvc;

    [ObservableProperty] private ObservableCollection<LigneRapport> _lignesRapport = [];
    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private string _typeRapportSelectionne = "Chiffre d'affaires";
    [ObservableProperty] private DateTime _dateDebut = DateTime.Today.AddMonths(-1);
    [ObservableProperty] private DateTime _dateFin = DateTime.Today;
    [ObservableProperty] private string _titreTableau = string.Empty;

    [ObservableProperty] private decimal _kpiCA;
    [ObservableProperty] private decimal _kpiMarge;
    [ObservableProperty] private decimal _kpiAR;
    [ObservableProperty] private decimal _kpiAP;
    [ObservableProperty] private decimal _kpiBanque;

    public List<string> TypesRapport { get; } =
        ["Chiffre d'affaires", "Factures impayées (AR)", "Factures fournisseurs en retard (AP)", "Résumé période"];

    public RapportsComptablesViewModel(RapportComptableService svc, FactureService fs,
        FactureFournisseurService ap, BanqueService bs)
    { _svc = svc; _factureSvc = fs; _apSvc = ap; _banqueSvc = bs; }

    [RelayCommand] public async Task ChargerAsync() => await GenererRapportAsync();

    [RelayCommand]
    public async Task GenererRapportAsync()
    {
        IsLoading = true;
        try
        {
            var factures = await _factureSvc.GetFacturesAsync();
            var ffList   = await _apSvc.GetAllAsync();
            var comptes  = await _banqueSvc.GetComptesAsync();

            KpiCA     = factures.Sum(f => f.MontantTTC);
            KpiAR     = factures.Where(f => f.SoldeRestant > 0).Sum(f => f.SoldeRestant);
            KpiAP     = ffList.Where(f => f.SoldeRestant > 0).Sum(f => f.SoldeRestant);
            KpiBanque = comptes.Sum(c => c.SoldeCourant);
            KpiMarge  = KpiCA * 0.35m;

            TitreTableau = TypeRapportSelectionne;
            var lignes = new List<LigneRapport>();

            switch (TypeRapportSelectionne)
            {
                case "Chiffre d'affaires":
                    var filtrees = factures.Where(f => f.DateFacture >= DateDebut && f.DateFacture <= DateFin).ToList();
                    var totalCA  = filtrees.Sum(f => f.MontantTTC);
                    foreach (var f in filtrees.OrderByDescending(f => f.MontantTTC))
                        lignes.Add(new($"{f.Client?.Nom ?? "?"} — {f.Numero}", f.DateFacture.ToString("dd/MM"),
                            f.MontantTTC, totalCA > 0 ? $"{f.MontantTTC / totalCA * 100:F1}%" : "-"));
                    if (lignes.Any()) lignes.Add(new("TOTAL", "", totalCA, "100%"));
                    break;

                case "Factures impayées (AR)":
                    foreach (var f in factures.Where(f => f.SoldeRestant > 0).OrderByDescending(f => f.SoldeRestant))
                    {
                        var j = (DateTime.Today - f.DateFacture).Days;
                        lignes.Add(new($"{f.Client?.Nom ?? "?"} — {f.Numero}", $"{j} j",
                            f.SoldeRestant, j > 60 ? "🔴 >60j" : j > 30 ? "🟡 >30j" : "🟢"));
                    }
                    break;

                case "Factures fournisseurs en retard (AP)":
                    foreach (var f in ffList.Where(f => f.SoldeRestant > 0 && f.DateEcheance < DateTime.Today).OrderBy(f => f.DateEcheance))
                    {
                        var j = (DateTime.Today - f.DateEcheance).Days;
                        lignes.Add(new($"{f.Fournisseur?.Nom ?? "?"} — {f.Numero}", $"+{j} j",
                            f.SoldeRestant, j > 30 ? "🔴 Urgent" : "🟡"));
                    }
                    break;

                default:
                    lignes.Add(new("Chiffre d'affaires", "", KpiCA, ""));
                    lignes.Add(new("Marge brute estimée (35%)", "", KpiMarge, "35%"));
                    lignes.Add(new("Comptes clients (AR)", "", KpiAR, ""));
                    lignes.Add(new("Comptes fournisseurs (AP)", "", KpiAP, ""));
                    lignes.Add(new("Solde bancaire total", "", KpiBanque, ""));
                    break;
            }
            LignesRapport = new(lignes);
        }
        finally { IsLoading = false; }
    }

    [RelayCommand]
    public async Task ExporterCSVAsync()
    {
        await GenererRapportAsync();
        var sb = new StringBuilder("Description,Quantité,Montant,%\n");
        foreach (var l in LignesRapport)
            sb.AppendLine($"\"{l.Description}\",\"{l.Quantite}\",{l.Montant},\"{l.PourcentageStr}\"");
        var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            $"Rapport_{TypeRapportSelectionne.Replace(" ", "_")}_{DateTime.Today:yyyyMMdd}.csv");
        await File.WriteAllTextAsync(path, sb.ToString(), Encoding.UTF8);
    }
}

// ══════════════════════════════════════════════════════════════════════════════
// CONFIG TAXES (ConfigurationFiscale)
// ══════════════════════════════════════════════════════════════════════════════
public partial class ConfigTaxesViewModel : ObservableObject
{
    private readonly TaxeService _svc;

    [ObservableProperty] private ObservableCollection<ConfigurationFiscale> _taxes = [];
    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private ConfigurationFiscale? _selection;
    [ObservableProperty] private string _messageStatut = string.Empty;

    public IEnumerable<ConfigurationFiscale> ProvincesDisponibles { get; } 
        = TaxeService.GetProvincesPredefinies();

    public ConfigTaxesViewModel(TaxeService svc) => _svc = svc;

    [RelayCommand]
    public async Task ChargerAsync()
    {
        IsLoading = true;
        try { Taxes = new(await _svc.GetToutesAsync()); }
        finally { IsLoading = false; }
    }

    [RelayCommand]
    public void Selectionner(ConfigurationFiscale c) => Selection = c;

    [RelayCommand]
    public async Task SauvegarderAsync()
    {
        if (Selection == null) return;
        try
        {
            await _svc.SauvegarderConfigAsync(Selection);
            MessageStatut = "✔ Configuration fiscale sauvegardée.";
            await ChargerAsync();
        }
        catch (Exception ex) { MessageStatut = $"✖ {ex.Message}"; }
    }
}

// ══════════════════════════════════════════════════════════════════════════════
// STUB — COMPTABILITE DASHBOARD
// ══════════════════════════════════════════════════════════════════════════════
public partial class ComptabiliteViewModel : ObservableObject
{
    [RelayCommand] public Task ChargerAsync() => Task.CompletedTask;
}

// ══════════════════════════════════════════════════════════════════════════════
// UTILISATEURS
// ══════════════════════════════════════════════════════════════════════════════
public partial class UtilisateursViewModel : ObservableObject
{
    private readonly AuthService _svc;

    [ObservableProperty] private ObservableCollection<UtilisateurApp> _utilisateurs = [];
    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private bool _afficherFormulaire;
    [ObservableProperty] private UtilisateurApp? _selection;
    [ObservableProperty] private string _nomUtilisateur = string.Empty;
    [ObservableProperty] private string _email = string.Empty;
    [ObservableProperty] private string _motDePasse = string.Empty;
    [ObservableProperty] private string _messageStatut = string.Empty;

    public UtilisateursViewModel(AuthService svc) => _svc = svc;

    [RelayCommand]
    public async Task ChargerAsync()
    {
        IsLoading = true;
        try { Utilisateurs = new(await _svc.GetUtilisateursAsync()); }
        finally { IsLoading = false; }
    }

    [RelayCommand] public void NouvelUtilisateur() { Selection = null; NomUtilisateur = Email = MotDePasse = ""; AfficherFormulaire = true; }
    [RelayCommand] public void ModifierUtilisateur(UtilisateurApp u) { Selection = u; NomUtilisateur = u.NomUtilisateur; Email = u.Email; MotDePasse = ""; AfficherFormulaire = true; }

    [RelayCommand]
    public async Task SauvegarderAsync()
    {
        try
        {
            if (Selection == null)
                await _svc.CreerUtilisateurAsync(NomUtilisateur, MotDePasse, 1, NomUtilisateur, Email);
            else
            {
                Selection.NomUtilisateur = NomUtilisateur; Selection.Email = Email;
                await _svc.ChangerMotDePasseAsync(Selection.Id, MotDePasse, MotDePasse); // simplification
            }
            AfficherFormulaire = false;
            MessageStatut = "✔ Sauvegardé.";
            await ChargerAsync();
        }
        catch (Exception ex) { MessageStatut = $"✖ {ex.Message}"; }
    }

    [RelayCommand] public async Task ToggleActifAsync(UtilisateurApp u) { await _svc.ToggleActifAsync(u.Id); await ChargerAsync(); }
    [RelayCommand] public void Annuler() { AfficherFormulaire = false; }
}
