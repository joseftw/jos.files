using System;
using System.Collections.Generic;

namespace JOS.Files.Implementations.Sorting;

public class CsvColumnSorter_Span : IComparer<string>
{
    private readonly int _column;
    private readonly char _separator;

    public CsvColumnSorter_Span(int column, char separator = ',')
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

        return xColumn.CompareTo(yColumn, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Really naive implementation, just used as a POC.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    private ReadOnlySpan<char> GetColumnValue(string value)
    {
        var span = value.AsSpan();
        var columnCounter = 1;
        var columnStartIndex = 0;
        for (var i = 0; i < span.Length; i++)
        {
            if (span[i].Equals(_separator))
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
        var slice = span[columnStartIndex..];
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

        return span.Slice(columnStartIndex, columnLength);
    }
}