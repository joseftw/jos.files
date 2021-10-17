using System;
using System.Collections.Generic;
using JOS.Files.Implementations.Sorting;
using Shouldly;
using Xunit;

namespace JOS.Files.Implementation.Tests
{
    public class CsvMultipleColumnSorterTests
    {
        private static readonly List<int> Columns = new List<int> { 1, 3, 5 };
        private const char Separator = ',';
        private readonly UnsortedRows _unsortedRows;

        public CsvMultipleColumnSorterTests()
        {
            _unsortedRows = new UnsortedRows();
        }

        [Theory]
        [InlineData(typeof(CsvMultipleColumnSorter_StringSplit))]
        [InlineData(typeof(CsvMultipleColumnSorter_Substring))]
        [InlineData(typeof(CsvMultipleColumnSorter_Span))]
        public void CompareShouldReturn0WhenColumnValueIsTheSameInBothParameters(Type sutType)
        {
            var sut = (IComparer<string>)Activator.CreateInstance(sutType, args: new object[] { Columns, Separator })!;

            var result = sut.Compare(_unsortedRows.Row1, _unsortedRows.Row1);

            result.ShouldBe(0);
        }

        [Theory]
        [InlineData(typeof(CsvMultipleColumnSorter_StringSplit))]
        [InlineData(typeof(CsvMultipleColumnSorter_Substring))]
        [InlineData(typeof(CsvMultipleColumnSorter_Span))]
        public void CompareShouldReturnValueLessThan0IfFirstColumnValueSortsBeforeColumnValue2(Type sutType)
        {
            var sut = (IComparer<string>)Activator.CreateInstance(sutType, args: new object[] { Columns, Separator })!;

            var result = sut.Compare(_unsortedRows.Row1, _unsortedRows.Row2);

            result.ShouldBeLessThan(0);
        }

        [Theory]
        [InlineData(typeof(CsvMultipleColumnSorter_StringSplit))]
        [InlineData(typeof(CsvMultipleColumnSorter_Substring))]
        [InlineData(typeof(CsvMultipleColumnSorter_Span))]
        public void CompareShouldReturnValueBiggerThan0IfFirstColumnValueSortsAfterColumnValue2(Type sutType)
        {
            var sut = (IComparer<string>)Activator.CreateInstance(sutType, args: new object[] { Columns, Separator })!;

            var result = sut.Compare(_unsortedRows.Row2, _unsortedRows.Row1);

            result.ShouldBeGreaterThan(0);
        }

        [Theory]
        [InlineData(typeof(CsvMultipleColumnSorter_StringSplit))]
        [InlineData(typeof(CsvMultipleColumnSorter_Substring))]
        [InlineData(typeof(CsvMultipleColumnSorter_Span))]
        public void ShouldSortArrayInAscendingOrder(Type sutType)
        {
            var sut = (IComparer<string>)Activator.CreateInstance(sutType, args: new object[] { Columns, Separator })!;
            var rows = new string[_unsortedRows.Count];
            _unsortedRows.CopyTo(rows);

            Array.Sort(rows, sut);

            rows[0].ShouldBe(_unsortedRows.Row1);
            rows[1].ShouldBe(_unsortedRows.Row4);
            rows[2].ShouldBe(_unsortedRows.Row5);
            rows[3].ShouldBe(_unsortedRows.Row2);
            rows[4].ShouldBe(_unsortedRows.Row3);
        }
    }
}
