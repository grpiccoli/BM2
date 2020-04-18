using BiblioMit.Data;
using BiblioMit.Models;
using BiblioMit.Models.Entities.Digest;
using Microsoft.AspNetCore.Http;
using OfficeOpenXml;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace BiblioMit.Services
{
    public interface IImport
    {
        Task<Task> AddRangeAsync(string pwd, IEnumerable<string> files);
        Task<Task> AddAsync(IFormFile file);
        Task<Task> AddAsync(string file);
        Task<Task> AddAsync(Stream file);
        Task<Task> ReadAsync(ExcelPackage package, SernapescaEntry entry, DeclarationType tipo);
    }
}
