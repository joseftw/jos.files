using System;
using System.Collections.Generic;
using JOS.Files.Implementations.Sorting;
using Shouldly;
using Xunit;

namespace JOS.Files.Implementation.Tests
{
    public class CsvColumnSorterTests
    {
        private const int Column = 2;
        private const char Separator = ',';
        private readonly UnsortedRows _unsortedRows;
        public CsvColumnSorterTests()
        {
            _unsortedRows = new UnsortedRows();
        }

        [Theory]
        [InlineData(typeof(CsvColumnSorter_StringSplit))]
        [InlineData(typeof(CsvColumnSorter_Substring))]
        [InlineData(typeof(CsvColumnSorter_Span))]
        public void CompareShouldReturn0WhenColumnValueIsTheSameInBothParameters(Type sutType)
        {
            var sut = (IComparer<string>)Activator.CreateInstance(sutType, args: new object[]{Column, Separator})!;

            var result = sut.Compare(_unsortedRows.Row1, _unsortedRows.Row1);

            result.ShouldBe(0);
        }

        [Theory]
        [InlineData(typeof(CsvColumnSorter_StringSplit))]
        [InlineData(typeof(CsvColumnSorter_Substring))]
        [InlineData(typeof(CsvColumnSorter_Span))]
        public void CompareShouldReturnValueLessThan0IfFirstColumnValueSortsBeforeColumnValue2(Type sutType)
        {
            var sut = (IComparer<string>)Activator.CreateInstance(sutType, args: new object[] { Column, Separator })!;

            var result = sut.Compare(_unsortedRows.Row1, _unsortedRows.Row2);

            result.ShouldBeLessThan(0);
        }

        [Theory]
        [InlineData(typeof(CsvColumnSorter_StringSplit))]
        [InlineData(typeof(CsvColumnSorter_Substring))]
        [InlineData(typeof(CsvColumnSorter_Span))]
        public void CompareShouldReturnValueBiggerThan0IfFirstColumnValueSortsAfterColumnValue2(Type sutType)
        {
            var sut = (IComparer<string>)Activator.CreateInstance(sutType, args: new object[] { Column, Separator })!;

            var result = sut.Compare(_unsortedRows.Row2, _unsortedRows.Row1);

            result.ShouldBeGreaterThan(0);
        }

        [Theory]
        [InlineData(typeof(CsvColumnSorter_StringSplit))]
        [InlineData(typeof(CsvColumnSorter_Substring))]
        [InlineData(typeof(CsvColumnSorter_Span))]
        public void ShouldSortArrayInAscendingOrder(Type sutType)
        {
            var sut = (IComparer<string>)Activator.CreateInstance(sutType, args: new object[] { Column, Separator })!;
            var rows = new[] { _unsortedRows.Row3, _unsortedRows.Row2, _unsortedRows.Row1 };

            Array.Sort(rows, sut);

            rows[0].ShouldBe(_unsortedRows.Row1); // Abernathy
            rows[1].ShouldBe(_unsortedRows.Row3); // Brown
            rows[2].ShouldBe(_unsortedRows.Row2); // Homenick
        }
    }

    //public class CsvColumnSorter_StringSplitTests : CsvColumnSorterTests
    //{
    //    public CsvColumnSorter_StringSplitTests() : base(new CsvColumnSorter_StringSplit(Column))
    //    {
    //    }
    //}

    //public class CsvColumnSorter_SubstringTests : CsvColumnSorterTests
    //{
    //    public CsvColumnSorter_SubstringTests() : base(new CsvColumnSorter_Substring(Column))
    //    {
    //    }
    //}

    //public class CsvColumnSorter_SpanTests : CsvColumnSorterTests
    //{
    //    public CsvColumnSorter_SpanTests() : base(new CsvColumnSorter_Span(Column))
    //    {
    //    }
    //}
}
