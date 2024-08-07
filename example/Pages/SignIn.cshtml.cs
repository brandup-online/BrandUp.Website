﻿using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using BrandUp.Website.Pages;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace ExampleWebSite.Pages
{
    public class SignInModel : AppPageModel
    {
        readonly static Guid userId = new Guid("1df7d641-2fd4-415a-8530-7b0a2afa88ae");

        public override string Title => "SignIn";
        public override string Description => Title;
        public override string Keywords => Title;

        [FromQuery]
        public string ReturnUrl { get; set; }
        [BindProperty, Required, Display(Name = "E-mail"), EmailAddress]
        public string Email { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var claims = new List<Claim> {
                new(ClaimTypes.NameIdentifier, userId.ToString()),
                new(ClaimTypes.Name, Email)
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

            if (string.IsNullOrEmpty(ReturnUrl))
                ReturnUrl = Url.Page("/Index");

            return PageRedirect(ReturnUrl, replace: true);
        }
    }
}