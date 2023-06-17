using System;
using System.Collections.Generic;

namespace JOS.Files.Implementations.Sorting;

/// <summary>
/// Don't use this code.
/// </summary>
public class CsvColumnSorter_StringSplit : IComparer<string>
{
    private readonly int _column;
    private readonly char _separator;

    public CsvColumnSorter_StringSplit(int column, char separator = ',')
    {
        _column = column;
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

        var xColumn = GetColumnValue(x);
        var yColumn = GetColumnValue(y);

        return Comparer<string>.Default.Compare(xColumn, yColumn);
    }

    private string GetColumnValue(string value)
    {
        var columns = value.Split(_separator);
        if (columns.Length < _column)
        {
            throw new ArgumentOutOfRangeException($"Found fewer columns than expected. Actual: {columns.Length}, Expected: {_column}");
        }

        return columns[_column - 1];
    }
}