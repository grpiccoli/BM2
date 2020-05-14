using BiblioMit.Models;
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
        Task<Task> AddFilesAsync(string pwd);
        Task<Task> ReadAsync<T>(ExcelPackage package, SernapescaEntry entry)
            where T : SernapescaDeclaration;
    }
}
