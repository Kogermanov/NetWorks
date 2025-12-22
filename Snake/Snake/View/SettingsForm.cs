using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Snake.view
{
    public partial class SettingsForm : Form
    {
        // Свойства для хранения значений
        public int Width { get; private set; } = 15;
        public int Height { get; private set; } = 15;
        public int Delay { get; private set; } = 400;

        // Элементы управления
        private TrackBar trackBarWidth;
        private TrackBar trackBarHeight;
        private TrackBar trackBarDelay;
        private Label labelWidth;
        private Label labelHeight;
        private Label labelDelay;
        private Button buttonOK;

        public SettingsForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            // Настройка формы
            this.Text = "Настройки";
            this.Size = new System.Drawing.Size(360, 300);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MaximizeBox = false;

            // TrackBar для Width
            trackBarWidth = new TrackBar();
            trackBarWidth.Location = new System.Drawing.Point(20, 20);
            trackBarWidth.Size = new System.Drawing.Size(200, 50);
            trackBarWidth.Minimum = 10;
            trackBarWidth.Maximum = 70;
            trackBarWidth.Value = Width;
            trackBarWidth.ValueChanged += TrackBarWidth_ValueChanged;

            // Label для Width
            labelWidth = new Label();
            labelWidth.Location = new System.Drawing.Point(230, 20);
            labelWidth.Text = $"Ширина: {trackBarWidth.Value}";

            // TrackBar для Height
            trackBarHeight = new TrackBar();
            trackBarHeight.Location = new System.Drawing.Point(20, 70);
            trackBarHeight.Size = new System.Drawing.Size(200, 50);
            trackBarHeight.Minimum = 10;
            trackBarHeight.Maximum = 70;
            trackBarHeight.Value = Height;
            trackBarHeight.ValueChanged += TrackBarHeight_ValueChanged;

            // Label для Height
            labelHeight = new Label();
            labelHeight.Location = new System.Drawing.Point(230, 70);
            labelHeight.Text = $"Высота: {trackBarHeight.Value}";

            // TrackBar для Delay
            trackBarDelay = new TrackBar();
            trackBarDelay.Location = new System.Drawing.Point(20, 120);
            trackBarDelay.Size = new System.Drawing.Size(200, 50);
            trackBarDelay.Minimum = 100;
            trackBarDelay.Maximum = 1000;
            trackBarDelay.Value = Delay;
            trackBarDelay.TickFrequency = 100;
            trackBarDelay.ValueChanged += TrackBarDelay_ValueChanged;

            // Label для Delay
            labelDelay = new Label();
            labelDelay.Location = new System.Drawing.Point(230, 120);
            labelDelay.Text = $"Задержка: {trackBarDelay.Value} мс";

            // Кнопка OK
            buttonOK = new Button();
            buttonOK.Location = new System.Drawing.Point(100, 170);
            buttonOK.Text = "OK";
            buttonOK.Click += ButtonOK_Click;

            // Добавляем элементы на форму
            this.Controls.Add(trackBarWidth);
            this.Controls.Add(labelWidth);
            this.Controls.Add(trackBarHeight);
            this.Controls.Add(labelHeight);
            this.Controls.Add(trackBarDelay);
            this.Controls.Add(labelDelay);
            this.Controls.Add(buttonOK);
        }

        // Обработчики событий для TrackBar
        private void TrackBarWidth_ValueChanged(object sender, EventArgs e)
        {
            Width = trackBarWidth.Value;
            labelWidth.Text = $"Ширина: {Width}";
        }

        private void TrackBarHeight_ValueChanged(object sender, EventArgs e)
        {
            Height = trackBarHeight.Value;
            labelHeight.Text = $"Высота: {Height}";
        }

        private void TrackBarDelay_ValueChanged(object sender, EventArgs e)
        {
            Delay = trackBarDelay.Value;
            labelDelay.Text = $"Задержка: {Delay} мс";
        }

        // Обработчик нажатия кнопки OK
        private void ButtonOK_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
