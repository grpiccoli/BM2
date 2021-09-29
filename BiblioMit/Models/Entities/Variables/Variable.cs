using System;

namespace BiblioMit.Models.Entities.Variables
{
    public class Variable
    {
        public int Id { get; set; }
        public int PsmbId { get; set; }
        public virtual Psmb Psmb { get; set; }
        public int VariableTypeId { get; set; }
        public virtual VariableType VariableType { get; set; }
        public int Value { get; set; }
        public DateTime Date { get; set; }
    }
}
