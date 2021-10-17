using System.Collections.Generic;
using JOS.Files.Implementations.Sorting;
using Shouldly;
using Xunit;

namespace JOS.Files.Implementation.Tests
{
    public class CsvMultipleColumnSorter_Span_Tests
    {
        [Fact]
        public void ShouldSortCorrectlyBasedOnOneColumn_FirstColumn()
        {
            var columns = new List<int> { 4 };
            var sut = new CsvMultipleColumnSorter_Span(columns);
            var rows = new List<string>
            {
                "cccc,bbbb,cccc,1111",
                "aaaa,bbbb,cccc,1111"
            };

            rows.Sort(sut);

            rows[0].ShouldBe("cccc,bbbb,cccc,1111");
            rows[1].ShouldBe("aaaa,bbbb,cccc,1111");
        }

        [Fact]
        public void ShouldSortCorrectlyBasedOnOneColumn_LastColumn()
        {
            var columns = new List<int> { 4 };
            var sut = new CsvMultipleColumnSorter_Span(columns);
            var rows = new List<string>
            {
                "aaaa,bbbb,cccc,2222",
                "aaaa,bbbb,cccc,1111"
            };

            rows.Sort(sut);

            rows[0].ShouldBe("aaaa,bbbb,cccc,1111");
            rows[1].ShouldBe("aaaa,bbbb,cccc,2222");
        }

        [Fact]
        public void ShouldSortCorrectlyBasedOnTwoColumns()
        {
            var columns = new List<int> { 1, 4 };
            var sut = new CsvMultipleColumnSorter_Span(columns);
            var rows = new List<string>
            {
                "aaaa,bbbb,cccc,2222",
                "aaaa,bbbb,cccc,1111"
            };

            rows.Sort(sut);

            rows[0].ShouldBe("aaaa,bbbb,cccc,1111");
            rows[1].ShouldBe("aaaa,bbbb,cccc,2222");
        }

        [Fact]
        public void ShouldSortCorrectlyBasedOnThreeColumns()
        {
            var columns = new List<int> { 1, 4 };
            var sut = new CsvMultipleColumnSorter_Span(columns);
            var rows = new List<string>
            {
                "aaaa,bbbb,cccc,2222",
                "cccc,bbbb,aaaa,2222"
            };

            rows.Sort(sut);

            rows[0].ShouldBe("aaaa,bbbb,cccc,2222");
            rows[1].ShouldBe("cccc,bbbb,aaaa,2222");
        }

        [Fact]
        public void ShouldNotChangeOrderOnSameStrings()
        {
            var columns = new List<int> { 1, 4 };
            var sut = new CsvMultipleColumnSorter_Span(columns);
            var rows = new List<string>
            {
                "aaaa,bbbb,cccc,2222",
                "aaaa,bbbb,cccc,2222",
                "aaaa,bbbb,cccc,3333",
            };

            rows.Sort(sut);

            rows[0].ShouldBe("aaaa,bbbb,cccc,2222");
            rows[1].ShouldBe("aaaa,bbbb,cccc,2222");
            rows[2].ShouldBe("aaaa,bbbb,cccc,3333");
        }

