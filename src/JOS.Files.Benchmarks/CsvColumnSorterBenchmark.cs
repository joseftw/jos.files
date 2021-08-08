using System;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using JOS.Files.Implementations.Sorting;

namespace JOS.Files.Benchmarks
{
    [MemoryDiagnoser]
    [SimpleJob(RuntimeMoniker.Net60)]
    [HtmlExporter]
    public class CsvColumnSorterBenchmark
    {
        private string[] _rows;
        private readonly UnsortedRows _unsortedRows;
        private readonly CsvColumnSorter_StringSplit _stringSplitSorter;
        private readonly CsvColumnSorter_Substring _substringSorter;
        private readonly CsvColumnSorter_Span _spanSorter;
        
        public CsvColumnSorterBenchmark()
        {
            var column = 2;
            _unsortedRows = new UnsortedRows();
            _stringSplitSorter = new CsvColumnSorter_StringSplit(column);
            _substringSorter = new CsvColumnSorter_Substring(column);
            _spanSorter = new CsvColumnSorter_Span(column);
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
