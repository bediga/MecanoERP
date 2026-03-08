using MecanoERP.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MecanoERP.Infrastructure;

/// <summary>
/// Utilisé uniquement par les outils EF Core (dotnet ef migrations).
/// Permet d'exécuter les migrations depuis le projet Infrastructure seul,
/// sans avoir besoin du projet MAUI UI en tant que startup.
/// </summary>
public class MecanoDbContextFactory : IDesignTimeDbContextFactory<MecanoDbContext>
{
    public MecanoDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<MecanoDbContext>();
        // Connexion par défaut pour les migrations (remplacez si nécessaire)
        optionsBuilder.UseNpgsql(
            "Host=localhost;Port=5432;Database=Mecano;Username=gisebs;Password=Goodeeg!12");
        return new MecanoDbContext(optionsBuilder.Options);
    }
}
