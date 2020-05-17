using BiblioMit.Resources;
using System.ComponentModel.DataAnnotations;

namespace BiblioMit.Models.Entities.Semaforo
{
    public enum LarvaType
    {
        [Display(ResourceType = typeof(Models_Entities_Semaforo_LarvaType), Name = "D-Larva", GroupName = "Larva_Type_(count)", Prompt = "D")]
        D = 0,
        [Display(ResourceType = typeof(Models_Entities_Semaforo_LarvaType), Name = "Umbanate_Larva", GroupName = "Larva_Type_(count)", Prompt = "U")]
        U = 1,
        [Display(ResourceType = typeof(Models_Entities_Semaforo_LarvaType), Name = "Eyed_Larva", GroupName = "Larva_Type_(count)", Prompt = "O")]
        O = 2
    }
}
