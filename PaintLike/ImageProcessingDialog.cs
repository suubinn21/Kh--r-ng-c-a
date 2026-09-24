using System;
using System.Drawing;
using System.Windows.Forms;

namespace PaintLike
{
    /// <summary>
    /// Dialog cho phép người dùng chọn các tùy chọn xử lý ảnh
    /// </summary>
    public partial class ImageProcessingDialog : Form
    {
        public int SelectedProcessing { get; set; } = 0; // 0 = không xử lý, 1 = đơn giản, 2 = nâng cao, 3 = curve

        // Các tùy chọn xử lý hình ảnh khác
        public bool ApplyContrast { get; set; } = false;
        public bool ApplySharpen { get; set; } = false;
        public bool ApplyBlur { get; set; } = false;
        public bool ApplyBrightness { get; set; } = false;

        public int ContrastValue { get; set; } = 100;
        public int SharpenValue { get; set; } = 1;
        public int BlurValue { get; set; } = 0;
        public int BrightnessValue { get; set; } = 0;

        public ImageProcessingDialog()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Tùy Chọn Xử Lý Ảnh";
            this.Size = new Size(500, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Tạo tabControl
            TabControl tabControl = new TabControl();
            tabControl.Dock = DockStyle.Fill;
            tabControl.Padding = new Point(5, 5);

            // Tab 1: Khử Răng Cưa
            TabPage tabAntiAliasing = CreateAntiAliasingTab();
            tabControl.TabPages.Add(tabAntiAliasing);

            // Tab 2: Các Bộ Lọc Khác
            TabPage tabFilters = CreateFiltersTab();
            tabControl.TabPages.Add(tabFilters);

            this.Controls.Add(tabControl);

            // Panel nút dưới cùng
            Panel bottomPanel = new Panel();
            bottomPanel.Height = 50;
            bottomPanel.Dock = DockStyle.Bottom;
            bottomPanel.Padding = new Padding(10);

            Button btnApply = new Button();
            btnApply.Text = "Áp Dụng";
            btnApply.Dock = DockStyle.Right;
            btnApply.Width = 100;
            btnApply.Click += (s, e) => { this.DialogResult = DialogResult.OK; this.Close(); };

            Button btnCancel = new Button();
            btnCancel.Text = "Hủy";
            btnCancel.Dock = DockStyle.Right;
            btnCancel.Width = 100;
            btnCancel.Margin = new Padding(5, 0, 0, 0);
            btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };

            bottomPanel.Controls.Add(btnCancel);
            bottomPanel.Controls.Add(btnApply);

            this.Controls.Add(bottomPanel);
        }

        private TabPage CreateAntiAliasingTab()
        {
            TabPage tab = new TabPage("Khử Răng Cưa");

            Panel panel = new Panel();
            panel.Dock = DockStyle.Fill;
            panel.AutoScroll = true;
            panel.Padding = new Padding(15);

            int yPos = 10;
            const int controlHeight = 30;
            const int spacing = 15;

            // Label
            Label lblTitle = new Label();
            lblTitle.Text = "Chọn phương pháp khử răng cưa:";
            lblTitle.Location = new Point(10, yPos);
            lblTitle.Size = new Size(300, controlHeight);
            lblTitle.Font = new Font("Arial", 10, FontStyle.Bold);
            panel.Controls.Add(lblTitle);

            yPos += controlHeight + spacing;

            // Radio buttons cho các loại khử răng cưa
            RadioButton rbNone = new RadioButton();
            rbNone.Text = "Không xử lý";
            rbNone.Location = new Point(10, yPos);
            rbNone.Size = new Size(300, controlHeight);
            rbNone.Checked = true;
            rbNone.Tag = 0;
            rbNone.CheckedChanged += (s, e) => { if (rbNone.Checked) SelectedProcessing = 0; };
            panel.Controls.Add(rbNone);

            yPos += controlHeight + spacing;

            RadioButton rbSimple = new RadioButton();
            rbSimple.Text = "Khử räng cưa đơn giản (Bilateral Filter + Open)";
            rbSimple.Location = new Point(10, yPos);
            rbSimple.Size = new Size(400, controlHeight);
            rbSimple.Tag = 1;
            rbSimple.CheckedChanged += (s, e) => { if (rbSimple.Checked) SelectedProcessing = 1; };
            panel.Controls.Add(rbSimple);

            yPos += controlHeight + spacing;

            RadioButton rbAdvanced = new RadioButton();
            rbAdvanced.Text = "Khử räng cưa nâng cao (Bilateral + Open + Close + Median)";
            rbAdvanced.Location = new Point(10, yPos);
            rbAdvanced.Size = new Size(400, controlHeight);
            rbAdvanced.Tag = 2;
            rbAdvanced.CheckedChanged += (s, e) => { if (rbAdvanced.Checked) SelectedProcessing = 2; };
            panel.Controls.Add(rbAdvanced);

            yPos += controlHeight + spacing;

            RadioButton rbCurves = new RadioButton();
            rbCurves.Text = "Khử räng cưa cho đường cong (Gaussian + Bilateral + Close)";
            rbCurves.Location = new Point(10, yPos);
            rbCurves.Size = new Size(400, controlHeight);
            rbCurves.Tag = 3;
            rbCurves.CheckedChanged += (s, e) => { if (rbCurves.Checked) SelectedProcessing = 3; };
            panel.Controls.Add(rbCurves);

            tab.Controls.Add(panel);
            return tab;
        }

