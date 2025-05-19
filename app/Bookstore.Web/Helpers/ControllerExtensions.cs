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

    // Implement a local version of the Humanize extension if needed
    public static class StringExtensions
    {
        public static string Humanize(this string input)
        {
            // Basic implementation of Humanize
            if (string.IsNullOrEmpty(input))
                return input;

            // Replace underscores with spaces
            string result = input.Replace('_', ' ');

            return result;
        }
    }
}