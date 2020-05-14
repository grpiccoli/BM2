using BiblioMit.Models.Entities.Digest;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace BiblioMit.Models
{
    public class ProductionDeclaration : SernapescaDeclaration
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
        [NotMapped]
        public string RawOrProd { get; set; }
        [NotMapped]
        public Item ItemType { get; set; }
        [NotMapped]
        public ProductionType ProductionType { get; set; }
    }
}
