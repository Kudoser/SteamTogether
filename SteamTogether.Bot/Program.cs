using Microsoft.Extensions.Options;
using SteamTogether.Bot;
using SteamTogether.Bot.Options;
using SteamTogether.Bot.Services;
using Telegram.Bot;

var host = Host.CreateDefaultBuilder()
    .ConfigureAppConfiguration((context, config) =>
    {
        config.SetBasePath(Directory.GetCurrentDirectory());
        config.AddJsonFile("appsettings.json", optional: false, true);
        config.AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", true, true);
        config.AddEnvironmentVariables(prefix: "BOT_");
        config.AddCommandLine(args);
    })
    .ConfigureServices((builder, services) =>
    {
        services.Configure<TelegramOptions>(builder.Configuration.GetSection(TelegramOptions.Telegram));

        services
            .AddHttpClient(nameof(TelegramBotClient))
            .AddTypedClient<ITelegramBotClient>((httpClient, sp) =>
            {
                var opts = sp.GetService<IOptions<TelegramOptions>>();
                var telegramOpts = new TelegramBotClientOptions(opts.Value.Token);
                return new TelegramBotClient(telegramOpts, httpClient);
            })
            .SetHandlerLifetime(TimeSpan.FromMinutes(5));

        services.AddScoped<ITelegramService, TelegramService>();
        services.AddHostedService<PollingWorker>();
    })
    .Build();

await host.RunAsync();