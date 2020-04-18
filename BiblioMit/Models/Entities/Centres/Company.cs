using BiblioMit.Extensions;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BiblioMit.Models
{
    public class Company
    {
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [DisplayFormat(DataFormatString = "{0,9:N0}")]
        [Display(Name = "RUT")]
        public int Id { get; set; }
        [Display(Name = "Business Name")]
        public string BusinessName { get; private set; }
        public void SetBusinessName(string value)
        {
            NormalizedBusinessName = value?.RemoveDiacritics().ToUpperInvariant();
            BusinessName = value;
        }
        [Display(Name = "Trade Name")]
        public string TradeName { get; private set; }
        public void SetTradeName(string value)
        {
            NormalizedTradeName = value?.RemoveDiacritics().ToUpperInvariant();
            TradeName = value;
        }
        [Display(Name = "Acronym")]
        public string Acronym { get; private set; }
        public void SetAcronym(string value)
        {
            Acronym = value?.ParseAcronym();
        }
        [Display(Name = "Address")]
        public string Address { get; set; }
        public string NormalizedBusinessName { get; private set; }
        public string NormalizedTradeName { get; private set; }
        [Display(Name = "Farming Centres")]
        public virtual ICollection<Psmb> Psmbs { get; } = new List<Psmb>();
        public string GetRUT()
        {
            return Id.RUTFormat();
        }
        public string GetDV()
        {
            return Id.RUTGetDigit();
        }
    }
}