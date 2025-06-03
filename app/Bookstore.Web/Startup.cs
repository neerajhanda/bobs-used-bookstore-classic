using Microsoft.AspNetCore.Owin;
using Microsoft.Owin;
using Owin;


[assembly: OwinStartup(typeof(Bookstore.Web.Startup))]

namespace Bookstore.Web
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // Initialize logging
            // LoggingSetup.ConfigureLogging();

            // ConfigurationSetup.ConfigureConfiguration();

            DependencyInjectionSetup.ConfigureDependencyInjection(app);

            AuthenticationConfig.ConfigureAuthentication(app);
        }
    }

    public static class DependencyInjectionSetup
    {
        public static void ConfigureDependencyInjection(IAppBuilder app)
        {
            // TODO: Configure dependency injection here
        }
    }

    public static class AuthenticationConfig
    {
        public static void ConfigureAuthentication(IAppBuilder app)
        {
            // TODO: Configure authentication here
        }
    }
}