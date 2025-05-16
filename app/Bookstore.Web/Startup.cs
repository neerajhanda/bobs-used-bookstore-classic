using Microsoft.AspNetCore.Owin;
using Microsoft.Owin;
using Owin;
using NLog;
using System;

[assembly: OwinStartup(typeof(Bookstore.Web.Startup))]

namespace Bookstore.Web
{
    public static class LoggingSetup
    {
        public static void ConfigureLogging()
        {
            // Configure logging
            try
            {
                LogManager.Setup().LoadConfiguration(builder => {
                    builder.ForLogger().FilterMinLevel(NLog.LogLevel.Info).WriteToConsole();
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error configuring logging: {ex}");
            }
        }
    }

    public static class ConfigurationSetup
    {
        public static void ConfigureConfiguration()
        {
            // Configuration setup
        }
    }

    public static class DependencyInjectionSetup
    {
        public static void ConfigureDependencyInjection(IAppBuilder app)
        {
            // Dependency injection setup
        }
    }

    public static class AuthenticationConfig
    {
        public static void ConfigureAuthentication(IAppBuilder app)
        {
            // Authentication configuration
        }
    }

    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            LoggingSetup.ConfigureLogging();

            ConfigurationSetup.ConfigureConfiguration();

            DependencyInjectionSetup.ConfigureDependencyInjection(app);

            AuthenticationConfig.ConfigureAuthentication(app);
        }
    }
}