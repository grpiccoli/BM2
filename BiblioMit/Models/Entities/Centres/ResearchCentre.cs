using System;
using System.ComponentModel.DataAnnotations;

namespace BiblioMit.Models.Entities.Centres
{
    public class ResearchCentre : Psmb
    {
        [Display(Name = "Website")]
        public Uri Url { get; set; }
    }
}
