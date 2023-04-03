using SteamTogether.Bot.Services;
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
            services.Configure<ScraperOptions>(
                builder.Configuration.GetSection(ScraperOptions.Scraper)
            );

            services.AddTransient<IDateTimeService, DateTimeService>();
            services.AddTransient<IScrapperService, ScrapperService>();
            services.AddHostedService<Worker>();
        }
    )
    .Build();

await host.RunAsync();
