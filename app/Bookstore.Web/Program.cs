
    using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Routing;
using EntityFramework = System.Data.Entity;

    namespace Bookstore
    {
        public class Program
        {
            public static void Main(string[] args)
            {
                var builder = WebApplication.CreateBuilder(args);

                // Add configuration from appsettings.json and other sources
                builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
                    .AddEnvironmentVariables();

                // Store configuration in static ConfigurationManager
                ConfigurationManager.Configuration = builder.Configuration;

                // Configure services
                builder.Services.AddControllersWithViews()
                    .AddMvcOptions(options => {
                        // Enable client validation (from Web.config)
                        options.ModelBindingMessageProvider.SetValueMustNotBeNullAccessor(_ => "The field is required.");
                    });

                // Configure views
                builder.Services.Configure<MvcViewOptions>(options => {
                    options.HtmlHelperOptions.ClientValidationEnabled = true;
                });

                // Register connection strings
                // Add the connection string from Web.config
                var connectionString = builder.Configuration.GetConnectionString("BookstoreDatabaseConnection")
                    ?? "Server=(localdb)\\MSSQLLocalDB;Initial Catalog=BookStoreClassic;MultipleActiveResultSets=true;Integrated Security=SSPI;";

                // Get environment settings from appSettings
                var environment = builder.Configuration["Environment"] ?? "Development";
                var authService = builder.Configuration["Services:Authentication"] ?? "local";
                var dbService = builder.Configuration["Services:Database"] ?? "local";
                var fileService = builder.Configuration["Services:FileService"] ?? "local";
                var imageValidationService = builder.Configuration["Services:ImageValidationService"] ?? "local";
                var loggingService = builder.Configuration["Services:LoggingService"] ?? "local";

                // Register areas
                builder.Services.AddMvc();

                // Register filter configuration (equivalent to FilterConfig.RegisterGlobalFilters)
                builder.Services.AddMvc(options => {
                    // Add any global filters here
                });

                // Register bundles (equivalent to BundleConfig.RegisterBundles)
                // In .NET Core, we use the built-in static files middleware instead of bundles

                //Added Services
                
                var app = builder.Build();
                
                // Configure the HTTP request pipeline (formerly Configure method)
                if (app.Environment.IsDevelopment())
                {
                    app.UseDeveloperExceptionPage();
                }
                else
                {
                    app.UseExceptionHandler("/Home/Error");
                    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                    app.UseHsts();
                }
                
                app.UseHttpsRedirection();
                app.UseStaticFiles();
                
                //Added Middleware

                // Configure error handling equivalent to Application_Error
                app.UseExceptionHandler(errorApp =>
                {
                    errorApp.Run(async context =>
                    {
                        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
                        var exceptionHandlerPathFeature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerPathFeature>();
                        var exception = exceptionHandlerPathFeature?.Error;
                        logger.LogError(exception, "An unhandled exception occurred");

                        // You could redirect to an error page here
                        context.Response.StatusCode = 500;
                        await context.Response.WriteAsync("An error occurred. Please try again later.");
                    });
                });

                app.UseRouting();
                
                app.UseAuthorization();
                
                app.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                
                app.Run();
            }
        }
        
        public class ConfigurationManager
        {
            public static IConfiguration Configuration { get; set; }
        }
    }