        private TabPage CreateFiltersTab()
        {
            TabPage tab = new TabPage("Bộ Lọc Khác");

            Panel panel = new Panel();
            panel.Dock = DockStyle.Fill;
            panel.AutoScroll = true;
            panel.Padding = new Padding(15);

            int yPos = 10;
            const int controlHeight = 30;
            const int spacing = 20;

            // Contrast
            CheckBox cbContrast = new CheckBox();
            cbContrast.Text = "Điều chỉnh Tương Phản";
            cbContrast.Location = new Point(10, yPos);
            cbContrast.Size = new Size(300, controlHeight);
            cbContrast.CheckedChanged += (s, e) => ApplyContrast = cbContrast.Checked;
            panel.Controls.Add(cbContrast);

            yPos += controlHeight;

            Label lblContrast = new Label();
            lblContrast.Text = $"Giá trị: {ContrastValue}% (50% - 300%)";
            lblContrast.Location = new Point(30, yPos);
            lblContrast.Size = new Size(300, 20);
            panel.Controls.Add(lblContrast);

            yPos += 25;

            TrackBar trkContrast = new TrackBar();
            trkContrast.Minimum = 50;
            trkContrast.Maximum = 300;
            trkContrast.Value = 100;
            trkContrast.Location = new Point(30, yPos);
            trkContrast.Size = new Size(350, 40);
            trkContrast.ValueChanged += (s, e) =>
            {
                ContrastValue = trkContrast.Value;
                lblContrast.Text = $"Giá trị: {ContrastValue}%";
            };
            panel.Controls.Add(trkContrast);

            yPos += 50 + spacing;

            // Sharpness
            CheckBox cbSharpen = new CheckBox();
            cbSharpen.Text = "Làm Sắc Nét";
            cbSharpen.Location = new Point(10, yPos);
            cbSharpen.Size = new Size(300, controlHeight);
            cbSharpen.CheckedChanged += (s, e) => ApplySharpen = cbSharpen.Checked;
            panel.Controls.Add(cbSharpen);

            yPos += controlHeight;

            Label lblSharpen = new Label();
            lblSharpen.Text = $"Độ mạnh: {SharpenValue} (1-5)";
            lblSharpen.Location = new Point(30, yPos);
            lblSharpen.Size = new Size(300, 20);
            panel.Controls.Add(lblSharpen);

            yPos += 25;

            TrackBar trkSharpen = new TrackBar();
            trkSharpen.Minimum = 1;
            trkSharpen.Maximum = 5;
            trkSharpen.Value = 1;
            trkSharpen.Location = new Point(30, yPos);
            trkSharpen.Size = new Size(350, 40);
            trkSharpen.ValueChanged += (s, e) =>
            {
                SharpenValue = trkSharpen.Value;
                lblSharpen.Text = $"Độ mạnh: {SharpenValue}";
            };
            panel.Controls.Add(trkSharpen);

            yPos += 50 + spacing;

            // Blur
            CheckBox cbBlur = new CheckBox();
            cbBlur.Text = "Áp Dụng Blur (Làm Mờ)";
            cbBlur.Location = new Point(10, yPos);
            cbBlur.Size = new Size(300, controlHeight);
            cbBlur.CheckedChanged += (s, e) => ApplyBlur = cbBlur.Checked;
            panel.Controls.Add(cbBlur);

            yPos += controlHeight;

            Label lblBlur = new Label();
            lblBlur.Text = $"Độ mạnh: {BlurValue} (0-10, 0 = không blur)";
            lblBlur.Location = new Point(30, yPos);
            lblBlur.Size = new Size(300, 20);
            panel.Controls.Add(lblBlur);

            yPos += 25;

            TrackBar trkBlur = new TrackBar();
            trkBlur.Minimum = 0;
            trkBlur.Maximum = 10;
            trkBlur.Value = 0;
            trkBlur.Location = new Point(30, yPos);
            trkBlur.Size = new Size(350, 40);
            trkBlur.ValueChanged += (s, e) =>
            {
                BlurValue = trkBlur.Value;
                lblBlur.Text = $"Độ mạnh: {BlurValue}";
            };
            panel.Controls.Add(trkBlur);

            yPos += 50 + spacing;

            // Brightness
            CheckBox cbBrightness = new CheckBox();
            cbBrightness.Text = "Điều Chỉnh Độ Sáng";
            cbBrightness.Location = new Point(10, yPos);
            cbBrightness.Size = new Size(300, controlHeight);
            cbBrightness.CheckedChanged += (s, e) => ApplyBrightness = cbBrightness.Checked;
            panel.Controls.Add(cbBrightness);

            yPos += controlHeight;

            Label lblBrightness = new Label();
            lblBrightness.Text = $"Giá trị: {BrightnessValue} (-100 to 100)";
            lblBrightness.Location = new Point(30, yPos);
            lblBrightness.Size = new Size(300, 20);
            panel.Controls.Add(lblBrightness);

            yPos += 25;

            TrackBar trkBrightness = new TrackBar();
            trkBrightness.Minimum = -100;
            trkBrightness.Maximum = 100;
            trkBrightness.Value = 0;
            trkBrightness.Location = new Point(30, yPos);
            trkBrightness.Size = new Size(350, 40);
            trkBrightness.ValueChanged += (s, e) =>
            {
                BrightnessValue = trkBrightness.Value;
                lblBrightness.Text = $"Giá trị: {BrightnessValue}";
            };
            panel.Controls.Add(trkBrightness);

            tab.Controls.Add(panel);
            return tab;
        }
    }
}
