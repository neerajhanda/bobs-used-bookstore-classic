
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bookstore.Data.FileServices;
using Bookstore.Data.ImageResizeService;
using Bookstore.Data.ImageValidationServices;
using Bookstore.Data.Repositories;
using Bookstore.Data;
using Bookstore.Domain.Books;
using Bookstore.Domain.Orders;
using Bookstore.Domain;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Reflection;
using Autofac.Core;
using Bookstore.Domain.ReferenceData;
using Bookstore.Domain.Offers;
using Bookstore.Domain.Customers;
using Bookstore.Domain.Addresses;
using Bookstore.Domain.Carts;
using Amazon.Rekognition;
using Amazon.S3;
using Bookstore.Web.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Bookstore.Web
{
    public class Startup
    {
        private IWebHostEnvironment _webHostEnvironment;
        public Startup(IConfiguration configuration, IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
            Configuration = configuration;
            ConfigurationManager.Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();
            services.AddTransient<IBookService, BookService>();
            services.AddTransient<IOrderService, OrderService>();
            services.AddTransient<IReferenceDataService, ReferenceDataService>();
            services.AddTransient<IOfferService, OfferService>();
            services.AddTransient<ICustomerService, CustomerService>();
            services.AddTransient<IAddressService, AddressService>();
            services.AddTransient<IShoppingCartService, ShoppingCartService>();
            services.AddTransient<IImageResizeService, ImageResizeService>();

            services.AddTransient<ApplicationDbContext>(s => new ApplicationDbContext(Configuration.GetConnectionString("BookstoreDatabaseConnection")));

            services.AddTransient<ICustomerRepository, CustomerRepository>();
            services.AddTransient<IAddressRepository, AddressRepository>();
            services.AddTransient<IBookRepository, BookRepository>();
            services.AddTransient<IOfferRepository, OfferRepository>();
            services.AddTransient<IShoppingCartRepository, ShoppingCartRepository>();
            services.AddTransient<IOrderRepository, OrderRepository>();
            services.AddTransient<IReferenceDataRepository, ReferenceDataRepository>();

            // File Service
            if (Configuration.GetValue<string>("Services:FileService") == "aws")
            {
                services.AddTransient<IAmazonS3, AmazonS3Client>();
                services.AddTransient<IFileService, S3FileService>();
            }
            else
            {
                var webRootPath = GetWebRootPath();
                services.AddTransient<IFileService, LocalFileService>(s => new LocalFileService(webRootPath));
            }

            // Image Validation Service
            if (Configuration.GetValue<string>("Services:ImageValidationService") == "aws")
            {
                services.AddTransient<IAmazonRekognition, AmazonRekognitionClient>();
                services.AddTransient<IImageValidationService, RekognitionImageValidationService>();
            }
            else
            {
                services.AddTransient<IImageValidationService, LocalImageValidationService>();
            }

            // Authentication
            if (Configuration.GetValue<string>("Services:Authentication") != "aws")
            {
                //services.AddTransient<LocalAuthenticationMiddleware>();
            }

            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();

        }

        private string GetWebRootPath()
        {
            if (_webHostEnvironment.WebRootPath != null)
            {
                // If running in a web context
                return Path.Combine(_webHostEnvironment.WebRootPath, "Content");
            }
            else
            {
                // If running in a non-web context (e.g., console application)
                return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
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

            //Added Middleware
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });


        }
    }

    public class ConfigurationManager
    {
        public static IConfiguration Configuration { get; set; }
    }

}

