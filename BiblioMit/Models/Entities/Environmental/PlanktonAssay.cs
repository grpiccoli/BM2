using BiblioMit.Models.Entities.Environmental.Plancton;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace BiblioMit.Models
{
    public class PlanktonAssay : Indexed, IEquatable<PlanktonAssay>
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Display(Name = "Form Number")]
        public int Id { get; set; }
        public DateTime SamplingDate { get; set; }
        [ParseSkip]
        public int? StationId { get; set; }
        public virtual Station Station { get; set; }
        [ParseSkip]
        public int? SamplingEntityId { get; set; }
        public virtual SamplingEntity SamplingEntity { get; set; }
        public DateTime? AssayStart { get; set; }
        //public int Formulario { get; set; }
        public virtual ICollection<PlanktonAssayEmail> Emails { get; } = new List<PlanktonAssayEmail>();
        //public int Codigo { get; set; }
        public DateTime? ReceptionDate { get; set; }
        public DateTime? AssayEnd { get; set; }
        [ParseSkip]
        public int? LaboratoryId { get; set; }
        public virtual Laboratory Laboratory { get; set; }
        [ParseSkip]
        public int? PhoneId { get; set; }
        public virtual Phone Phone { get; set; }
        [Required]
        [ParseSkip]
        public int PsmbId { get; set; }
        [ParseSkip]
        public virtual Psmb Psmb { get; set; }
        public int NoSamples { get; set; }
        public DateTime? DepartureDate { get; set; }
        [ParseSkip]
        public int? AnalistId { get; set; }
        public virtual Analist Analist { get; set; }
        [Range(-2, 40)]
        public double? Temperature { get; set; }
        [Range(0, 100)]
        public double? Oxigen { get; set; }
        [Range(5, 13)]
        public double? Ph { get; set; }
        [Range(0, 100)]
        public double? Salinity { get; set; }
        [NotMapped]
        public string Name { get; set; }
        [NotMapped]
        public string Acronym { get; set; }
        [NotMapped]
        public int? FarmCode { get; set; }
        //[ParseSkip]
        //public virtual Centre Centre { get; set; }
        [NotMapped]
        public int? AreaCode { get; set; }
        [ParseSkip]
        public int? PlanktonUserId { get; set; }
        public virtual PlanktonUser PlanktonUser { get; set; }
        [ParseSkip]
        public virtual ICollection<Phytoplankton> Phytoplanktons { get; } = new List<Phytoplankton>();
        public override bool Equals(object obj)
        {
            if (obj is null) return this is null;
            if (this is null) return obj is null;
            if (ReferenceEquals(this, obj)) return true;
            return obj is PlanktonAssay q
            && Id == q.Id
            && StationId == q.StationId
            && SamplingEntityId == q.SamplingEntityId
            && SamplingDate == q.SamplingDate
            && AssayStart == q.AssayStart
            && ReceptionDate == q.ReceptionDate
            && AssayEnd == q.AssayEnd
            && LaboratoryId == q.LaboratoryId
            && PhoneId == q.PhoneId
            && PsmbId == q.PsmbId
            && NoSamples == q.NoSamples
            && DepartureDate == q.DepartureDate
            && AnalistId == q.AnalistId
            && Temperature == q.Temperature
            && Oxigen == q.Oxigen
            && Ph == q.Ph
            && Salinity == q.Salinity;
        }
        [SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "Tests for nullity")]
        public static bool operator ==(PlanktonAssay x, PlanktonAssay y)
        {
            if (x is null) return y is null;
            return x.Equals(y);
        }
        public static bool operator !=(PlanktonAssay x, PlanktonAssay y) => !(x == y);
        public override int GetHashCode() => HashCode.Combine(Id);
        public bool Equals(PlanktonAssay other)
        {
            if (other is null) return this is null;
            if (this is null) return other is null;
            if (ReferenceEquals(this, other)) return true;
            return Id.Equals(other.Id)
            && Id.Equals(other.Id)
            && StationId.Equals(other.StationId)
            && SamplingEntityId.Equals(other.SamplingEntityId)
            && SamplingDate.Equals(other.SamplingDate)
            && AssayStart.Equals(other.AssayStart)
            && ReceptionDate.Equals(other.ReceptionDate)
            && AssayEnd.Equals(other.AssayEnd)
            && LaboratoryId.Equals(other.LaboratoryId)
            && PhoneId.Equals(other.PhoneId)
            //&& CentreId.Equals(other.CentreId)
            && PsmbId.Equals(other.PsmbId)
            && NoSamples.Equals(other.NoSamples)
            && DepartureDate.Equals(other.DepartureDate)
            && AnalistId.Equals(other.AnalistId)
            && Temperature.Equals(other.Temperature)
            && Oxigen.Equals(other.Oxigen)
            && Ph.Equals(other.Ph)
            && Salinity.Equals(other.Salinity);
        }
    }
}
