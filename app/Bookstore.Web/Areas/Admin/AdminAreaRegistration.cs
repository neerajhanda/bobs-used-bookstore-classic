using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;

namespace Bookstore.Web.Areas.Admin
{
    public static class AdminAreaRegistration
    {
        public static void RegisterAdminArea(IApplicationBuilder app)
        {
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapAreaControllerRoute(
                    name: "Admin_default",
                    areaName: "Admin",
                    pattern: "Admin/{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}