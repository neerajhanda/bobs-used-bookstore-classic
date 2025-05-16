using Microsoft.AspNetCore.Mvc;


namespace Bookstore.Web.Helpers
{
    public static class ControllerExtensions
    {
        public static void SetNotification(this Controller controller, string message)
        {
            controller.TempData["Notification"] = message;
        }
    }

    public static class StringExtensions
    {
        // Simple implementation of Humanize - can be expanded as needed
        public static string Humanize(this string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            // Simple implementation - replace underscores with spaces
            return input.Replace('_', ' ');
        }
    }
}