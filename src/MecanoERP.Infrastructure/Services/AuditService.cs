using MecanoERP.Core.Entities;
using MecanoERP.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace MecanoERP.Infrastructure.Services;

public class AuditService
{
    private readonly MecanoDbContext _ctx;

    // Utilisateur courant (à setter au login)
    public static int? UtilisateurCourantId { get; set; }
    public static string NomUtilisateurCourant { get; set; } = "Système";

    public AuditService(MecanoDbContext ctx) => _ctx = ctx;

    public async Task LogAsync(
        string module,
        TypeAction typeAction,
        int? documentId = null,
        string documentNumero = "",
        string ancienneValeur = "",
        string nouvelleValeur = "",
        string description = "")
    {
        var log = new AuditLog
        {
            DateAction = DateTime.UtcNow,
            Module = module,
            TypeAction = typeAction,
            DocumentId = documentId,
            DocumentNumero = documentNumero,
            UtilisateurId = UtilisateurCourantId,
            NomUtilisateur = NomUtilisateurCourant,
            AncienneValeur = ancienneValeur,
            NouvelleValeur = nouvelleValeur,
            Description = description
        };
        _ctx.AuditLogs.Add(log);
        await _ctx.SaveChangesAsync();
    }

    public async Task LogChangementStatutOTAsync(OrdreTravail ot, StatutOT ancienStatut)
    {
        await LogAsync("OT", TypeAction.ChangementStatut,
            ot.Id, ot.Numero,
            ancienStatut.ToString(), ot.Statut.ToString(),
            $"Statut OT {ot.Numero} : {ancienStatut} → {ot.Statut}");
    }

    public async Task<IEnumerable<AuditLog>> GetLogsAsync(
        string? module = null,
        DateTime? debut = null,
        DateTime? fin = null,
        int? utilisateurId = null)
    {
        var query = _ctx.AuditLogs.AsQueryable();
        if (!string.IsNullOrEmpty(module))    query = query.Where(l => l.Module == module);
        if (debut.HasValue)                   query = query.Where(l => l.DateAction >= debut.Value);
        if (fin.HasValue)                     query = query.Where(l => l.DateAction <= fin.Value);
        if (utilisateurId.HasValue)           query = query.Where(l => l.UtilisateurId == utilisateurId.Value);
        return await query.OrderByDescending(l => l.DateAction).Take(500).ToListAsync();
    }
}
