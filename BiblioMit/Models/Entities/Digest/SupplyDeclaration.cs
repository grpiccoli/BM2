using BiblioMit.Models.Entities.Digest;
using System.ComponentModel.DataAnnotations.Schema;

namespace BiblioMit.Models
{
    public class SupplyDeclaration : NotProduction
    {
        [NotMapped]
        public int Day
        {
            get
            {
                return Date.Day;
            }
            set
            {
                Date = Date.AddDays(value - Date.Day);
            }
        }
        [NotMapped]
        public string OriginPsmbName { get; set; }
    }
}
