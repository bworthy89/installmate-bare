using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using InstallVibe.Infrastructure.Constants;

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
        optionsBuilder.UseSqlite($"Data Source={PathConstants.DatabasePath}");

        return new InstallVibeContext(optionsBuilder.Options);
    }
}
