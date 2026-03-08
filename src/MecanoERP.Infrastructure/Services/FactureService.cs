using MecanoERP.Core.Entities;
using MecanoERP.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MecanoERP.Infrastructure.Services;

public class FactureService
{
    private readonly MecanoDbContext _context;

    public FactureService(MecanoDbContext context) => _context = context;

    public async Task<Facture> CreerFactureDepuisOT(int ordreTravailId)
    {
        var ot = await _context.OrdresTravail
            .Include(o => o.Lignes)
            .Include(o => o.Vehicule).ThenInclude(v => v.Client)
            .FirstOrDefaultAsync(o => o.Id == ordreTravailId)
            ?? throw new Exception("Ordre de travail introuvable.");

        var count = await _context.Factures.CountAsync() + 1;
        var montantHT = ot.Lignes.Sum(l => l.Total);

        var facture = new Facture
        {
            Numero = $"FAC-{DateTime.Now.Year}-{count:D4}",
            ClientId = ot.Vehicule.ClientId,
            OrdreTravailId = ordreTravailId,
            DateFacture = DateTime.UtcNow,
            DateEcheance = DateTime.UtcNow.AddDays(30),
            MontantHT = montantHT,
            Statut = StatutFacture.Emise
        };

        _context.Factures.Add(facture);
        ot.Statut = StatutOT.Facture;

        // Écriture comptable automatique
        _context.EcrituresComptables.Add(new EcritureComptable
        {
            Date = DateTime.UtcNow,
            Type = TypeEcriture.Vente,
            Reference = facture.Numero,
            Description = $"Facture {facture.Numero} - OT#{ordreTravailId}",
            Credit = facture.MontantHT + (facture.MontantHT * facture.TauxTPS) + (facture.MontantHT * facture.TauxTVQ),
            Compte = "4000-Ventes"
        });

        await _context.SaveChangesAsync();
        return facture;
    }

    public async Task EnregistrerPaiement(int factureId, decimal montant, ModePaiement mode, string reference = "")
    {
        var facture = await _context.Factures.FindAsync(factureId)
            ?? throw new Exception("Facture introuvable.");

        var paiement = new Paiement
        {
            FactureId = factureId,
            Montant = montant,
            DatePaiement = DateTime.UtcNow,
            ModePaiement = mode,
            Reference = reference
        };

        _context.Paiements.Add(paiement);
        facture.MontantPaye += montant;
        var montantTTC = facture.MontantHT * (1 + facture.TauxTPS + facture.TauxTVQ);
        facture.Statut = facture.MontantPaye >= montantTTC
            ? StatutFacture.Payee
            : StatutFacture.PartiellementPayee;

        // Écriture comptable
        _context.EcrituresComptables.Add(new EcritureComptable
        {
            Date = DateTime.UtcNow,
            Type = TypeEcriture.Paiement,
            Reference = reference,
            Description = $"Paiement facture #{factureId}",
            Debit = montant,
            Compte = "1100-Caisse",
            FactureId = factureId
        });

        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Garantie>> GetGarantiesAsync()
        => await _context.Garanties
            .Include(g => g.Facture).ThenInclude(f => f.Client)
            .OrderByDescending(g => g.DateFin)
            .ToListAsync();

    public async Task<IEnumerable<Facture>> GetFacturesAsync()
        => await _context.Factures
            .Include(f => f.Client)
            .Include(f => f.OrdreTravail).ThenInclude(o => o.Vehicule)
            .Include(f => f.Paiements)
            .OrderByDescending(f => f.DateFacture)
            .ToListAsync();

    public async Task<Facture?> GetByIdAsync(int id)
        => await _context.Factures
            .Include(f => f.Client)
            .Include(f => f.OrdreTravail)
            .Include(f => f.Paiements)
            .Include(f => f.Garanties)
            .FirstOrDefaultAsync(f => f.Id == id);
}

