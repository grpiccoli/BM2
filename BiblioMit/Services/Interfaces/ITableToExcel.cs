using OfficeOpenXml;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace BiblioMit.Services
{
    public interface ITableToExcel
    {
        Task<ExcelPackage> ProcessAsync(string filePath);
        Task<ExcelPackage> ProcessAsync(Stream html);
        Task<Dictionary<(int, int), string>> HtmlTable2Matrix(Stream html);
        Task<Dictionary<(int, int), string>> HtmlTable2Matrix(string filePath);
    }
}
