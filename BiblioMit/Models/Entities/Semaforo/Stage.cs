using BiblioMit.Resources;
using System.ComponentModel.DataAnnotations;

namespace BiblioMit.Models.Entities.Semaforo
{
    public enum Stage
    {
        [Display(ResourceType = typeof(Models_Entities_Semaforo_Stage), Name = "Maturing", Prompt = "%", GroupName = "Reproductive_Stage")]
        Maturing = 0,
        [Display(ResourceType = typeof(Models_Entities_Semaforo_Stage), Name = "Mature", Prompt = "%", GroupName = "Reproductive_Stage")]
        Mature = 1,
        [Display(ResourceType = typeof(Models_Entities_Semaforo_Stage), Name = "Spawned", Prompt = "%", GroupName = "Reproductive_Stage")]
        Spawned = 2,
        [Display(ResourceType = typeof(Models_Entities_Semaforo_Stage), Name = "Spawning", Prompt = "%", GroupName = "Reproductive_Stage")]
        Spawning = 3
    }
}
