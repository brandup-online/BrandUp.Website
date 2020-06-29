using BrandUp.Website.Pages;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ExampleWebSite.Pages
{
    public class SignInModel : AppPageModel
    {
        readonly static Guid userId = new Guid("1df7d641-2fd4-415a-8530-7b0a2afa88ae");

        public override string Title => "SignIn";
        public override string Description => Title;
        public override string Keywords => Title;

        [BindProperty, Required, Display(Name = "E-mail"), EmailAddress]
        public string Email { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var claims = new List<Claim> {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Name, Email)
            };

            var identity = new ClaimsIdentity(claims, Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                IssuedUtc = DateTime.Now,
                ExpiresUtc = DateTime.Now.AddMonths(1)
            };

            await HttpContext.SignInAsync(Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme, principal, authProperties);

            return PageRedirect(Url.Page("/Index"));
        }
    }
}