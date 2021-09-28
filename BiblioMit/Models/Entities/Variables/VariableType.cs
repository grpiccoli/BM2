using System.Collections.Generic;

namespace BiblioMit.Models.Entities.Variables
{
    public class VariableType
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Units { get; set; }
        public virtual ICollection<Variable> Variables { get; } = new List<Variable>();
    }
}
