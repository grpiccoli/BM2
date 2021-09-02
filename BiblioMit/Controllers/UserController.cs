using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using BiblioMit.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using BiblioMit.Data;
using Microsoft.Extensions.Localization;
using BiblioMit.Extensions;
using System;

namespace BiblioMit.Controllers
{
    [Authorize(Policy = nameof(UserClaims.Users))]
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger _logger;
        private readonly IStringLocalizer<UserController> _localizer;

        public UserController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            ILogger<AccountController> logger,
            IStringLocalizer<UserController> localizer
            )
        {
            _localizer = localizer;
            _context = context;
            _logger = logger;
            _userManager = userManager;
        }

        [HttpGet]
        public IActionResult Index()
        {
            List<UserListViewModel> model = new();
            model = _userManager.Users.Select(u => new UserListViewModel
            {
                Id = u.Id,
                Email = u.Email,
                UserRating = u.Rating,
                MemberSince = u.MemberSince,
                ProfileImageUrl = u.ProfileImageUrl,
                RoleName = u.UserRoles.Any() ? _context.ApplicationUsers
                .Where(r => r.Id == u.UserRoles.FirstOrDefault().RoleId).SingleOrDefault().Name : "Estándar"
            }).ToList();
            return View(model);
        }

        [HttpGet]
        [Authorize(Roles = nameof(RoleData.Administrator))]
        public IActionResult AddUser()
        {
            UserViewModel model = new(UserClaims.Banners.Enum2MultiSelect(),
                RoleData.Administrator.Enum2MultiSelect());
            return PartialView("_AddUser", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        [Authorize(Roles = nameof(RoleData.Administrator))]
        public async Task<IActionResult> AddUser(UserViewModel model)
        {
            if (ModelState.IsValid && model != null)
            {
                ApplicationUser user = new()
                {
                    UserName = model.Email,
                    Email = model.Email
                };
                IdentityResult result = await _userManager.CreateAsync(user, model.Password).ConfigureAwait(false);
                if (result.Succeeded)
                {
                    ApplicationUser applicationUser = await _userManager.FindByEmailAsync(user.Email).ConfigureAwait(false);
                    IdentityResult claimsIdentityResult = await _userManager
                        .AddClaimsAsync(applicationUser, model.UserClaims.Where(c => c.Selected)
                        .Select(claim => new Claim(claim.Value, claim.Value))).ConfigureAwait(false);
                    if (claimsIdentityResult.Succeeded)
                    {
                        IdentityResult identityResult = await _userManager
                            .AddToRolesAsync(user, model.AppRoles.SelectedValues.Cast<string>()).ConfigureAwait(false);
                        if (identityResult.Succeeded)
                        {
                            _logger.LogInformation(_localizer["Se ha añadido un nuevo usuario exitosamente."]);
                            return RedirectToAction("Index");
                        }
                    }
                }
                AddErrors(result);
            }
            return RedirectToAction("_AddUser");
            //return View(model);
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }
//        [Authorize(Policy = "Usuarios")]
        [HttpGet]
        [Authorize(Roles = nameof(RoleData.Administrator))]
        [Authorize(Roles = nameof(RoleData.Editor))]
        public async Task<IActionResult> EditUser(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                ApplicationUser applicationUser = await _userManager.FindByIdAsync(id).ConfigureAwait(false);
                if (applicationUser != null)
                {
                    var claims = await _userManager.GetClaimsAsync(applicationUser).ConfigureAwait(false);
                    var roles = await _userManager.GetRolesAsync(applicationUser).ConfigureAwait(false);
                    EditUserViewModel model = new(
                        RoleData.Administrator.Enum2MultiSelect(
                            claims.Select(c => c.Value)
                            ),
                        UserClaims.Banners.Enum2MultiSelect(
                            roles
                            )
                        )
                    {
                        Email = applicationUser.Email
                    };
                    return PartialView("_EditUser", model);
                }
            }
            return PartialView("_EditUser");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        [Authorize(Roles = nameof(RoleData.Administrator))]
        [Authorize(Roles = nameof(RoleData.Editor))]
        public async Task<IActionResult> EditUser(string id, EditUserViewModel model)
        {
            if (model == null) return NotFound();
            if (ModelState.IsValid)
            {
                ApplicationUser applicationUser = await _userManager.FindByIdAsync(id).ConfigureAwait(false);
                if (applicationUser != null && applicationUser.Email != "contacto@epicsolutions.cl")
                {
                    var claims = (await _userManager.GetClaimsAsync(applicationUser).ConfigureAwait(false))
                        .Select(c => c.Value).ToHashSet();
                    var roles = (await _userManager.GetRolesAsync(applicationUser).ConfigureAwait(false))
                        .ToHashSet();

                    var claimChanges = model.UserClaims.Where(c => (claims.Contains(c.Value) && !c.Selected) 
                    || (!claims.Contains(c.Value) && c.Selected))
                        .ToDictionary(c => c.Value, c => c.Selected);

                    var removeClaims = await _userManager
                        .AddClaimsAsync(applicationUser, claimChanges.Where(c => c.Value)
                        .Select(c => new Claim(c.Key, c.Key)))
                        .ConfigureAwait(false);

                    if (removeClaims.Succeeded)
                        throw new InvalidOperationException($"{applicationUser.Name} claims could not be removed");

                    var addClaims = await _userManager
                        .RemoveClaimsAsync(applicationUser, claimChanges.Where(c => !c.Value)
                        .Select(c => new Claim(c.Key, c.Key)))
                        .ConfigureAwait(false);

                    if (addClaims.Succeeded)
                        throw new InvalidOperationException($"{applicationUser.Name} claims could not be added");

                    var roleChanges = model.AppRoles.Where(c => (roles.Contains(c.Value) && !c.Selected)
                    || (!claims.Contains(c.Value) && c.Selected))
                        .ToDictionary(c => c.Value, c => c.Selected);

                    var removeRoles = await _userManager
                        .AddToRolesAsync(applicationUser, roleChanges.Where(c => c.Value)
                        .Select(c => c.Key))
                        .ConfigureAwait(false);

                    if (removeRoles.Succeeded)
                        throw new InvalidOperationException($"{applicationUser.Name} roles could not be removed");

                    var addRoles = await _userManager
                        .RemoveFromRolesAsync(applicationUser, roleChanges.Where(c => !c.Value)
                        .Select(c => c.Key))
                        .ConfigureAwait(false);

                    if (addRoles.Succeeded)
                        throw new InvalidOperationException($"{applicationUser.Name} roles could not be added");
                }
            }
            return RedirectToAction("Index");
        }
        [HttpGet]
        [Authorize(Roles = nameof(RoleData.Administrator))]
        public async Task<IActionResult> DeleteUser(string id)
        {            
            string name = string.Empty;
            if (!string.IsNullOrEmpty(id))
            {
                ApplicationUser applicationUser = await _userManager.FindByIdAsync(id).ConfigureAwait(false);
                if (applicationUser != null)
                {
                    name = applicationUser.Email;
                }
            }
            return PartialView("_DeleteUser", name);
        }

        [HttpPost]
        [Authorize(Roles = nameof(RoleData.Administrator))]
        [ValidateAntiForgeryToken]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> DeleteUser(string id, IFormCollection form)
        {
            if (!string.IsNullOrEmpty(id) && form != null)
            {
                ApplicationUser applicationUser = await _userManager.FindByIdAsync(id).ConfigureAwait(false);
                if (applicationUser != null && applicationUser.Email != "contacto@epicsolutions.cl")
                {
                    IdentityResult result = await _userManager.DeleteAsync(applicationUser).ConfigureAwait(false);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("Index");
                    }
                }
            }
            return View();
        }
    }
}