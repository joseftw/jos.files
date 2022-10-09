using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using JOS.Files.Implementations.Sorting;

namespace JOS.Files.Benchmarks
{
    [MemoryDiagnoser]
    [SimpleJob(RuntimeMoniker.Net60)]
    [HtmlExporter]
    public class CsvMultipleColumnSorterBenchmark
    {
        private string[] _rows;
        private readonly UnsortedRows _unsortedRows;
        private readonly CsvMultipleColumnSorter_StringSplit _stringSplitSorter;
        private readonly CsvMultipleColumnSorter_Substring _substringSorter;
        private readonly CsvMultipleColumnSorter_Span _spanSorter;

        public CsvMultipleColumnSorterBenchmark()
        {
            var columns = new List<int> { 1, 3, 5 };
            _rows = Array.Empty<string>();
            _unsortedRows = new UnsortedRows();
            _stringSplitSorter = new CsvMultipleColumnSorter_StringSplit(columns);
            _substringSorter = new CsvMultipleColumnSorter_Substring(columns);
            _spanSorter = new CsvMultipleColumnSorter_Span(columns);
        }

        [IterationSetup]
        public void IterationSetup()
        {
            _rows = new string[_unsortedRows.Count];
            _unsortedRows.CopyTo(_rows);
        }

        [Benchmark(Baseline = true)]
        public string[] StringSplit()
        {
            Array.Sort(_rows, _stringSplitSorter);
            return _rows;
        }

        [Benchmark]
        public string[] Substring()
        {
            Array.Sort(_rows, _substringSorter);
            return _rows;
        }

        [Benchmark]
        public string[] Span()
        {
            Array.Sort(_rows, _spanSorter);
            return _rows;
        }
    }
}