        [Fact]
        public void ShouldSortMultipleRowsCorrectly()
        {
            var columns = new List<int> { 1, 6 };
            var sut = new CsvMultipleColumnSorter_Span(columns);
            var rows = new List<string>
            {
                "Alexis,Abernathy,Alexis Abernathy,Alexis17,Alexis.Abernathy@gmail.com,Value 0,5217416e-d271-4aff-92eb-b44185637790,https://cloudflare-ipfs.com/ipfs/Qmd3W5DuhgHirLHGVixi6V76LhCkZUz6pnFt5AJBiyvHye/avatar/139.jpg",
                "Katherine,Homenick,Katherine Homenick,Katherine_Homenick33,Katherine.Homenick24@hotmail.com,Value 2,10f41091-9aa5-4bf7-84a2-b0418e62a487,https://cloudflare-ipfs.com/ipfs/Qmd3W5DuhgHirLHGVixi6V76LhCkZUz6pnFt5AJBiyvHye/avatar/917.jpg",
                "Wendell,Brown,Wendell Brown,Wendell_Brown26,Wendell_Brown62@yahoo.com,Value 8,73b735b4-5e6e-42cb-bfa3-1c80c324874f,https://cloudflare-ipfs.com/ipfs/Qmd3W5DuhgHirLHGVixi6V76LhCkZUz6pnFt5AJBiyvHye/avatar/100.jpg",
                "Alexis,Schneider,Alexis Schneider,Alexis_Schneider40,Alexis_Schneider@hotmail.com,Value 4,df7b9351-a049-4fa3-a4ea-b753bc6df517,https://cloudflare-ipfs.com/ipfs/Qmd3W5DuhgHirLHGVixi6V76LhCkZUz6pnFt5AJBiyvHye/avatar/660.jpg",
                "Guido,Wolf,Guido Wolf,Guido53,Guido.Wolf10@hotmail.com,Value 3,12618f37-e4eb-4c62-9a8f-104d77bea283,https://cloudflare-ipfs.com/ipfs/Qmd3W5DuhgHirLHGVixi6V76LhCkZUz6pnFt5AJBiyvHye/avatar/500.jpg"
            };

            rows.Sort(sut);

            rows[0].ShouldBe("Alexis,Abernathy,Alexis Abernathy,Alexis17,Alexis.Abernathy@gmail.com,Value 0,5217416e-d271-4aff-92eb-b44185637790,https://cloudflare-ipfs.com/ipfs/Qmd3W5DuhgHirLHGVixi6V76LhCkZUz6pnFt5AJBiyvHye/avatar/139.jpg");
            rows[1].ShouldBe("Alexis,Schneider,Alexis Schneider,Alexis_Schneider40,Alexis_Schneider@hotmail.com,Value 4,df7b9351-a049-4fa3-a4ea-b753bc6df517,https://cloudflare-ipfs.com/ipfs/Qmd3W5DuhgHirLHGVixi6V76LhCkZUz6pnFt5AJBiyvHye/avatar/660.jpg");
            rows[2].ShouldBe("Guido,Wolf,Guido Wolf,Guido53,Guido.Wolf10@hotmail.com,Value 3,12618f37-e4eb-4c62-9a8f-104d77bea283,https://cloudflare-ipfs.com/ipfs/Qmd3W5DuhgHirLHGVixi6V76LhCkZUz6pnFt5AJBiyvHye/avatar/500.jpg");
            rows[3].ShouldBe("Katherine,Homenick,Katherine Homenick,Katherine_Homenick33,Katherine.Homenick24@hotmail.com,Value 2,10f41091-9aa5-4bf7-84a2-b0418e62a487,https://cloudflare-ipfs.com/ipfs/Qmd3W5DuhgHirLHGVixi6V76LhCkZUz6pnFt5AJBiyvHye/avatar/917.jpg");
            rows[4].ShouldBe("Wendell,Brown,Wendell Brown,Wendell_Brown26,Wendell_Brown62@yahoo.com,Value 8,73b735b4-5e6e-42cb-bfa3-1c80c324874f,https://cloudflare-ipfs.com/ipfs/Qmd3W5DuhgHirLHGVixi6V76LhCkZUz6pnFt5AJBiyvHye/avatar/100.jpg");
        }

        [Fact]
        public void ShouldSortMultipleRowsCorrectly_Readable()
        {
            var columns = new List<int> { 1, 2, 3 };
            var sut = new CsvMultipleColumnSorter_Span(columns);
            var rows = new List<string>
            {
                "Marshall,Mathers,The Slim Shady LP",
                "Marshall,Mathers,The Eminem Show",
                "Marshall,Mathers,Relapse",
                "Marshall,Mathers,Relapse 2",
                "Marshall,Mathers,Revival",
                "Marshall,Mathers,Kamikaze",
                "Eminem,,Infinite"
            };

            rows.Sort(sut);

            rows[0].ShouldBe("Eminem,,Infinite");
            rows[1].ShouldBe("Marshall,Mathers,Kamikaze");
            rows[2].ShouldBe("Marshall,Mathers,Relapse");
            rows[3].ShouldBe("Marshall,Mathers,Relapse 2");
            rows[4].ShouldBe("Marshall,Mathers,Revival");
            rows[5].ShouldBe("Marshall,Mathers,The Eminem Show");
            rows[6].ShouldBe("Marshall,Mathers,The Slim Shady LP");
        }

        [Fact]
        public void ShouldSortMultipleRowsCorrectly_NumberColumn()
        {
            var columns = new List<int> { 4, 1 };
            var sut = new CsvMultipleColumnSorter_Span(columns);
            var rows = new List<string>
            {
                "Marshall,Mathers,The Slim Shady LP,55000",
                "Marshall,Mathers,The Slim Shady LP,45000",
                "Marshall,Mathers,The Slim Shady LP,40000",
                "Marshall,Mathers,The Slim Shady LP,30000",
                "Marshall,Mathers,The Slim Shady LP,90000",
                "Marshall,Mathers,The Slim Shady LP,70000",
                "Eminem,,Infinite,70000"
            };

            rows.Sort(sut);

            rows[0].ShouldBe("Marshall,Mathers,The Slim Shady LP,30000");
            rows[1].ShouldBe("Marshall,Mathers,The Slim Shady LP,40000");
            rows[2].ShouldBe("Marshall,Mathers,The Slim Shady LP,45000");
            rows[3].ShouldBe("Marshall,Mathers,The Slim Shady LP,55000");
            rows[4].ShouldBe("Eminem,,Infinite,70000");
            rows[5].ShouldBe("Marshall,Mathers,The Slim Shady LP,70000");
            rows[6].ShouldBe("Marshall,Mathers,The Slim Shady LP,90000");
        }
    }
}
