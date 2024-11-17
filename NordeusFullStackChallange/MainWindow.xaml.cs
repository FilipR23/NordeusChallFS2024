using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NordeusFullStackChallange
{

    public partial class MainWindow : Window
    {
        private const string FetchUrl = "https://jobfair.nordeus.com/jf24-fullstack-challenge/test";

        public MainWindow()
        {
            InitializeComponent();
            LoadGridData();
            MessageBox.Show("You have 3 chances to guess which island has the highest median altitude. The darker the color the higher the altitude. (green < brown)", "Explanation");
        }

        public int tries = 3;
        public List<Island> islands;
        public double highestAverageHeight;

        private void RestartApplication()
        {
            var process = System.Diagnostics.Process.GetCurrentProcess(); // Trenutni proces
            System.Diagnostics.Process.Start(process.MainModule.FileName); // Ponovno pokretanje
            Application.Current.Shutdown(); // Zatvaranje trenutne instance
        }

        private Brush GetLandColor(int height)
        {
            if (height > 0 && height <= 200) // Niska kopna
                return Brushes.LawnGreen;
            else if (height > 200 && height <= 400) // Srednje visine
                return Brushes.DarkGreen;
            else if (height > 400 && height <= 600) // Planine
                return Brushes.SandyBrown;
            else if (height > 600 && height <= 800) // Više planine
                return Brushes.Sienna;
            else if (height > 800) // Najviše visine
                return Brushes.SaddleBrown;

            return Brushes.Blue; // Voda
        }
        private async Task LoadGridData()
        {
            try
            {
                // Preuzimanje podataka
                int[,] gridData = await DataFetcher.FetchGridData(FetchUrl);

                // Pronalaženje svih ostrva
                islands = FindAllIslands(gridData);

                // Pronalazak ostrva sa najvećom prosečnom visinom
                highestAverageHeight = FindMaxAverageHeight(islands);

                // Popunjavanje mreže
                PopulateGrid(gridData);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error has occurred: {ex.Message}", "Error");
            }
        }

        private List<Island> FindAllIslands(int[,] gridData)
        {
            int rows = gridData.GetLength(0);
            int cols = gridData.GetLength(1);
            bool[,] visited = new bool[rows, cols];
            List<Island> islands = new List<Island>();

            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    if (gridData[row, col] > 0 && !visited[row, col]) // Kopno i nije posećeno
                    {
                        Island island = new Island();
                        DFS(gridData, row, col, visited, island);
                        islands.Add(island);
                    }
                }
            }

            return islands;
        }
        private double FindMaxAverageHeight(List<Island> islands)
        {
            if (islands.Count == 0)
                return 0; // Nema ostrva, vraca 0 ili neku drugu zadatu vrednost

            return (double)islands.Max(island => island.AverageHeight); // Najveca prosecna visina
        }

        private void HighlightIsland(int[,] gridData, int startRow, int startCol)
        {
            int rows = gridData.GetLength(0);
            int cols = gridData.GetLength(1);

            // Ako se klikne na vodu, nista se ne desava
            if (gridData[startRow, startCol] == 0)
                return;

            // Matrica za pracenje posecenih celija
            bool[,] visited = new bool[rows, cols];
            Island island = new Island();

            // Pokretanje DFS-a da pronadje sve celije ostrva
            DFS(gridData, startRow, startCol, visited, island);

            // Obelezavanje ostrva promenom boje dugmica
            if (island.AverageHeight == highestAverageHeight)
            {
                foreach (var (row, col) in island.Tiles)
                {
                    foreach (UIElement element in MainGrid.Children)
                    {
                        if (Grid.GetRow(element) == row && Grid.GetColumn(element) == col && element is Button button)
                        {
                            button.Background = Brushes.Green;
                        }
                    }
                }
                MessageBox.Show("Correct!", "Correct guess");
                MessageBoxResult result = MessageBox.Show("You won! Do you want to start another game?", "New game", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    RestartApplication(); // Funkcija za restartovanje aplikacije
                }
                else
                {
                    Application.Current.Shutdown(); // Zatvori aplikaciju
                }
            }
            else
            {
                foreach (var (row, col) in island.Tiles)
                {
                    foreach (UIElement element in MainGrid.Children)
                    {
                        if (Grid.GetRow(element) == row && Grid.GetColumn(element) == col && element is Button button)
                        {
                            button.Background = Brushes.Red;
                        }
                    }
                }
                MessageBox.Show($"Incorrect! Remaining tries: {--tries}", "Incorrect guess.");
                if (tries == 0)
                {
                    MessageBoxResult result = MessageBox.Show("You lost! Do you want to try again?", "New game", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes)
                    {
                        RestartApplication(); // Funkcija za restartovanje aplikacije
                    }
                    else
                    {
                        Application.Current.Shutdown(); // Zatvori aplikaciju
                    }
                }
            }
        }

        private void DFS(int[,] gridData, int row, int col, bool[,] visited, Island island)
        {
            int rows = gridData.GetLength(0);
            int cols = gridData.GetLength(1);

            // Provera granica i uslova
            if (row < 0 || row >= rows || col < 0 || col >= cols || gridData[row, col] == 0 || visited[row, col])
                return;

            // Obeleži ćeliju kao posećenu i dodaj je u ostrvo
            visited[row, col] = true;
            island.AddTile(row, col, gridData[row, col]);

            // Susedi (gore, dole, levo, desno)
            int[] rowDirs = { -1, 1, 0, 0 };
            int[] colDirs = { 0, 0, -1, 1 };

            for (int i = 0; i < 4; i++)
            {
                DFS(gridData, row + rowDirs[i], col + colDirs[i], visited, island);
            }
        }


        private void PopulateGrid(int[,] gridData)
        {
            int size = gridData.GetLength(0);

            // Dodajem redove i kolone
            MainGrid.RowDefinitions.Clear();
            MainGrid.ColumnDefinitions.Clear();
            for (int i = 0; i < size; i++)
            {
                MainGrid.RowDefinitions.Add(new RowDefinition());
                MainGrid.ColumnDefinitions.Add(new ColumnDefinition());
            }

            // Dodajem polja na polje
            MainGrid.Children.Clear();
            for (int row = 0; row < size; row++)
            {
                for (int col = 0; col < size; col++)
                {
                    int height = gridData[row, col];
                    int capturedRow = row; // Eksplicitno prihvatam vrednosti
                    int capturedCol = col;

                    Button cell = new Button
                    {
                        //Content = height.ToString(), // Ako zelim da prikazem visine
                        Background = height == 0 ? Brushes.Blue : GetLandColor(height),
                        //BorderBrush = Brushes.Gray,
                        BorderThickness = new Thickness(0.03)
                    };

                    // Dodajem Click event
                    cell.Click += (sender, e) =>
                    {
                        Console.WriteLine($"Button clicked at row={capturedRow}, col={capturedCol}");
                        if (cell.Background != Brushes.Red)
                        {
                            HighlightIsland(gridData, capturedRow, capturedCol);
                        }
                        else
                        {
                            MessageBox.Show("You have already guessed this one (It's not it if the red paint wasn't obvious enough). Try another one.", "Wrong guess?");
                        }
                    };
                    //Povezujem dugme sa poljem
                    Grid.SetRow(cell, row);
                    Grid.SetColumn(cell, col);
                    MainGrid.Children.Add(cell);
                }
            }
        }

    }

}
