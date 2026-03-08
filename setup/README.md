# 🔧 Créer le Setup d'Installation — MecanoERP

## Prérequis

1. **Inno Setup 6** → [Télécharger ici](https://jrsoftware.org/isdl.php)
2. **.NET 8 SDK** → déjà installé si tu builds le projet
3. **PowerShell 5.1+** → fourni avec Windows 10/11

---

## Structure du dossier `setup/`

```
setup/
├── MecanoERP.iss       ← Script Inno Setup (configuration du setup)
├── license.txt         ← Licence affichée dans le setup
├── build-installer.ps1 ← Script de build tout-en-un
└── output/             ← Généré automatiquement (contient le .exe final)
```

---

## Générer le setup (.exe)

### Méthode simple — double-clic

1. Installer **Inno Setup 6** si ce n'est pas fait
2. Clic droit sur `build-installer.ps1` → **Exécuter avec PowerShell**
3. Le fichier `setup/output/MecanoERP_Setup_v1.0.0.exe` est créé ✅

### Méthode ligne de commande

```powershell
cd c:\Users\bedig\.gemini\antigravity\scratch\MecanoERP\setup
.\build-installer.ps1
```

---

## Ce que fait le setup installé

| Action | Détail |
|---|---|
| Dossier d'installation | `C:\Program Files\MecanoERP` (modifiable) |
| Raccourci Menu Démarrer | ✅ Créé automatiquement |
| Raccourci Bureau | ✅ Optionnel (case à cocher) |
| Registre Windows | Entrée dans "Programmes installés" |
| Désinstallation | Via "Ajouter/Supprimer des programmes" |
| .NET Runtime | Vérifié au lancement (message d'avertissement si absent) |

---

## Distribution

Le fichier `MecanoERP_Setup_v1.0.0.exe` est **autonome** (self-contained) — il embarque le runtime .NET. L'utilisateur final n'a **rien d'autre à installer**.

> **Copyright © 2026 GISEBS** — Tous droits réservés.
