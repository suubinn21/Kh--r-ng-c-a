using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;

namespace PaintLike
{
    /// <summary>
    /// Mục đích: Bảng điều khiển layer chính - quản lý tất cả các layer
    /// Vị trí: Góc dưới phải của Form
    /// Chứa: ListBox hiển thị tất cả layer, buttons để Add/Delete/Move layers
    /// </summary>
    public partial class LayerPanel : UserControl
    {
        private List<Layer> layers;
        private int currentLayerIndex = 0;
        private FlowLayoutPanel? flowLayerItems;
        private Label? lblLayerTitle;
        private int canvasWidth = 800;  // Kích thước canvas - mặc định 800
        private int canvasHeight = 600; // Kích thước canvas - mặc định 600

        // Dragging fields
        private bool isDragging = false;
        private Point dragStartPosition = Point.Empty;

        // Resizing fields
        private bool isResizing = false;
        private Point resizeStartPosition = Point.Empty;
        private Size resizeStartSize = Size.Empty;
        private const int ResizeHandleSize = 10; // Kích thước xử lý resize (góc dưới phải)

        // Sự kiện thông báo cho Form1 khi có thay đổi
        public event EventHandler? LayerSelectionChanged;
        public event EventHandler? LayersChanged;

        public LayerPanel(List<Layer> layerList)
        {
            InitializeComponent();
            this.layers = layerList;
            this.DoubleBuffered = true;
            this.BackColor = Color.WhiteSmoke;
            this.BorderStyle = BorderStyle.FixedSingle;

            // Thiết lập kích thước mặc định
            this.Size = new Size(500, 580);  // Tăng để chứa phần filters
            this.Anchor = AnchorStyles.None; // Cho phép di chuyển tự do

            // Khởi tạo UI
            InitializeControls();
        }

