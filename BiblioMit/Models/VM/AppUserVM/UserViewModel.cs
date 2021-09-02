using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace BiblioMit.Models
{
    public class UserViewModel
    {
        public UserViewModel(MultiSelectList appRoles, MultiSelectList userClaims)
        {
            AppRoles = appRoles;
            UserClaims = userClaims;
        }
        public string Id { get; set; }

        [Required(ErrorMessage = "Se requiere una contraseña")]
        [StringLength(100, ErrorMessage = "La {0} debe tener entre {2} y {1} caracteres de largo.", MinimumLength = 6)]
        [Display(Name = "Contraseña")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Se requiere confirmar la contraseña")]
        [Display(Name = "Confirmar Contraseña")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "La contraseña y la contraseña de confirmación no coinciden.")]
        public string ConfirmPassword { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "Correo")]
        [EmailAddress]
        public string Email { get; set; }

        [Display(Name = "Permisos de usuario")]
        public MultiSelectList UserClaims { get; internal set; }
        [Display(Name = "Roles de usuario")]
        public MultiSelectList AppRoles { get; internal set; }

        [Display(Name = "Rol")]
        public string AppRoleId { get; set; }
    }
}
