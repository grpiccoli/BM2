using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using BiblioMit.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;

namespace BiblioMit.Controllers
{
    [Authorize(Policy = "Usuarios")]
    public class AppRoleController : Controller
    {
        private readonly RoleManager<ApplicationRole> _roleManager;
        
        public AppRoleController(RoleManager<ApplicationRole> roleManager)
        {
            _roleManager = roleManager;
        }

        [HttpGet]
        public IActionResult Index()
        {
            List<AppRoleListViewModel> model = new List<AppRoleListViewModel>();
            model = _roleManager.Roles.Select(r => new AppRoleListViewModel
            {
                Id = r.Id,
                RoleName = r.Name,
                Description = r.Description,
                NumberOfUsers = r.Users.Count
            }).ToList();
            return View(model);
        }

        [HttpGet]
        [Authorize(Roles ="Editor,Administrator")]
        public async Task<PartialViewResult> AddEditAppRole(string id)
        {
            AppRoleViewModel model = new AppRoleViewModel();
            if (!string.IsNullOrEmpty(id))
            {
                ApplicationRole applicationRole = await _roleManager.FindByIdAsync(id).ConfigureAwait(false);
                if (applicationRole != null)
                {
                    model.Id = applicationRole.Id;
                    model.RoleName = applicationRole.Name;
                    model.Description = applicationRole.Description;
                }
            }
            return PartialView("_AddEditAppRole", model);
        }
        [HttpPost]
        [Authorize(Roles = "Editor,Administrator")]
        [ValidateAntiForgeryToken]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> AddEditAppRole(string id, AppRoleViewModel model)
        {
            if (model == null) return NotFound();
            if (ModelState.IsValid)
            {
                bool isExist = !string.IsNullOrEmpty(id);
                ApplicationRole applicationRole = isExist ? await _roleManager.FindByIdAsync(id).ConfigureAwait(false) :
               new ApplicationRole
               {
                   CreatedDate = DateTime.UtcNow
               };
                applicationRole.Name = model.RoleName;
                applicationRole.Description = model.Description;
                applicationRole.IPAddress = Request.HttpContext.Connection.RemoteIpAddress.ToString();
                IdentityResult roleRuslt = isExist ? await _roleManager.UpdateAsync(applicationRole).ConfigureAwait(false)
                                                    : await _roleManager.CreateAsync(applicationRole).ConfigureAwait(false);
                if (roleRuslt.Succeeded)
                {
                    return RedirectToAction("Index");
                }
            }
            return View(model);
        }

        [HttpGet]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> DeleteAppRole(string id)
        {
            string name = string.Empty;
            if (!string.IsNullOrEmpty(id))
            {
                ApplicationRole applicationRole = await _roleManager.FindByIdAsync(id).ConfigureAwait(false);
                if (applicationRole != null)
                {
                    name = applicationRole.Name;
                }
            }
            return PartialView("_DeleteAppRole", name);
        }

        [HttpPost]
        [Authorize(Roles = "Administrator")]
        [ValidateAntiForgeryToken]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> DeleteAppRole(string id, IFormCollection form)
        {
            if (!string.IsNullOrEmpty(id) && form != null)
            {
                ApplicationRole applicationRole = await _roleManager.FindByIdAsync(id).ConfigureAwait(false);
                if (applicationRole != null)
                {
                    IdentityResult roleRuslt = _roleManager.DeleteAsync(applicationRole).Result;
                    if (roleRuslt.Succeeded)
                    {
                        return RedirectToAction("Index");
                    }
                }
            }
            return View();
        }
    }
}