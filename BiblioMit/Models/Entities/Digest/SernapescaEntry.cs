using BiblioMit.Models.Entities.Digest;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BiblioMit.Models
{
    public class SernapescaEntry : Entry
    {
        [NotMapped]
        public IFormFile InputFile { get; set; }
        public string FileName { get; set; }
        [Range(1, 4)]
        public DeclarationType DeclarationType { get; set; }
    }

    //public enum Reportes
    //{
    //    [Display(Name = "Abastecimiento", GroupName = "Reportes Plantas", Description = "")]
    //    Abastecimiento = 1,
    //    [Display(Name = "Materia Prima y Producción", GroupName = "Reportes Plantas", Description = "")]
    //    MateriaPrima = 2,
    //    [Display(Name = "Semillas", GroupName = "Reportes Centros de Cultivo", Description = "")]
    //    Semilla = 3,
    //    [Display(Name = "Cosecha", GroupName = "Reportes Centros de Cultivo", Description = "")]
    //    Cosecha = 4
    //}

}
