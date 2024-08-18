using System;
using System.Web;
using BobsBookstoreClassic.Data;
using Microsoft.AspNetCore.Mvc;

namespace Bookstore.Web.Controllers
{
    using Microsoft.AspNetCore.Mvc;

    public class AuthenticationController : Controller
    {
        public ActionResult Login(string redirectUri = null)
        {
            if (string.IsNullOrWhiteSpace(redirectUri))
                return RedirectToAction("Index", "Home");
            return Redirect(redirectUri);
        }

        public ActionResult LogOut()
        {
            return BookstoreConfiguration.Get("Services/Authentication") == "aws" ? CognitoSignOut() : LocalSignOut();
        }

        private ActionResult LocalSignOut()
        {
            if (HttpContext.Request.Cookies["LocalAuthentication"] != null)
            {
Response.Cookies.Append("LocalAuthentication", string.Empty, new Microsoft.AspNetCore.Http.CookieOptions { Expires = DateTime.Now.AddDays(-1) });
            }

            return RedirectToAction("Index", "Home");
        }

        private ActionResult CognitoSignOut()
        {
            if (Request.Cookies[".AspNet.Cookies"] != null)
            {
Response.Cookies.Append(".AspNet.Cookies", "", new Microsoft.AspNetCore.Http.CookieOptions { Expires = DateTime.Now.AddDays(-1) });
            }

            var domain = BookstoreConfiguration.Get("Authentication/Cognito/CognitoDomain");
            var clientId = BookstoreConfiguration.Get("Authentication/Cognito/LocalClientId");
var logoutUri = $"{Request.Scheme}://{Request.Host}{Request.PathBase}/";
            return Redirect($"{domain}/logout?client_id={clientId}&logout_uri={logoutUri}");
        }
    }
}