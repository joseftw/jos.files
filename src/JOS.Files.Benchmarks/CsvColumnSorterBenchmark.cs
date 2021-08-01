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
        private const string Row1 = "Alexis,Abernathy,Alexis Abernathy,Alexis17,Alexis.Abernathy@gmail.com,Value 0,5217416e-d271-4aff-92eb-b44185637790,https://cloudflare-ipfs.com/ipfs/Qmd3W5DuhgHirLHGVixi6V76LhCkZUz6pnFt5AJBiyvHye/avatar/139.jpg";
        private const string Row2 = "Katherine,Homenick,Katherine Homenick,Katherine_Homenick33,Katherine.Homenick24@hotmail.com,Value 2,10f41091-9aa5-4bf7-84a2-b0418e62a487,https://cloudflare-ipfs.com/ipfs/Qmd3W5DuhgHirLHGVixi6V76LhCkZUz6pnFt5AJBiyvHye/avatar/917.jpg";
        private const string Row3 = "Wendell,Brown,Wendell Brown,Wendell_Brown26,Wendell_Brown62@yahoo.com,Value 8,73b735b4-5e6e-42cb-bfa3-1c80c324874f,https://cloudflare-ipfs.com/ipfs/Qmd3W5DuhgHirLHGVixi6V76LhCkZUz6pnFt5AJBiyvHye/avatar/100.jpg";

        private readonly CsvColumnSorter_StringSplit _stringSplitSorter;
        private readonly CsvColumnSorter_Substring _substringSorter;
        private readonly CsvColumnSorter_Span _spanSorter;
        
        public CsvColumnSorterBenchmark()
        {
            var column = 2;
            _stringSplitSorter = new CsvColumnSorter_StringSplit(column);
            _substringSorter = new CsvColumnSorter_Substring(column);
            _spanSorter = new CsvColumnSorter_Span(column);
        }

        [Benchmark(Baseline = true)]
        public string[] StringSplit()
        {
            var rows = new[] { Row2, Row1, Row3 };
            Array.Sort(rows, _stringSplitSorter);
            return rows;
        }

        [Benchmark]
        public string[] Substring()
        {
            var rows = new[] { Row2, Row1, Row3 };
            Array.Sort(rows, _substringSorter);
            return rows;
        }

        [Benchmark]
        public string[] Span()
        {
            var rows = new[] { Row2, Row1, Row3 };
            Array.Sort(rows, _spanSorter);
            return rows;
        }
    }
}
