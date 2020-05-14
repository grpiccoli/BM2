using Microsoft.Build.Framework;
using System.Diagnostics.CodeAnalysis;

namespace BiblioMit.Models
{
    public class Phone : Indexed
    {
        public int Id { get; set; }
        [Required, DisallowNull]
        public string Number { get; set; }
    }
}
