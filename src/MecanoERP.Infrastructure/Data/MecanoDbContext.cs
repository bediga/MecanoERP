using MecanoERP.Core.Entities;
using MecanoERP.Core.Entities.Comptabilite;
using MecanoERP.Core.Entities.Identite;
using Microsoft.EntityFrameworkCore;

namespace MecanoERP.Infrastructure.Data;

public class MecanoDbContext : DbContext
{
    public MecanoDbContext(DbContextOptions<MecanoDbContext> options) : base(options) { }

    // ── Entités métier ──────────────────────────────────────────────
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<Vehicule> Vehicules => Set<Vehicule>();
    public DbSet<OrdreTravail> OrdresTravail => Set<OrdreTravail>();
    public DbSet<LigneOT> LignesOT => Set<LigneOT>();
    public DbSet<Piece> Pieces => Set<Piece>();
    public DbSet<Fournisseur> Fournisseurs => Set<Fournisseur>();
    public DbSet<CommandeAchat> CommandesAchat => Set<CommandeAchat>();
    public DbSet<LigneCommandeAchat> LignesCommandeAchat => Set<LigneCommandeAchat>();
    public DbSet<Facture> Factures => Set<Facture>();
    public DbSet<Paiement> Paiements => Set<Paiement>();
    public DbSet<Employe> Employes => Set<Employe>();
    public DbSet<RendezVous> RendezVous => Set<RendezVous>();
    public DbSet<Garantie> Garanties => Set<Garantie>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<EcritureComptable> EcrituresComptables => Set<EcritureComptable>();

    // ── Sécurité / RBAC ────────────────────────────────────────────
    public DbSet<RoleApp> Roles => Set<RoleApp>();
    public DbSet<UtilisateurApp> Utilisateurs => Set<UtilisateurApp>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();

    // ── Comptabilité GL ────────────────────────────────────────────
    public DbSet<CompteGL> ComptesGL => Set<CompteGL>();
    public DbSet<JournalComptable> Journaux => Set<JournalComptable>();
    public DbSet<LigneJournal> LignesJournal => Set<LigneJournal>();
    public DbSet<CentreDeCoût> CentresDeCoût => Set<CentreDeCoût>();
    public DbSet<PeriodeComptable> PeriodесComptables => Set<PeriodeComptable>();
    public DbSet<ConfigurationFiscale> ConfigurationsFiscales => Set<ConfigurationFiscale>();
    public DbSet<ConfigFournisseurCompta> ConfigsFournisseurs => Set<ConfigFournisseurCompta>();

    // ── Phase 1 — Finance, Achats & Ventes ─────────────────────────
    public DbSet<Devis> Devis => Set<Devis>();
    public DbSet<LigneDevis> LignesDevis => Set<LigneDevis>();
    public DbSet<DemandeAchat> DemandesAchat => Set<DemandeAchat>();
    public DbSet<LigneDemandeAchat> LignesDemandeAchat => Set<LigneDemandeAchat>();
    public DbSet<FactureFournisseur> FacturesFournisseurs => Set<FactureFournisseur>();
    public DbSet<PaiementFournisseur> PaiementsFournisseurs => Set<PaiementFournisseur>();
    public DbSet<AvoirClient> AvoirsClients => Set<AvoirClient>();
    public DbSet<CompteBancaire> ComptesBancaires => Set<CompteBancaire>();
    public DbSet<TransactionBancaire> TransactionsBancaires => Set<TransactionBancaire>();

    // ── Vague A — OT avancé, Check-in, Audit, RH ─────────────────
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<FicheEntree> FichesEntree => Set<FicheEntree>();
    public DbSet<AlerteEntretien> AlertesEntretien => Set<AlerteEntretien>();
    public DbSet<Pointage> Pointages => Set<Pointage>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        base.OnModelCreating(mb);

