using BiblioMit.Models.Entities.Digest;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BiblioMit.Models
{
    public class SernapescaEntry : Entry
    {
        [NotMapped]
        public IFormFile InputFile { get; set; }
        public string FileName { get; set; }
        [Range(1, 4)]
        public DeclarationType DeclarationType { get; set; }
        public virtual ICollection<SernapescaDeclaration> SernapescaDeclarations { get; } = new List<SernapescaDeclaration>();
    }
}
