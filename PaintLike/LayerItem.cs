using System;
using System.Drawing;
using System.Windows.Forms;

namespace PaintLike
{
    /// <summary>
    /// Mục đích: Hiển thị một layer duy nhất trong LayerPanel
    /// Chứa: Checkbox để bật/tắt visibility, Label để hiển thị tên layer
    /// </summary>
    public partial class LayerItem : UserControl
    {
        private Layer? layer;
        private bool isSelected = false;

        // Sự kiện khi người dùng thay đổi trạng thái visibility
        public event EventHandler? VisibilityChanged;
        
        // Sự kiện khi layer được chọn/bỏ chọn
        public event EventHandler? SelectionChanged;

        public LayerItem(Layer layer)
        {
            System.Diagnostics.Debug.WriteLine($"[LayerItem CTOR] Creating LayerItem for: {layer?.Name}");
            InitializeComponent();
            this.layer = layer;
            this.DoubleBuffered = true;
            this.Cursor = Cursors.Hand;

            // Khởi tạo các control
            InitializeControls();
            System.Diagnostics.Debug.WriteLine($"[LayerItem CTOR] Done. Size={this.Size}, Visible={this.Visible}, BackColor={this.BackColor}");
        }

        private void InitializeControls()
        {
            System.Diagnostics.Debug.WriteLine($"[LayerItem InitializeControls] START for: {layer?.Name}");
            // Xóa controls mặc định của UserControl
            this.Controls.Clear();

            // Checkbox để hiển thị/ẩn layer
            CheckBox chkVisible = new CheckBox();
            chkVisible.Name = "chkVisible";
            chkVisible.Text = "Visible";
            chkVisible.Checked = layer?.IsVisible ?? false;
            chkVisible.Size = new Size(70, 20);
            chkVisible.Location = new Point(5, 5);
            chkVisible.CheckedChanged += (sender, e) => ChkVisible_CheckedChanged(sender, e);
            this.Controls.Add(chkVisible);

            // Label hiển thị tên layer
            Label lblName = new Label();
            lblName.Name = "lblName";
            lblName.Text = layer?.Name ?? "Unknown";
            lblName.AutoSize = false;
            lblName.Size = new Size(140, 20);
            lblName.Location = new Point(80, 5);
            lblName.TextAlign = ContentAlignment.MiddleLeft;
            lblName.BackColor = Color.Transparent;
            lblName.Click += (sender, e) => Label_Click(sender, e);
            this.Controls.Add(lblName);

            // Label hiển thị Opacity
            Label lblOpacity = new Label();
            lblOpacity.Name = "lblOpacity";
            lblOpacity.Text = $"Op: {layer?.Opacity ?? 0}";
            lblOpacity.AutoSize = false;
            lblOpacity.Size = new Size(50, 20);
            lblOpacity.Location = new Point(225, 5);
            lblOpacity.TextAlign = ContentAlignment.MiddleCenter;
            lblOpacity.BackColor = Color.Transparent;
            lblOpacity.Click += (sender, e) => Label_Click(sender, e);
            this.Controls.Add(lblOpacity);

            // Đặt kích thước LayerItem
            this.Size = new Size(280, 30);
            this.BackColor = Color.LightGray;
            this.BorderStyle = BorderStyle.FixedSingle;
            System.Diagnostics.Debug.WriteLine($"[LayerItem InitializeControls] END. Size={this.Size}, ControlsCount={this.Controls.Count}");
        }

        private void ChkVisible_CheckedChanged(object? sender, EventArgs e)
        {
            if (layer != null)
            {
                layer.IsVisible = ((CheckBox?)sender)?.Checked ?? false;
                VisibilityChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private void Label_Click(object? sender, EventArgs e)
        {
            // Gọi sự kiện khi layer được nhấp
            isSelected = !isSelected;
            UpdateSelection();
            SelectionChanged?.Invoke(this, EventArgs.Empty);
        }

        private void UpdateSelection()
        {
            if (isSelected)
            {
                this.BackColor = SystemColors.Highlight;
                this.ForeColor = Color.White;
            }
            else
            {
                this.BackColor = Color.LightGray;
                this.ForeColor = Color.Black;
            }
        }

        public void SetSelected(bool selected)
        {
            isSelected = selected;
            UpdateSelection();
        }

        public bool IsSelected => isSelected;

        public Layer? Layer => layer;

        /// <summary>
        /// Cập nhật tên layer trên UI
        /// </summary>
        public void UpdateLayerName(string newName)
        {
            if (layer != null)
            {
                layer.Name = newName;
                Label? lblName = this.Controls["lblName"] as Label;
                if (lblName != null)
                    lblName.Text = newName;
            }
        }

        /// <summary>
        /// Cập nhật thông tin opacity trên UI
        /// </summary>
        public void UpdateOpacity(int opacity)
        {
            if (layer != null)
            {
                layer.Opacity = opacity;
                Label? lblOpacity = this.Controls["lblOpacity"] as Label;
                if (lblOpacity != null)
                    lblOpacity.Text = $"Op: {opacity}";
            }
        }
    }

    /// <summary>
    /// Mục đích: Thiết kế cho LayerItem (Designer class - tự động sinh ra)
    /// </summary>
    partial class LayerItem
    {
        private System.ComponentModel.IContainer? components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
        }
    }
}
