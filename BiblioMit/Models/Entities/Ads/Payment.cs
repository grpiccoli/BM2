using BiblioMit.Models.Entities.Ads;
using System;
using System.ComponentModel.DataAnnotations;

namespace BiblioMit.Models.Entities.Ads
{
    public class Payment
    {
        public int Id { get; set; }
        [DataType(DataType.Currency)]
        [DisplayFormat(DataFormatString = "{0:C}")]
        [Display(Name = "Total Cost")]
        public int Price { get; set; }
        public DateTime? PaidDate { get; set; }
        public DateTime DueDate { get; set; }
        [DisplayFormat(DataFormatString = "{0:MMMM YYYY}")]
        [Display(Name = "Period")]
        public DateTime PeriodDate { get; set; }
        public virtual Banner Banner { get; set; }
        public int BannerId { get; set; }
        public bool Paid() => PaidDate.HasValue;
        public bool OverDue() => DueDate < DateTime.Today && !Paid();
    }
}
