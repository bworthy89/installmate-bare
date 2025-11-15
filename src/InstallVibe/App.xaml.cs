using InstallVibe.Core.Configuration;
using InstallVibe.Core.Interfaces.Device;
using InstallVibe.Core.Interfaces.Security;
using InstallVibe.Core.Services.Activation;
using InstallVibe.Core.Services.Cache;
using InstallVibe.Core.Services.Data;
using InstallVibe.Core.Services.Editor;
using InstallVibe.Core.Services.Engine;
using InstallVibe.Core.Services.Media;
using InstallVibe.Core.Services.SharePoint;
using InstallVibe.Core.Services.Update;
using InstallVibe.Data.Context;
using InstallVibe.Infrastructure.Device;
using InstallVibe.Infrastructure.Security.Cryptography;
using InstallVibe.Infrastructure.Security.Encryption;
using InstallVibe.Infrastructure.Security.Graph;
using InstallVibe.Services.Navigation;
using InstallVibe.ViewModels.About;
using InstallVibe.ViewModels.Activation;
using InstallVibe.ViewModels.Editor;
using InstallVibe.ViewModels.Guides;
using InstallVibe.ViewModels.Settings;
using InstallVibe.Views.About;
using InstallVibe.Views.Activation;
using InstallVibe.Views.Editor;
using InstallVibe.Views.Guides;
using InstallVibe.Views.Settings;
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

    /// <summary>
    /// Helper method to get a service from the service provider.
    /// </summary>
    public static T GetService<T>() where T : class
    {
        return ((App)Application.Current).Services.GetRequiredService<T>();
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

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        _window = _serviceProvider.GetRequiredService<MainWindow>();
        _window.Activate();
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
        services.AddSingleton(configuration);
        var sharePointConfig = configuration.GetSection("SharePoint").Get<SharePointConfiguration>()
            ?? new SharePointConfiguration();
        services.AddSingleton(sharePointConfig);

        // HttpClient for update service
        services.AddHttpClient("UpdateClient", client =>
        {
            client.Timeout = TimeSpan.FromMinutes(10);
            client.DefaultRequestHeaders.Add("User-Agent", "InstallVibe-UpdateClient/1.0");
        });

        // Database
        var dbPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "InstallVibe", "Data", "installvibe.db");

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
        services.AddScoped<IProductKeyValidator, ProductKeyValidator>();
        services.AddScoped<ITokenManager, TokenManager>();
        services.AddScoped<IActivationService, ActivationService>();
        services.AddScoped<ILicenseManager, LicenseManager>();
        services.AddScoped<ISharePointService, SharePointService>();
        services.AddScoped<IMediaService, MediaService>();
        services.AddScoped<IGuideEngine, GuideEngine>();

        // Editor Services
        services.AddScoped<IGuideEditorService, GuideEditorService>();
        services.AddScoped<IMediaUploadService, MediaUploadService>();

        // Update Service
        services.AddSingleton<IUpdateService, UpdateService>();

        // UI Services
        services.AddSingleton<INavigationService>(sp =>
        {
            var nav = new NavigationService(sp.GetRequiredService<ILogger<NavigationService>>());

            // Register pages
            nav.RegisterPage<ActivationPage>("Activation");
            nav.RegisterPage<GuidesPage>("Guides");
            nav.RegisterPage<GuideDetailPage>("GuideDetail");
            nav.RegisterPage<SettingsPage>("Settings");
            nav.RegisterPage<AboutPage>("About");
            nav.RegisterPage<GuideEditorPage>("GuideEditor");

            return nav;
        });

        // ViewModels
        services.AddTransient<ActivationViewModel>();
        services.AddTransient<GuidesViewModel>();
        services.AddTransient<GuideDetailViewModel>();
        services.AddTransient<SettingsViewModel>();
        services.AddTransient<AboutViewModel>();

        // Editor ViewModels
        services.AddTransient<GuideEditorViewModel>();
        services.AddTransient<StepEditorViewModel>();

        // Views
        services.AddTransient<ActivationPage>();
        services.AddTransient<GuidesPage>();
        services.AddTransient<GuideDetailPage>();
        services.AddTransient<SettingsPage>();
        services.AddTransient<AboutPage>();

        // Editor Views
        services.AddTransient<GuideEditorPage>();

        // Shell
        services.AddTransient<ShellPage>();

        // MainWindow
        services.AddSingleton<MainWindow>();

        return services.BuildServiceProvider();
    }
}
