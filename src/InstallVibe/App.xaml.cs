using InstallVibe.Core.Services.Activation;
using InstallVibe.Core.Services.Cache;
using InstallVibe.Core.Services.Data;
using InstallVibe.Core.Services.Engine;
using InstallVibe.Core.Services.Export;
using InstallVibe.Core.Services.Media;
using InstallVibe.Core.Services.OneDrive;
using InstallVibe.Core.Services.SharePoint;
using InstallVibe.Core.Services.User;
using InstallVibe.Data.Context;
using InstallVibe.Data.Seeders;
using InstallVibe.Infrastructure.Configuration;
using InstallVibe.Infrastructure.Device;
using InstallVibe.Infrastructure.Security.Cryptography;
using InstallVibe.Infrastructure.Security.Graph;
using InstallVibe.Services.Navigation;
using InstallVibe.ViewModels.Activation;
using InstallVibe.ViewModels.Admin;
using InstallVibe.ViewModels.Dashboard;
using InstallVibe.ViewModels.Guides;
using InstallVibe.ViewModels.Settings;
using InstallVibe.ViewModels.Setup;
using InstallVibe.ViewModels.Shell;
using InstallVibe.Views.Activation;
using InstallVibe.Views.Admin;
using InstallVibe.Views.Dashboard;
using InstallVibe.Views.Guides;
using InstallVibe.Views.Settings;
using InstallVibe.Views.Setup;
using InstallVibe.Views.Shell;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;
using Serilog;

namespace InstallVibe;

/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
public partial class App : Application
{
    private Window? _window;
    private readonly IServiceProvider _serviceProvider;

    public new static App Current => (App)Application.Current;

    public IServiceProvider Services => _serviceProvider;

    public static T GetService<T>() where T : class
    {
        return Current.Services.GetRequiredService<T>();
    }

    public App()
    {
        InitializeComponent();

        // Build configuration
        var configuration = BuildConfiguration();

        // Setup Serilog
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .Enrich.FromLogContext()
            .WriteTo.File(
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "InstallVibe", "Logs", "installvibe-.log"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7)
            .WriteTo.Debug()
            .CreateLogger();

        // Build service provider
        _serviceProvider = ConfigureServices(configuration);

        Log.Information("InstallVibe application starting...");
    }

    protected override async void OnLaunched(LaunchActivatedEventArgs args)
    {
        _window = _serviceProvider.GetRequiredService<MainWindow>();

        // Initialize database and seed sample data
        await InitializeDatabaseAsync();

        // Check activation status and navigate accordingly
        var activationService = _serviceProvider.GetRequiredService<IActivationService>();
        var navigationService = _serviceProvider.GetRequiredService<INavigationService>();

        try
        {
            var licenseInfo = await activationService.GetLicenseInfoAsync();

            if (licenseInfo.IsActivated)
            {
                Log.Information("App is activated - navigating to Dashboard via Shell");
                navigationService.NavigateTo("Shell", "Dashboard");
            }
            else
            {
                Log.Information("App is not activated - starting setup wizard");
                navigationService.NavigateTo("WelcomeSetup");
            }
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Could not determine activation status - starting setup wizard");
            navigationService.NavigateTo("WelcomeSetup");
        }

        // Note: OneDrive sync disabled - using manual import/export mode
        // await StartOneDriveSyncIfEnabledAsync();

        _window.Activate();
    }