        private void InitializeControls()
        {
            // Tiêu đề "Layers"
            lblLayerTitle = new Label();
            lblLayerTitle.Text = "LAYERS";
            lblLayerTitle.Font = new Font("Arial", 10, FontStyle.Bold);
            lblLayerTitle.Size = new Size(490, 25);
            lblLayerTitle.Location = new Point(5, 5);
            lblLayerTitle.TextAlign = ContentAlignment.MiddleLeft;
            lblLayerTitle.BackColor = Color.DarkGray;
            lblLayerTitle.ForeColor = Color.White;
            lblLayerTitle.Cursor = Cursors.Hand; // Thay đổi con trỏ để chỉ ra có thể kéo
            lblLayerTitle.MouseDown += (sender, e) => LblLayerTitle_MouseDown(sender, e);
            lblLayerTitle.MouseMove += (sender, e) => LblLayerTitle_MouseMove(sender, e);
            lblLayerTitle.MouseUp += (sender, e) => LblLayerTitle_MouseUp(sender, e);
            this.Controls.Add(lblLayerTitle);

            // FlowLayoutPanel để hiển thị danh sách layer
            flowLayerItems = new FlowLayoutPanel();
            flowLayerItems.Name = "flowLayerItems";
            flowLayerItems.Size = new Size(490, 200);  // Tăng height từ 145 lên 200 để fit nhiều items hơn
            flowLayerItems.Location = new Point(5, 35);
            flowLayerItems.AutoScroll = true;
            flowLayerItems.WrapContents = false;
            flowLayerItems.FlowDirection = FlowDirection.TopDown;
            flowLayerItems.BackColor = Color.White;
            flowLayerItems.BorderStyle = BorderStyle.FixedSingle;
            flowLayerItems.Margin = new Padding(0);
            flowLayerItems.Padding = new Padding(0);
            this.Controls.Add(flowLayerItems);

            // Panel chứa các nút điều khiển
            Panel panelButtons = new Panel();
            panelButtons.Size = new Size(490, 65);
            panelButtons.Location = new Point(5, 240);  // Cập nhật vị trí từ 190 lên 240 (do flowLayerItems lớn hơn)
            panelButtons.BackColor = Color.LightGray;
            this.Controls.Add(panelButtons);

            // Nút "Add Layer" - Thêm layer mới
            Button btnAddLayer = new Button();
            btnAddLayer.Text = "Add Layer";
            btnAddLayer.Size = new Size(110, 25);
            btnAddLayer.Location = new Point(5, 5);
            btnAddLayer.Click += (sender, e) => BtnAddLayer_Click(sender, e);
            panelButtons.Controls.Add(btnAddLayer);

            // Nút "Delete Layer" - Xóa layer hiện tại
            Button btnDeleteLayer = new Button();
            btnDeleteLayer.Text = "Delete";
            btnDeleteLayer.Size = new Size(100, 25);
            btnDeleteLayer.Location = new Point(118, 5);
            btnDeleteLayer.Click += (sender, e) => BtnDeleteLayer_Click(sender, e);
            panelButtons.Controls.Add(btnDeleteLayer);

            // Nút "Move Up" - Di chuyển layer lên
            Button btnMoveUp = new Button();
            btnMoveUp.Text = "Up ↑";
            btnMoveUp.Size = new Size(100, 25);
            btnMoveUp.Location = new Point(221, 5);
            btnMoveUp.Click += (sender, e) => BtnMoveUp_Click(sender, e);
            panelButtons.Controls.Add(btnMoveUp);

            // Nút "Move Down" - Di chuyển layer xuống
            Button btnMoveDown = new Button();
            btnMoveDown.Text = "Down ↓";
            btnMoveDown.Size = new Size(161, 25);
            btnMoveDown.Location = new Point(324, 5);
            btnMoveDown.Click += (sender, e) => BtnMoveDown_Click(sender, e);
            panelButtons.Controls.Add(btnMoveDown);

            // Nút "Rename" - Đổi tên layer
            Button btnRename = new Button();
            btnRename.Text = "Rename";
            btnRename.Size = new Size(150, 25);
            btnRename.Location = new Point(5, 33);
            btnRename.Click += (sender, e) => BtnRename_Click(sender, e);
            panelButtons.Controls.Add(btnRename);

            // Nút "Opacity" - Điều chỉnh độ trong suốt layer
            Button btnOpacity = new Button();
            btnOpacity.Text = "Opacity";
            btnOpacity.Size = new Size(150, 25);
            btnOpacity.Location = new Point(158, 33);
            btnOpacity.Click += (sender, e) => BtnOpacity_Click(sender, e);
            panelButtons.Controls.Add(btnOpacity);

            // Nút "Duplicate" - Nhân bản layer
            Button btnDuplicate = new Button();
            btnDuplicate.Text = "Duplicate";
            btnDuplicate.Size = new Size(174, 25);
            btnDuplicate.Location = new Point(311, 33);
            btnDuplicate.Click += (sender, e) => BtnDuplicate_Click(sender, e);
            panelButtons.Controls.Add(btnDuplicate);

            // ===== FILTERS PANEL =====
            Panel panelFilters = new Panel();
            panelFilters.Size = new Size(490, 270);
            panelFilters.Location = new Point(5, 310);
            panelFilters.BackColor = Color.LightGray;
            panelFilters.BorderStyle = BorderStyle.FixedSingle;
            this.Controls.Add(panelFilters);

            // Tiêu đề "FILTERS"
            Label lblFiltersTitle = new Label();
            lblFiltersTitle.Text = "IMAGE FILTERS";
            lblFiltersTitle.Font = new Font("Arial", 9, FontStyle.Bold);
            lblFiltersTitle.Size = new Size(480, 20);
            lblFiltersTitle.Location = new Point(5, 5);
            lblFiltersTitle.TextAlign = ContentAlignment.MiddleLeft;
            lblFiltersTitle.BackColor = Color.DarkGray;
            lblFiltersTitle.ForeColor = Color.White;
            panelFilters.Controls.Add(lblFiltersTitle);

            // Dropdown để chọn filter
            ComboBox cmbFilters = new ComboBox();
            cmbFilters.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbFilters.Size = new Size(300, 22);
            cmbFilters.Location = new Point(5, 28);
            cmbFilters.Items.AddRange(new object[]
            {
                "-- Chọn bộ lọc --",
                "Anti-Aliasing (Simple)",
                "Anti-Aliasing (Advanced)",
                "Anti-Aliasing (Curves)",
                "Brightness",
                "Contrast",
                "Blur",
                "Gaussian Blur",
                "Grayscale",
                "Edge Detection",
                "Sharpen",
                "Invert Colors",
                "Sepia",
                "Saturation"
            });
            cmbFilters.SelectedIndex = 0;
            panelFilters.Controls.Add(cmbFilters);

            // Nút Apply Filter
            Button btnApplyFilter = new Button();
            btnApplyFilter.Text = "Apply";
            btnApplyFilter.Size = new Size(70, 25);
            btnApplyFilter.Location = new Point(310, 28);
            btnApplyFilter.Click += (sender, e) => BtnApplyFilter_Click(sender, e, cmbFilters);
            panelFilters.Controls.Add(btnApplyFilter);

            // GridView / ListView hiển thị các filter nhanh
            Label lblQuickFilters = new Label();
            lblQuickFilters.Text = "Quick Filters:";
            lblQuickFilters.Size = new Size(480, 18);
            lblQuickFilters.Location = new Point(5, 58);
            lblQuickFilters.Font = new Font("Arial", 8, FontStyle.Bold);
            panelFilters.Controls.Add(lblQuickFilters);

            // Quick Filter Buttons - Dòng 1
            Button btnGrayscale = new Button();
            btnGrayscale.Text = "Grayscale";
            btnGrayscale.Size = new Size(70, 25);
            btnGrayscale.Location = new Point(5, 80);
            btnGrayscale.Click += (sender, e) => ApplyQuickFilter("Grayscale");
            panelFilters.Controls.Add(btnGrayscale);

            Button btnSepia = new Button();
            btnSepia.Text = "Sepia";
            btnSepia.Size = new Size(70, 25);
            btnSepia.Location = new Point(80, 80);
            btnSepia.Click += (sender, e) => ApplyQuickFilter("Sepia");
            panelFilters.Controls.Add(btnSepia);

            Button btnInvert = new Button();
            btnInvert.Text = "Invert";
            btnInvert.Size = new Size(70, 25);
            btnInvert.Location = new Point(155, 80);
            btnInvert.Click += (sender, e) => ApplyQuickFilter("Invert");
            panelFilters.Controls.Add(btnInvert);

            Button btnEdges = new Button();
            btnEdges.Text = "Edges";
            btnEdges.Size = new Size(70, 25);
            btnEdges.Location = new Point(230, 80);
            btnEdges.Click += (sender, e) => ApplyQuickFilter("Edges");
            panelFilters.Controls.Add(btnEdges);

            Button btnAA = new Button();
            btnAA.Text = "AA Simple";
            btnAA.Size = new Size(80, 25);
            btnAA.Location = new Point(305, 80);
            btnAA.Click += (sender, e) => ApplyQuickFilter("AASimple");
            panelFilters.Controls.Add(btnAA);

            // Quick Filter Buttons - Dòng 2
            Button btnBlur = new Button();
            btnBlur.Text = "Blur";
            btnBlur.Size = new Size(70, 25);
            btnBlur.Location = new Point(5, 110);
            btnBlur.Click += (sender, e) => ApplyQuickFilter("Blur");
            panelFilters.Controls.Add(btnBlur);

            Button btnSharpen = new Button();
            btnSharpen.Text = "Sharpen";
            btnSharpen.Size = new Size(70, 25);
            btnSharpen.Location = new Point(80, 110);
            btnSharpen.Click += (sender, e) => ApplyQuickFilter("Sharpen");
            panelFilters.Controls.Add(btnSharpen);

            Button btnBrightness = new Button();
            btnBrightness.Text = "Brightness";
            btnBrightness.Size = new Size(75, 25);
            btnBrightness.Location = new Point(155, 110);
            btnBrightness.Click += (sender, e) => ApplyQuickFilter("Brightness");
            panelFilters.Controls.Add(btnBrightness);

            Button btnContrast = new Button();
            btnContrast.Text = "Contrast";
            btnContrast.Size = new Size(70, 25);
            btnContrast.Location = new Point(235, 110);
            btnContrast.Click += (sender, e) => ApplyQuickFilter("Contrast");
            panelFilters.Controls.Add(btnContrast);

            Button btnSaturation = new Button();
            btnSaturation.Text = "Saturation";
            btnSaturation.Size = new Size(75, 25);
            btnSaturation.Location = new Point(310, 110);
            btnSaturation.Click += (sender, e) => ApplyQuickFilter("Saturation");
            panelFilters.Controls.Add(btnSaturation);

            // TextBox để hiển thị thông báo
            TextBox txtFilterStatus = new TextBox();
            txtFilterStatus.Size = new Size(480, 110);
            txtFilterStatus.Location = new Point(5, 140);
            txtFilterStatus.Multiline = true;
            txtFilterStatus.ReadOnly = true;
            txtFilterStatus.Text = "Chọn filter và nhấn 'Apply' để áp dụng\n\nCác nút Quick Filters áp dụng ngay với tham số mặc định\n\nHướng dẫn:\n- Brightness/Contrast/Blur: Hiện hộp thoại để nhập tham số\n- Sepia/Grayscale/Invert/..: Áp dụng trực tiếp";
            panelFilters.Controls.Add(txtFilterStatus);

            // Lưu reference để có thể cập nhật từ các hàm khác
            this.Tag = txtFilterStatus;

            // Cập nhật danh sách layer lên UI
            RefreshLayerList();
        }

