using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using SteamTogether.Core.Context;
using SteamTogether.Core.Options;
using SteamTogether.Core.Services;
using SteamTogether.Scraper.Options;
using SteamTogether.Scraper.Services;
using HttpServerOptions = SteamTogether.Core.Options.HttpServerOptions;

namespace SteamTogether.Scraper.UnitTests;

public class WorkerTests
{
    private HostBuilder _builder;

    public WorkerTests()
    {
        _builder = new HostBuilder();
        _builder
            .ConfigureServices((builder, services) =>
            {
                services
                    .Configure<HttpServerOptions>(opts =>
                    {
                        opts.Enabled = false;
                    })
                    .Configure<ScraperOptions>(opts =>
                    {
                        opts.RunOnStartup = false;
                        opts.Schedule = "* * * * * *";
                    });

                services
                    .AddOptions<DatabaseOptions>()
                    .Bind(builder.Configuration.GetSection(DatabaseOptions.Database));

                services
                    .AddScoped<IDateTimeService, DateTimeService>()
                    .AddScoped<IScraperService, ScraperService>();

                services
                    .AddLogging(x => x.AddConsole())
                    .AddDbContext<ApplicationDbContext>(
                        opt => { opt.UseInMemoryDatabase("test.db"); });
            });
    }

    [Fact]
    public async Task Worker_Should_Run_On_StartupTest()
    {
        var mockedScraperService = new Mock<IScraperService>();
        _builder.ConfigureServices(services =>
        {
            services.AddScoped<IScraperService>(_ => mockedScraperService.Object);
            services.AddSingleton<IHostedService, ScraperWorker>();
        });

        var host = _builder.Build();
        
        var worker = host.Services.GetRequiredService<IHostedService>();

        var source = new CancellationTokenSource();
        worker.StartAsync(source.Token);

        await Task.Delay(TimeSpan.FromSeconds(5));
        source.Cancel();
        
        mockedScraperService.Verify(x => x.RunSync(), times: Times.AtLeast(5));
    }
}