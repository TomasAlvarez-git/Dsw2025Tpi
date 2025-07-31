namespace Dsw2025Tpi.Api.Helpers
{
    public static class LoggingServiceExtensions
    {
        public static IServiceCollection AddLoggingService(this IServiceCollection services, IConfiguration config)
        {
            var path = config.GetSection("LogPath").Value;

            services.AddLogging(config =>
            {
                config.ClearProviders();

                config.AddConsole(consoleOptions =>
                {
                    consoleOptions.TimestampFormat = "[dd-MM-yyyy]-[HH:mm:ss] ";
                    consoleOptions.IncludeScopes = true; // Si usas scopes y quieres verlos
                    consoleOptions.Format = Microsoft.Extensions.Logging.Console.ConsoleLoggerFormat.Systemd;
                    // Opciones: Default, Systemd, Json
                });

                config.AddFile(
                    path,
                    outputTemplate: "{Timestamp:[dd-MM-yyyy]-[HH:mm:ss.fff]} [{Level:u3}] {SourceContext} {Message:lj}{NewLine}{Exception}",
                    minimumLevel: LogLevel.Debug,
                    levelOverrides: new Dictionary<string, LogLevel>
                    {
                        ["Microsoft"] = LogLevel.Information,
                        ["Microsoft.AspNetCore"] = LogLevel.Warning,
                        ["Microsoft.EntityFrameworkCore"] = LogLevel.Warning
                    }
                );
            });

            return services;
        }
    }
}
