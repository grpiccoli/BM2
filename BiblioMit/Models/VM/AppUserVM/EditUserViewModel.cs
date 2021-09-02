using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace BiblioMit.Models
{
    public class EditUserViewModel
    {
        public EditUserViewModel(MultiSelectList appRoles, MultiSelectList userClaims)
        {
            UserClaims = userClaims;
            AppRoles = appRoles;
        }
        public string Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }

        [Display(Name = "Permisos de Usuario")]
        public MultiSelectList UserClaims { get; internal set; }
        public MultiSelectList AppRoles { get; internal set; }
        [Display(Name = "Rol")]
        public string AppRoleId { get; set; }
    }
}