    private async Task StartOneDriveSyncIfEnabledAsync()
    {
        try
        {
            var oneDriveService = _serviceProvider.GetRequiredService<IOneDriveSyncService>();
            var settings = await oneDriveService.GetSettingsAsync();

            if (settings.Enabled)
            {
                Log.Information("OneDrive sync is enabled");

                // Run initial sync on startup if configured
                if (settings.SyncOnStartup)
                {
                    Log.Information("Running initial OneDrive sync on startup");
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            var result = await oneDriveService.SyncNowAsync();
                            Log.Information(
                                "Startup OneDrive sync completed: Downloaded={Downloaded}, Imported={Imported}, Failed={Failed}",
                                result.FilesDownloaded,
                                result.FilesImported,
                                result.FilesFailed);
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex, "Error during startup OneDrive sync");
                        }
                    });
                }

                // Start auto-sync timer
                await oneDriveService.StartAutoSyncAsync();
                Log.Information("OneDrive auto-sync started");
            }
            else
            {
                Log.Information("OneDrive sync is disabled");
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error starting OneDrive sync");
        }
    }

    private async Task InitializeDatabaseAsync()
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<InstallVibeContext>();

            // Ensure database is created and migrations are applied
            Log.Information("Applying database migrations...");
            await context.Database.MigrateAsync();
            Log.Information("Database migrations applied successfully");

            // Seed sample data if database is empty
            var guideService = scope.ServiceProvider.GetRequiredService<IGuideService>();
            var seeder = new SampleDataSeeder(guideService);
            await seeder.SeedAsync();
            Log.Information("Sample data seeding completed");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to initialize database");
            throw;
        }
    }

    private IConfiguration BuildConfiguration()
    {
        return new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production"}.json", optional: true)
            .Build();
    }

    private IServiceProvider ConfigureServices(IConfiguration configuration)
    {
        var services = new ServiceCollection();

        // Logging
        services.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.AddSerilog(dispose: true);
        });

        // Configuration
        var sharePointConfig = configuration.GetSection("SharePoint").Get<SharePointConfiguration>()
            ?? new SharePointConfiguration();
        services.AddSingleton(sharePointConfig);

        var appSettings = configuration.GetSection("AppSettings").Get<Core.Models.Settings.AppSettings>()
            ?? new Core.Models.Settings.AppSettings();
        services.AddSingleton(appSettings);

        var oneDriveSettings = configuration.GetSection("OneDrive").Get<Core.Models.Settings.OneDriveSyncSettings>()
            ?? new Core.Models.Settings.OneDriveSyncSettings();
        services.AddSingleton(oneDriveSettings);

        // Ensure all application directories exist
        Infrastructure.Constants.PathConstants.EnsureDirectoriesExist();
        Log.Information("Application directories initialized");

        // Database
        var dbPath = Infrastructure.Constants.PathConstants.DatabasePath;

        services.AddDbContext<InstallVibeContext>(options =>
        {
            options.UseSqlite($"Data Source={dbPath}");
        });

        // Infrastructure Services
        services.AddSingleton<IHashService, HashService>();
        services.AddSingleton<IRsaValidator, RsaValidator>();
        services.AddSingleton<IDpapiEncryption, DpapiEncryption>();
        services.AddSingleton<IDeviceIdProvider, DeviceIdProvider>();
        services.AddSingleton<IGraphClientFactory, GraphClientFactory>();

        // Core Services
        services.AddScoped<ICacheService, CacheService>();
        services.AddScoped<IGuideService, GuideService>();
        services.AddScoped<IProgressService, ProgressService>();
        services.AddScoped<IFavoritesService, FavoritesService>();
        services.AddScoped<IBackupService, BackupService>();
        services.AddSingleton<IUserService, UserService>();
        services.AddScoped<IProductKeyValidator, ProductKeyValidator>();
        services.AddScoped<ITokenManager, TokenManager>();
        services.AddScoped<IActivationService, ActivationService>();
        services.AddScoped<ILicenseManager, LicenseManager>();

        // SharePoint Service - Use feature flag to determine implementation
        if (appSettings.UseSharePoint)
        {
            services.AddScoped<ISharePointService, SharePointService>();
            Log.Information("Using SharePoint integration mode");
        }
        else
        {
            services.AddScoped<ISharePointService, NoOpSharePointService>();
            Log.Information("Using local-only mode (SharePoint disabled)");
        }

        services.AddScoped<IMediaService, MediaService>();
        services.AddScoped<IGuideEngine, GuideEngine>();
        services.AddScoped<IGuideArchiveService, GuideArchiveService>();
        services.AddSingleton<IOneDriveSyncService, OneDriveSyncService>();

        // UI Services
        services.AddSingleton<INavigationService>(sp =>
        {
            var nav = new NavigationService(sp.GetRequiredService<ILogger<NavigationService>>());

            // Register pages
            nav.RegisterPage<ShellPage>("Shell");
            nav.RegisterPage<WelcomeSetupPage>("WelcomeSetup");
            nav.RegisterPage<ActivationPage>("Activation");
            nav.RegisterPage<ActivationPage>("LicenseSetup"); // Alias for activation page in setup context
            nav.RegisterPage<DashboardPage>("Dashboard");
            nav.RegisterPage<GuideListPage>("GuideList");
            nav.RegisterPage<GuideDetailPage>("GuideDetail");
            nav.RegisterPage<GuideEditorPage>("GuideEditor");
            nav.RegisterPage<StepPage>("Step");
            nav.RegisterPage<SettingsPage>("Settings");
            nav.RegisterPage<AdminEditorPage>("AdminEditor");

            return nav;
        });

        // ViewModels
        services.AddTransient<ShellViewModel>();
        services.AddTransient<WelcomeSetupViewModel>();
        services.AddTransient<ActivationViewModel>();
        services.AddTransient<DashboardViewModel>();
        services.AddTransient<GuideListViewModel>();
        services.AddTransient<GuideDetailViewModel>();
        services.AddTransient<GuideEditorViewModel>();
        services.AddTransient<StepViewModel>();
        services.AddTransient<SettingsViewModel>();
        services.AddTransient<AdminEditorViewModel>();

        // Views
        services.AddTransient<ShellPage>();
        services.AddTransient<WelcomeSetupPage>();
        services.AddTransient<ActivationPage>();
        services.AddTransient<DashboardPage>();
        services.AddTransient<GuideListPage>();
        services.AddTransient<GuideDetailPage>();
        services.AddTransient<GuideEditorPage>();
        services.AddTransient<StepPage>();
        services.AddTransient<SettingsPage>();
        services.AddTransient<AdminEditorPage>();

        // MainWindow
        services.AddSingleton<MainWindow>();

        return services.BuildServiceProvider();
    }
}
