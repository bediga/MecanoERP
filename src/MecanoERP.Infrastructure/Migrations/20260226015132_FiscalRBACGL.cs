using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MecanoERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FiscalRBACGL : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Clients",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nom = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Prenom = table.Column<string>(type: "text", nullable: false),
                    Telephone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Adresse = table.Column<string>(type: "text", nullable: false),
                    Ville = table.Column<string>(type: "text", nullable: false),
                    CodePostal = table.Column<string>(type: "text", nullable: false),
                    Solde = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ConsentementSMS = table.Column<bool>(type: "boolean", nullable: false),
                    ConsentementEmail = table.Column<bool>(type: "boolean", nullable: false),
                    NotesInternes = table.Column<string>(type: "text", nullable: false),
                    DateCreation = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clients", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ComptesGL",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Numero = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Nom = table.Column<string>(type: "text", nullable: false),
                    TypeCompte = table.Column<string>(type: "text", nullable: false),
                    SousType = table.Column<string>(type: "text", nullable: false),
                    CompteParentId = table.Column<int>(type: "integer", nullable: true),
                    EstSysteme = table.Column<bool>(type: "boolean", nullable: false),
                    EstActif = table.Column<bool>(type: "boolean", nullable: false),
                    Solde = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    DateCreation = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComptesGL", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComptesGL_ComptesGL_CompteParentId",
                        column: x => x.CompteParentId,
                        principalTable: "ComptesGL",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ConfigurationsFiscales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Province = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    NomProvince = table.Column<string>(type: "text", nullable: false),
                    Regime = table.Column<string>(type: "text", nullable: false),
                    TauxTPS = table.Column<decimal>(type: "numeric(6,5)", precision: 6, scale: 5, nullable: false),
                    TauxTVQ = table.Column<decimal>(type: "numeric(6,5)", precision: 6, scale: 5, nullable: false),
                    TauxTVH = table.Column<decimal>(type: "numeric(6,5)", precision: 6, scale: 5, nullable: true),
                    NumeroInscriptionTPS = table.Column<string>(type: "text", nullable: false),
                    NumeroInscriptionTVQ = table.Column<string>(type: "text", nullable: false),
                    NumeroInscriptionTVH = table.Column<string>(type: "text", nullable: false),
                    CompteGLTPS_PercueId = table.Column<int>(type: "integer", nullable: true),
                    CompteGLTVQ_PercueId = table.Column<int>(type: "integer", nullable: true),
                    CompteGLTVH_PercueId = table.Column<int>(type: "integer", nullable: true),
                    CompteGLTPS_RecuperableId = table.Column<int>(type: "integer", nullable: true),
                    CompteGLTVQ_RecuperableId = table.Column<int>(type: "integer", nullable: true),
                    EstActive = table.Column<bool>(type: "boolean", nullable: false),
                    DateMiseAJour = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfigurationsFiscales", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Employes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nom = table.Column<string>(type: "text", nullable: false),
                    Prenom = table.Column<string>(type: "text", nullable: false),
                    Poste = table.Column<string>(type: "text", nullable: false),
                    Telephone = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    TauxHoraire = table.Column<decimal>(type: "numeric", nullable: false),
                    TauxCommission = table.Column<decimal>(type: "numeric", nullable: false),
                    DateEmbauche = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EstActif = table.Column<bool>(type: "boolean", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Fournisseurs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nom = table.Column<string>(type: "text", nullable: false),
                    ContactNom = table.Column<string>(type: "text", nullable: false),
                    Telephone = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    Adresse = table.Column<string>(type: "text", nullable: false),
                    Ville = table.Column<string>(type: "text", nullable: false),
                    NumeroCompte = table.Column<string>(type: "text", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: false),
                    DateCreation = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fournisseurs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Journaux",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Numero = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    TypeJournal = table.Column<string>(type: "text", nullable: false),
                    EstBrouillon = table.Column<bool>(type: "boolean", nullable: false),
                    EstAnnule = table.Column<bool>(type: "boolean", nullable: false),
                    UtilisateurId = table.Column<int>(type: "integer", nullable: true),
                    DateCreation = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateValidation = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FactureId = table.Column<int>(type: "integer", nullable: true),
                    CommandeAchatId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Journaux", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Titre = table.Column<string>(type: "text", nullable: false),
                    Message = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Lu = table.Column<bool>(type: "boolean", nullable: false),
                    ClientId = table.Column<int>(type: "integer", nullable: true),
                    OrdreTravailId = table.Column<int>(type: "integer", nullable: true),
                    DateCreation = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Permissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Module = table.Column<string>(type: "text", nullable: false),
                    Action = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permissions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "text", nullable: false),
                    Nom = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    EstSysteme = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Vehicules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ClientId = table.Column<int>(type: "integer", nullable: false),
                    Marque = table.Column<string>(type: "text", nullable: false),
                    Modele = table.Column<string>(type: "text", nullable: false),
                    Annee = table.Column<int>(type: "integer", nullable: false),
                    VIN = table.Column<string>(type: "character varying(17)", maxLength: 17, nullable: false),
                    Immatriculation = table.Column<string>(type: "text", nullable: false),
                    Kilometrage = table.Column<int>(type: "integer", nullable: false),
                    TypeMoteur = table.Column<string>(type: "text", nullable: false),
                    Couleur = table.Column<string>(type: "text", nullable: false),
                    Transmission = table.Column<string>(type: "text", nullable: false),
                    DateCreation = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vehicules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Vehicules_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EcrituresComptables",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Reference = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Debit = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Credit = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CompteGLId = table.Column<int>(type: "integer", nullable: true),
                    Compte = table.Column<string>(type: "text", nullable: false),
                    FactureId = table.Column<int>(type: "integer", nullable: true),
                    CommandeAchatId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EcrituresComptables", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EcrituresComptables_ComptesGL_CompteGLId",
                        column: x => x.CompteGLId,
                        principalTable: "ComptesGL",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CommandesAchat",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Numero = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    FournisseurId = table.Column<int>(type: "integer", nullable: false),
                    DateCommande = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateReception = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Statut = table.Column<int>(type: "integer", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommandesAchat", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CommandesAchat_Fournisseurs_FournisseurId",
                        column: x => x.FournisseurId,
                        principalTable: "Fournisseurs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ConfigsFournisseurs",
                columns: table => new
                {
                    FournisseurId = table.Column<int>(type: "integer", nullable: false),
                    NumeroTPS = table.Column<string>(type: "text", nullable: false),
                    NumeroTVQ = table.Column<string>(type: "text", nullable: false),
                    NumeroTVH = table.Column<string>(type: "text", nullable: false),
                    EstInscritTPS = table.Column<bool>(type: "boolean", nullable: false),
                    EstInscritTVQ = table.Column<bool>(type: "boolean", nullable: false),
                    CompteGLChargeId = table.Column<int>(type: "integer", nullable: true),
                    CompteGLCTI_Id = table.Column<int>(type: "integer", nullable: true),
                    Termes = table.Column<string>(type: "text", nullable: false),
                    TypeFournisseur = table.Column<string>(type: "text", nullable: false),
                    LimiteCredit = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfigsFournisseurs", x => x.FournisseurId);
                    table.ForeignKey(
                        name: "FK_ConfigsFournisseurs_ComptesGL_CompteGLChargeId",
                        column: x => x.CompteGLChargeId,
                        principalTable: "ComptesGL",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ConfigsFournisseurs_Fournisseurs_FournisseurId",
                        column: x => x.FournisseurId,
                        principalTable: "Fournisseurs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Pieces",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Reference = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CodeBarres = table.Column<string>(type: "text", nullable: false),
                    Designation = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    FournisseurId = table.Column<int>(type: "integer", nullable: true),
                    PrixAchat = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    PrixVente = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    StockActuel = table.Column<int>(type: "integer", nullable: false),
                    StockMinimum = table.Column<int>(type: "integer", nullable: false),
                    Emplacement = table.Column<string>(type: "text", nullable: false),
                    Categorie = table.Column<string>(type: "text", nullable: false),
                    DateCreation = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EstActif = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pieces", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Pieces_Fournisseurs_FournisseurId",
                        column: x => x.FournisseurId,
                        principalTable: "Fournisseurs",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "LignesJournal",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    JournalComptableId = table.Column<int>(type: "integer", nullable: false),
                    CompteGLId = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Debit = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Credit = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Ordre = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LignesJournal", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LignesJournal_ComptesGL_CompteGLId",
                        column: x => x.CompteGLId,
                        principalTable: "ComptesGL",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LignesJournal_Journaux_JournalComptableId",
                        column: x => x.JournalComptableId,
                        principalTable: "Journaux",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RolePermissions",
                columns: table => new
                {
                    RoleId = table.Column<int>(type: "integer", nullable: false),
                    PermissionId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePermissions", x => new { x.RoleId, x.PermissionId });
                    table.ForeignKey(
                        name: "FK_RolePermissions_Permissions_PermissionId",
                        column: x => x.PermissionId,
                        principalTable: "Permissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RolePermissions_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Utilisateurs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NomUtilisateur = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    MotDePasseHash = table.Column<string>(type: "text", nullable: false),
                    Sel = table.Column<string>(type: "text", nullable: false),
                    NomComplet = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    RoleId = table.Column<int>(type: "integer", nullable: false),
                    EstActif = table.Column<bool>(type: "boolean", nullable: false),
                    DernierConnexion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DateCreation = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Utilisateurs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Utilisateurs_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrdresTravail",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Numero = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    VehiculeId = table.Column<int>(type: "integer", nullable: false),
                    EmployeId = table.Column<int>(type: "integer", nullable: true),
                    DateEntree = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateSortie = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Diagnostic = table.Column<string>(type: "text", nullable: false),
                    TravauxDemandes = table.Column<string>(type: "text", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: false),
                    TempsEstime = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    TempsReel = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    KilometrageEntree = table.Column<int>(type: "integer", nullable: false),
                    Statut = table.Column<int>(type: "integer", nullable: false),
                    SignatureClient = table.Column<bool>(type: "boolean", nullable: false),
                    DateCreation = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrdresTravail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrdresTravail_Employes_EmployeId",
                        column: x => x.EmployeId,
                        principalTable: "Employes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_OrdresTravail_Vehicules_VehiculeId",
                        column: x => x.VehiculeId,
                        principalTable: "Vehicules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RendezVous",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ClientId = table.Column<int>(type: "integer", nullable: false),
                    VehiculeId = table.Column<int>(type: "integer", nullable: true),
                    EmployeId = table.Column<int>(type: "integer", nullable: true),
                    DateHeure = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DureeMinutes = table.Column<int>(type: "integer", nullable: false),
                    TypeService = table.Column<string>(type: "text", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: false),
                    Statut = table.Column<int>(type: "integer", nullable: false),
                    RappelEnvoye = table.Column<bool>(type: "boolean", nullable: false),
                    DateCreation = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RendezVous", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RendezVous_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RendezVous_Employes_EmployeId",
                        column: x => x.EmployeId,
                        principalTable: "Employes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RendezVous_Vehicules_VehiculeId",
                        column: x => x.VehiculeId,
                        principalTable: "Vehicules",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "LignesCommandeAchat",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CommandeAchatId = table.Column<int>(type: "integer", nullable: false),
                    PieceId = table.Column<int>(type: "integer", nullable: false),
                    Quantite = table.Column<int>(type: "integer", nullable: false),
                    QuantiteRecue = table.Column<int>(type: "integer", nullable: false),
                    PrixUnitaire = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LignesCommandeAchat", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LignesCommandeAchat_CommandesAchat_CommandeAchatId",
                        column: x => x.CommandeAchatId,
                        principalTable: "CommandesAchat",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LignesCommandeAchat_Pieces_PieceId",
                        column: x => x.PieceId,
                        principalTable: "Pieces",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Factures",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Numero = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ClientId = table.Column<int>(type: "integer", nullable: false),
                    OrdreTravailId = table.Column<int>(type: "integer", nullable: false),
                    DateFacture = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateEcheance = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    MontantHT = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ConfigurationFiscaleId = table.Column<int>(type: "integer", nullable: true),
                    TauxTPS = table.Column<decimal>(type: "numeric(6,5)", precision: 6, scale: 5, nullable: false),
                    TauxTVQ = table.Column<decimal>(type: "numeric(6,5)", precision: 6, scale: 5, nullable: false),
                    TauxTVH = table.Column<decimal>(type: "numeric(6,5)", precision: 6, scale: 5, nullable: true),
                    MontantPaye = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    NumeroTPS_Garage = table.Column<string>(type: "text", nullable: false),
                    NumeroTVQ_Garage = table.Column<string>(type: "text", nullable: false),
                    NumeroTVH_Garage = table.Column<string>(type: "text", nullable: false),
                    Statut = table.Column<int>(type: "integer", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: false),
                    JournalComptableId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Factures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Factures_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Factures_ConfigurationsFiscales_ConfigurationFiscaleId",
                        column: x => x.ConfigurationFiscaleId,
                        principalTable: "ConfigurationsFiscales",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Factures_Journaux_JournalComptableId",
                        column: x => x.JournalComptableId,
                        principalTable: "Journaux",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Factures_OrdresTravail_OrdreTravailId",
                        column: x => x.OrdreTravailId,
                        principalTable: "OrdresTravail",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LignesOT",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrdreTravailId = table.Column<int>(type: "integer", nullable: false),
                    PieceId = table.Column<int>(type: "integer", nullable: true),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Quantite = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    PrixUnitaire = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Rabais = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LignesOT", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LignesOT_OrdresTravail_OrdreTravailId",
                        column: x => x.OrdreTravailId,
                        principalTable: "OrdresTravail",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LignesOT_Pieces_PieceId",
                        column: x => x.PieceId,
                        principalTable: "Pieces",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Garanties",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FactureId = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    DateDebut = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateFin = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Garanties", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Garanties_Factures_FactureId",
                        column: x => x.FactureId,
                        principalTable: "Factures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Paiements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FactureId = table.Column<int>(type: "integer", nullable: false),
                    Montant = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    DatePaiement = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModePaiement = table.Column<int>(type: "integer", nullable: false),
                    Reference = table.Column<string>(type: "text", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Paiements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Paiements_Factures_FactureId",
                        column: x => x.FactureId,
                        principalTable: "Factures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CommandesAchat_FournisseurId",
                table: "CommandesAchat",
                column: "FournisseurId");

            migrationBuilder.CreateIndex(
                name: "IX_ComptesGL_CompteParentId",
                table: "ComptesGL",
                column: "CompteParentId");

            migrationBuilder.CreateIndex(
                name: "IX_ComptesGL_Numero",
                table: "ComptesGL",
                column: "Numero",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ConfigsFournisseurs_CompteGLChargeId",
                table: "ConfigsFournisseurs",
                column: "CompteGLChargeId");

            migrationBuilder.CreateIndex(
                name: "IX_EcrituresComptables_CompteGLId",
                table: "EcrituresComptables",
                column: "CompteGLId");

            migrationBuilder.CreateIndex(
                name: "IX_Factures_ClientId",
                table: "Factures",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_Factures_ConfigurationFiscaleId",
                table: "Factures",
                column: "ConfigurationFiscaleId");

            migrationBuilder.CreateIndex(
                name: "IX_Factures_JournalComptableId",
                table: "Factures",
                column: "JournalComptableId");

            migrationBuilder.CreateIndex(
                name: "IX_Factures_OrdreTravailId",
                table: "Factures",
                column: "OrdreTravailId");

            migrationBuilder.CreateIndex(
                name: "IX_Garanties_FactureId",
                table: "Garanties",
                column: "FactureId");

            migrationBuilder.CreateIndex(
                name: "IX_LignesCommandeAchat_CommandeAchatId",
                table: "LignesCommandeAchat",
                column: "CommandeAchatId");

            migrationBuilder.CreateIndex(
                name: "IX_LignesCommandeAchat_PieceId",
                table: "LignesCommandeAchat",
                column: "PieceId");

            migrationBuilder.CreateIndex(
                name: "IX_LignesJournal_CompteGLId",
                table: "LignesJournal",
                column: "CompteGLId");

            migrationBuilder.CreateIndex(
                name: "IX_LignesJournal_JournalComptableId",
                table: "LignesJournal",
                column: "JournalComptableId");

            migrationBuilder.CreateIndex(
                name: "IX_LignesOT_OrdreTravailId",
                table: "LignesOT",
                column: "OrdreTravailId");

            migrationBuilder.CreateIndex(
                name: "IX_LignesOT_PieceId",
                table: "LignesOT",
                column: "PieceId");

            migrationBuilder.CreateIndex(
                name: "IX_OrdresTravail_EmployeId",
                table: "OrdresTravail",
                column: "EmployeId");

            migrationBuilder.CreateIndex(
                name: "IX_OrdresTravail_VehiculeId",
                table: "OrdresTravail",
                column: "VehiculeId");

            migrationBuilder.CreateIndex(
                name: "IX_Paiements_FactureId",
                table: "Paiements",
                column: "FactureId");

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_Module_Action",
                table: "Permissions",
                columns: new[] { "Module", "Action" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Pieces_FournisseurId",
                table: "Pieces",
                column: "FournisseurId");

            migrationBuilder.CreateIndex(
                name: "IX_RendezVous_ClientId",
                table: "RendezVous",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_RendezVous_EmployeId",
                table: "RendezVous",
                column: "EmployeId");

            migrationBuilder.CreateIndex(
                name: "IX_RendezVous_VehiculeId",
                table: "RendezVous",
                column: "VehiculeId");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_PermissionId",
                table: "RolePermissions",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_Code",
                table: "Roles",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Utilisateurs_NomUtilisateur",
                table: "Utilisateurs",
                column: "NomUtilisateur",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Utilisateurs_RoleId",
                table: "Utilisateurs",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Vehicules_ClientId",
                table: "Vehicules",
                column: "ClientId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConfigsFournisseurs");

            migrationBuilder.DropTable(
                name: "EcrituresComptables");

            migrationBuilder.DropTable(
                name: "Garanties");

            migrationBuilder.DropTable(
                name: "LignesCommandeAchat");

            migrationBuilder.DropTable(
                name: "LignesJournal");

            migrationBuilder.DropTable(
                name: "LignesOT");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "Paiements");

            migrationBuilder.DropTable(
                name: "RendezVous");

            migrationBuilder.DropTable(
                name: "RolePermissions");

            migrationBuilder.DropTable(
                name: "Utilisateurs");

            migrationBuilder.DropTable(
                name: "CommandesAchat");

            migrationBuilder.DropTable(
                name: "ComptesGL");

            migrationBuilder.DropTable(
                name: "Pieces");

            migrationBuilder.DropTable(
                name: "Factures");

            migrationBuilder.DropTable(
                name: "Permissions");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "Fournisseurs");

            migrationBuilder.DropTable(
                name: "ConfigurationsFiscales");

            migrationBuilder.DropTable(
                name: "Journaux");

            migrationBuilder.DropTable(
                name: "OrdresTravail");

            migrationBuilder.DropTable(
                name: "Employes");

            migrationBuilder.DropTable(
                name: "Vehicules");

            migrationBuilder.DropTable(
                name: "Clients");
        }
    }
}
