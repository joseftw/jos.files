using System;
using System.Collections.Generic;

namespace JOS.Files.Implementations.Sorting;

public class CsvMultipleColumnSorter_StringSplit : IComparer<string>
{
    private readonly IReadOnlyCollection<int> _columns;
    private readonly char _separator;

    public CsvMultipleColumnSorter_StringSplit(IReadOnlyCollection<int> columns, char separator = ',')
    {
        _columns = columns;
        _separator = separator;
    }

    public int Compare(string? x, string? y)
    {
        if (x == null && y != null)
        {
            return -1;
        }

        if (y == null && x != null)
        {
            return 1;
        }

        if (x == null || y == null)
        {
            return 0;
        }

        var result = 0;
        var xColumns = x.Split(_separator);
        var yColumns = y.Split(_separator);
        foreach (var column in _columns)
        {
            var xColumn = GetColumnValue(xColumns, column);
            var yColumn = GetColumnValue(yColumns, column);

            result = string.Compare(xColumn, yColumn, StringComparison.OrdinalIgnoreCase);

            if (result != 0)
            {
                return result;
            }
        }

        return result;
    }

    private static string GetColumnValue(string[] columns, int column)
    {
        if (columns.Length < column)
        {
            throw new ArgumentOutOfRangeException($"Found fewer columns than expected. Actual: {columns.Length}, Expected: {column}");
        }

        return columns[column - 1];
    }
}