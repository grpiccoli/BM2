using System;
using System.Data;

namespace BiblioMit.Extensions
{
    public static class DataTableExtensions
    {
        public static DataTable SetColumnsOrder(this DataTable table, params string[] columnNames)
        {
            if (columnNames is null) throw new ArgumentNullException(nameof(columnNames));
            if(table != null)
            {
                int columnIndex = 0;
                foreach (var columnName in columnNames)
                {
                    table.Columns[columnName].SetOrdinal(columnIndex);
                    columnIndex++;
                }
            }
            return table;
        }
        public static DataTable SetColumnsNames(this DataTable table, params string[] columnNames)
        {
            if (columnNames is null) throw new ArgumentNullException(nameof(columnNames));
            if (table != null)
                for (int i = 0; i < table.Columns.Count; i++)
                    table.Columns[i].ColumnName = columnNames[i];
            return table;
        }
        public static Type GetEnumType(string enumName)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var type = assembly.GetType(enumName);
                if (type == null)
                    continue;
                if (type.IsEnum)
                    return type;
            }
            return null;
        }

    }
}
