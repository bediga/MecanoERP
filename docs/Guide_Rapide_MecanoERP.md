# Guide Rapide — MecanoERP v1.0
**© 2026 GISEBS**

---

## Connexion
| Champ | Valeur par défaut |
|---|---|
| Identifiant | `admin` |
| Mot de passe | `Admin123!` |

---

## Raccourcis clavier

| Action | Raccourci |
|---|---|
| Nouveau client | Ctrl + N |
| Rechercher | Ctrl + F |
| Actualiser | F5 |
| Déconnexion | Alt + F4 |

---

## Workflow quotidien typique

```
1. Rendez-vous        → PRINCIPAL > Rendez-vous
2. Check-in véhicule  → ATELIER AVANCÉ > Check-in
3. Ordre de travail   → ATELIER > Ordres de travail > Nouveau
4. Pièces utilisées   → (dans l'OT) > Ajouter pièce
5. Clôture OT         → Statut "Terminé" > Créer facture
6. Paiement client    → FINANCES > Comptes clients (AR) > Encaissements
7. Fin de journée     → COMPTABILITÉ > Journaux (vérification)
```

---

## Statuts des factures

| Statut | Couleur | Signification |
|---|---|---|
| Brouillon | Gris | Non envoyée |
| Émise | Bleu | Envoyée, en attente de paiement |
| Partiellement payée | Orange | Acompte reçu |
| Payée | Vert | Soldée |
| Annulée | Rouge | Annulée |

---

## Calcul des taxes (Québec)

```
Sous-total HT   = main-d'oeuvre + pièces
TPS (5%)        = Sous-total × 0.05
TVQ (9.975%)    = Sous-total × 0.09975
Total TTC       = Sous-total + TPS + TVQ
```

---

## Rapport d'âge AR — Signification des colonnes

| Colonne | Signification |
|---|---|
| Courant | Pas encore échu (date OK) |
| 1-30 j | En retard de 1 à 30 jours |
| 31-60 j | En retard de 31 à 60 jours |
| 61-90 j | En retard de 61 à 90 jours |
| 90+ j | En souffrance — action urgente |

---

## Import relevé bancaire CSV

Format attendu (avec en-tête) :
```
date,description,montant
2026-02-01,Paiement client Tremblay,1500.00
2026-02-03,Fournisseur PartsPro,-850.00
```
Positif = crédit (encaissement), Négatif = débit (décaissement).

---

## Contacts support

| | |
|---|---|
| Éditeur | GISEBS |
| Email | support@gisebs.com |
| Version | 1.0.0 |
| Date | Février 2026 |
