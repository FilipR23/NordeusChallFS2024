using System.Collections.Generic;

public class Island
{
    public List<(int row, int col)> Tiles { get; private set; }
    private int totalHeight; // Ukupan zbir visina
    private int tileCount;   // Broj celija

    public Island()
    {
        Tiles = new List<(int row, int col)>();
        totalHeight = 0;
        tileCount = 0;
    }

    public void AddTile(int row, int col, int height)
    {
        Tiles.Add((row, col));
        totalHeight += height; // Dodaj visinu
        tileCount++;           // Uvecaj broj celija
    }

    public double AverageHeight
    {
        get
        {
            return tileCount == 0 ? 0 : (double)totalHeight / tileCount;
        }
    }
}
