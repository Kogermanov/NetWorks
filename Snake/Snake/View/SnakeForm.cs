using Snake.Model.Server;
using Snakes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using Snake.GameObjects;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Snakes.GameState.Types;

namespace Snake
{
    class MyButton : Button
    {
        public MyButton()
        {
            this.SetStyle(ControlStyles.Selectable, false);
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ResumeLayout(false);
        }
    }

    class MyListBox : ListBox
    {
        public MyListBox()
        {
            this.SetStyle(ControlStyles.Selectable, false);
        }
    }

    class MyDataGridView : DataGridView
    {
        public MyDataGridView()
        {
            this.SetStyle(ControlStyles.Selectable, false);
        }
    }

    public interface IGameView
    {
        event EventHandler<Direction> DirectionChanged;

        public void InitialState();
        void PrintListOfServers(List<string> str);
        void UpdateView(GameStateData gameStateData, int yourSnakeId);
    }

    public partial class GameForm : Form, IGameView
    {
        private int cellSize = 25;
        private int yourSnakeId;
        private GameStateData state;
        private GameStateData oldState;

        public event EventHandler<Direction> DirectionChanged;

        private static Mutex mutex = new Mutex();

        GamePresenter presenter;

        public GameForm(GamePresenter presenter)
        {
            this.presenter = presenter;
            InitializeComponent();
            InitializeGame();

            foreach (Control control in this.Controls)
            {
                control.TabStop = false;
            }

            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form_FormClosed);
        }

        private MyButton button1;
        private MyButton button2;
        private MyButton exitButton;
        private MyListBox listBox1;
        private MyDataGridView dataGridView;
        private Label infLabel;