        // ── Client ──────────────────────────────────────────────────
        mb.Entity<Client>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Nom).IsRequired().HasMaxLength(100);
            e.Property(x => x.Email).HasMaxLength(200);
            e.Property(x => x.Telephone).HasMaxLength(20);
            e.Property(x => x.Solde).HasPrecision(18, 2);
        });

        // ── Vehicule ────────────────────────────────────────────────
        mb.Entity<Vehicule>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Client).WithMany(c => c.Vehicules).HasForeignKey(x => x.ClientId).OnDelete(DeleteBehavior.Cascade);
            e.Property(x => x.VIN).HasMaxLength(17);
        });

        // ── OrdreTravail ────────────────────────────────────────────
        mb.Entity<OrdreTravail>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Numero).IsRequired().HasMaxLength(20);
            e.HasOne(x => x.Vehicule).WithMany(v => v.OrdresTravail).HasForeignKey(x => x.VehiculeId);
            e.HasOne(x => x.Employe).WithMany(emp => emp.OrdresTravail).HasForeignKey(x => x.EmployeId).IsRequired(false);
            e.Property(x => x.TempsEstime).HasPrecision(10, 2);
            e.Property(x => x.TempsReel).HasPrecision(10, 2);
        });

        // ── LigneOT ─────────────────────────────────────────────────
        mb.Entity<LigneOT>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.OrdreTravail).WithMany(o => o.Lignes).HasForeignKey(x => x.OrdreTravailId);
            e.HasOne(x => x.Piece).WithMany(p => p.LignesOT).HasForeignKey(x => x.PieceId).IsRequired(false);
            e.Property(x => x.Quantite).HasPrecision(10, 2);
            e.Property(x => x.PrixUnitaire).HasPrecision(18, 2);
            e.Property(x => x.Rabais).HasPrecision(18, 2);
            e.Ignore(x => x.Total);
        });

        // ── Piece ───────────────────────────────────────────────────
        mb.Entity<Piece>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Reference).IsRequired().HasMaxLength(50);
            e.Property(x => x.PrixAchat).HasPrecision(18, 2);
            e.Property(x => x.PrixVente).HasPrecision(18, 2);
            e.HasOne(x => x.Fournisseur).WithMany(f => f.Pieces).HasForeignKey(x => x.FournisseurId).IsRequired(false);
        });

        // ── CommandeAchat ───────────────────────────────────────────
        mb.Entity<CommandeAchat>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Numero).IsRequired().HasMaxLength(20);
            e.HasOne(x => x.Fournisseur).WithMany(f => f.CommandesAchat).HasForeignKey(x => x.FournisseurId);
        });

        // ── Facture ─────────────────────────────────────────────────
        mb.Entity<Facture>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Numero).IsRequired().HasMaxLength(20);
            e.Property(x => x.MontantHT).HasPrecision(18, 2);
            e.Property(x => x.TauxTPS).HasPrecision(6, 5);
            e.Property(x => x.TauxTVQ).HasPrecision(6, 5);
            e.Property(x => x.TauxTVH).HasPrecision(6, 5);
            e.Property(x => x.MontantPaye).HasPrecision(18, 2);
            e.Ignore(x => x.MontantTPS);
            e.Ignore(x => x.MontantTVQ);
            e.Ignore(x => x.MontantTVH);
            e.Ignore(x => x.MontantTTC);
            e.Ignore(x => x.SoldeRestant);
            e.HasOne(x => x.Client).WithMany(c => c.Factures).HasForeignKey(x => x.ClientId);
            e.HasOne(x => x.OrdreTravail).WithMany(o => o.Factures).HasForeignKey(x => x.OrdreTravailId);
            e.HasOne(x => x.JournalComptable).WithMany().HasForeignKey(x => x.JournalComptableId).IsRequired(false);
            e.HasOne<ConfigurationFiscale>().WithMany().HasForeignKey(x => x.ConfigurationFiscaleId).IsRequired(false);
        });

        // ── Paiement ────────────────────────────────────────────────
        mb.Entity<Paiement>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Montant).HasPrecision(18, 2);
            e.HasOne(x => x.Facture).WithMany(f => f.Paiements).HasForeignKey(x => x.FactureId);
        });

        // ── Garantie ────────────────────────────────────────────────
        mb.Entity<Garantie>(e =>
        {
            e.HasKey(x => x.Id);
            e.Ignore(x => x.EstActive);
            e.HasOne(x => x.Facture).WithMany(f => f.Garanties).HasForeignKey(x => x.FactureId);
        });

        // ── RendezVous ──────────────────────────────────────────────
        mb.Entity<RendezVous>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Client).WithMany(c => c.RendezVous).HasForeignKey(x => x.ClientId);
            e.HasOne(x => x.Vehicule).WithMany(v => v.RendezVous).HasForeignKey(x => x.VehiculeId).IsRequired(false);
        });

        // ── EcritureComptable ───────────────────────────────────────
        mb.Entity<EcritureComptable>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Debit).HasPrecision(18, 2);
            e.Property(x => x.Credit).HasPrecision(18, 2);
            e.HasOne(x => x.CompteGL).WithMany().HasForeignKey(x => x.CompteGLId).IsRequired(false);
        });

        // ── Phase 1 : Devis ─────────────────────────────────────────
        mb.Entity<Devis>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Numero).IsRequired().HasMaxLength(30);
            e.HasOne(x => x.Client).WithMany().HasForeignKey(x => x.ClientId);
            e.HasOne(x => x.Vehicule).WithMany().HasForeignKey(x => x.VehiculeId).IsRequired(false);
            e.HasOne(x => x.OrdreTravail).WithMany().HasForeignKey(x => x.OrdreTravailId).IsRequired(false);
            e.Ignore(x => x.SousTotal);
            e.Ignore(x => x.TotalTaxes);
            e.Ignore(x => x.TotalTTC);
        });
        mb.Entity<LigneDevis>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Devis).WithMany(d => d.Lignes).HasForeignKey(x => x.DevisId);
            e.HasOne(x => x.Piece).WithMany().HasForeignKey(x => x.PieceId).IsRequired(false);
            e.Property(x => x.Quantite).HasPrecision(10, 2);
            e.Property(x => x.PrixUnitaire).HasPrecision(18, 2);
            e.Property(x => x.Rabais).HasPrecision(18, 2);
            e.Ignore(x => x.Total);
        });

        // ── Phase 1 : DemandeAchat ──────────────────────────────────
        mb.Entity<DemandeAchat>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Numero).IsRequired().HasMaxLength(30);
            e.HasOne(x => x.Demandeur).WithMany().HasForeignKey(x => x.DemandeurId).IsRequired(false);
            e.HasOne(x => x.CommandeAchat).WithMany().HasForeignKey(x => x.CommandeAchatId).IsRequired(false);
        });
        mb.Entity<LigneDemandeAchat>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.DemandeAchat).WithMany(d => d.Lignes).HasForeignKey(x => x.DemandeAchatId);
            e.HasOne(x => x.Piece).WithMany().HasForeignKey(x => x.PieceId).IsRequired(false);
            e.Property(x => x.PrixEstime).HasPrecision(18, 2);
            e.Ignore(x => x.Total);
        });

        // ── Phase 1 : FactureFournisseur & PaiementFournisseur ───────
        mb.Entity<FactureFournisseur>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Numero).IsRequired().HasMaxLength(30);
            e.Property(x => x.MontantHT).HasPrecision(18, 2);
            e.Property(x => x.TauxTaxe).HasPrecision(6, 5);
            e.Property(x => x.MontantPaye).HasPrecision(18, 2);
            e.Ignore(x => x.MontantTaxes);
            e.Ignore(x => x.MontantTTC);
            e.Ignore(x => x.SoldeRestant);
            e.HasOne(x => x.Fournisseur).WithMany().HasForeignKey(x => x.FournisseurId);
            e.HasOne(x => x.CommandeAchat).WithMany().HasForeignKey(x => x.CommandeAchatId).IsRequired(false);
        });
        mb.Entity<PaiementFournisseur>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Montant).HasPrecision(18, 2);
            e.HasOne(x => x.FactureFournisseur).WithMany(f => f.Paiements).HasForeignKey(x => x.FactureFournisseurId);
        });

        // ── Phase 1 : AvoirClient ───────────────────────────────────
        mb.Entity<AvoirClient>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Numero).IsRequired().HasMaxLength(30);
            e.Property(x => x.Montant).HasPrecision(18, 2);
            e.HasOne(x => x.Client).WithMany().HasForeignKey(x => x.ClientId);
            e.HasOne(x => x.FactureOrigine).WithMany().HasForeignKey(x => x.FactureOrigineId).IsRequired(false);
        });

        // ── Phase 1 : Banque ────────────────────────────────────────
        mb.Entity<CompteBancaire>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.SoldeOuverture).HasPrecision(18, 2);
            e.Property(x => x.SoldeCourant).HasPrecision(18, 2);
        });
        mb.Entity<TransactionBancaire>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Montant).HasPrecision(18, 2);
            e.HasOne(x => x.CompteBancaire).WithMany(c => c.Transactions).HasForeignKey(x => x.CompteBancaireId);
        });

        // ── Vague A : AuditLog ─────────────────────────────────────
        mb.Entity<AuditLog>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Module).HasMaxLength(50);
            e.Property(x => x.NomUtilisateur).HasMaxLength(100);
            e.HasIndex(x => x.DateAction);
            e.HasIndex(x => x.Module);
        });

        // ── Vague A : FicheEntree ──────────────────────────────────
        mb.Entity<FicheEntree>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.OrdreTravail).WithMany().HasForeignKey(x => x.OrdreTravailId);
            e.HasOne(x => x.EmployeReception).WithMany().HasForeignKey(x => x.EmployeReceptionId).IsRequired(false);
            e.Property(x => x.ChecklistJson).HasColumnType("text");
            e.Property(x => x.SignatureClientBase64).HasColumnType("text");
        });

        // ── Vague A : AlerteEntretien ──────────────────────────────
        mb.Entity<AlerteEntretien>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Vehicule).WithMany().HasForeignKey(x => x.VehiculeId);
        });

        // ── Vague A : Pointage ─────────────────────────────────────
        mb.Entity<Pointage>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Employe).WithMany().HasForeignKey(x => x.EmployeId);
            e.HasOne(x => x.OrdreTravail).WithMany().HasForeignKey(x => x.OrdreTravailId).IsRequired(false);
            e.Ignore(x => x.HeuresTravaillees);
        });

        mb.Entity<RoleApp>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Code).HasConversion<string>();
            e.HasIndex(x => x.Code).IsUnique();
        });

        mb.Entity<UtilisateurApp>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.NomUtilisateur).IsRequired().HasMaxLength(50);
            e.HasIndex(x => x.NomUtilisateur).IsUnique();
            e.HasOne(x => x.Role).WithMany(r => r.Utilisateurs).HasForeignKey(x => x.RoleId);
        });

        mb.Entity<Permission>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Module).HasConversion<string>();
            e.Property(x => x.Action).HasConversion<string>();
            e.HasIndex(x => new { x.Module, x.Action }).IsUnique();
        });

        mb.Entity<RolePermission>(e =>
        {
            e.HasKey(x => new { x.RoleId, x.PermissionId });
            e.HasOne(x => x.Role).WithMany(r => r.RolePermissions).HasForeignKey(x => x.RoleId);
            e.HasOne(x => x.Permission).WithMany(p => p.RolePermissions).HasForeignKey(x => x.PermissionId);
        });

        // ── Comptabilité GL ────────────────────────────────────────
        mb.Entity<CompteGL>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Numero).IsRequired().HasMaxLength(10);
            e.HasIndex(x => x.Numero).IsUnique();
            e.Property(x => x.Solde).HasPrecision(18, 2);
            e.Property(x => x.TypeCompte).HasConversion<string>();
            e.Property(x => x.SousType).HasConversion<string>();
            e.HasOne(x => x.CompteParent).WithMany(c => c.SousComptes).HasForeignKey(x => x.CompteParentId).IsRequired(false);
        });

        mb.Entity<JournalComptable>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Numero).IsRequired().HasMaxLength(20);
            e.Property(x => x.TypeJournal).HasConversion<string>();
            e.Ignore(x => x.TotalDebit);
            e.Ignore(x => x.TotalCredit);
            e.Ignore(x => x.EstEquilibre);
        });

        mb.Entity<LigneJournal>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Debit).HasPrecision(18, 2);
            e.Property(x => x.Credit).HasPrecision(18, 2);
            e.HasOne(x => x.Journal).WithMany(j => j.Lignes).HasForeignKey(x => x.JournalComptableId);
            e.HasOne(x => x.CompteGL).WithMany(c => c.LignesJournal).HasForeignKey(x => x.CompteGLId);
        });

        mb.Entity<ConfigurationFiscale>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Province).HasConversion<string>().HasMaxLength(5);
            e.Property(x => x.Regime).HasConversion<string>();
            e.Property(x => x.TauxTPS).HasPrecision(6, 5);
            e.Property(x => x.TauxTVQ).HasPrecision(6, 5);
            e.Property(x => x.TauxTVH).HasPrecision(6, 5);
            e.Ignore(x => x.TauxEffectifTotal);
        });

        mb.Entity<ConfigFournisseurCompta>(e =>
        {
            e.HasKey(x => x.FournisseurId);
            e.Property(x => x.LimiteCredit).HasPrecision(18, 2);
            e.Property(x => x.Termes).HasConversion<string>();
            e.Property(x => x.TypeFournisseur).HasConversion<string>();
            e.HasOne(x => x.Fournisseur).WithOne().HasForeignKey<ConfigFournisseurCompta>(x => x.FournisseurId);
            e.HasOne(x => x.CompteGLCharge).WithMany().HasForeignKey(x => x.CompteGLChargeId).IsRequired(false);
        });
    }
}
