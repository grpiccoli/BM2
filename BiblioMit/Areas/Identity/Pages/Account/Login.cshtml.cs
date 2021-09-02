using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using BiblioMit.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Localization;
using System.Collections.ObjectModel;

namespace BiblioMit.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<LoginModel> _logger;
        private readonly IStringLocalizer<LoginModel> _localizer;
        public LoginModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager, 
            ILogger<LoginModel> logger,
            IStringLocalizer<LoginModel> localizer)
        {
            _userManager = userManager;
            _localizer = localizer;
            _signInManager = signInManager;
            _logger = logger;
        }

        [BindProperty]
        public LoginInputModel Input { get; set; }

        public Collection<AuthenticationScheme> ExternalLogins { get; } = new Collection<AuthenticationScheme>();

        public Uri ReturnUrl { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public async Task OnGetAsync(Uri returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl ??= new Uri(HttpContext.Request.Host.ToUriComponent());

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme)
                .ConfigureAwait(false);

            foreach (var auth in await _signInManager.GetExternalAuthenticationSchemesAsync()
                .ConfigureAwait(false))
            {
                ExternalLogins.Add(auth);
            }

            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(Uri returnUrl = null)
        {
            returnUrl ??= new Uri(HttpContext.Request.Host.ToUriComponent());
            ViewData["ReturnUrl"] = returnUrl?.ToString();
            if (ModelState.IsValid)
            {
                // Require the user to have a confirmed email before they can log on.
                var user = await _userManager.FindByEmailAsync(Input.Email).ConfigureAwait(false);
                if (user != null)
                {
                    if (!await _userManager.IsEmailConfirmedAsync(user).ConfigureAwait(false))
                    {
                        ModelState.AddModelError(string.Empty,
                                      "Debes confirmar tu correo electrónico para poder iniciar sesión.");
                        return Page();
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return Page();
                }
                // This doesn't count login failures towards account lockout
                // To enable password failures to trigger account lockout, set lockoutOnFailure: true
                var result = await _signInManager
                    .PasswordSignInAsync(user.UserName, Input.Password, Input.RememberMe, lockoutOnFailure: true)
                    .ConfigureAwait(false);
                if (result.Succeeded)
                {
                    _logger.LogInformation(_localizer["User logged in."]);
                    return LocalRedirect(returnUrl.ToString());
                }
                if (result.RequiresTwoFactor)
                {
                    return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, Input.RememberMe });
                }
                if (result.IsLockedOut)
                {
                    _logger.LogWarning(_localizer[ "User account locked out."]);
                    return RedirectToPage("./Lockout");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return Page();
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }
    }
    public class LoginInputModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }
}
