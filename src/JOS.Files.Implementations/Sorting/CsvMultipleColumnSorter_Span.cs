using System;
using System.Collections.Generic;

namespace JOS.Files.Implementations.Sorting
{
    public class CsvMultipleColumnSorter_Span : IComparer<string>
    {
        private readonly IReadOnlyCollection<int> _columns;
        private readonly char _separator;

        public CsvMultipleColumnSorter_Span(
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

            var xSpan = x.AsSpan();
            var ySpan = y.AsSpan();
            var result = 0;
            foreach (var column in _columns)
            {
                var xValue = GetColumnValue(xSpan, column);
                var yValue = GetColumnValue(ySpan, column);

                result = xValue.CompareTo(yValue, StringComparison.OrdinalIgnoreCase);

                if (result != 0)
                {
                    return result;
                }
            }

            return result;
        }

        /// <summary>
        /// Really naive implementation, just used as a POC.
        /// </summary>
        /// <param name="span"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        private ReadOnlySpan<char> GetColumnValue(ReadOnlySpan<char> span, int column)
        {
            var columnCounter = 1;
            var columnStartIndex = 0;
            for (var i = 0; i < span.Length; i++)
            {
                if (span[i].Equals(_separator))
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
}
