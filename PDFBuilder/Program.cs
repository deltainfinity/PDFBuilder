using System.IO;
using System.Reflection;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PDFBuilder.Config;
using Serilog;

namespace PDFBuilder
{
    /// <summary>
    /// Handles application configuration tasks
    /// </summary>
    public class Program
    {
        private static readonly ILogger Logger = Log.ForContext<Program>();

        /// <summary>
        /// Working directory the application launched from
        /// </summary>
        public static string WorkingDirectory => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        /// <summary>
        /// .NET Configuration Service
        /// </summary>
        public static IConfiguration Configuration => new ConfigurationBuilder()
            .SetBasePath(WorkingDirectory)
            .AddJsonFile("appsettings.json", false, true)
            .AddEnvironmentVariables()
            .Build();

        /// <summary>
        /// Entry point for the application
        /// </summary>
        /// <param name="args">Command line arguments (will be ignored)</param>
        public static void Main(string[] args)
        {
            var serviceCollection = new ServiceCollection();

            SerilogConfig.Configure(serviceCollection, Configuration);

            Logger.Debug("Starting create host builder");
            CreateHostBuilder(args).Build().Run();
            Logger.Debug("Web host built and configured");
        }

        /// <summary>
        /// Create the Kestrel web host 
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                .UseSerilog()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseConfiguration(Configuration);
                    webBuilder.UseIISIntegration();
                    webBuilder.UseStartup<Startup>();
                });
    }
}
