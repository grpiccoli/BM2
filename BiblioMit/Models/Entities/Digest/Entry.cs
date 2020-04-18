using System;
using System.ComponentModel.DataAnnotations;

namespace BiblioMit.Models
{
    public abstract class Entry : Indexed
    {
        public int Id { get; set; }
        [Display(Name = "User")]
        public string ApplicationUserId { get; set; }
        public virtual ApplicationUser ApplicationUser { get; set; }
        [Display(Name = "Date of Upload")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy hh:mm}", ApplyFormatInEditMode = true)]
        public DateTime Date { get; set; }
        public string OutPut { get; set; }
        public string IP { get; set; }
        public int Updated { get; set; }
        public int Added { get; set; }
        public int Observations { get; set; }
        public bool Success { get; set; }
        [Display(Name ="Range")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime Min { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime Max { get; set; }

        //public Informe Informe { get; set; }
    }
    //public enum Informe
    //{
    //    [Display(Name = "Productivo")]
    //    Productivo = 1,
    //    [Display(Name = "Ambiental")]
    //    Ambiental = 2,
    //    [Display(Name = "Semáforo")]
    //    Semaforo = 3
    //}
}
