using SteamTogether.Bot.Services;
using SteamTogether.Core;
using SteamTogether.Core.Options;
using SteamTogether.Core.Services.Steam;
using SteamTogether.Scraper;
using SteamTogether.Scraper.Options;
using SteamTogether.Scraper.Services;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(
        (context, config) =>
        {
            config
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, true)
                .AddJsonFile(
                    $"appsettings.{context.HostingEnvironment.EnvironmentName}.json",
                    true,
                    true
                )
                .AddEnvironmentVariables(prefix: "SCRAPER_")
                .AddCommandLine(args);
        }
    )
    .ConfigureServices(
        (builder, services) =>
        {
            services.RegisterDataServices();

            services
                .AddOptions<ScraperOptions>()
                .Bind(builder.Configuration.GetSection(ScraperOptions.Scraper))
                .ValidateDataAnnotations();

            services.Configure<DatabaseOptions>(
                builder.Configuration.GetSection(DatabaseOptions.Database)
            );
            services.Configure<SteamOptions>(builder.Configuration.GetSection(SteamOptions.Steam));

            services.AddHttpClient();
            services.AddTransient<IDateTimeService, DateTimeService>();
            services.AddTransient<IScrapperService, ScrapperService>();
            services.AddTransient<ISteamService, SteamService>();
            services.AddHostedService<Worker>();
        }
    )
    .Build();

await host.RunAsync();
