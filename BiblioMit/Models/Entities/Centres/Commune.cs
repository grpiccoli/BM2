using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BiblioMit.Models
{
    public class Commune : Locality
    {
        [Display(Name = "Province")]
        public int ProvinceId { get; set; }
        public virtual Province Province { get; set; }
        [Display(Name = "Catchment Area")]
        public int? CatchmentAreaId { get; set; }
        public virtual CatchmentArea CatchmentArea { get; set; }
        public virtual ICollection<Psmb> Psmbs { get; } = new List<Psmb>();
        public string GetFullName() => $"{Name}, {Province.GetFullName()}";
    }
    //public class Comuna
    //{
    //    [Display(Name = "Código Único Territorial")]
    //    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    //    public int Id { get; set; }
    //    public int ProvinciaId { get; set; }
    //    public virtual Provincia Provincia { get; set; }
    //    public int CuencaId { get; set; }
    //    public virtual Cuenca Cuenca { get; set; }
    //    [Display(Name = "Nombre de Comuna")]
    //    public string Name { get; set; }
    //    [Display(Name = "Distrito Electoral")]
    //    public int DE { get; set; }
    //    [Display(Name = "Circunscripción Senatorial")]
    //    public int CS { get; set; }
    //    public virtual ICollection<Centre> Centres { get; } = new List<Centre>();
    //    public virtual ICollection<PSMB> PSMBs { get; } = new List<PSMB>();
    //    public virtual ICollection<Polygon> Polygons { get; } = new List<Polygon>();
    //}
}
