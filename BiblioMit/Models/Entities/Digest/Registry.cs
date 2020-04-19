using BiblioMit.Models.Entities.Digest;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BiblioMit.Models
{
    public class Registry
    {
        public int Id { get; set; }
        [Display(Name = "Código interno dato")]
        public string Attribute { get; private set; }
        public void SetAttribute(string value)
        {
            NormalizedAttribute = value?.ToString().ToUpperInvariant();
            Attribute = value;
        }
        [Display(Name = "Descripción para usuarios")]
        public string Description { get; set; }
        [Display(Name = "Archivo de Entrada")]
        public int InputFileId { get; set; }
        public virtual InputFile InputFile { get; set; }
        [Display(Name ="Operación de conversión")]
        public string Operation { get; set; }
        public int? DecimalPlaces { get; set; }
        public char? DecimalSeparator { get; set; }
        public bool? DeleteAfter2ndNegative { get; set; }
        public string NormalizedAttribute { get; private set; }
        [Display(Name = "Encabezados (sep doble ;;)")]
        public virtual ICollection<Header> Headers { get; } = new List<Header>();
    }
}
