using Microsoft.AspNetCore.Owin;
using Microsoft.AspNetCore.Http;

namespace Bookstore.Web.Helpers
{
    using Microsoft.AspNetCore.Owin;
    using Microsoft.AspNetCore.Http;

    public static class OwinRequestExtensions
    {
        public static string GetReturnUrl(this HttpRequest request)
        {
            return $"{request.Scheme}://{request.Host}/signin-oidc";
        }
    }
}