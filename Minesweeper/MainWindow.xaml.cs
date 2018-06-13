using System;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Minesweeper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static string BOMB_CHAR = "💣";
        private static string FLAG_CHAR = "🚩";
        private static int X_SIZE = 9;
        private static int Y_SIZE = 9;
        private static int MINES = 10;
        private string[,] mineField;
        private Button[,] buttons;
        private Timer t = new Timer(1000);
        private int secondsPassed;
        private int flags_left;

        public MainWindow()
        {
            InitializeComponent();
            t.Elapsed += T_Elapsed;
        }

        private void BtnRestart_Click(object sender, RoutedEventArgs e)
        {
            // Make a field
            RestartField(X_SIZE, Y_SIZE, MINES);

            // Start the clock
            secondsPassed = 0;
            t.Start();
        }

        private void UpdateTime()
        {
            Dispatcher.Invoke(() => tbTime.Text = secondsPassed.ToString());
        }

        private void T_Elapsed(object sender, ElapsedEventArgs e)
        {
            secondsPassed++;
            UpdateTime();
        }

        private void RestartField(int width, int height, int mines)
        {
            // Prep Flags
            flags_left = mines;
            tbMines.Text = flags_left.ToString();

            // Choose Minespots
            var mineLocations = new bool[width, height];
            var rnd = new Random();
            while (mines > 0)
            {
                int x = rnd.Next(width);
                int y = rnd.Next(height);
                if (!mineLocations[x, y]) mines--;
                mineLocations[x, y] = true;
            }

            // Generate minefield
            mineField = new string[width, height];
            for (int x = 0; x < mineField.GetLength(0); x++)
            {
                for (int y = 0; y < mineField.GetLength(1); y++)
                {
                    // Mine!
                    if (mineLocations[x, y]) mineField[x, y] = BOMB_CHAR;
                    else
                    {
                        int cnt = 0;

                        // look at sides                        
                        if (x - 1 >= 0 && y + 1 < height)
                            if (mineLocations[x - 1, y + 1]) cnt++;
                        if (y + 1 < height)
                            if (mineLocations[x, y + 1]) cnt++;
                        if (x + 1 < width && y + 1 < height)
                            if (mineLocations[x + 1, y + 1]) cnt++;
                        if (x - 1 >= 0)
                            if (mineLocations[x - 1, y]) cnt++;
                        if (x + 1 < width)
                            if (mineLocations[x + 1, y]) cnt++;
                        if (x - 1 >= 0 && y - 1 >= 0)
                            if (mineLocations[x - 1, y - 1]) cnt++;
                        if (y - 1 >= 0)
                            if (mineLocations[x, y - 1]) cnt++;
                        if (x + 1 < width && y - 1 >= 0)
                            if (mineLocations[x + 1, y - 1]) cnt++;

                        // Fill in
                        mineField[x, y] = cnt.ToString();
                    }
                }
            }

            // Reset Grid
            buttons = new Button[width, height];
            grdField.Children.Clear();
            while (grdField.RowDefinitions.Count > 0) grdField.RowDefinitions.RemoveAt(0);
            while (grdField.RowDefinitions.Count < height) grdField.RowDefinitions.Add(new RowDefinition());
            while (grdField.ColumnDefinitions.Count > 0) grdField.ColumnDefinitions.RemoveAt(0);
            while (grdField.ColumnDefinitions.Count < width) grdField.ColumnDefinitions.Add(new ColumnDefinition());

            // Fill grid with buttons
            for (int x = 0; x < grdField.RowDefinitions.Count; x++)
            {
                for (int y = 0; y < grdField.ColumnDefinitions.Count; y++)
                {
                    // Define button
                    Button b = new Button
                    {
                        Tag = mineField[x, y],
                        FontWeight = FontWeights.Bold,
                        FontSize = 28,
                        Width = Double.NaN, // auto
                        Height = Double.NaN // auto                        
                    };
                    b.MouseRightButtonDown += B_MouseRightButtonDown;
                    b.Click += B_Click;

                    // Insert into grid
                    Grid.SetColumn(b, y);
                    Grid.SetRow(b, x);
                    Grid.SetColumnSpan(b, 1);
                    Grid.SetRowSpan(b, 1);
                    grdField.Children.Add(b);
                    buttons[x, y] = b;
                }
            }
        }

        private void B_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var b = (sender as Button);
            if ((string)b.Content == FLAG_CHAR)
            {
                b.Content = "";
                flags_left++;
            }
            else if (flags_left > 0)
            {
                b.Content = FLAG_CHAR;
                flags_left--;
            }

            tbMines.Text = flags_left.ToString();
        }

        private void B_Click(object sender, RoutedEventArgs e)
        {
            if(Reveal(sender as Button))CheckGameState();
        }

        private void CheckGameState()
        {
            var state = false;
            foreach (var b in buttons)
            {
                if (b.IsEnabled && (string)b.Tag != BOMB_CHAR)
                {
                    state = true;
                    break;
                }
            }
            if (!state) GameOver(false);
        }

        private void RevealSurrounding(Button b)
        {
            // Set empty
            b.Content = "";

            for (int x = 0; x < grdField.ColumnDefinitions.Count; x++)
            {
                for (int y = 0; y < grdField.RowDefinitions.Count; y++)
                {
                    if (buttons[x, y] == b)
                    {
                        if (x - 1 >= 0 && y + 1 < grdField.RowDefinitions.Count) if (buttons[x - 1, y + 1].IsEnabled) Reveal(buttons[x - 1, y + 1]);
                        if (y + 1 < grdField.RowDefinitions.Count) if (buttons[x, y + 1].IsEnabled) Reveal(buttons[x, y + 1]);
                        if (x + 1 < grdField.ColumnDefinitions.Count && y + 1 < grdField.RowDefinitions.Count) if (buttons[x + 1, y + 1].IsEnabled) Reveal(buttons[x + 1, y + 1]);
                        if (x - 1 >= 0) if (buttons[x - 1, y].IsEnabled) Reveal(buttons[x - 1, y]);
                        if (x + 1 < grdField.ColumnDefinitions.Count) if (buttons[x + 1, y].IsEnabled) Reveal(buttons[x + 1, y]);
                        if (x - 1 >= 0 && y - 1 >= 0) if (buttons[x - 1, y - 1].IsEnabled) Reveal(buttons[x - 1, y - 1]);
                        if (y - 1 >= 0) if (buttons[x, y - 1].IsEnabled) Reveal(buttons[x, y - 1]);
                        if (x + 1 < grdField.ColumnDefinitions.Count && y - 1 >= 0) if (buttons[x + 1, y - 1].IsEnabled) Reveal(buttons[x + 1, y - 1]);
                        break;
                    }
                }
            }
        }

        private bool Reveal(Button b)
        {
            if ((string)b.Content == FLAG_CHAR) return true;
            // Reveal
            b.Content = b.Tag;
            b.IsEnabled = false;

            // What is it?
            if ((string)b.Tag == BOMB_CHAR)
            {
                GameOver(true);
                return false;
            }
            if ((string)b.Tag == "0")
            {
                RevealSurrounding(b);
            }
            return true;
        }

        private void GameOver(bool lose)
        {
            t.Stop();
            foreach (var b in buttons) QuickReveal(b);
            if (lose)
            {
                new GameOver("You lost...").Show();
            }
            else
            {
                new GameOver("You won in " + secondsPassed + " seconds!").Show();
            }
        }

        private void QuickReveal(Button b)
        {
            b.IsEnabled = false;
            if ((string)b.Tag != "0") b.Content = b.Tag;
        }
    }
}
