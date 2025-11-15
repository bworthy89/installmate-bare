using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace InstallVibe.Data.Context;

/// <summary>
/// Design-time factory for Entity Framework Core migrations.
/// Used by EF Core tools for creating migrations and updating database.
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<InstallVibeContext>
{
    public InstallVibeContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<InstallVibeContext>();

        // Use a design-time database path for migrations
        // The actual runtime path is configured in App.xaml.cs
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var dbPath = Path.Combine(localAppData, "InstallVibe", "Data", "installvibe.db");

        optionsBuilder.UseSqlite($"Data Source={dbPath}");

        return new InstallVibeContext(optionsBuilder.Options);
    }
}
