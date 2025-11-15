using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace InstallVibe.Data.Context;

/// <summary>
/// Design-time factory for Entity Framework Core migrations.
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<InstallVibeContext>
{
    public InstallVibeContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<InstallVibeContext>();

        // Use the actual database path for migrations
        // This mirrors PathConstants.DatabasePath but avoids circular dependency
        var appDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "InstallVibe");
        var dataPath = Path.Combine(appDataPath, "Data");
        var databasePath = Path.Combine(dataPath, "installvibe.db");

        optionsBuilder.UseSqlite($"Data Source={databasePath}");

        return new InstallVibeContext(optionsBuilder.Options);
    }
}
