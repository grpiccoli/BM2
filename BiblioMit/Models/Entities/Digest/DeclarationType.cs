using System.ComponentModel.DataAnnotations;

namespace BiblioMit.Models.Entities.Digest
{
    public enum DeclarationType
    {
        None,
        [Display(
            Description = "Action whose purpose is the fixation of invertebrate larvae through the arrangement of collectors.",
            GroupName = "Farm",
            Name = "Seed Uptake",
            Prompt = "Tons")]
        Seed = 1,

        [Display(Description = "Extractive activity that takes place in aquaculture centers in order to obtain a product for subsequent commercialization.",
            GroupName = "Farm",
            Name = "Adult Harvest",
            Prompt = "Tons")]
        Harvest = 2,

        [Display(Description = "Resource that an agent enters and is available for commercialization and / or transformation.",
            GroupName = "Plant",
            Name = "Plant Supply",
            Prompt = "Tons")]
        Supply = 3,

        [Display(Description = "Net weight of products obtained from a raw material, through a manufacturing process.",
            GroupName = "Plant",
            Name = "Production of Manufacturing Plants",
            Prompt = "Tons")]
        Production = 4,

        [Display(Description = "Corresponds to the interface of heat exchange between the atmosphere and the ocean. In practical terms, the meaning of 'surface' will vary according to the measurement method used to determine the temperature.",
            GroupName = "Environmental",
            Name = "Surface Temperature",
            Prompt = "SST - Sea Surface Temperature")]
        Temperature = 5,

        [Display(Description = "Relative amount of dissolved salts (gr) in a body of water (L) measured in Practical Salinity Units (PSU) defined as 35 g of salt / Lt of water.",
            GroupName = "Environmental",
            Name = "Salinity",
            Prompt = "PSU - Practical Salinity Units")]
        Salinity = 6
    }
}
