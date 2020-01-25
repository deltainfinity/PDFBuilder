using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using Serilog.Filters;

namespace PDFBuilder.Config
{
    /// <summary>
    /// Configure Serilog
    /// </summary>
    public class SerilogConfig
    {
        /// <summary>
        /// Static method to configure Serilog during application startup
        /// </summary>
        /// <param name="serviceCollection">ServiceCollection</param>
        /// <param name="configuration">Configuration built from appsettings.json</param>
        public static void Configure(IServiceCollection serviceCollection, IConfiguration configuration)
        {
            var baseLogPath = configuration["Logging:Path"];
            if (string.IsNullOrEmpty(baseLogPath))
            {
                throw new Exception("Logging:Path is not found in appsettings.json");
            }

            var deploymentMode = configuration["DeploymentMode"];
            if (string.IsNullOrEmpty(deploymentMode))
            {
                throw new Exception("DeploymentMode is not found in appsettings.json ");
            }

            var appName = configuration["ApplicationName"];
            if (string.IsNullOrEmpty(appName))
            {
                throw new Exception("ApplicationName is not found in appsettings.json");
            }

            var datadogAPIKey = configuration["DatadogAPIKey"];
            if (string.IsNullOrWhiteSpace(datadogAPIKey))
            {
                throw new Exception("DatadogAPIKey is not set in the web.config.");
            }

            var appLogName = $"{appName}--{deploymentMode}";
            var detailedLogFile = baseLogPath + "\\" + appLogName + $"--{DateTime.Now:yyyy-MM-dd}.log";

            var outputTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level}] <" + deploymentMode + "|{SourceContext}|{CorrelationId}> {Message}{NewLine}{NewLine}{Exception}{NewLine}";

            switch (deploymentMode.ToUpper())
            {
                //write to the local file system
                case "LOCAL":
                    Log.Logger = new LoggerConfiguration()
                        .Enrich.FromLogContext()
                        .Enrich.WithMachineName()
                        .Enrich.WithEnvironmentUserName()
                        .MinimumLevel.Verbose()
                        .Filter.ByExcluding(Matching.FromSource("Microsoft.AspNetCore.DataProtection"))
                        .Filter.ByExcluding(Matching.FromSource("Microsoft.AspNetCore.Mvc"))
                        .Filter.ByExcluding(Matching.FromSource("Microsoft.AspNetCore.Server.Kestrel"))
                        .Filter.ByExcluding(Matching.FromSource("Microsoft.AspNetCore.StaticFiles.StaticFileMiddleware"))
                        .Filter.ByExcluding(Matching.FromSource("Microsoft.AspNetCore.Routing.Matching.DfaMatcher"))
                        .Filter.ByExcluding(Matching.FromSource("Microsoft.AspNetCore.Routing.RouteValuesAddress"))
                        .Filter.ByExcluding(Matching.FromSource("Microsoft.AspNetCore.Hosting.Internal.WebHost"))
                        .WriteTo.File(detailedLogFile, LogEventLevel.Debug, outputTemplate, rollingInterval: RollingInterval.Day, fileSizeLimitBytes: 1024 * 1024 * 100) // 100MB
                        .CreateLogger();
                    break;

                case "DEV":
                    Log.Logger = new LoggerConfiguration()
                        .Enrich.FromLogContext()
                        .Enrich.WithMachineName()
                        .Enrich.WithEnvironmentUserName()
                        .MinimumLevel.Debug()
                        .Filter.ByExcluding(Matching.FromSource("Microsoft.AspNetCore.DataProtection"))
                        .Filter.ByExcluding(Matching.FromSource("Microsoft.AspNetCore.Mvc"))
                        .Filter.ByExcluding(Matching.FromSource("Microsoft.AspNetCore.Server.Kestrel"))
                        .Filter.ByExcluding(Matching.FromSource("Microsoft.AspNetCore.StaticFiles.StaticFileMiddleware"))
                        .Filter.ByExcluding(Matching.FromSource("Microsoft.AspNetCore.Routing.Matching.DfaMatcher"))
                        .Filter.ByExcluding(Matching.FromSource("Microsoft.AspNetCore.Routing.RouteValuesAddress"))
                        .Filter.ByExcluding(Matching.FromSource("Microsoft.AspNetCore.Hosting.Internal.WebHost"))
                        .WriteTo.DatadogLogs(datadogAPIKey, "Serilog", appName, appName, new[] { $"environment:{deploymentMode}" }, null, null, LogEventLevel.Information)
                        .CreateLogger();
                    break;
                case "PROD":
                    Log.Logger = new LoggerConfiguration()
                        .Enrich.FromLogContext()
                        .Enrich.WithMachineName()
                        .Enrich.WithEnvironmentUserName()
                        .MinimumLevel.Debug()
                        .Filter.ByExcluding(Matching.FromSource("Microsoft.AspNetCore.DataProtection"))
                        .Filter.ByExcluding(Matching.FromSource("Microsoft.AspNetCore.Mvc"))
                        .Filter.ByExcluding(Matching.FromSource("Microsoft.AspNetCore.Server.Kestrel"))
                        .Filter.ByExcluding(Matching.FromSource("Microsoft.AspNetCore.StaticFiles.StaticFileMiddleware"))
                        .Filter.ByExcluding(Matching.FromSource("Microsoft.AspNetCore.Routing.Matching.DfaMatcher"))
                        .Filter.ByExcluding(Matching.FromSource("Microsoft.AspNetCore.Routing.RouteValuesAddress"))
                        .Filter.ByExcluding(Matching.FromSource("Microsoft.AspNetCore.Hosting.Internal.WebHost"))
                        .WriteTo.DatadogLogs(datadogAPIKey, "Serilog", appName, appName, new[] { $"environment:{deploymentMode}" }, null, null, LogEventLevel.Information)
                        .CreateLogger();
                    break;
                        
                default:
                    throw new IndexOutOfRangeException($"Unsupported deployment mode encountered: {deploymentMode}");
            }

            serviceCollection.AddLogging(l => l.AddSerilog(dispose: true));

            var logger = Log.ForContext<SerilogConfig>();
            logger.Information(deploymentMode.ToUpper() == "LOCAL"
                ? $"Detailed log file is being written to {detailedLogFile}"
                : $"Detailed log data is being sent to Datadog under the service name {appName}");

            logger.Debug("Serilog configuration complete");
        }
    }
}
