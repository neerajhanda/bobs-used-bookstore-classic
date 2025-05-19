
    using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using System.Data.SqlClient;
using System.Data.Entity;

    namespace Bookstore.Web
    {
        public class Program
        {
            public static void Main(string[] args)
            {
                var builder = WebApplication.CreateBuilder(args);

                // Configuration - Add connection string from Web.config
                builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                builder.Configuration.AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true);
                builder.Configuration.AddEnvironmentVariables();

                // Add connection string for Entity Framework
                var connectionString = builder.Configuration.GetConnectionString("BookstoreDatabaseConnection") ??
                    "Server=(localdb)\\MSSQLLocalDB;Initial Catalog=BookStoreClassic;MultipleActiveResultSets=true;Integrated Security=SSPI;";

                // Add app settings from Web.config
                builder.Services.AddSingleton<IConfiguration>(builder.Configuration);

                // Add services to the container (formerly ConfigureServices)
                builder.Services.AddControllersWithViews(options => {
                    options.EnableEndpointRouting = true;
                })
                .AddJsonOptions(options => {
                    // Configure JSON serialization options if needed
                });

                // Client-side validation
                builder.Services.AddMvc(options => {
                    options.EnableEndpointRouting = true;
                    // Add global filters equivalent to FilterConfig.RegisterGlobalFilters
                });

                // Areas support
                builder.Services.AddRazorPages();
                builder.Services.AddMvc()
                    .AddControllersAsServices();
                
                var app = builder.Build();
                
                // Configure the HTTP request pipeline (formerly Configure method)
                // Read environment value from config
                var environmentSetting = builder.Configuration["Environment"] ?? "Development";

                if (app.Environment.IsDevelopment() || environmentSetting == "Development")
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
                
                app.UseRouting();
                
                app.UseAuthorization();

                // Register routes
                app.UseEndpoints(endpoints =>
                {
                    // Area registration
                    endpoints.MapControllerRoute(
                        name: "areas",
                        pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

                    endpoints.MapControllerRoute(
                        name: "default",
                        pattern: "{controller=Home}/{action=Index}/{id?}");

                    endpoints.MapRazorPages();
                });

                // Global error handler (equivalent to Application_Error)
                app.UseExceptionHandler(errorApp => {
                    errorApp.Run(async context => {
                        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
                        var exceptionHandlerPathFeature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerPathFeature>();
                        var exception = exceptionHandlerPathFeature?.Error;

                        logger.LogError(exception, "Unhandled exception");
                        await context.Response.WriteAsync("An error occurred. Please try again later.");
                    });
                });
                
                app.Run();
            }
        }
    }