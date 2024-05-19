using System.IO;
using System.Windows;
using System.Windows.Threading;

using CommunityToolkit.Mvvm.Messaging;

using MasterApplication.Feature.BookReviews;
using MasterApplication.Feature.Md5HashFileGenerator;
using MasterApplication.Feature.YoutubeAudioDownloader;
using MasterApplication.Menus.Other;
using MasterApplication.Services.Dialog;
using MasterApplication.Services.Feature.Md5Hash;

using MaterialDesignThemes.Wpf;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Serilog;
using Serilog.Events;

namespace MasterApplication;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    [STAThread]
    private static void Main(string[] args)
    {
        MainAsync(args).GetAwaiter().GetResult();
    }

    private static async Task MainAsync(string[] args)
    {
        string executingDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)!;
        Log.Logger = new LoggerConfiguration()
            .WriteTo.File(Path.Combine(executingDirectory, "Logs/log.txt"),
                rollingInterval: RollingInterval.Infinite,
                retainedFileCountLimit: null,
                fileSizeLimitBytes: 10485760, // 10 MB limit (adjust as needed)
                outputTemplate: "{Timestamp:dd-MM-yyyy HH:mm:ss.fff} [{Level}] {Message:}{NewLine}{Exception}")
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Debug()
            .CreateLogger();

        using IHost host = CreateHostBuilder(args).Build();
        await host.StartAsync().ConfigureAwait(true);

        Log.Logger.Information("------------------------------------");
        Log.Logger.Information("*** - APPLICATION INITIALIZING - ***");
        Log.Logger.Information("------------------------------------");
        App app = new();
        app.InitializeComponent();
        app.MainWindow = host.Services.GetRequiredService<MainWindow>();
        app.MainWindow.Visibility = Visibility.Visible;
        app.Run();

        await host.StopAsync().ConfigureAwait(true);
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
        .ConfigureAppConfiguration((hostBuilderContext, configurationBuilder)
            => configurationBuilder.AddUserSecrets(typeof(App).Assembly))
        .ConfigureServices((hostContext, services) =>
        {
            //Services
            services.AddSingleton<IDialogService, WindowsDialogService>();
            services.AddSingleton<IMd5HashFileGeneratorService, Md5HashFileGeneratorService>();

            //Menus
            services.AddSingleton<OtherViewModel>();

            //Features
            services.AddSingleton<Md5HashFileGeneratorViewModel>();
            services.AddSingleton<YoutubeAudioDownloaderViewModel>();
            services.AddSingleton<LinkViewModel>();
            services.AddSingleton<FileViewModel>();
            services.AddSingleton<BookReviewViewModel>();
            
            //Logging
            services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.ClearProviders();
                loggingBuilder.AddSerilog();
            });

            //Other
            services.AddSingleton<MainWindow>();
            services.AddSingleton<MainWindowViewModel>();
            services.AddSingleton<WeakReferenceMessenger>();
            services.AddSingleton<IMessenger, WeakReferenceMessenger>(provider => provider.GetRequiredService<WeakReferenceMessenger>());
            services.AddSingleton(_ => Current.Dispatcher);
            services.AddTransient<ISnackbarMessageQueue>(provider =>
            {
                Dispatcher dispatcher = provider.GetRequiredService<Dispatcher>();
                return new SnackbarMessageQueue(TimeSpan.FromSeconds(1.5), dispatcher);
            });
        });
}
