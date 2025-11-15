using Microsoft.EntityFrameworkCore;
using InstallVibe.Data.Entities;

namespace InstallVibe.Data.Context;

/// <summary>
/// Entity Framework Core database context for InstallVibe.
/// </summary>
public class InstallVibeContext : DbContext
{
    public DbSet<GuideEntity> Guides { get; set; } = null!;
    public DbSet<StepEntity> Steps { get; set; } = null!;
    public DbSet<MediaCacheEntity> MediaCache { get; set; } = null!;
    public DbSet<ProgressEntity> Progress { get; set; } = null!;
    public DbSet<SyncMetadataEntity> SyncMetadata { get; set; } = null!;
    public DbSet<SettingEntity> Settings { get; set; } = null!;
    public DbSet<FavoriteEntity> Favorites { get; set; } = null!;

    public InstallVibeContext(DbContextOptions<InstallVibeContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure GuideEntity
        modelBuilder.Entity<GuideEntity>(entity =>
        {
            entity.HasKey(e => e.GuideId);

            entity.HasIndex(e => e.Category);
            entity.HasIndex(e => e.SyncStatus);
            entity.HasIndex(e => e.LastModified);

            entity.HasMany(e => e.Steps)
                .WithOne(e => e.Guide)
                .HasForeignKey(e => e.GuideId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.Progress)
                .WithOne(e => e.Guide)
                .HasForeignKey(e => e.GuideId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.SyncStatus)
                .HasDefaultValue("synced");
        });

        // Configure StepEntity
        modelBuilder.Entity<StepEntity>(entity =>
        {
            entity.HasKey(e => e.StepId);

            entity.HasIndex(e => new { e.GuideId, e.StepNumber });
        });

        // Configure MediaCacheEntity
        modelBuilder.Entity<MediaCacheEntity>(entity =>
        {
            entity.HasKey(e => e.MediaId);

            entity.HasIndex(e => e.LastAccessed);
            entity.HasIndex(e => e.Category);
            entity.HasIndex(e => e.FileSize);

            entity.Property(e => e.CachedDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.LastAccessed)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        // Configure ProgressEntity
        modelBuilder.Entity<ProgressEntity>(entity =>
        {
            entity.HasKey(e => e.ProgressId);

            entity.HasIndex(e => e.GuideId);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.LastUpdated);

            entity.Property(e => e.StartedDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.LastUpdated)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        // Configure SyncMetadataEntity
        modelBuilder.Entity<SyncMetadataEntity>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasIndex(e => new { e.EntityType, e.EntityId });
            entity.HasIndex(e => e.SyncStatus);

            entity.Property(e => e.SyncStatus)
                .HasDefaultValue("synced");
        });

        // Configure SettingEntity
        modelBuilder.Entity<SettingEntity>(entity =>
        {
            entity.HasKey(e => e.Key);

            entity.HasIndex(e => e.Category);

            entity.Property(e => e.LastModified)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        // Configure FavoriteEntity
        modelBuilder.Entity<FavoriteEntity>(entity =>
        {
            entity.HasKey(e => e.FavoriteId);

            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => new { e.UserId, e.GuideId }).IsUnique();
            entity.HasIndex(e => new { e.UserId, e.SortOrder });

            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.SortOrder)
                .HasDefaultValue(0);
        });
    }
}
