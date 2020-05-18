using System.Collections.Generic;

namespace BiblioMit.Models.VM
{
    public class AmData
    {
        public string Date { get; set; }
        public double Value { get; set; }
    }
    public class GMapCoordinate
    {
        public double Lat { get; set; }
        public double Lng { get; set; }
    }
    public class GMapPolygonCentre : GMapPolygon
    {
        public string Rut { get; set; }
        public string BusinessName { get; set; }
    }
    public class GMapPolygon
    {
        public IEnumerable<IEnumerable<GMapCoordinate>> Position { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public string Comuna { get; set; }
        public string Provincia { get; set; }
        public string Region { get; set; }
        public string Code { get; set; }
    }
    public class IExport
    {
        public string Label { get; set; }
    }
    public class ExportMenu : IExport
    {
        public IEnumerable<IExport> Menu { get; set; }
    }
    public class ExportItem : IExport
    {
        public string Type { get; set; }
    }
    public class IChoices
    {
        public string Label { get; set; }
    }
    public class ChoicesItem : IChoices
    {
        public string Value { get; set; }
    }
    public class ChoicesGroup : IChoices
    {
        public int Id { get; set; }
        public IEnumerable<ChoicesItem> Choices { get; set; }
    }
}
