using Microsoft.Extensions.Options;
using SteamTogether.Bot;
using SteamTogether.Bot.Options;
using SteamTogether.Bot.Services;
using SteamTogether.Bot.Services.Command.Handlers;
using SteamTogether.Bot.Services.Command.Parser;
using SteamTogether.Core;
using SteamTogether.Core.Options;
using SteamTogether.Core.Services.Steam;
using Telegram.Bot;

var host = Host.CreateDefaultBuilder()
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
                .AddEnvironmentVariables(prefix: "BOT_")
                .AddCommandLine(args);
        }
    )
    .ConfigureServices(
        (builder, services) =>
        {
            services
                .AddOptions<DatabaseOptions>()
                .Bind(builder.Configuration.GetSection(DatabaseOptions.Database))
                .ValidateDataAnnotations();
            services
                .AddOptions<TelegramOptions>()
                .Bind(builder.Configuration.GetSection(TelegramOptions.Telegram))
                .ValidateDataAnnotations();
            services
                .AddOptions<SteamOptions>()
                .Bind(builder.Configuration.GetSection(SteamOptions.Steam))
                .ValidateDataAnnotations();

            services.RegisterDataServices();

            services.Configure<TelegramOptions>(
                builder.Configuration.GetSection(TelegramOptions.Telegram)
            );
            services.Configure<DatabaseOptions>(
                builder.Configuration.GetSection(DatabaseOptions.Database)
            );
            services.Configure<SteamOptions>(builder.Configuration.GetSection(SteamOptions.Steam));

            services.AddHttpClient();
            services
                .AddHttpClient(nameof(TelegramBotClient))
                .AddTypedClient<ITelegramBotClient>(
                    (httpClient, sp) =>
                    {
                        var token = sp.GetService<IOptions<TelegramOptions>>()?.Value.Token;
                        if (string.IsNullOrEmpty(token))
                        {
                            throw new InvalidOperationException("Telegram token is not set");
                        }

                        return new TelegramBotClient(token, httpClient);
                    }
                )
                .SetHandlerLifetime(TimeSpan.FromMinutes(5));

            services.AddScoped<ITelegramCommandParser, TelegramCommandParser>();
            services.AddScoped<ITelegramCommandHandler, TelegramCommandHandler>();
            services.AddScoped<ITelegramService, TelegramService>();
            services.AddScoped<ISteamService, SteamService>();

            services.AddHostedService<PollingWorker>();
        }
    )
    .Build();

await host.RunAsync();
