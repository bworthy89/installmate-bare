using InstallVibe.Core.Services.Activation;
using InstallVibe.Core.Services.Cache;
using InstallVibe.Core.Services.Data;
using InstallVibe.Core.Services.Engine;
using InstallVibe.Core.Services.Media;
using InstallVibe.Core.Services.Settings;
using InstallVibe.Core.Services.SharePoint;
using InstallVibe.Data.Context;
using InstallVibe.Data.Repositories;
using InstallVibe.Infrastructure.Configuration;
using InstallVibe.Infrastructure.Device;
using InstallVibe.Infrastructure.Security.Cryptography;
using InstallVibe.Infrastructure.Security.Graph;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace InstallVibe.Core.Extensions;

/// <summary>
/// Extension methods for registering InstallVibe services in the DI container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all InstallVibe core services.
    /// </summary>
    public static IServiceCollection AddInstallVibeServices(this IServiceCollection services, SharePointConfiguration spConfig)
    {
        // Configuration
        services.AddSingleton(spConfig);

        // Database
        services.AddDbContext<InstallVibeContext>(options =>
        {
            var dbPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "InstallVibe", "Data", "installvibe.db");
            options.UseSqlite($"Data Source={dbPath}");
        });

        // Infrastructure Services
        services.AddSingleton<IDeviceIdProvider, DeviceIdProvider>();
        services.AddSingleton<IHashService, HashService>();
        services.AddSingleton<IDpapiEncryption, DpapiEncryption>();
        services.AddSingleton<IRsaValidator, RsaValidator>();
        services.AddSingleton<IGraphClientFactory, GraphClientFactory>();

        // Repositories
        services.AddScoped<ISettingsRepository, SettingsRepository>();

        // Core Services
        services.AddScoped<IGuideService, GuideService>();
        services.AddScoped<IProgressService, ProgressService>();
        services.AddScoped<IMediaService, MediaService>();
        services.AddScoped<ICacheService, CacheService>();
        services.AddScoped<ISharePointService, SharePointService>();
        services.AddScoped<ISettingsService, SettingsService>();
        services.AddScoped<IGuideEngine, GuideEngine>();

        // Activation Services
        services.AddSingleton<IProductKeyValidator, ProductKeyValidator>();
        services.AddSingleton<ITokenManager, TokenManager>();
        services.AddSingleton<ILicenseManager, LicenseManager>();
        services.AddScoped<IActivationService, ActivationService>();

        return services;
    }
}
