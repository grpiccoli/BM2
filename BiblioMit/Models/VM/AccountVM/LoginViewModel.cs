using System.ComponentModel.DataAnnotations;

namespace BiblioMit.Models.AccountViewModels
{
    public class LoginViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Correo")]
        public string EmailLogin { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string PasswordLogin { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMeLogin { get; set; }
    }
}