        /// <summary>
        /// Trả về số lượng layer hiện có
        /// </summary>
        public int GetLayerCount()
        {
            return layers.Count;
        }

        /// <summary>
        /// Cập nhật danh sách layer trên UI
        /// Gọi mỗi khi có thay đổi (add/delete/move layer)
        /// </summary>
        public void RefreshLayerList()
        {
            if (flowLayerItems == null) return;
            
            System.Diagnostics.Debug.WriteLine($"\n[RefreshLayerList] START");
            System.Diagnostics.Debug.WriteLine($"[RefreshLayerList] this.layers object ID: {layers?.GetHashCode()}");
            System.Diagnostics.Debug.WriteLine($"[RefreshLayerList] layers.Count = {layers?.Count ?? 0}");
            System.Diagnostics.Debug.WriteLine($"[RefreshLayerList] Actual layers in list:");
            for (int idx = 0; idx < (layers?.Count ?? 0); idx++)
            {
                System.Diagnostics.Debug.WriteLine($"[RefreshLayerList]   [{idx}] {layers?[idx]?.Name ?? "Unknown"}");
            }
            
            flowLayerItems.Controls.Clear();

            System.Diagnostics.Debug.WriteLine($"[RefreshLayerList] After clear: flowLayerItems.Controls.Count = {flowLayerItems.Controls.Count}");
            System.Diagnostics.Debug.WriteLine($"[RefreshLayerList] Adding {layers?.Count ?? 0} layers...");
            
            if (layers == null) return;
            
            // Thêm từng layer từ dưới lên trên (hiển thị theo thứ tự đúng)
            for (int i = layers.Count - 1; i >= 0; i--)
            {
                System.Diagnostics.Debug.WriteLine($"[RefreshLayerList]   Processing layer index {i}: {layers[i].Name}");
                LayerItem layerItem = new LayerItem(layers[i]);
                System.Diagnostics.Debug.WriteLine($"[RefreshLayerList]   Created LayerItem, size = {layerItem.Size}");
                
                if (i == currentLayerIndex)
                {
                    System.Diagnostics.Debug.WriteLine($"[RefreshLayerList]   Setting selected");
                    layerItem.SetSelected(true);
                }

                layerItem.VisibilityChanged += (s, e) => LayersChanged?.Invoke(this, EventArgs.Empty);
                layerItem.SelectionChanged += (s, e) =>
                {
                    // Cập nhật lựa chọn layer hiện tại
                    foreach (LayerItem item in flowLayerItems.Controls)
                    {
                        item.SetSelected(false);
                    }
                    layerItem.SetSelected(true);
                    
                    // Tìm index của layer được chọn
                    for (int j = 0; j < layers.Count; j++)
                    {
                        if (layers[j] == layerItem.Layer)
                        {
                            currentLayerIndex = j;
                            break;
                        }
                    }
                    LayerSelectionChanged?.Invoke(this, EventArgs.Empty);
                };

                flowLayerItems.Controls.Add(layerItem);
                System.Diagnostics.Debug.WriteLine($"[RefreshLayerList]   Added to flowLayerItems. Now has {flowLayerItems.Controls.Count} items");
                System.Diagnostics.Debug.WriteLine($"[RefreshLayerList]   Item parent is: {layerItem.Parent?.Name ?? "NULL"}, Parent type: {layerItem.Parent?.GetType().Name ?? "NULL"}");
            }
            
            // Force refresh để hiển thị items
            System.Diagnostics.Debug.WriteLine($"[RefreshLayerList] Before Invalidate: flowLayerItems.Size = {flowLayerItems.Size}");
            System.Diagnostics.Debug.WriteLine($"[RefreshLayerList] Before Invalidate: flowLayerItems.Controls.Count = {flowLayerItems.Controls.Count}");
            for (int k = 0; k < flowLayerItems.Controls.Count; k++)
            {
                var control = flowLayerItems.Controls[k];
                System.Diagnostics.Debug.WriteLine($"[RefreshLayerList]   Control[{k}]: Type={control.GetType().Name}, Size={control.Size}, Visible={control.Visible}");
            }
            
            flowLayerItems.Invalidate();
            flowLayerItems.Refresh();
            this.Invalidate();
            this.Refresh();
            
            System.Diagnostics.Debug.WriteLine($"[RefreshLayerList] END\n");
        }

