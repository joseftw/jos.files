using System.Collections.Generic;

namespace JOS.Files.Implementations.Sorting;

public class UnsortedRows : List<string>
{
    public string Row1 => this[0];
    public string Row2 => this[1];
    public string Row3 => this[2];
    public string Row4 => this[3];
    public string Row5 => this[4];

    public UnsortedRows()
    {
        Add("Alexis,Abernathy,Alexis Abernathy,Alexis17,Alexis.Abernathy@gmail.com,Value 0,5217416e-d271-4aff-92eb-b44185637790,https://cloudflare-ipfs.com/ipfs/Qmd3W5DuhgHirLHGVixi6V76LhCkZUz6pnFt5AJBiyvHye/avatar/139.jpg");
        Add("Katherine,Homenick,Katherine Homenick,Katherine_Homenick33,Katherine.Homenick24@hotmail.com,Value 2,10f41091-9aa5-4bf7-84a2-b0418e62a487,https://cloudflare-ipfs.com/ipfs/Qmd3W5DuhgHirLHGVixi6V76LhCkZUz6pnFt5AJBiyvHye/avatar/917.jpg");
        Add("Wendell,Brown,Wendell Brown,Wendell_Brown26,Wendell_Brown62@yahoo.com,Value 8,73b735b4-5e6e-42cb-bfa3-1c80c324874f,https://cloudflare-ipfs.com/ipfs/Qmd3W5DuhgHirLHGVixi6V76LhCkZUz6pnFt5AJBiyvHye/avatar/100.jpg");
        Add("Alexis,Schneider,Alexis Schneider,Alexis_Schneider40,Alexis_Schneider@hotmail.com,Value 4,df7b9351-a049-4fa3-a4ea-b753bc6df517,https://cloudflare-ipfs.com/ipfs/Qmd3W5DuhgHirLHGVixi6V76LhCkZUz6pnFt5AJBiyvHye/avatar/660.jpg");
        Add("Guido,Wolf,Guido Wolf,Guido53,Guido.Wolf10@hotmail.com,Value 3,12618f37-e4eb-4c62-9a8f-104d77bea283,https://cloudflare-ipfs.com/ipfs/Qmd3W5DuhgHirLHGVixi6V76LhCkZUz6pnFt5AJBiyvHye/avatar/500.jpg");
    }
}