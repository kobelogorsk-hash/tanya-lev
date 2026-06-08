using System;
using System.Drawing;
using System.Windows.Forms;

namespace Minesweeper
{
    public class Cell : Button
    {
        public int Row { get; set; }
        public int Col { get; set; }
        public bool IsMine { get; set; }
        public bool IsRevealed { get; set; }
        public bool IsFlagged { get; set; }
        public int NeighborMines { get; set; }

        public Cell(int row, int col)
        {
            Row = row;
            Col = col;
            Size = new Size(30, 30);
            Font = new Font("Arial", 12, FontStyle.Bold);
            FlatStyle = FlatStyle.Flat;
            BackColor = Color.LightGray;
            Tag = this; // Store reference to self
        }
    }

    public class MinesweeperGame : Form
    {
        private const int Rows = 10;
        private const int Cols = 10;
        private const int MineCount = 15;

        private Cell[,] grid;
        private Label statusLabel;
        private bool gameOver;
        private int flagsPlaced;
        private int minesLeft;

        public MinesweeperGame()
        {
            Text = "Сапер (Minesweeper)";
            ClientSize = new Size(Cols * 30 + 20, Rows * 30 + 60);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;

            InitializeGrid();
            PlaceMines();
            CalculateNeighbors();
            CreateStatusLabel();
            
            gameOver = false;
            flagsPlaced = 0;
            minesLeft = MineCount;
            UpdateStatus();
        }

        private void InitializeGrid()
        {
            grid = new Cell[Rows, Cols];
            for (int r = 0; r < Rows; r++)
            {
                for (int c = 0; c < Cols; c++)
                {
                    var cell = new Cell(r, c);
                    cell.Location = new Point(c * 30 + 10, r * 30 + 10);
                    cell.MouseDown += Cell_MouseDown;
                    Controls.Add(cell);
                    grid[r, c] = cell;
                }
            }
        }

        private void PlaceMines()
        {
            Random rand = new Random();
            int placed = 0;
            while (placed < MineCount)
            {
                int r = rand.Next(Rows);
                int c = rand.Next(Cols);
                if (!grid[r, c].IsMine)
                {
                    grid[r, c].IsMine = true;
                    placed++;
                }
            }
        }

        private void CalculateNeighbors()
        {
            for (int r = 0; r < Rows; r++)
            {
                for (int c = 0; c < Cols; c++)
                {
                    if (grid[r, c].IsMine) continue;

                    int count = 0;
                    for (int dr = -1; dr <= 1; dr++)
                    {
                        for (int dc = -1; dc <= 1; dc++)
                        {
                            int nr = r + dr;
                            int nc = c + dc;
                            if (nr >= 0 && nr < Rows && nc >= 0 && nc < Cols && grid[nr, nc].IsMine)
                            {
                                count++;
                            }
                        }
                    }
                    grid[r, c].NeighborMines = count;
                }
            }
        }

        private void CreateStatusLabel()
        {
            statusLabel = new Label
            {
                Location = new Point(10, Rows * 30 + 15),
                Size = new Size(Cols * 30, 30),
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Arial", 10, FontStyle.Bold)
            };
            Controls.Add(statusLabel);

            // Add restart button hint
            var hintLabel = new Label
            {
                Location = new Point(10, Rows * 30 + 40),
                Size = new Size(Cols * 30, 20),
                TextAlign = ContentAlignment.MiddleCenter,
                Text = "Нажмите R для перезапуска",
                Font = new Font("Arial", 8)
            };
            Controls.Add(hintLabel);
        }

        private void Cell_MouseDown(object? sender, MouseEventArgs e)
        {
            if (gameOver || sender is not Cell cell) return;

            if (e.Button == MouseButtons.Left)
            {
                if (cell.IsFlagged) return;
                RevealCell(cell);
                CheckWin();
            }
            else if (e.Button == MouseButtons.Right)
            {
                ToggleFlag(cell);
            }
        }

        private void RevealCell(Cell cell)
        {
            if (cell.IsRevealed || cell.IsFlagged) return;

            cell.IsRevealed = true;
            cell.Enabled = false;
            cell.BackColor = Color.White;

            if (cell.IsMine)
            {
                cell.BackColor = Color.Red;
                cell.Text = "💣";
                GameOver(false);
                return;
            }

            if (cell.NeighborMines > 0)
            {
                cell.Text = cell.NeighborMines.ToString();
                cell.ForeColor = GetNumberColor(cell.NeighborMines);
            }
            else
            {
                // Reveal neighbors recursively
                for (int dr = -1; dr <= 1; dr++)
                {
                    for (int dc = -1; dc <= 1; dc++)
                    {
                        int nr = cell.Row + dr;
                        int nc = cell.Col + dc;
                        if (nr >= 0 && nr < Rows && nc >= 0 && nc < Cols)
                        {
                            RevealCell(grid[nr, nc]);
                        }
                    }
                }
            }
        }

        private Color GetNumberColor(int number)
        {
            return number switch
            {
                1 => Color.Blue,
                2 => Color.Green,
                3 => Color.Red,
                4 => Color.DarkBlue,
                5 => Color.Brown,
                6 => Color.Cyan,
                7 => Color.Black,
                8 => Color.Gray,
                _ => Color.Black
            };
        }

        private void ToggleFlag(Cell cell)
        {
            if (cell.IsRevealed) return;

            if (cell.IsFlagged)
            {
                cell.IsFlagged = false;
                cell.Text = "";
                cell.ForeColor = Color.Black;
                flagsPlaced--;
            }
            else
            {
                cell.IsFlagged = true;
                cell.Text = "🚩";
                flagsPlaced++;
            }
            minesLeft = MineCount - flagsPlaced;
            UpdateStatus();
        }

        private void CheckWin()
        {
            int revealedCount = 0;
            for (int r = 0; r < Rows; r++)
            {
                for (int c = 0; c < Cols; c++)
                {
                    if (grid[r, c].IsRevealed)
                    {
                        revealedCount++;
                    }
                }
            }

            if (revealedCount == Rows * Cols - MineCount)
            {
                GameOver(true);
            }
        }

        private void GameOver(bool won)
        {
            gameOver = true;
            if (won)
            {
                statusLabel.Text = "🎉 Победа! Вы нашли все мины!";
                statusLabel.ForeColor = Color.Green;
            }
            else
            {
                statusLabel.Text = "💥 Взрыв! Игра окончена.";
                statusLabel.ForeColor = Color.Red;
                RevealAllMines();
            }
        }

        private void RevealAllMines()
        {
            for (int r = 0; r < Rows; r++)
            {
                for (int c = 0; c < Cols; c++)
                {
                    if (grid[r, c].IsMine)
                    {
                        grid[r, c].Text = "💣";
                        grid[r, c].BackColor = Color.Pink;
                        grid[r, c].Enabled = false;
                    }
                }
            }
        }

        private void UpdateStatus()
        {
            statusLabel.Text = $"Мины осталось: {minesLeft}";
            statusLabel.ForeColor = Color.Black;
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.R && gameOver)
            {
                RestartGame();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void RestartGame()
        {
            Controls.Clear();
            InitializeComponent();
        }

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MinesweeperGame());
        }
    }
}
