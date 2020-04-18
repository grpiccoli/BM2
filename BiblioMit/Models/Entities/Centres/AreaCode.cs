using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BiblioMit.Models
{
    public class AreaCode
    {
        [Display(Name = "Area Code")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }
        public virtual ICollection<AreaCodeProvince> AreaCodeProvinces { get; } = new List<AreaCodeProvince>();
    }
}
