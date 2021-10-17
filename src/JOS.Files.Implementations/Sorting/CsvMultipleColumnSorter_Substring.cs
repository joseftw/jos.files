using System;
using System.Collections.Generic;

namespace JOS.Files.Implementations.Sorting
{
    public class CsvMultipleColumnSorter_Substring : IComparer<string>
    {
        private readonly IReadOnlyCollection<int> _columns;
        private readonly char _separator;

        public CsvMultipleColumnSorter_Substring(
            IReadOnlyCollection<int> columns,
            char separator = ',')
        {
            _columns = columns ?? throw new ArgumentNullException(nameof(columns));
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
            foreach (var column in _columns)
            {
                var xColumn = GetColumnValue(x, column);
                var yColumn = GetColumnValue(y, column);

                result = string.Compare(xColumn, yColumn, StringComparison.OrdinalIgnoreCase);

                if (result != 0)
                {
                    return result;
                }
            }

            return result;
        }

        private string GetColumnValue(string value, int column)
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

                if (columnCounter != column)
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
}
