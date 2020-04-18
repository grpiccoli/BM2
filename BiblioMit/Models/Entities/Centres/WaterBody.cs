using System.ComponentModel.DataAnnotations;

namespace BiblioMit.Models.Entities.Centres
{
    public enum WaterBody
    {
        [Display(Name = "River")]
        River,
        [Display(Name = "Mat")]
        Mat,
        [Display(Name = "Ocean")]
        Ocean,
        [Display(Name = "Channel")]
        Channel,
        [Display(Name = "Lake")]
        Lake
    }
}