        private void InitializeComponent()
        {
            this.button1 = new MyButton();
            this.button2 = new MyButton();
            this.exitButton = new MyButton();
            this.listBox1 = new MyListBox();
            this.dataGridView = new MyDataGridView();
            this.infLabel = new Label();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(658, 46);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(153, 50);
            this.button1.TabIndex = 0;
            this.button1.Text = "Новая игра";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(869, 46);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(229, 50);
            this.button2.TabIndex = 1;
            this.button2.Text = "присоединиться";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // exitButton
            // 
            this.exitButton.Location = new System.Drawing.Point(658, 46);
            this.exitButton.Name = "button3";
            this.exitButton.Size = new System.Drawing.Size(440, 60);
            this.exitButton.TabIndex = 1;
            this.exitButton.Text = "Выйти";
            this.exitButton.UseVisualStyleBackColor = true;
            this.exitButton.BackColor = Color.DarkRed;
            this.exitButton.Click += new System.EventHandler(this.ExitButton_Click);
            this.exitButton.Visible = false;
            // 
            // listBox1
            // 
            this.listBox1.FormattingEnabled = true;
            this.listBox1.ItemHeight = 25;
            this.listBox1.Location = new System.Drawing.Point(658, 161);
            this.listBox1.Name = "listBox1";
            this.listBox1.Size = new System.Drawing.Size(440, 80);
            this.listBox1.TabIndex = 2;
            // 
            // dataGridView
            // 
            this.dataGridView.Location = new System.Drawing.Point(658, 300);
            this.dataGridView.Name = "listBox2";
            this.dataGridView.Size = new System.Drawing.Size(343, 120);
            this.dataGridView.TabIndex = 2;
            this.dataGridView.Visible = false;
            this.dataGridView.ColumnCount = 3;
            this.dataGridView.Columns[0].Name = "Id";
            this.dataGridView.Columns[1].Name = "Name";
            this.dataGridView.Columns[2].Name = "Score";
            // 
            //  infLabel
            // 
            this.infLabel.Location = new System.Drawing.Point(658, 270);
            this.infLabel.Name = "infLabel";
            this.infLabel.Text = "все змейки:";
            this.infLabel.Font = new Font(infLabel.Font.FontFamily, 16);
            this.infLabel.Size = new System.Drawing.Size(200, 79);
            this.infLabel.TabIndex = 2;
            this.infLabel.Visible = false;
            // 
            // GameForm
            // 
            this.ClientSize = new System.Drawing.Size(1110, 600);
            this.Controls.Add(this.listBox1);
            this.Controls.Add(this.dataGridView);
            this.Controls.Add(this.exitButton);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.infLabel);
            this.KeyPreview = true;
            this.Name = "GameForm";
            this.ResumeLayout(false);
        }

        private void InitializeGame()
        {
            this.DoubleBuffered = true;
            this.KeyDown += GameForm_KeyDown;
        }

        private void GameForm_KeyDown(object sender, KeyEventArgs e)
        {
            Direction newDirection = Direction.Up;

            switch (e.KeyCode)
            {
                case Keys.Up:
                    newDirection = Direction.Up;
                    break;
                case Keys.Down:
                    newDirection = Direction.Down;
                    break;
                case Keys.Left:
                    newDirection = Direction.Left;
                    break;
                case Keys.Right:
                    newDirection = Direction.Right;
                    break;
            }

            DirectionChanged?.Invoke(this, newDirection);
        }

        private static readonly Random _random = new Random();

        private Color GetColorForSnake(int index)
        {
            // Массив из 10 сильно отличающихся цветов
            Color[] colors = new Color[]
            {
        Color.Red,          // Красный
        Color.Blue,         // Синий
        Color.Orange,       // Оранжевый
        Color.Purple,       // Фиолетовый
        Color.Cyan,         // Бирюзовый
        Color.Magenta,      // Пурпурный
        Color.Gold,         // Золотой
        Color.DarkCyan,     // Темно-бирюзовый
        Color.DarkMagenta   // Темно-пурпурный
            };
            return colors[index];
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (state.snakes != null)
            {
                cellSize = 600 / state.field.width;

                using (Pen pen = new Pen(Color.LightGray))
                {
                    for (int x = 0; x <= state.field.width; x++)
                    {
                        e.Graphics.DrawLine(pen, x * cellSize, 0, x * cellSize, state.field.height * cellSize);
                    }

                    for (int y = 0; y <= state.field.height; y++)
                    {
                        e.Graphics.DrawLine(pen, 0, y * cellSize, state.field.width * cellSize, y * cellSize);
                    }
                }

                mutex.WaitOne();

                bool isStateChanged = UpdateOldStateIfChanged();
                
                foreach (var snake in state.snakes)
                {
                    Color color = snake.Id == yourSnakeId ? Color.Green : GetColorForSnake(snake.Id);

                    if (isStateChanged)
                    {
                        string role = ".N";
                        switch (snake.Role)
                        {
                            case NodeRole.Deputy:
                                role = ".D";
                                break;
                            case NodeRole.Master:
                                role = ".M";
                                break;
                            case NodeRole.Normal:
                                role = ".N";
                                break;
                            case NodeRole.Viewer:
                                role = ".V";
                                break;
                        }
                        //if (snake.SnakeState == GameState.Types.Snake.Types.SnakeState.Alive)
                        //{
                        AddItemToDataGridView(snake.Id, snake.Name + role, snake.Score, color);
                        //}
                    }

                    foreach (var point in snake.SnakePosition)
                    {
                        Color newColor = (point == snake.SnakePosition[0]) ? Color.DarkGoldenrod : color;
                        e.Graphics.FillRectangle(new SolidBrush(newColor), point.X * cellSize, point.Y * cellSize, cellSize, cellSize);
                    }
                }

                mutex.ReleaseMutex();

                e.Graphics.FillRectangle(Brushes.Red, state.foodsCord[0].X * cellSize, state.foodsCord[0].Y * cellSize, cellSize, cellSize);
            }
        }

        private bool UpdateOldStateIfChanged()
        {
            if (oldState.snakes == null || !AreStatesEqual(state, oldState))
            {
                oldState = new GameStateData(state);
                dataGridView.Rows.Clear();
                return true;
            }
            return false;
        }

        private bool AreStatesEqual(GameStateData state1, GameStateData state2)
        {
            if (state1.snakes.Count != state2.snakes.Count)
                return false;

            foreach (var snake1 in state1.snakes)
            {
                var snake2 = state2.snakes.FirstOrDefault(s => s.Id == snake1.Id);
                if (snake1.Score != snake2.Score || snake1.Name != snake2.Name || snake1.Role != snake2.Role)
                    return false;
            }

            return true;
        }

        private void AddItemToDataGridView(int id, string name, int score, Color color)
        {
            int rowIndex = dataGridView.Rows.Add(id, name, score);
            dataGridView.Rows[rowIndex].Cells[1].Style.ForeColor = color;
        }

        public void UpdateView(GameStateData state, int yourSnakeId)
        {
            mutex.WaitOne();
            this.state = state;
            this.yourSnakeId = yourSnakeId;
            mutex.ReleaseMutex();
            Invalidate();
        }

        public void PrintListOfServers(List<string> str)
        {
            if (listBox1.InvokeRequired)
            {
                listBox1.Invoke(new Action(() =>
                {
                    listBox1.Items.Clear();
                    listBox1.Items.AddRange(str.ToArray());
                }));
            }
            else
            {
                listBox1.Items.AddRange(str.ToArray());
            }
        }

        private void Form_FormClosed(object sender, FormClosedEventArgs e)
        {
            Environment.Exit(0);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            HideAllObjects();
            listBox1.Visible = true;
            dataGridView.Visible = true;
            infLabel.Visible = true;
            exitButton.Visible = true;

            presenter.SwitchToServerModel();
        }

        public void HideAllObjects()
        {
            for (int i = 0; i < this.Controls.Count; i++)
            {
                if (this.Controls[i].InvokeRequired)
                {
                    this.Controls[i].Invoke(new Action(() =>
                    {
                        this.Controls[i].Visible = false;
                    }));
                }
                else
                {
                    this.Controls[i].Visible = false;
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            HideAllObjects();
            listBox1.Visible = true;
            dataGridView.Visible = true;
            infLabel.Visible = true;
            exitButton.Visible = true;

            int selectNum = listBox1.SelectedIndex;
            if (selectNum != -1)
            {
                presenter.SwitchToClientModel(selectNum);
            }
        }

        public void InitialState()
        {
            HideAllObjects();
            if (button1.InvokeRequired || button2.InvokeRequired || listBox1.InvokeRequired)
            {
                // Используем BeginInvoke для асинхронного выполнения
                this.BeginInvoke(new Action(() =>
                {
                    button1.Visible = true;
                    button2.Visible = true;
                    listBox1.Visible = true;
                }));
            }
            else
            {
                // Если вызов из UI-потока, изменяем напрямую
                button1.Visible = true;
                button2.Visible = true;
                listBox1.Visible = true;
            }
        }

        private void ExitButton_Click(object sender, EventArgs e)
        {
            //InitialState();

            presenter.ExitModel();
        }
    }
}