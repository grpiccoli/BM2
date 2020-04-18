using BiblioMit.Models;

namespace BiblioMit.Services
{
    public interface IHasBasicIndexer
    {
        [ParseSkip]
        object this[string propertyName] { get;set; }
    }
}
