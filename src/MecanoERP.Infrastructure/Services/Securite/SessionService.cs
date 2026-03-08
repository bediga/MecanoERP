using MecanoERP.Core.Entities.Identite;

namespace MecanoERP.Infrastructure.Services.Securite;

/// <summary>
/// Singleton — session de l'utilisateur courant pendant la durée de vie de l'app.
/// </summary>
public class SessionService
{
    private SessionUtilisateur? _session;

    public SessionUtilisateur? Courant => _session;
    public bool EstConnecte => _session is not null;

    public void OuvrirSession(SessionUtilisateur session) => _session = session;
    public void FermerSession() => _session = null;

    public bool EstAutorise(ModuleApp module, ActionApp action)
        => _session?.EstAutorise(module, action) ?? false;

    public bool EstAdmin   => _session?.Role == CodeRole.Admin;
    public bool EstComptable => _session?.Role is CodeRole.Admin or CodeRole.Comptable;
    public bool EstMecanicien => _session?.Role == CodeRole.Mecanicien;
}
