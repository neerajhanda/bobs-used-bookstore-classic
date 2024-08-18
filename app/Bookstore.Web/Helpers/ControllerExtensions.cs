using Microsoft.AspNetCore.Mvc;

namespace Bookstore.Web.Helpers
{
    using Microsoft.AspNetCore.Mvc;

    public static class ControllerExtensions
    {
        public static void SetNotification(this Controller controller, string message)
        {
            controller.TempData["Notification"] = message;
        }
    }
}