        // ========== Các Button Handler ==========

        private void BtnAddLayer_Click(object? sender, EventArgs e)
        {
            // Tạo layer mới với kích thước canvas hiện tại
            string layerName = $"Layer {layers.Count + 1}";
            Layer newLayer = new Layer(layerName, canvasWidth, canvasHeight); // Sử dụng kích thước canvas thực tế

            // Thêm vào đầu danh sách (trên cùng)
            layers.Insert(0, newLayer);
            currentLayerIndex = 0;

            RefreshLayerList();
            LayersChanged?.Invoke(this, EventArgs.Empty);
        }

        private void BtnDeleteLayer_Click(object? sender, EventArgs e)
        {
            if (layers.Count <= 1)
            {
                MessageBox.Show("Phải giữ lại ít nhất 1 layer!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (currentLayerIndex >= 0 && currentLayerIndex < layers.Count)
            {
                layers[currentLayerIndex].Dispose();
                layers.RemoveAt(currentLayerIndex);

                // Điều chỉnh currentLayerIndex nếu cần
                if (currentLayerIndex >= layers.Count)
                    currentLayerIndex = layers.Count - 1;

                RefreshLayerList();
                LayersChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private void BtnMoveUp_Click(object? sender, EventArgs e)
        {
            // Di chuyển layer lên (tăng index)
            if (currentLayerIndex < layers.Count - 1)
            {
                Layer temp = layers[currentLayerIndex];
                layers[currentLayerIndex] = layers[currentLayerIndex + 1];
                layers[currentLayerIndex + 1] = temp;
                
                currentLayerIndex++;
                RefreshLayerList();
                LayersChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private void BtnMoveDown_Click(object? sender, EventArgs e)
        {
            // Di chuyển layer xuống (giảm index)
            if (currentLayerIndex > 0)
            {
                Layer temp = layers[currentLayerIndex];
                layers[currentLayerIndex] = layers[currentLayerIndex - 1];
                layers[currentLayerIndex - 1] = temp;
                
                currentLayerIndex--;
                RefreshLayerList();
                LayersChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private void BtnRename_Click(object? sender, EventArgs e)
        {
            if (currentLayerIndex >= 0 && currentLayerIndex < layers.Count)
            {
                Layer currentLayer = layers[currentLayerIndex];
                
                // Hiệu ứng Input Dialog
                string? newName = PromptDialog("Nhập tên layer mới:", currentLayer.Name);
                if (!string.IsNullOrEmpty(newName))
                {
                    currentLayer.Name = newName;
                    RefreshLayerList();
                    LayersChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        private void BtnOpacity_Click(object? sender, EventArgs e)
        {
            if (currentLayerIndex >= 0 && currentLayerIndex < layers.Count)
            {
                Layer currentLayer = layers[currentLayerIndex];
                
                // Tạo form để điều chỉnh opacity
                Form opacityForm = new Form();
                opacityForm.Text = "Điều chỉnh Opacity";
                opacityForm.Size = new Size(300, 120);
                opacityForm.StartPosition = FormStartPosition.CenterParent;
                opacityForm.FormBorderStyle = FormBorderStyle.FixedDialog;
                opacityForm.MaximizeBox = false;
                opacityForm.MinimizeBox = false;

                TrackBar trackOpacity = new TrackBar();
                trackOpacity.Minimum = 0;
                trackOpacity.Maximum = 255;
                trackOpacity.Value = currentLayer.Opacity;
                trackOpacity.Size = new Size(260, 40);
                trackOpacity.Location = new Point(10, 10);
                opacityForm.Controls.Add(trackOpacity);

                Label lblValue = new Label();
                lblValue.Text = $"Opacity: {currentLayer.Opacity}";
                lblValue.Size = new Size(260, 20);
                lblValue.Location = new Point(10, 50);
                opacityForm.Controls.Add(lblValue);

                trackOpacity.ValueChanged += (s, e) =>
                {
                    lblValue.Text = $"Opacity: {trackOpacity.Value}";
                };

                Button btnOk = new Button();
                btnOk.Text = "OK";
                btnOk.Size = new Size(75, 25);
                btnOk.Location = new Point(100, 70);
                btnOk.Click += (s, e) =>
                {
                    currentLayer.Opacity = trackOpacity.Value;
                    opacityForm.Close();
                    RefreshLayerList();
                    LayersChanged?.Invoke(this, EventArgs.Empty);
                };
                opacityForm.Controls.Add(btnOk);

                opacityForm.ShowDialog();
            }
        }

        private void BtnDuplicate_Click(object? sender, EventArgs e)
        {
            if (currentLayerIndex >= 0 && currentLayerIndex < layers.Count)
            {
                Layer duplicatedLayer = layers[currentLayerIndex].Clone();
                layers.Insert(currentLayerIndex, duplicatedLayer);
                
                RefreshLayerList();
                LayersChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Helper function để tạo dialog nhập liệu
        /// </summary>
        private string? PromptDialog(string prompt, string defaultValue = "")
        {
            Form promptForm = new Form();
            promptForm.Text = "Nhập dữ liệu";
            promptForm.Width = 300;
            promptForm.Height = 120;
            promptForm.StartPosition = FormStartPosition.CenterParent;
            promptForm.FormBorderStyle = FormBorderStyle.FixedDialog;
            promptForm.MaximizeBox = false;
            promptForm.MinimizeBox = false;

            Label label = new Label() { Left = 20, Top = 20, Text = prompt, Width = 250 };
            TextBox textBox = new TextBox() { Left = 20, Top = 50, Width = 250, Text = defaultValue };
            Button okButton = new Button() { Text = "OK", Left = 120, Width = 70, Top = 80, DialogResult = DialogResult.OK };
            Button cancelButton = new Button() { Text = "Hủy", Left = 200, Width = 70, Top = 80, DialogResult = DialogResult.Cancel };

            promptForm.Controls.Add(label);
            promptForm.Controls.Add(textBox);
            promptForm.Controls.Add(okButton);
            promptForm.Controls.Add(cancelButton);
            promptForm.AcceptButton = okButton;
            promptForm.CancelButton = cancelButton;

            return promptForm.ShowDialog() == DialogResult.OK ? textBox.Text : null;
        }

        /// <summary>
        /// Trả về layer hiện tại được chọn
        /// </summary>
        public Layer? GetCurrentLayer()
        {
            if (currentLayerIndex >= 0 && currentLayerIndex < layers.Count)
                return layers[currentLayerIndex];
            return null;
        }

        /// <summary>
        /// Đặt kích thước canvas để tạo layer với kích thước phù hợp
        /// </summary>
        public void SetCanvasDimensions(int width, int height)
        {
            canvasWidth = width;
            canvasHeight = height;
        }

        /// <summary>
        /// Trả về index của layer hiện tại
        /// </summary>
        public int GetCurrentLayerIndex()
        {
            return currentLayerIndex;
        }

        /// <summary>
        /// Cập nhật kích thước canvas cho tất cả layer (dùng khi Form resize)
        /// </summary>
        public void UpdateAllLayersSize(int newWidth, int newHeight)
        {
            foreach (Layer layer in layers)
            {
                if (layer.Image.Width != newWidth || layer.Image.Height != newHeight)
                {
                    Bitmap newBitmap = new Bitmap(newWidth, newHeight, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                    using (Graphics g = Graphics.FromImage(newBitmap))
                    {
                        g.Clear(Color.Transparent);
                        g.DrawImage(layer.Image, 0, 0);
                    }
                    layer.Image.Dispose();
                    layer.Image = newBitmap;
                }
            }
        }

        /// <summary>
        /// Xử lý khi nhấn chuột trên thanh tiêu đề để bắt đầu kéo
        /// </summary>
        private void LblLayerTitle_MouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isDragging = true;
                dragStartPosition = e.Location;
            }
        }

        /// <summary>
        /// Xử lý di chuyển chuột để kéo panel
        /// </summary>
        private void LblLayerTitle_MouseMove(object? sender, MouseEventArgs e)
        {
            if (isDragging && e.Button == MouseButtons.Left)
            {
                int offsetX = e.X - dragStartPosition.X;
                int offsetY = e.Y - dragStartPosition.Y;

                // Lấy vị trí hiện tại của panel trên Form
                Point newLocation = this.Location;
                newLocation.X += offsetX;
                newLocation.Y += offsetY;

                // Giới hạn panel không vượt quá ranh giới của Form
                Control? parent = this.Parent;
                if (parent != null)
                {
                    newLocation.X = Math.Max(0, Math.Min(newLocation.X, parent.ClientSize.Width - this.Width));
                    newLocation.Y = Math.Max(0, Math.Min(newLocation.Y, parent.ClientSize.Height - this.Height));
                }

                this.Location = newLocation;
            }
        }

        /// <summary>
        /// Xử lý khi thả chuột để kết thúc kéo
        /// </summary>
        private void LblLayerTitle_MouseUp(object? sender, MouseEventArgs e)
        {
            isDragging = false;
        }

        /// <summary>
        /// Xử lý sự kiện chuột trên toàn bộ panel để hỗ trợ resize
        /// </summary>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            // Kiểm tra nếu chuột ở trong vùng resize (góc dưới phải)
            if (IsInResizeArea(e.Location))
            {
                isResizing = true;
                resizeStartPosition = e.Location;
                resizeStartSize = this.Size;
                this.Cursor = Cursors.SizeNWSE;
            }
        }

        /// <summary>
        /// Xử lý di chuyển chuột để resize panel
        /// </summary>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            // Kiểm tra nếu chuột ở trong vùng resize
            if (IsInResizeArea(e.Location) && !isResizing)
            {
                this.Cursor = Cursors.SizeNWSE;
            }
            else if (!IsInResizeArea(e.Location) && !isResizing)
            {
                this.Cursor = Cursors.Default;
            }

            // Thực hiện resize nếu đang kéo
            if (isResizing)
            {
                int offsetX = e.X - resizeStartPosition.X;
                int offsetY = e.Y - resizeStartPosition.Y;

                int newWidth = Math.Max(300, resizeStartSize.Width + offsetX);
                int newHeight = Math.Max(200, resizeStartSize.Height + offsetY);

                this.Size = new Size(newWidth, newHeight);
                this.Refresh();
            }
        }

        /// <summary>
        /// Xử lý khi thả chuột để kết thúc resize
        /// </summary>
        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            isResizing = false;
            this.Cursor = Cursors.Default;
        }

        /// <summary>
        /// Kiểm tra nếu chuột nằm trong vùng resize (góc dưới phải)
        /// </summary>
        private bool IsInResizeArea(Point point)
        {
            return point.X > this.Width - ResizeHandleSize &&
                   point.Y > this.Height - ResizeHandleSize;
        }

        /// <summary>
        /// Vẽ chỉ báo vùng resize trên panel
        /// </summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            // Vẽ hình tam giác nhỏ ở góc dưới phải để chỉ ra có thể resize
            Point[] resizeIndicator = new Point[]
            {
                new Point(this.Width - 12, this.Height - 3),
                new Point(this.Width - 3, this.Height - 3),
                new Point(this.Width - 3, this.Height - 12)
            };
            e.Graphics.FillPolygon(Brushes.Gray, resizeIndicator);
        }

        // ===== IMAGE FILTER HANDLERS =====

        /// <summary>
        /// Áp dụng bộ lọc được chọn từ dropdown
        /// </summary>
        private void BtnApplyFilter_Click(object? sender, EventArgs e, ComboBox cmbFilters)
        {
            if (currentLayerIndex < 0 || currentLayerIndex >= layers.Count)
            {
                MessageBox.Show("Vui lòng chọn layer trước!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Layer currentLayer = layers[currentLayerIndex];
            
            switch (cmbFilters.SelectedIndex)
            {
                case 1: // Anti-Aliasing (Simple)
                    currentLayer.ApplyAntiAliasing("simple");
                    UpdateFilterStatus("✓ Đã áp dụng Anti-Aliasing (Simple)");
                    break;
                case 2: // Anti-Aliasing (Advanced)
                    currentLayer.ApplyAntiAliasing("advanced");
                    UpdateFilterStatus("✓ Đã áp dụng Anti-Aliasing (Advanced)");
                    break;
                case 3: // Anti-Aliasing (Curves)
                    currentLayer.ApplyAntiAliasing("curves");
                    UpdateFilterStatus("✓ Đã áp dụng Anti-Aliasing (Curves)");
                    break;
                case 4: // Brightness
                    ShowBrightnessDialog(currentLayer);
                    break;
                case 5: // Contrast
                    ShowContrastDialog(currentLayer);
                    break;
                case 6: // Blur
                    ShowBlurDialog(currentLayer, false);
                    break;
                case 7: // Gaussian Blur
                    ShowBlurDialog(currentLayer, true);
                    break;
                case 8: // Grayscale
                    currentLayer.ConvertToGrayscale();
                    UpdateFilterStatus("✓ Đã chuyển sang Grayscale");
                    break;
                case 9: // Edge Detection
                    currentLayer.DetectEdges();
                    UpdateFilterStatus("✓ Đã áp dụng Edge Detection");
                    break;
                case 10: // Sharpen
                    currentLayer.ApplySharpen(2);
                    UpdateFilterStatus("✓ Đã áp dụng Sharpen");
                    break;
                case 11: // Invert Colors
                    currentLayer.InvertColors();
                    UpdateFilterStatus("✓ Đã đảo ngược màu");
                    break;
                case 12: // Sepia
                    currentLayer.ApplySepia();
                    UpdateFilterStatus("✓ Đã áp dụng Sepia");
                    break;
                case 13: // Saturation
                    ShowSaturationDialog(currentLayer);
                    break;
                default:
                    UpdateFilterStatus("Chưa chọn bộ lọc nào!");
                    return;
            }

            LayersChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Áp dụng filter nhanh
        /// </summary>
        private void ApplyQuickFilter(string filterName)
        {
            if (currentLayerIndex < 0 || currentLayerIndex >= layers.Count)
            {
                MessageBox.Show("Vui lòng chọn layer trước!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Layer currentLayer = layers[currentLayerIndex];

            switch (filterName)
            {
                case "Grayscale":
                    currentLayer.ConvertToGrayscale();
                    UpdateFilterStatus("✓ Đã chuyển sang Grayscale");
                    break;
                case "Sepia":
                    currentLayer.ApplySepia();
                    UpdateFilterStatus("✓ Đã áp dụng Sepia");
                    break;
                case "Invert":
                    currentLayer.InvertColors();
                    UpdateFilterStatus("✓ Đã đảo ngược màu");
                    break;
                case "Edges":
                    currentLayer.DetectEdges();
                    UpdateFilterStatus("✓ Đã áp dụng Edge Detection");
                    break;
                case "AASimple":
                    currentLayer.ApplyAntiAliasing("simple");
                    UpdateFilterStatus("✓ Đã áp dụng Anti-Aliasing (Simple)");
                    break;
                case "Blur":
                    ShowBlurDialog(currentLayer, false);
                    break;
                case "Sharpen":
                    currentLayer.ApplySharpen(2);
                    UpdateFilterStatus("✓ Đã áp dụng Sharpen");
                    break;
                case "Brightness":
                    ShowBrightnessDialog(currentLayer);
                    break;
                case "Contrast":
                    ShowContrastDialog(currentLayer);
                    break;
                case "Saturation":
                    ShowSaturationDialog(currentLayer);
                    break;
            }

            LayersChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Hiển thị dialog để điều chỉnh brightness
        /// </summary>
        private void ShowBrightnessDialog(Layer currentLayer)
        {
            Form brightnessForm = new Form();
            brightnessForm.Text = "Điều chỉnh Brightness";
            brightnessForm.Size = new Size(300, 150);
            brightnessForm.StartPosition = FormStartPosition.CenterParent;
            brightnessForm.FormBorderStyle = FormBorderStyle.FixedDialog;
            brightnessForm.MaximizeBox = false;
            brightnessForm.MinimizeBox = false;

            TrackBar trackBrightness = new TrackBar();
            trackBrightness.Minimum = -100;
            trackBrightness.Maximum = 100;
            trackBrightness.Value = 0;
            trackBrightness.Size = new Size(260, 40);
            trackBrightness.Location = new Point(10, 10);
            brightnessForm.Controls.Add(trackBrightness);

            Label lblValue = new Label();
            lblValue.Text = "Brightness: 0";
            lblValue.Size = new Size(260, 20);
            lblValue.Location = new Point(10, 50);
            brightnessForm.Controls.Add(lblValue);

            trackBrightness.ValueChanged += (s, e) =>
            {
                lblValue.Text = $"Brightness: {trackBrightness.Value}";
            };

            Button btnOk = new Button();
            btnOk.Text = "OK";
            btnOk.Size = new Size(75, 25);
            btnOk.Location = new Point(75, 80);
            btnOk.Click += (s, e) =>
            {
                currentLayer.ApplyBrightness(trackBrightness.Value);
                brightnessForm.Close();
                UpdateFilterStatus($"✓ Brightness áp dụng: {trackBrightness.Value}");
                LayersChanged?.Invoke(this, EventArgs.Empty);
            };
            brightnessForm.Controls.Add(btnOk);

            Button btnCancel = new Button();
            btnCancel.Text = "Hủy";
            btnCancel.Size = new Size(75, 25);
            btnCancel.Location = new Point(155, 80);
            btnCancel.Click += (s, e) => brightnessForm.Close();
            brightnessForm.Controls.Add(btnCancel);

            brightnessForm.ShowDialog();
        }

        /// <summary>
        /// Hiển thị dialog để điều chỉnh contrast
        /// </summary>
        private void ShowContrastDialog(Layer currentLayer)
        {
            Form contrastForm = new Form();
            contrastForm.Text = "Điều chỉnh Contrast";
            contrastForm.Size = new Size(300, 150);
            contrastForm.StartPosition = FormStartPosition.CenterParent;
            contrastForm.FormBorderStyle = FormBorderStyle.FixedDialog;
            contrastForm.MaximizeBox = false;
            contrastForm.MinimizeBox = false;

            TrackBar trackContrast = new TrackBar();
            trackContrast.Minimum = 1;
            trackContrast.Maximum = 30; // 0.5 to 3.0 với step 0.1
            trackContrast.Value = 10; // 1.0
            trackContrast.Size = new Size(260, 40);
            trackContrast.Location = new Point(10, 10);
            contrastForm.Controls.Add(trackContrast);

            Label lblValue = new Label();
            lblValue.Text = "Contrast: 1.0";
            lblValue.Size = new Size(260, 20);
            lblValue.Location = new Point(10, 50);
            contrastForm.Controls.Add(lblValue);

            trackContrast.ValueChanged += (s, e) =>
            {
                double contrast = 0.5 + (trackContrast.Value - 1) * 0.1;
                lblValue.Text = $"Contrast: {contrast:F1}";
            };

            Button btnOk = new Button();
            btnOk.Text = "OK";
            btnOk.Size = new Size(75, 25);
            btnOk.Location = new Point(75, 80);
            btnOk.Click += (s, e) =>
            {
                double contrast = 0.5 + (trackContrast.Value - 1) * 0.1;
                currentLayer.ApplyContrast(contrast);
                contrastForm.Close();
                UpdateFilterStatus($"✓ Contrast áp dụng: {contrast:F1}");
                LayersChanged?.Invoke(this, EventArgs.Empty);
            };
            contrastForm.Controls.Add(btnOk);

            Button btnCancel = new Button();
            btnCancel.Text = "Hủy";
            btnCancel.Size = new Size(75, 25);
            btnCancel.Location = new Point(155, 80);
            btnCancel.Click += (s, e) => contrastForm.Close();
            contrastForm.Controls.Add(btnCancel);

            contrastForm.ShowDialog();
        }

        /// <summary>
        /// Hiển thị dialog để điều chỉnh Blur
        /// </summary>
        private void ShowBlurDialog(Layer currentLayer, bool isGaussian)
        {
            Form blurForm = new Form();
            blurForm.Text = isGaussian ? "Gaussian Blur" : "Blur";
            blurForm.Size = new Size(300, 150);
            blurForm.StartPosition = FormStartPosition.CenterParent;
            blurForm.FormBorderStyle = FormBorderStyle.FixedDialog;
            blurForm.MaximizeBox = false;
            blurForm.MinimizeBox = false;

            TrackBar trackBlur = new TrackBar();
            trackBlur.Minimum = 1;
            trackBlur.Maximum = 10;
            trackBlur.Value = 3;
            trackBlur.Size = new Size(260, 40);
            trackBlur.Location = new Point(10, 10);
            blurForm.Controls.Add(trackBlur);

            Label lblValue = new Label();
            lblValue.Text = "Blur Strength: 3";
            lblValue.Size = new Size(260, 20);
            lblValue.Location = new Point(10, 50);
            blurForm.Controls.Add(lblValue);

            trackBlur.ValueChanged += (s, e) =>
            {
                lblValue.Text = $"Blur Strength: {trackBlur.Value}";
            };

            Button btnOk = new Button();
            btnOk.Text = "OK";
            btnOk.Size = new Size(75, 25);
            btnOk.Location = new Point(75, 80);
            btnOk.Click += (s, e) =>
            {
                if (isGaussian)
                {
                    currentLayer.ApplyGaussianBlur(trackBlur.Value);
                    UpdateFilterStatus($"✓ Gaussian Blur áp dụng: {trackBlur.Value}");
                }
                else
                {
                    currentLayer.ApplyBlur(trackBlur.Value);
                    UpdateFilterStatus($"✓ Blur áp dụng: {trackBlur.Value}");
                }
                blurForm.Close();
                LayersChanged?.Invoke(this, EventArgs.Empty);
            };
            blurForm.Controls.Add(btnOk);

            Button btnCancel = new Button();
            btnCancel.Text = "Hủy";
            btnCancel.Size = new Size(75, 25);
            btnCancel.Location = new Point(155, 80);
            btnCancel.Click += (s, e) => blurForm.Close();
            blurForm.Controls.Add(btnCancel);

            blurForm.ShowDialog();
        }

        /// <summary>
        /// Hiển thị dialog để điều chỉnh Saturation
        /// </summary>
        private void ShowSaturationDialog(Layer currentLayer)
        {
            Form saturationForm = new Form();
            saturationForm.Text = "Điều chỉnh Saturation";
            saturationForm.Size = new Size(300, 150);
            saturationForm.StartPosition = FormStartPosition.CenterParent;
            saturationForm.FormBorderStyle = FormBorderStyle.FixedDialog;
            saturationForm.MaximizeBox = false;
            saturationForm.MinimizeBox = false;

            TrackBar trackSaturation = new TrackBar();
            trackSaturation.Minimum = 1;
            trackSaturation.Maximum = 20; // 0.0 to 2.0 với step 0.1
            trackSaturation.Value = 10; // 1.0
            trackSaturation.Size = new Size(260, 40);
            trackSaturation.Location = new Point(10, 10);
            saturationForm.Controls.Add(trackSaturation);

            Label lblValue = new Label();
            lblValue.Text = "Saturation: 1.0";
            lblValue.Size = new Size(260, 20);
            lblValue.Location = new Point(10, 50);
            saturationForm.Controls.Add(lblValue);

            trackSaturation.ValueChanged += (s, e) =>
            {
                double saturation = trackSaturation.Value * 0.1;
                lblValue.Text = $"Saturation: {saturation:F1}";
            };

            Button btnOk = new Button();
            btnOk.Text = "OK";
            btnOk.Size = new Size(75, 25);
            btnOk.Location = new Point(75, 80);
            btnOk.Click += (s, e) =>
            {
                double saturation = trackSaturation.Value * 0.1;
                currentLayer.AdjustSaturation(saturation);
                saturationForm.Close();
                UpdateFilterStatus($"✓ Saturation áp dụng: {saturation:F1}");
                LayersChanged?.Invoke(this, EventArgs.Empty);
            };
            saturationForm.Controls.Add(btnOk);

            Button btnCancel = new Button();
            btnCancel.Text = "Hủy";
            btnCancel.Size = new Size(75, 25);
            btnCancel.Location = new Point(155, 80);
            btnCancel.Click += (s, e) => saturationForm.Close();
            saturationForm.Controls.Add(btnCancel);

            saturationForm.ShowDialog();
        }

        /// <summary>
        /// Cập nhật status message
        /// </summary>
        private void UpdateFilterStatus(string message)
        {
            if (this.Tag is TextBox txtStatus)
            {
                txtStatus.AppendText($"\n{message}");
                // Giữ chỉ 10 dòng cuối
                string[] lines = txtStatus.Text.Split('\n');
                if (lines.Length > 10)
                {
                    txtStatus.Text = string.Join("\n", lines.Skip(lines.Length - 10));
                }
                txtStatus.SelectionStart = txtStatus.Text.Length;
                txtStatus.ScrollToCaret();
            }
        }
        //Thanh
        // Hàm này giúp làm mới danh sách hiển thị các Layer trên giao diện sau khi Undo/Redo
        public void RefreshLayersList(List<Layer> newLayers)
        {
            if (newLayers == null) return; // FIX null safety
            // Cập nhật lại danh sách dữ liệu
            this.layers = newLayers;

            // Kiểm tra nếu Panel chứa các dòng layer tồn tại
            if (flowLayerItems == null) return;
            // 1. Xóa sạch các dòng LayerItem cũ đang hiển thị trên giao diện
            flowLayerItems.Controls.Clear();

            // 2. Tạo lại các dòng LayerItem mới từ danh sách vừa cập nhật
            // Duyệt từ cuối lên đầu để hiển thị đúng thứ tự (giống RefreshLayerList)
            for (int i = layers.Count - 1; i >= 0; i--)
            {
                Layer layer = layers[i];
                LayerItem item = new LayerItem(layer);

                // Set selected state cho layer hiện tại
                if (i == currentLayerIndex)
                {
                    item.SetSelected(true);
                }

                // Gán lại các sự kiện quan trọng để người dùng vẫn nhấn được
                item.VisibilityChanged += (s, e) => LayersChanged?.Invoke(this, EventArgs.Empty);
                item.SelectionChanged += (s, e) => {
                    // Deselect tất cả items khác
                    foreach (LayerItem otherItem in flowLayerItems.Controls)
                    {
                        otherItem.SetSelected(false);
                    }
                    // Select item này
                    item.SetSelected(true);
                    
                    currentLayerIndex = layers.IndexOf(layer);
                    LayerSelectionChanged?.Invoke(this, EventArgs.Empty);
                };

                // Đưa dòng layer mới vào bảng hiển thị
                flowLayerItems.Controls.Add(item);
            }
        }
    }

    /// <summary>
    /// Designer class cho LayerPanel (tự động sinh ra)
    /// </summary>
}
