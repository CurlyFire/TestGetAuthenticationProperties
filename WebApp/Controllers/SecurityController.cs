using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using WebApp.Models;

namespace WebApp.Controllers
{
    public class SecurityController : Controller
    {

        private const string TOKEN_VALUE = "MyAwesomeToken";

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login()
        {
            var model = new LoginModel();
            return View(model);
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if (ModelState.IsValid)
            {
                var claimsPrincipal = CreateClaimsPrincipal(model.Name);
                var authenticationProperties = CreateAuthenticationProperties();

                await HttpContext.SignInAsync(claimsPrincipal, authenticationProperties);
                // This works, however I can't find how to set the AuthenticationProperties
                // for the current http request.
                HttpContext.User = claimsPrincipal;
                // I thought something like
                // HttpContext.Authentication.Properties = authenticationProperties ????
                await FetchTokenAndVerify();

                return RedirectToAction(nameof(HomeController), nameof(HomeController.Index));
            }
            else
            {
                return View(model);
            }
        }

        private ClaimsPrincipal CreateClaimsPrincipal(string userName)
        {
            var identity = new ClaimsIdentity("test");
            identity.AddClaim(new Claim(ClaimTypes.Name, userName));
            var principal = new ClaimsPrincipal(identity);

            return principal;
        }

        private AuthenticationProperties CreateAuthenticationProperties()
        {
            var accessToken = new AuthenticationToken()
            {
                Name = OpenIdConnectParameterNames.AccessToken,
                Value = TOKEN_VALUE
            };
            AuthenticationToken[] tokens = { accessToken };
            var authenticationProperties = new AuthenticationProperties();
            authenticationProperties.StoreTokens(tokens);
            authenticationProperties.IsPersistent = true;

            return authenticationProperties;
        }

        /// <summary>
        /// This is just to isolate and demonstrate that I can't retrieve the token.
        /// </summary>
        private async Task FetchTokenAndVerify()
        {
            // I also tried await HttpContext.GetTokenAsync(OpenIdConnectParameterNames.AccessToken)
            // and it doesn't work also, as I think it goes through the method AuthenticateAsync also
            var result = await HttpContext.AuthenticateAsync();

            if (result.Properties == null)
            {
                throw new Exception("Can't fetch authentication properties within current HttpRequest");
            }

            var tokenValue = result.Properties.GetTokenValue(OpenIdConnectParameterNames.AccessToken);
            if (tokenValue != TOKEN_VALUE)
            {
                throw new Exception("Can't fetch access token within current HttpRequest");
            }
        }
    }
}
