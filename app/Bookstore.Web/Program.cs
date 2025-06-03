
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
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Http;
using System.Data.Entity;
using System.Data.SqlClient;

    namespace Bookstore
    {
        public class AppSettings
        {
            public string Environment { get; set; }
            // Add other properties that exist in your AppSettings section
        }

        public class Program
        {
            public static void Main(string[] args)
            {
                var builder = WebApplication.CreateBuilder(args);
                
                // Add DbContext for EntityFramework 6
                builder.Services.AddScoped<DbContext>(_ =>
                {
                    string connectionString = builder.Configuration.GetConnectionString("BookstoreDatabaseConnection");
// Using System.Data.SqlClient because we're using EntityFramework 6, not EF Core
                    return new DbContext(connectionString);
                });

                // Store configuration in static ConfigurationManager
                ConfigurationManager.Configuration = builder.Configuration;

                // Client validation settings from Web.config
                builder.Services.AddControllersWithViews()
                    .AddMvcOptions(options =>
                    {
                        // Add any required MVC options here
                    });

                // Register areas
                builder.Services.AddMvc().AddControllersAsServices();

                // Add application specific settings
                builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));

// Configure bundles (if needed, consider using bundling and minification in ASP.NET Core)
                // https://docs.microsoft.com/en-us/aspnet/core/client-side/bundling-and-minification

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

                // Set environment based on configuration
                var environment = app.Configuration["AppSettings:Environment"] ?? "Development";

                
                app.UseRouting();

                app.UseAuthorization();

                // Configure global exception handling
                app.UseExceptionHandler(errorApp =>
                {
                    errorApp.Run(async context =>
                    {
                        var logger = app.Services.GetRequiredService<ILogger<Program>>();
                        var exceptionHandlerPathFeature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerPathFeature>();
                        var exception = exceptionHandlerPathFeature?.Error;

                        if (exception != null)
                        {
                            logger.LogError(exception, "Unhandled exception");
                        }

                        await Task.CompletedTask;
                    });
                });
                
                // Register all areas
                app.MapControllerRoute(
                    name: "areas",
                    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

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