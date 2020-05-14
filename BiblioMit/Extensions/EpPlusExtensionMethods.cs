using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BiblioMit.Extensions
{
    public static class EpPlusExtensionMethods
    {
        public static int GetColumnByName(this ExcelWorksheet ws, string columnName)
        {
            if (ws == null) throw new ArgumentNullException(nameof(ws));
            return ws.Cells["1:1"].FirstOrDefault(c => c.Value.ToString().Equals(columnName, StringComparison.InvariantCultureIgnoreCase)).Start.Column;
        }
        public static int GetColumnByNames(this ExcelWorksheet ws, List<string> columnNames)
        {
            if (ws == null) throw new ArgumentNullException(nameof(ws));
            return ws.Cells["1:1"]
                .FirstOrDefault(c => columnNames
                .Contains(c.Value.ToString().CleanCell())).Start.Column;
        }
        public static int GetColumnByNames(this List<string> headers, List<string> columnNames)
        {
            if (headers == null || columnNames == null) throw new ArgumentNullException(nameof(headers));
            foreach (var name in columnNames)
            {
                int index = headers.IndexOf(name);
                if(index != -1) return index;
            }
            return -1;
        }
        public static int GetRowByValue(this ExcelWorksheet ws, char col, string columnName)
        {
            if (ws == null) throw new ArgumentNullException(nameof(ws));
            return ws.Cells[$"{col}:{col}"].FirstOrDefault(c => c.Value.ToString().Equals(columnName, StringComparison.InvariantCultureIgnoreCase)).Start.Row;
        }
    }
}
