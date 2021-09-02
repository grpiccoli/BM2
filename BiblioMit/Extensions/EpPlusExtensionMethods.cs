using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace BiblioMit.Extensions
{
    public static class EpPlusExtensionMethods
    {
        public static int GetColumnByName(this ExcelWorksheet ws, string columnName)
        {
            if (ws is null) throw new ArgumentNullException(nameof(ws));
            return ws.Cells["1:1"].FirstOrDefault(c => c.Value.ToString().Equals(columnName, StringComparison.OrdinalIgnoreCase)).Start.Column;
        }
        public static int GetColumnByNames(this ExcelWorksheet ws, IEnumerable<string> columnNames)
        {
            if (ws is null)
                throw new ArgumentNullException(nameof(ws));
            if (columnNames is null)
                throw new ArgumentNullException(nameof(columnNames));

            return ws.Cells["1:1"]
                .FirstOrDefault(c => columnNames
                .Contains(c.Value.ToString().CleanCell())).Start.Column;
        }
        public static int GetColumnByNames(this IList<string> headers, IEnumerable<string> columnNames)
        {
            if (headers is null) throw new ArgumentNullException(nameof(headers));
            if (columnNames is null) throw new ArgumentNullException(nameof(columnNames));
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
            return ws.Cells[$"{col}:{col}"].FirstOrDefault(c => c.Value.ToString().Equals(columnName, StringComparison.OrdinalIgnoreCase)).Start.Row;
        }
    }
}
