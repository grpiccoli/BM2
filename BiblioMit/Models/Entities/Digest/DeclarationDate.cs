using System;

namespace BiblioMit.Models.Entities.Digest
{
    public class DeclarationDate
    {
        public int Id { get; set; }
        public int SernapescaDeclarationId { get; set; }
        public virtual SernapescaDeclaration SernapescaDeclaration { get; set; }
        public DateTime Date { get; set; }
        public double Weight { get; set; }//2
        public Item? ItemType { get; set; }
        public ProductionType? ProductionType { get; set; }
        public bool? RawMaterial { get; set; }
    }
}
