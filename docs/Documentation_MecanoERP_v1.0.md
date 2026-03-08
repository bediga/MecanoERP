# Documentation MecanoERP v1.0
**Copyright © 2026 GISEBS — Tous droits réservés**

---

## Table des matières

1. [Présentation du système](#1-présentation-du-système)
2. [Installation](#2-installation)
3. [Premiers pas](#3-premiers-pas)
4. [Modules fonctionnels](#4-modules-fonctionnels)
   - 4.1 [Tableau de bord](#41-tableau-de-bord)
   - 4.2 [Clients & Véhicules](#42-clients--véhicules)
   - 4.3 [Atelier — Ordres de travail](#43-atelier--ordres-de-travail)
   - 4.4 [Inventaire & Pièces](#44-inventaire--pièces)
   - 4.5 [Facturation & Paiements](#45-facturation--paiements)
   - 4.6 [Comptabilité (GL)](#46-comptabilité-gl)
   - 4.7 [Comptes Clients AR](#47-comptes-clients-ar)
   - 4.8 [Comptes Fournisseurs AP](#48-comptes-fournisseurs-ap)
   - 4.9 [Trésorerie & Banque](#49-trésorerie--banque)
   - 4.10 [Ressources Humaines](#410-ressources-humaines)
   - 4.11 [Administration](#411-administration)
5. [Architecture technique](#5-architecture-technique)
6. [Base de données](#6-base-de-données)
7. [Sécurité & Accès](#7-sécurité--accès)
8. [Génération du setup](#8-génération-du-setup)
9. [Dépannage](#9-dépannage)

---

## 1. Présentation du système

**MecanoERP** est un progiciel de gestion intégré (ERP) spécialement conçu pour les garages automobiles. Il couvre l'ensemble des opérations d'un garage : de la prise de rendez-vous à la comptabilité complète, en passant par la gestion de l'atelier, des pièces et de la trésorerie.

### Technologies

| Composant | Technologie |
|---|---|
| Interface | WPF (.NET 10, Windows) |
| Architecture | MVVM + DI (Microsoft.Extensions) |
| Base de données | SQL Server (LocalDB en dev) |
| ORM | Entity Framework Core 9 |
| Rapports | En cours |

---

## 2. Installation

### Prérequis

- Windows 10/11 (64 bits)
- .NET Desktop Runtime 8+ (inclus dans le setup)

### Procédure

1. Double-cliquer sur **`MecanoERP_Setup_v1.0.0.exe`**
2. Accepter la licence GISEBS
3. Choisir le dossier d'installation (défaut : `C:\Program Files\MecanoERP`)
4. Cocher « Créer un raccourci bureau » si désiré
5. Cliquer **Installer**
6. Lancer MecanoERP depuis le Menu Démarrer ou le Bureau

### Désinstallation

Panneau de configuration → Programmes → **MecanoERP** → Désinstaller

---

## 3. Premiers pas

### Connexion

Au premier lancement, utiliser les identifiants par défaut :

| Champ | Valeur |
|---|---|
| Identifiant | `admin` |
| Mot de passe | `Admin123!` |

> ⚠ Changer le mot de passe dès la première connexion dans **Administration → Utilisateurs**.

### Navigation

La barre de navigation gauche est organisée en groupes expansibles :

| Groupe | Modules |
|---|---|
| PRINCIPAL | Tableau de bord, Rendez-vous |
| ATELIER | Clients, Véhicules, Ordres de travail |
| COMMERCIAL | Inventaire, Facturation, Devis |
| COMPTABILITÉ | Plan comptable, Journaux, Grand Livre, Balance, États financiers |
| FINANCES | Comptes clients (AR), Comptes fournisseurs (AP), Trésorerie |
| ACHATS & VENTES | Achats, Factures fourn., Avoirs |
| ADMINISTRATION | Employés, Utilisateurs, Paramètres |
| ATELIER AVANCÉ | Check-in, Pointage, Audit |

---

## 4. Modules fonctionnels

### 4.1 Tableau de bord

Vue d'ensemble en temps réel :

- **KPIs** : chiffre d'affaires du mois, factures en attente, bons de travail ouverts, rendez-vous du jour
- **Graphique CA mensuel** (barres 12 mois)
- **Top 5 services** les plus vendus

### 4.2 Clients & Véhicules

**Clients**
- Fiche client complète (coordonnées, notes, consentements SMS/email)
- Historique des factures et véhicules
- Recherche rapide par nom, téléphone ou email

**Véhicules**
- Marque, modèle, année, VIN, kilométrage
- Historique des interventions par véhicule

### 4.3 Atelier — Ordres de travail

- Création d'un ordre de travail (OT) lié à un client/véhicule
- Suivi des statuts : `Ouvert → En cours → Terminé → Facturable`
- Assignation d'un technicien
- Ligne de travaux et pièces utilisées
- Conversion automatique en facture

### 4.4 Inventaire & Pièces

- Catalogue de pièces avec numéro SKU, coût et prix de vente
- Alertes de stock minimum
- Mouvement de stock (entrée/sortie)
- Fournisseur associé par pièce

### 4.5 Facturation & Paiements

**Factures clients**
- Génération à partir d'un OT ou manuellement
- Calcul automatique TPS (5%) + TVQ (9,975%)
- Statuts : `Brouillon → Émise → Partiellement payée → Payée → Annulée`
- Modes de paiement : Comptant, Carte, Virement, Chèque

**Avoirs**
- Émission d'une note de crédit sur une facture existante

**Devis**
- Création de devis, conversion en facture en un clic

### 4.6 Comptabilité (GL)

| Écran | Fonction |
|---|---|
| Plan comptable | Création et gestion des comptes GL (Actif, Passif, Charges, Produits) |
| Journaux | Saisie manuelle des écritures comptables |
| Grand Livre | Consultation du grand livre par compte et période |
| Balance de vérification | Rapport débit/crédit par compte avec solde |
| États financiers | Bilan et compte de résultat |
| Clôture des périodes | Clôture mensuelle/annuelle |

### 4.7 Comptes Clients (AR)

- **Rapport d'âge AR** : analyse du solde par tranche (Courant / 1-30j / 31-60j / 61-90j / 90j+)
- **KPIs** : Total AR, Échues aujourd'hui, En souffrance 90j+, Taux de recouvrement
- **Encaissements rapides** : saisie d'un paiement directement sur une facture
- **Relances** : liste des clients en retard avec niveau de relance (1=vert, 2=orange, 3=rouge)

### 4.8 Comptes Fournisseurs (AP)

- **Rapport d'âge AP** : même structure que l'AR mais côté fournisseurs
- **KPIs** : Total AP, À payer aujourd'hui, Cette semaine, En souffrance
- **Paiements en lot** : sélection multiple de factures + mode de paiement → paiement groupé
- **Échéancier 45 jours** : liste des échéances à venir

### 4.9 Trésorerie & Banque

- **Comptes bancaires** : liste avec solde courant
- **Rapprochement bancaire** : comparaison solde de livres vs solde banque, affichage de l'écart
- **Auto-rapprochement** : matching automatique par montant entre transactions et paiements
- **Import relevé CSV** : format `date, description, montant` (positif=crédit, négatif=débit)
- **Prévisions 60 jours** : encaissements attendus (AR) et décaissements attendus (AP)

### 4.10 Ressources Humaines

- Fiche employé (poste, salaire, heures)
- Pointage (entrée/sortie)
- Check-in véhicule à l'atelier

### 4.11 Administration

**Utilisateurs**
- Création de comptes avec rôle : `Administrateur`, `Technicien`, `Comptable`, `Réceptionniste`
- Réinitialisation de mot de passe

**Paramètres**
- Informations du garage (nom, adresse, taxes)
- Configuration email/SMS

**Journal d'audit**
- Historique de toutes les actions utilisateurs

---

## 5. Architecture technique

```
MecanoERP/
├── src/
│   ├── MecanoERP.Core/              # Entités, interfaces
│   │   └── Entities/                # Client, Facture, Vehicule, CompteBancaire...
│   ├── MecanoERP.Infrastructure/    # Services, DbContext, Migrations EF
│   │   ├── Data/                    # MecanoDbContext
│   │   └── Services/                # ARService, APService, TresorerieService...
│   └── MecanoERP.WPF/               # Interface WPF
│       ├── Views/                   # XAML + code-behind
│       ├── ViewModels/              # MVVM ViewModels
│       └── Assets/                  # Logo, icônes
└── setup/
    ├── MecanoERP.iss                # Script Inno Setup
    ├── build-installer.ps1          # Script de build
    └── output/                      # MecanoERP_Setup_v1.0.0.exe
```

### Pattern MVVM

```
View (XAML)  ←→  ViewModel (CommunityToolkit.Mvvm)  ←→  Service  ←→  DbContext (EF Core)
```

---

## 6. Base de données

### Connexion

La chaîne de connexion se configure dans `appsettings.json` :

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=...;Database=MecanoERP;..."
  }
}
```

### Migrations EF Core

```powershell
# Appliquer les migrations
dotnet ef database update --project src\MecanoERP.Infrastructure --startup-project src\MecanoERP.WPF

# Créer une nouvelle migration
dotnet ef migrations add NomMigration --project src\MecanoERP.Infrastructure --startup-project src\MecanoERP.WPF
```

### Tables principales

| Table | Description |
|---|---|
| `Clients` | Fiches clients |
| `Vehicules` | Véhicules des clients |
| `OrdresTravail` | Bons de travail atelier |
| `Factures` | Factures clients |
| `Paiements` | Encaissements clients |
| `FacturesFournisseurs` | Factures fournisseurs |
| `PaiementsFournisseurs` | Paiements fournisseurs |
| `ComptesBancaires` | Comptes bancaires |
| `TransactionsBancaires` | Relevé bancaire |
| `ComptesGL` | Plan comptable |
| `EcrituresComptables` | Journal général |
| `Utilisateurs` | Comptes utilisateurs |

---

## 7. Sécurité & Accès

- Mots de passe hashés (BCrypt)
- Sessions avec token en mémoire
- Rôles : Administrateur, Comptable, Technicien, Réceptionniste
- Journal d'audit de toutes les actions
- Déconnexion automatique (configurable)

---

## 8. Génération du setup

Pour générer un nouveau setup après modification du code :

```powershell
cd setup
.\build-installer.ps1
```

Cela exécute automatiquement :
1. `dotnet publish` — compilation self-contained win-x64
2. `ISCC.exe MecanoERP.iss` — compilation du .exe installateur

Résultat : `setup\output\MecanoERP_Setup_v1.0.0.exe`

> Prérequis : Inno Setup 6 installé (`winget install JRSoftware.InnoSetup`)

---

## 9. Dépannage

| Problème | Solution |
|---|---|
| Connexion DB échoue | Vérifier la chaîne de connexion dans `appsettings.json` |
| Fenêtre de login vide | Lancer en tant qu'administrateur au premier démarrage |
| Migrations EF manquantes | Exécuter `dotnet ef database update` |
| Erreur « assemblage introuvable » | Réinstaller via `MecanoERP_Setup_v1.0.0.exe` |
| Rapport d'âge vide | Vérifier qu'il existe des factures avec statut Émise |

---

*Documentation générée le 26 février 2026 — MecanoERP v1.0.0 — © 2026 GISEBS*
