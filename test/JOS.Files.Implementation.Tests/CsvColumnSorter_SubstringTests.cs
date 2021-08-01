using System;
using JOS.Files.Implementations;
using JOS.Files.Implementations.Sorting;
using Shouldly;
using Xunit;

namespace JOS.Files.Implementation.Tests
{
    public class CsvColumnSorter_SubstringTests
    {
        private const string Row1 = "Alexis,Abernathy,Alexis Abernathy,Alexis17,Alexis.Abernathy@gmail.com,Value 0,5217416e-d271-4aff-92eb-b44185637790,https://cloudflare-ipfs.com/ipfs/Qmd3W5DuhgHirLHGVixi6V76LhCkZUz6pnFt5AJBiyvHye/avatar/139.jpg";
        private const string Row2 = "Katherine,Homenick,Katherine Homenick,Katherine_Homenick33,Katherine.Homenick24@hotmail.com,Value 2,10f41091-9aa5-4bf7-84a2-b0418e62a487,https://cloudflare-ipfs.com/ipfs/Qmd3W5DuhgHirLHGVixi6V76LhCkZUz6pnFt5AJBiyvHye/avatar/917.jpg";
        private const string Row3 = "Wendell,Brown,Wendell Brown,Wendell_Brown26,Wendell_Brown62@yahoo.com,Value 8,73b735b4-5e6e-42cb-bfa3-1c80c324874f,https://cloudflare-ipfs.com/ipfs/Qmd3W5DuhgHirLHGVixi6V76LhCkZUz6pnFt5AJBiyvHye/avatar/100.jpg";
        private readonly CsvColumnSorter_Substring _sut;

        public CsvColumnSorter_SubstringTests()
        {
            _sut = new CsvColumnSorter_Substring(2);
        }

        [Fact]
        public void CompareShouldReturn0WhenColumnValueIsTheSameInBothParameters()
        {
            var result = _sut.Compare(Row1, Row1);

            result.ShouldBe(0);
        }

        [Fact]
        public void CompareShouldReturnValueLessThan0IfFirstColumnValueSortsBeforeColumnValue2()
        {
            var result = _sut.Compare(Row1, Row2);

            result.ShouldBeLessThan(0);
        }

        [Fact]
        public void CompareShouldReturnValueBiggerThan0IfFirstColumnValueSortsAfterColumnValue2()
        {
            var result = _sut.Compare(Row2, Row1);

            result.ShouldBeGreaterThan(0);
        }

        [Fact]
        public void ShouldSortArrayInAscendingOrder()
        {
            var rows = new string[] { Row3, Row2, Row1 };

            Array.Sort(rows, _sut);

            rows[0].ShouldBe(Row1); // Abernathy
            rows[1].ShouldBe(Row3); // Brown
            rows[2].ShouldBe(Row2); // Homenick
        }
    }
}
