using System;
using System.Collections.Generic;

namespace JOS.Files.Implementations.Sorting;

/// <summary>
/// Don't use this code
/// </summary>
public class CsvColumnSorter_Substring : IComparer<string>
{
    private readonly int _column;
    private readonly char _separator;

    public CsvColumnSorter_Substring(int column, char separator = ',')
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

        return string.Compare(xColumn, yColumn, StringComparison.OrdinalIgnoreCase);
    }

    private string GetColumnValue(string value)
    {
        var columnCounter = 1;
        var columnStartIndex = 0;
        for (var i = 0; i < value.Length; i++)
        {
            if (value[i].Equals(_separator))
            {
                columnCounter++;
                continue;
            }

            if (columnCounter != _column)
            {
                continue;
            }

            columnStartIndex = i;
            break;
        }

        var columnLength = 0;
        var slice = value[columnStartIndex..];
        for (var i = 0; i < slice.Length; i++)
        {
            if (slice[i] != _separator)
            {
                columnLength++;
            }
            else
            {
                break;
            }
        }

        return value.Substring(columnStartIndex, columnLength);
    }
}