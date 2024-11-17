using System;
using System.Net.Http;
using System.Threading.Tasks;

internal static class DataFetcher
{
    public static async Task<int[,]> FetchGridData(string url)
    {
        using (HttpClient client = new HttpClient())
        {
            try
            {
                // Slanje GET zahteva
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();

                // Citanje podataka kao string
                string responseData = await response.Content.ReadAsStringAsync();

                // Parsiranje stringa u matricu
                int[,] gridData = ParseGridData(responseData);
                return gridData;
            }
            catch (HttpRequestException e)
            {
                // Greska prilikom preuzimanja podataka
                throw new Exception($"Error while getting data: {e.Message}");
            }
        }
    }

    private static int[,] ParseGridData(string data)
    {
        // Razdvajanje redova
        string[] rows = data.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        int size = rows.Length;

        // Kreiranje matrice
        int[,] grid = new int[size, size];

        for (int i = 0; i < size; i++)
        {
            string[] columns = rows[i].Split(' ', StringSplitOptions.RemoveEmptyEntries);
            for (int j = 0; j < columns.Length; j++)
            {
                grid[i, j] = int.Parse(columns[j]);
            }
        }

        return grid;
    }
}
