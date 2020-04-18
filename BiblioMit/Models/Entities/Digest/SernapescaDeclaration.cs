using BiblioMit.Models.Entities.Digest;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace BiblioMit.Models
{
    public abstract class SernapescaDeclaration : Indexed
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [ParseSkip]
        public long Id { get; set; }
        public int DeclarationNumber { get; set; }
        public int CentreId { get; set; }
        [ParseSkip]
        public virtual Psmb Centre { get; set; }
        public DeclarationType Dato { get; set; }
        public int? OriginId { get; set; }
        [SemillaSkip]
        public virtual Origin Origin { get; set; }
        [ProduccionSkip]
        public ProductionType? ProductionType { get; set; }
        [ProduccionSkip]
        public Item? ItemType { get; set; }
        [ParseSkip]
        public DateTime Date { get; set; }
        public double Weight { get; set; }
        [NotMapped]
        public string CommuneName { get; set; }
        [NotMapped]
        public string CompanyName { get; set; }
        [NotMapped]
        public int Year { get; set; }
        [NotMapped]
        [ParseSkip]
        public int Row { get; set; }
        [NotMapped]
        [ParseSkip]
        public string Rows { get; set; }
        [NotMapped]
        [ParseSkip]
        public string Sheet { get; set; }
        [NotMapped]
        public int Month { get; set; }
        [NotMapped]
        [SemillaSkip]
        public string Origen { get; set; }
        public string Discriminator { get; set; }
    }
}
