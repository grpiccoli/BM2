namespace BiblioMit.Models
{
    public class AreaCodeProvince
    {
        public int ProvinceId { get; set; }
        public virtual Province Province { get; set; }
        public int AreaCodeId { get; set; }
        public virtual AreaCode AreaCode { get; set; }
    }
}
