using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D; // Cần cho SmoothingMode
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace PaintLike
{
    // Danh sách các công cụ
    public enum Tool { Line, Rectangle, Circle, Triangle, Eraser, Star, Trapezoid, WuLine,
        SmartSelect, Hexagon, Diamond, Arrows, Ellipse, Bezier, RoundedRectangle }

    public partial class Form1 : Form
    {

        private Bitmap canvas;
        private Point start, end;
        private Pen pen;
        private bool drawing = false;
        private Tool currentTool = Tool.Line;
        private int eraserSize = 10;
        private float zoomScale = 1.0f;
        private Label? lblCoordinates; // MỚI: Label hiển thị tọa độ
        private Bitmap? importedImage; // MỚI: Lưu ảnh đã import
        private Point importImagePos; // MỚI: Vị trí dán ảnh
        private int lineOpacity = 255; // MỚI: Độ trong suốt của đường vẽ (0-255)

        // MỚI: Layer System
        private List<Layer> layers = new List<Layer>(); // Danh sách tất cả các layer
        private LayerPanel? layerPanel; // Bảng điều khiển layer

        //Thanh 11/4/2026
        private List<List<Layer>> undoList = new List<List<Layer>>();
        private Stack<List<Layer>> redoStack = new Stack<List<Layer>>();
        private const int Max_undo_Steps = 5;

        private int currentLayerIndex = 0;

        // Thêm hàm này vào gần hàm GetRect cũ (dòng 170)
        private Rectangle GetRectangle(Point p1, Point p2)
        {
            return GetRect(p1, p2);
        }
        public Form1()
        {
            InitializeComponent();


            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.ResizeRedraw, true);


            // Khởi tạo bút vẽ
            pen = new Pen(Color.Black, 2);
            pen.StartCap = pen.EndCap = LineCap.Round;

            // Tạo Canvas ban đầu
            // Lưu ý: Dùng Math.Max  tránh kích thước = 0
            int w = Math.Max(1, pictureBox1.Width);
            int h = Math.Max(1, pictureBox1.Height);
            canvas = new Bitmap(w, h);

            using (Graphics g = Graphics.FromImage(canvas))
            {
                g.Clear(Color.White);
            }
            // FIX: Không gán Image - dùng Paint event để render có zoom
            // pictureBox1.Image = canvas;

            this.MouseWheel += (sender, e) => Form1_MouseWheel(sender, e);

            this.Resize += (sender, e) => Form1_Resize(sender, e);

            try { nudEraserSize.Value = eraserSize; } catch { }

            // MỚI: Khởi tạo Label hiển thị tọa độ
            InitializeCoordinatesLabel();

            // MỚI: Khởi tạo Layer System
            InitializeLayerSystem();
        }

        // MỚI: Hàm khởi tạo Label tọa độ
        private void InitializeCoordinatesLabel()
        {
            lblCoordinates = new Label();
            lblCoordinates.AutoSize = true;
            lblCoordinates.BackColor = Color.White;
            lblCoordinates.BorderStyle = BorderStyle.FixedSingle;
            lblCoordinates.Padding = new Padding(5);
            // Đặt ở góc trên phải, cách right 10px
            lblCoordinates.Location = new Point(this.ClientSize.Width - 250, 20);
            lblCoordinates.Font = new Font("Courier New", 10F, FontStyle.Regular);
            lblCoordinates.Text = "X: 0, Y: 0"; // giá trị mặc định
            lblCoordinates.ForeColor = Color.Black;
            // Đặt anchor để label luôn ở bên phải khi resize cửa sổ
            lblCoordinates.Anchor = AnchorStyles.Top | AnchorStyles.Right;

            // Thêm Label vào Form
            this.Controls.Add(lblCoordinates);
            lblCoordinates.BringToFront(); // đặt trên cùng để không bị che
        }

        // MỚI: Hàm khởi tạo Layer System
        private void InitializeLayerSystem()
        {
            // Tạo layer ban đầu với tên "Background"
            Layer backgroundLayer = new Layer("Background", canvas.Width, canvas.Height);

            // Sao chép nội dung canvas hiện tại vào background layer
            using (Graphics g = Graphics.FromImage(backgroundLayer.Image))
            {
                g.DrawImage(canvas, 0, 0);
            }

            layers.Add(backgroundLayer);

            // Tạo LayerPanel UserControl và thêm vào Form
            layerPanel = new LayerPanel(layers);
            layerPanel.Location = new Point(
                this.ClientSize.Width - layerPanel.Width - 10,
                this.ClientSize.Height - layerPanel.Height - 10);
            layerPanel.Anchor = AnchorStyles.None; // Cho phép di chuyển và resize tự do

            // Đặt kích thước canvas cho LayerPanel để tạo layer với kích thước đúng
            layerPanel.SetCanvasDimensions(canvas.Width, canvas.Height);

            // Đăng ký sự kiện từ LayerPanel
            layerPanel.LayersChanged += LayerPanel_LayersChanged;
            layerPanel.LayerSelectionChanged += LayerPanel_LayerSelectionChanged;

            this.Controls.Add(layerPanel);
            layerPanel.BringToFront();
        }

        // MỚI: Hàm để ẩn/hiện 6 button hình dạng advanced
        public void SetAdvancedShapeButtonsVisible(bool visible)
        {
            BttHexagon.Visible = visible;
            BttDiamond.Visible = visible;
            BttArrows.Visible = visible;
            BttEllipse.Visible = visible;
            BttBezier.Visible = visible;
            BttRoundedRectangle.Visible = visible;
        }

        // MỚI: Hàm để toggle ẩn/hiện 6 button hình dạng advanced
        public void ToggleAdvancedShapeButtons()
        {
            bool currentVisibility = BttHexagon.Visible;
            SetAdvancedShapeButtonsVisible(!currentVisibility);
        }

        // Xử lý khi có thay đổi layer
        private void LayerPanel_LayersChanged(object? sender, EventArgs e)
        {
            // Cập nhật currentLayerIndex để phù hợp với LayerPanel
            if (layerPanel != null)
            {
                currentLayerIndex = layerPanel.GetCurrentLayerIndex();
            }
            // FIX: Gộp các layer lại để cập nhật canvas khi layer ẩn/hiện
            RecomposeCanvas();
            pictureBox1.Invalidate(); // Vẽ lại canvas
        }

        // Xử lý khi chọn layer khác
        private void LayerPanel_LayerSelectionChanged(object? sender, EventArgs e)
        {
            // Cập nhật currentLayerIndex của Form1 để phù hợp với LayerPanel
            if (layerPanel != null)
            {
                currentLayerIndex = layerPanel.GetCurrentLayerIndex();
            }
            pictureBox1.Invalidate(); // Vẽ lại canvas
        }

        private Point GetCanvasPoint(Point screenPoint)
        {
            // FIX: Chia số thực và làm tròn để giữ độ chính xác khi zoom nhỏ
            int x = (int)Math.Round(screenPoint.X / (double)zoomScale);
            int y = (int)Math.Round(screenPoint.Y / (double)zoomScale);

            // Đảm bảo không văng ra ngoài phạm vi ảnh
            x = Math.Max(0, Math.Min(x, canvas.Width - 1));
            y = Math.Max(0, Math.Min(y, canvas.Height - 1));

            return new Point(x, y);
        }

        // Lấy hình chữ nhật từ 2 điểm
        private Rectangle GetRect(Point p1, Point p2)
        {
            return new Rectangle(
                Math.Min(p1.X, p2.X), Math.Min(p1.Y, p2.Y),
                Math.Abs(p1.X - p2.X), Math.Abs(p1.Y - p2.Y));
        }

        // Ngôi sao
        private Point[] CreateStarPoints(Rectangle bounds, int numPoints = 5, double innerRatio = 0.5)
        {
            var pts = new List<Point>();
            double cx = bounds.X + bounds.Width / 2.0;
            double cy = bounds.Y + bounds.Height / 2.0;
            double outer = Math.Min(bounds.Width, bounds.Height) / 2.0;
            double inner = outer * innerRatio;
            double step = Math.PI / numPoints;
            double angle = -Math.PI / 2;

            for (int i = 0; i < numPoints * 2; i++)
            {
                double r = (i % 2 == 0) ? outer : inner;
                pts.Add(new Point((int)(cx + Math.Cos(angle) * r), (int)(cy + Math.Sin(angle) * r)));
                angle += step;
            }
            return pts.ToArray();
        }

        //Tam giác
        private Point[] GetTrianglePoints(Rectangle rect)
        {
            return new Point[] {
                new Point(rect.Left + rect.Width / 2, rect.Top),
                new Point(rect.Left, rect.Bottom),
                new Point(rect.Right, rect.Bottom)
            };
        }

        //Hình thang
        private Point[] GetTrapezoidPoints(Rectangle rect)
        {
            int indent = rect.Width / 4;
            return new Point[] {
                new Point(rect.Left + indent, rect.Top),
                new Point(rect.Right - indent, rect.Top),
                new Point(rect.Right, rect.Bottom),
                new Point(rect.Left, rect.Bottom)
            };
        }

        // Lục giác (Hexagon)
        private Point[] GetHexagonPoints(Rectangle rect)
        {
            double cx = rect.X + rect.Width / 2.0;
            double cy = rect.Y + rect.Height / 2.0;
            double radius = Math.Min(rect.Width, rect.Height) / 2.0;
            var pts = new List<Point>();
            for (int i = 0; i < 6; i++)
            {
                double angle = Math.PI / 3 * i - Math.PI / 6;
                double x = cx + radius * Math.Cos(angle);
                double y = cy + radius * Math.Sin(angle);
                pts.Add(new Point((int)x, (int)y));
            }
            return pts.ToArray();
        }

        // Kim cương (Diamond)
        private Point[] GetDiamondPoints(Rectangle rect)
        {
            return new Point[] {
                new Point(rect.X + rect.Width / 2, rect.Top),
                new Point(rect.Right, rect.Y + rect.Height / 2),
                new Point(rect.X + rect.Width / 2, rect.Bottom),
                new Point(rect.Left, rect.Y + rect.Height / 2)
            };
        }

        // Mũi tên (Arrows)
        private Point[] GetArrowPoints(Rectangle rect)
        {
            int arrowSize = Math.Min(rect.Width, rect.Height) / 3;
            return new Point[] {
                new Point(rect.X + rect.Width / 2, rect.Top),
                new Point(rect.Right - arrowSize, rect.Top + arrowSize),
                new Point(rect.X + rect.Width / 2 + arrowSize / 2, rect.Top + arrowSize),
                new Point(rect.X + rect.Width / 2 + arrowSize / 2, rect.Bottom),
                new Point(rect.X + rect.Width / 2 - arrowSize / 2, rect.Bottom),
                new Point(rect.X + rect.Width / 2 - arrowSize / 2, rect.Top + arrowSize),
                new Point(rect.Left + arrowSize, rect.Top + arrowSize)
            };
        }

        // Hình chữ nhật bo góc (Rounded Rectangle)
        private GraphicsPath GetRoundedRectanglePath(Rectangle rect, int cornerRadius)
        {
            GraphicsPath path = new GraphicsPath();
            // Giới hạn cornerRadius để không vượt quá nửa kích thước nhỏ nhất
            int maxRadius = Math.Min(rect.Width, rect.Height) / 2;
            int radius = Math.Min(cornerRadius, maxRadius);

            if (radius <= 0)
            {
                // Nếu radius = 0, vẽ hình chữ nhật bình thường
                path.AddRectangle(rect);
                return path;
            }

            int diameter = radius * 2;
            Rectangle arc = new Rectangle(rect.Location, new Size(diameter, diameter));

            // Top left
            path.AddArc(arc, 180, 90);
            // Top right
            arc.X = rect.Right - diameter;
            path.AddArc(arc, 270, 90);
            // Bottom right
            arc.Y = rect.Bottom - diameter;
            path.AddArc(arc, 0, 90);
            // Bottom left
            arc.X = rect.Left;
            path.AddArc(arc, 90, 90);

            path.CloseFigure();
            return path;
        }

        // MỚI: Lấy màu với độ trong suốt được áp dụng
        private Color GetColorWithOpacity(Color baseColor)
        {
            return Color.FromArgb(lineOpacity, baseColor.R, baseColor.G, baseColor.B);
        }


        // 3. SỰ KIỆN RESIZE & ZOOM (FIX LỖI)

        // MỚI: Helper method để cập nhật canvas từ tất cả layer
        private void UpdateCanvasFromLayers()
        {
            if (layers == null || layers.Count == 0) return;

            using (Graphics g = Graphics.FromImage(canvas))
            {
                g.Clear(Color.White);
                // Vẽ từ layer dưới cùng lên trên
                for (int i = layers.Count - 1; i >= 0; i--)
                {
                    Layer layer = layers[i];
                    if (!layer.IsVisible) continue;

                    // Nếu opacity < 255, sử dụng ImageAttributes
                    if (layer.Opacity < 255)
                    {
                        using (System.Drawing.Imaging.ImageAttributes imageAttrs = new System.Drawing.Imaging.ImageAttributes())
                        {
                            float opacity = layer.Opacity / 255f;
                            float[][] colorMatrixElements = new float[][]
                            {
                                new float[] { 1, 0, 0, 0, 0 },
                                new float[] { 0, 1, 0, 0, 0 },
                                new float[] { 0, 0, 1, 0, 0 },
                                new float[] { 0, 0, 0, opacity, 0 },
                                new float[] { 0, 0, 0, 0, 1 }
                            };
                            System.Drawing.Imaging.ColorMatrix colorMatrix = new System.Drawing.Imaging.ColorMatrix(colorMatrixElements);
                            imageAttrs.SetColorMatrix(colorMatrix, System.Drawing.Imaging.ColorMatrixFlag.Default, System.Drawing.Imaging.ColorAdjustType.Bitmap);

                            Rectangle destRect = new Rectangle(0, 0, layer.Image.Width, layer.Image.Height);
                            g.DrawImage(layer.Image, destRect, 0, 0, layer.Image.Width, layer.Image.Height, GraphicsUnit.Pixel, imageAttrs);
                        }
                    }
                    else
                    {
                        g.DrawImage(layer.Image, 0, 0);
                    }
                }
            }
        }

        // FIX Bug #13: Áp dụng anti-aliasing cho imported image VÀ vẽ lại đầy đủ layers
        private void ApplyAntiAliasingToImportedImage(int method)
        {
            if (importedImage == null)
            {
                MessageBox.Show("Chưa import ảnh nào!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                Bitmap originalImage = (Bitmap)importedImage.Clone();
                Bitmap? processedImage = null;

                switch (method)
                {
                    case 1:
                        processedImage = ImageProcessor.RemoveAliasing(originalImage);
                        break;
                    case 2:
                        processedImage = ImageProcessor.RemoveAliasingAdvanced(originalImage);
                        break;
                    case 3:
                        processedImage = ImageProcessor.RemoveAliasingForCurves(originalImage);
                        break;
                    case 4:
                        processedImage = ImageProcessor.RemoveAliasingWithStrength(originalImage, 5);
                        break;
                    default:
                        processedImage = ImageProcessor.RemoveAliasing(originalImage);
                        break;
                }

                originalImage.Dispose();
                importedImage.Dispose();
                importedImage = processedImage;

                // FIX: Vẽ lại canvas từ tất cả layers thay vì chỉ vẽ imported image
                RecomposeCanvas();

                pictureBox1.Invalidate();
                MessageBox.Show("Xử lý khử răng cưa thành công!", "Thành công",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// FIX Bug #3 & #4: Áp dụng tất cả các bước xử lý ảnh, dispose đúng cách
        /// </summary>
        private Bitmap ApplyImageProcessing(Bitmap image, ImageProcessingDialog settings)
        {
            Bitmap result = (Bitmap)image.Clone();
            bool success = false;

            try
            {
                // Bước 1: Khử răng cưa
                if (settings.SelectedProcessing > 0)
                {
                    Bitmap oldResult = result;
                    switch (settings.SelectedProcessing)
                    {
                        case 1:
                            result = ImageProcessor.RemoveAliasing(result);
                            break;
                        case 2:
                            result = ImageProcessor.RemoveAliasingAdvanced(result);
                            break;
                        case 3:
                            result = ImageProcessor.RemoveAliasingForCurves(result);
                            break;
                    }
                    if (oldResult != result) oldResult.Dispose(); // FIX: Dispose bitmap cũ
                }

                // Bước 2: Điều chỉnh tương phản (FIX Bug #14: bỏ biến contrastValue thừa)
                if (settings.ApplyContrast)
                {
                    Bitmap oldResult = result;
                    result = ImageProcessor.AdjustContrast(result, settings.ContrastValue / 100.0);
                    if (oldResult != result) oldResult.Dispose();
                }

                // Bước 3: Làm sắc nét
                if (settings.ApplySharpen)
                {
                    Bitmap oldResult = result;
                    result = ImageProcessor.ApplySharpen(result, settings.SharpenValue);
                    if (oldResult != result) oldResult.Dispose();
                }

                // Bước 4: Làm mờ (Blur)
                if (settings.ApplyBlur && settings.BlurValue > 0)
                {
                    Bitmap oldResult = result;
                    result = ImageProcessor.ApplyGaussianBlur(result, settings.BlurValue);
                    if (oldResult != result) oldResult.Dispose();
                }

                // Bước 5: Điều chỉnh độ sáng
                if (settings.ApplyBrightness && settings.BrightnessValue != 0)
                {
                    Bitmap oldResult = result;
                    result = ImageProcessor.AdjustBrightness(result, settings.BrightnessValue);
                    if (oldResult != result) oldResult.Dispose();
                }

                success = true;
                // FIX Bug #4: Chỉ dispose image gốc SAU khi mọi thứ thành công
                image.Dispose();
                return result;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi xử lý ảnh: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                if (!success && result != null && result != image)
                    result.Dispose(); // Dispose kết quả lỗi
                return image; // Trả về ảnh gốc CHƯA bị dispose
            }
        }

        // FIX Bug #17: Chỉ xóa undo stack khi canvas thực sự thay đổi, và dispose đúng
        private void Form1_Resize(object? sender, EventArgs e)
        {
            // 1. Chặn lỗi Crash khi Minimize (thu nhỏ xuống thanh taskbar)
            if (pictureBox1.ClientSize.Width <= 0 || pictureBox1.ClientSize.Height <= 0)
                return;

            if (canvas != null)
            {
                // 2. Chỉ tạo lại canvas nếu kích thước cửa sổ LỚN HƠN canvas hiện tại
                if (pictureBox1.ClientSize.Width > canvas.Width || pictureBox1.ClientSize.Height > canvas.Height)
                {
                    Bitmap oldCanvas = canvas;

                    Bitmap newCanvas = new Bitmap(pictureBox1.ClientSize.Width, pictureBox1.ClientSize.Height);
                    using (Graphics g = Graphics.FromImage(newCanvas))
                    {
                        g.Clear(Color.White);
                        g.DrawImage(oldCanvas, 0, 0);
                    }
                    canvas = newCanvas;
                    oldCanvas.Dispose();
                    // FIX: Không gán Image - dùng Paint event để render có zoom
                    // pictureBox1.Image = canvas;

                    // Cập nhật kích thước tất cả các layer
                    if (layerPanel != null)
                    {
                        layerPanel.UpdateAllLayersSize(pictureBox1.ClientSize.Width, pictureBox1.ClientSize.Height);
                        layerPanel.SetCanvasDimensions(pictureBox1.ClientSize.Width, pictureBox1.ClientSize.Height);
                    }

                    // FIX: Chỉ xóa undo/redo khi canvas thực sự resize, VÀ dispose đúng
                    ClearUndoList();
                    ClearStack(redoStack);
                    SaveState();
                }
            }
        }

        private void Form1_MouseWheel(object? sender, MouseEventArgs e)
        {
            // Zoom In/Out
            if (e.Delta > 0) zoomScale += 0.1f;
            else zoomScale -= 0.1f;

            // Giới hạn Zoom
            if (zoomScale < 0.5f) zoomScale = 0.5f;
            if (zoomScale > 5.0f) zoomScale = 5.0f;

            pictureBox1.Invalidate(); // Vẽ lại
        }
        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                SaveState(); // Lưu lại trạng thái TRƯỚC khi vẽ nét mới
                drawing = true;
                start = GetCanvasPoint(e.Location); // FIX Bug #2: Chuyển đổi tọa độ khi zoom
            }
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (drawing)
            {
                end = GetCanvasPoint(e.Location);

                //Tẩy trực tiếp khi di chuột
                if (currentTool == Tool.Eraser)
                {
                    // MỚI: Lấy layer hiện tại (nếu có)
                    Bitmap eraserTarget = canvas;
                    if (layerPanel != null)
                    {
                        Layer? currentLayer = layerPanel.GetCurrentLayer();
                        if (currentLayer != null)
                            eraserTarget = currentLayer.Image;
                    }

                    // FIX Bug #10: Tính kích thước eraser thực tế trên canvas
                    int actualEraserSize = Math.Max(1, (int)(eraserSize / zoomScale));

                    using (Graphics g = Graphics.FromImage(eraserTarget))
                    {
                        g.SmoothingMode = SmoothingMode.None;
                        g.CompositingMode = CompositingMode.SourceCopy; // Cho phép vẽ pixel trong suốt
                        using (Brush b = new SolidBrush(Color.Transparent))
                        {
                            g.FillEllipse(b, end.X - actualEraserSize / 2, end.Y - actualEraserSize / 2, actualEraserSize, actualEraserSize);
                        }
                    }
                }

                pictureBox1.Invalidate();
                if (currentTool == Tool.SmartSelect && drawing)
                {
                    pictureBox1.Invalidate(); // Vẽ lại liên tục khi di chuột
                }
            }

            // MỚI: Cập nhật tọa độ trên Label
            Point canvasPos = GetCanvasPoint(e.Location);

            if (drawing)
            {
                // Nếu đang vẽ, hiển thị cả kích thước hình (W, H dưới X, Y)
                int width = Math.Abs(end.X - start.X);
                int height = Math.Abs(end.Y - start.Y);
                if (lblCoordinates != null)
                    lblCoordinates.Text = $"X: {canvasPos.X}, Y: {canvasPos.Y}\nW: {width}, H: {height}";
            }
            else
            {
                // Nếu không vẽ, chỉ hiển thị tọa độ con trỏ
                if (lblCoordinates != null)
                    lblCoordinates.Text = $"X: {canvasPos.X}, Y: {canvasPos.Y}";
            }
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            if (!drawing) return; // Bảo vệ nếu MouseUp kích hoạt mà không có MouseDown
            drawing = false;

            end = GetCanvasPoint(e.Location);
            Rectangle rect = GetRectangle(start, end);

            // 1. Kiểm tra an toàn đầy đủ
            if (layers == null || layers.Count == 0 || currentLayerIndex < 0 || currentLayerIndex >= layers.Count)
            {
                MessageBox.Show("Lỗi: Không có layer hợp lệ!", "Lỗi Layer", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                pictureBox1.Invalidate();
                return;
            }

            // 2. Xác định Layer mục tiêu để vẽ hoặc xử lý
            Layer currentLayer = layers[currentLayerIndex];
            Bitmap drawTarget = currentLayer.Image;

            // 3. Xử lý theo từng loại công cụ
            if (currentTool == Tool.SmartSelect)
            {
                // CHỈ chạy xóa phông khi đang chọn công cụ SmartSelect
                if (rect.Width > 5 && rect.Height > 5)
                {
                    // Gọi hàm xử lý GrabCut từ OpenCV
                    Bitmap resultBmp = ImageProcessor.RemoveBackgroundGrabCut(currentLayer.Image, rect);

                    // Giải phóng bộ nhớ ảnh cũ và cập nhật ảnh mới đã tách nền
                    if (currentLayer.Image != null) currentLayer.Image.Dispose();
                    currentLayer.Image = resultBmp;

                    // Cập nhật giao diện Layer và Canvas tổng
                    RecomposeCanvas();
                    layerPanel?.RefreshLayerList();
                }
            }
            else
           {
                // Các công cụ vẽ hình thông thường (Chỉ vẽ lên Layer hiện tại)
                switch (currentTool)
                {
                     case Tool.WuLine:
                        WuLineDrawer.DrawThickLine(drawTarget, start.X, start.Y, end.X, end.Y, GetColorWithOpacity(pen.Color), (int)pen.Width);
                        break;
                    case Tool.Rectangle:
                        WuLineDrawer.DrawThickRectangle(drawTarget, rect, GetColorWithOpacity(pen.Color), (int)pen.Width);
                        break;
                    case Tool.Circle:
                        int r = Math.Min(rect.Width, rect.Height) / 2;
                        WuLineDrawer.DrawThickCircle(drawTarget, rect.X + rect.Width / 2, rect.Y + rect.Height / 2, r, GetColorWithOpacity(pen.Color), (int)pen.Width);
                        break;
                    case Tool.Star:
                        WuLineDrawer.DrawThickPolygon(drawTarget, CreateStarPoints(rect), GetColorWithOpacity(pen.Color), (int)pen.Width);
                        break;
                    case Tool.Triangle:
                        WuLineDrawer.DrawThickPolygon(drawTarget, GetTrianglePoints(rect), GetColorWithOpacity(pen.Color), (int)pen.Width);
                        break;
                    case Tool.Trapezoid:
                        WuLineDrawer.DrawThickPolygon(drawTarget, GetTrapezoidPoints(rect), GetColorWithOpacity(pen.Color), (int)pen.Width);
                        break;
                    case Tool.Line:
                        using (Graphics g = Graphics.FromImage(drawTarget))
                        {
                            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                            using (Pen opacityPen = new Pen(GetColorWithOpacity(pen.Color), pen.Width))
                            {
                                opacityPen.StartCap = opacityPen.EndCap = System.Drawing.Drawing2D.LineCap.Round;
                                g.DrawLine(opacityPen, start, end);
                            }
                        }
                        break;
                    case Tool.Hexagon:
                        WuLineDrawer.DrawThickHexagon(drawTarget, rect, GetColorWithOpacity(pen.Color), (int)pen.Width);
                        break;
                    case Tool.Diamond:
                        WuLineDrawer.DrawThickDiamond(drawTarget, rect, GetColorWithOpacity(pen.Color), (int)pen.Width);
                        break;
                    case Tool.Arrows:
                        WuLineDrawer.DrawThickArrows(drawTarget, rect, GetColorWithOpacity(pen.Color), (int)pen.Width);
                        break;
                    case Tool.Ellipse:
                        WuLineDrawer.DrawThickEllipse(drawTarget, rect, GetColorWithOpacity(pen.Color), (int)pen.Width);
                        break;
                    case Tool.Bezier:
                        Point[] bezierPoints = GetBezierControlPoints(rect);
                        WuLineDrawer.DrawThickBezier(drawTarget, bezierPoints[0], bezierPoints[1], bezierPoints[2], bezierPoints[3], GetColorWithOpacity(pen.Color), (int)pen.Width);
                        break;
                    case Tool.RoundedRectangle:
                        int cornerRadius = Math.Min(rect.Width, rect.Height) / 4;
                        WuLineDrawer.DrawThickRoundedRectangle(drawTarget, rect, cornerRadius, GetColorWithOpacity(pen.Color), (int)pen.Width);
                        break;
                    
                }

                // Sau khi vẽ xong hình, cần gọi Recompose để gộp các Layer lại hiển thị lên màn hình
                RecomposeCanvas();
            }


            pictureBox1.Invalidate();
        }
        //hiển thị 

        // FIX Bug #7: Vẽ SmartSelect SAU ScaleTransform để tọa độ khớp
        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            // 1. Áp dụng Zoom TRƯỚC
            e.Graphics.ScaleTransform(zoomScale, zoomScale);

            // 2. Vẽ tất cả các layer từ dưới lên trên (composite layers)
            if (layers != null && layers.Count > 0)
            {
                for (int i = layers.Count - 1; i >= 0; i--)
                {
                    Layer layer = layers[i];
                    if (!layer.IsVisible) continue;

                    if (layer.Opacity < 255)
                    {
                        using (ImageAttributes imageAttrs = new ImageAttributes())
                        {
                            float opacity = layer.Opacity / 255f;
                            float[][] colorMatrixElements = new float[][]
                            {
                                new float[] { 1, 0, 0, 0, 0 },
                                new float[] { 0, 1, 0, 0, 0 },
                                new float[] { 0, 0, 1, 0, 0 },
                                new float[] { 0, 0, 0, opacity, 0 },
                                new float[] { 0, 0, 0, 0, 1 }
                            };
                            ColorMatrix colorMatrix = new ColorMatrix(colorMatrixElements);
                            imageAttrs.SetColorMatrix(colorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

                            Rectangle destRect = new Rectangle(0, 0, layer.Image.Width, layer.Image.Height);
                            e.Graphics.DrawImage(layer.Image, destRect, 0, 0, layer.Image.Width, layer.Image.Height, GraphicsUnit.Pixel, imageAttrs);
                        }
                    }
                    else
                    {
                        e.Graphics.DrawImage(layer.Image, 0, 0);
                    }
                }
            }
            else if (canvas != null)
            {
                e.Graphics.DrawImage(canvas, 0, 0);
            }

            // 3. Vẽ SmartSelect rectangle SAU ScaleTransform (FIX: tọa độ giờ đã đúng)
            if (currentTool == Tool.SmartSelect && drawing)
            {
                using (Pen dashPen = new Pen(Color.Blue, 2 / zoomScale))
                {
                    dashPen.DashStyle = DashStyle.Dash;
                    e.Graphics.DrawRectangle(dashPen, GetRectangle(start, end));
                }
            }

            // 3. Vẽ hình Preview (nét mờ/đen khi đang kéo chuột)
            if (drawing && currentTool != Tool.Eraser)
            {
                DrawPreview(e.Graphics);
            }

            // 4. Vẽ vị trí cục tẩy (để biết tẩy đang ở đâu)
            if (currentTool == Tool.Eraser)
            {
                // Lấy toạ độ chuột hiện tại (tương đối trên PictureBox)
                Point mousePos = pictureBox1.PointToClient(Cursor.Position);
                // Chuyển sang toạ độ Canvas
                Point canvasPos = GetCanvasPoint(mousePos);

                using (Pen p = new Pen(Color.Gray))
                {
                    p.DashStyle = DashStyle.Dot;
                    p.Width = 1 / zoomScale; // FIX: Điều chỉnh độ dày của pen theo zoom
                    e.Graphics.DrawEllipse(p, canvasPos.X - eraserSize / 2, canvasPos.Y - eraserSize / 2, eraserSize, eraserSize);
                }
            }
        }

        private void DrawPreview(Graphics g)
        {
            Rectangle rect = GetRect(start, end);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            // Điều chỉnh độ dày pen theo zoom để preview hiển thị đúng tỉ lệ
            float previewPenWidth = Math.Max(pen.Width / zoomScale, 1f);

            //chọn hình dạng
            switch (currentTool)
            {
                case Tool.Line:
                case Tool.WuLine:
                    using (Pen opacityPen = new Pen(GetColorWithOpacity(pen.Color), previewPenWidth))
                    {
                        opacityPen.StartCap = opacityPen.EndCap = LineCap.Round;
                        g.DrawLine(opacityPen, start, end);
                    }
                    break;
                case Tool.Rectangle:
                    using (Pen opacityPen = new Pen(GetColorWithOpacity(pen.Color), previewPenWidth))
                    {
                        g.DrawRectangle(opacityPen, rect);
                    }
                    break;
                case Tool.Circle:
                    using (Pen opacityPen = new Pen(GetColorWithOpacity(pen.Color), previewPenWidth))
                    {
                        // Vẽ hình tròn (width = height)
                        int diameter = Math.Min(rect.Width, rect.Height);
                        Rectangle circleRect = new Rectangle(rect.Left, rect.Top, diameter, diameter);
                        g.DrawEllipse(opacityPen, circleRect);
                    }
                    break;
                case Tool.Triangle:
                    using (Pen opacityPen = new Pen(GetColorWithOpacity(pen.Color), previewPenWidth))
                    {
                        g.DrawPolygon(opacityPen, GetTrianglePoints(rect));
                    }
                    break;
                case Tool.Star:
                    using (Pen opacityPen = new Pen(GetColorWithOpacity(pen.Color), previewPenWidth))
                    {
                        g.DrawPolygon(opacityPen, CreateStarPoints(rect));
                    }
                    break;
                case Tool.Trapezoid:
                    using (Pen opacityPen = new Pen(GetColorWithOpacity(pen.Color), previewPenWidth))
                    {
                        g.DrawPolygon(opacityPen, GetTrapezoidPoints(rect));
                    }
                    break;
                case Tool.Hexagon:
                    using (Pen opacityPen = new Pen(GetColorWithOpacity(pen.Color), previewPenWidth))
                    {
                        g.DrawPolygon(opacityPen, GetHexagonPoints(rect));
                    }
                    break;
                case Tool.Diamond:
                    using (Pen opacityPen = new Pen(GetColorWithOpacity(pen.Color), previewPenWidth))
                    {
                        g.DrawPolygon(opacityPen, GetDiamondPoints(rect));
                    }
                    break;
                case Tool.Arrows:
                    using (Pen opacityPen = new Pen(GetColorWithOpacity(pen.Color), previewPenWidth))
                    {
                        g.DrawPolygon(opacityPen, GetArrowPoints(rect));
                    }
                    break;
                case Tool.Ellipse:
                    using (Pen opacityPen = new Pen(GetColorWithOpacity(pen.Color), previewPenWidth))
                    {
                        g.DrawEllipse(opacityPen, rect);
                    }
                    break;
                case Tool.Bezier:
                    using (Pen opacityPen = new Pen(GetColorWithOpacity(pen.Color), previewPenWidth))
                    {
                        Point[] curvePoints = GetBezierControlPoints(rect);
                        if (curvePoints.Length >= 4)
                            g.DrawBeziers(opacityPen, curvePoints);
                    }
                    break;
                case Tool.RoundedRectangle:
                    using (Pen opacityPen = new Pen(GetColorWithOpacity(pen.Color), previewPenWidth))
                    {
                        int cornerRadius = Math.Min(rect.Width, rect.Height) / 4;
                        GraphicsPath path = GetRoundedRectanglePath(rect, cornerRadius);
                        g.DrawPath(opacityPen, path);
                        path.Dispose();
                    }
                    break;
            }
        }

        // Bezier curve control points
        private Point[] GetBezierControlPoints(Rectangle rect)
        {
            return new Point[] {
                new Point(rect.Left, rect.Top + rect.Height / 2),
                new Point(rect.Left + rect.Width / 3, rect.Top),
                new Point(rect.Left + 2 * rect.Width / 3, rect.Bottom),
                new Point(rect.Right, rect.Top + rect.Height / 2)
            };
        }

        //Thanh_start 11/4/2026: Handler cho Undo/Redo
        // FIX Bug #6: Dùng constant Max_undo_Steps nhất quán
        private void SaveState()
        {
            var snapshot = CloneCurrentLayers();

            if (snapshot != null && snapshot.Count > 0)
            {
                undoList.Add(snapshot);
                ClearStack(redoStack); // FIX: dispose trước khi clear

                // Giới hạn theo constant
                if (undoList.Count > Max_undo_Steps)
                {
                    foreach (var layer in undoList[0])
                    {
                        layer.Dispose();
                    }
                    undoList.RemoveAt(0);
                }
            }
        }

        // Ghi đè để bắt phím tắt Ctrl+Z (Undo) và Ctrl+Y (Redo)
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.Control | Keys.Z))
            {
                Undo();
                return true;
            }
            if (keyData == (Keys.Control | Keys.Y))
            {
                Redo();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }


        public void Undo()
        {
            if (undoList.Count == 0) return;

            try
            {
                // 1. Cất trạng thái hiện tại vào Redo
                redoStack.Push(CloneCurrentLayers());

                // 2. Lấy trạng thái cuối cùng (mới nhất) trong List ra
                int lastIdx = undoList.Count - 1;

                // Giải phóng list layer hiện tại trước khi thay thế
                foreach (var l in layers) l.Image?.Dispose();

                layers = undoList[lastIdx];
                undoList.RemoveAt(lastIdx); // Xóa khỏi danh sách sau khi lấy ra

                // 3. Cập nhật giao diện
                RecomposeCanvas();
                layerPanel?.RefreshLayersList(layers);
                pictureBox1.Invalidate();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Undo error: " + ex.Message);
            }
        }

        // FIX Bug #6: Dùng Max_undo_Steps nhất quán
        public void Redo()
        {
            if (redoStack == null || redoStack.Count == 0) return;

            try
            {
                undoList.Add(CloneCurrentLayers());

                // FIX: Dùng cùng constant
                if (undoList.Count > Max_undo_Steps)
                {
                    foreach (var l in undoList[0])
                    {
                        l.Dispose(); // FIX: Dùng Dispose() thay vì chỉ dispose Image
                    }
                    undoList.RemoveAt(0);
                }

                // Giải phóng layers hiện tại
                if (layers != null)
                {
                    foreach (var l in layers)
                    {
                        l.Dispose();
                    }
                }

                layers = redoStack.Pop();
                currentLayerIndex = Math.Max(0, Math.Min(currentLayerIndex, Math.Max(0, layers.Count - 1)));

                RecomposeCanvas();
                layerPanel?.RefreshLayersList(layers);
                pictureBox1.Invalidate();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Redo error: " + ex.Message);
            }
        }
        // FIX Bug #8: RecomposeCanvas giờ hỗ trợ Opacity qua ColorMatrix
        // FIX Bug #9: Xóa dead code UpdateAfterUndoRedo & UpdateCanvasFromLayers
        // FIX: Đảm bảo visibility toggle hoạt động đúng
        private void RecomposeCanvas()
        {
            if (layers == null || layers.Count == 0) return;

            try
            {
                using (Graphics g = Graphics.FromImage(canvas))
                {
                    g.Clear(Color.White);

                    // Duyệt từ bottom layer lên top layer
                    for (int i = layers.Count - 1; i >= 0; i--)
                    {
                        Layer layer = layers[i];
                        if (!layer.IsVisible) continue;

                        // FIX: Áp dụng Opacity qua ColorMatrix
                        if (layer.Opacity < 255)
                        {
                            using (ImageAttributes imageAttrs = new ImageAttributes())
                            {
                                float opacity = layer.Opacity / 255f;
                                float[][] colorMatrixElements = new float[][]
                                {
                                    new float[] { 1, 0, 0, 0, 0 },
                                    new float[] { 0, 1, 0, 0, 0 },
                                    new float[] { 0, 0, 1, 0, 0 },
                                    new float[] { 0, 0, 0, opacity, 0 },
                                    new float[] { 0, 0, 0, 0, 1 }
                                };
                                ColorMatrix colorMatrix = new ColorMatrix(colorMatrixElements);
                                imageAttrs.SetColorMatrix(colorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
                                Rectangle destRect = new Rectangle(0, 0, layer.Image.Width, layer.Image.Height);
                                g.DrawImage(layer.Image, destRect, 0, 0, layer.Image.Width, layer.Image.Height, GraphicsUnit.Pixel, imageAttrs);
                            }
                        }
                        else
                        {
                            g.DrawImage(layer.Image, 0, 0);
                        }
                    }
                }
                // FIX: Không gán Image - dùng Paint event để render có zoom
                // pictureBox1.Image = canvas;
                pictureBox1.Invalidate();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[RecomposeCanvas] Error: {ex.Message}");
            }
        }
        private List<Layer> CloneLayers(List<Layer> source)
        {
            List<Layer> newList = new List<Layer>();
            foreach (var l in source)
            {
                // KIỂM TRA AN TOÀN: Tránh lỗi Parameter is not valid
                if (l == null || l.Image == null) continue;

                try
                {
                    // Thử truy cập Width/Height, nếu lỗi sẽ nhảy vào catch
                    int w = l.Image.Width;
                    int h = l.Image.Height;

                    Layer copy = new Layer(l.Name, w, h);
                    copy.IsVisible = l.IsVisible;
                    copy.Opacity = l.Opacity;
                    using (Graphics g = Graphics.FromImage(copy.Image))
                    {
                        g.DrawImage(l.Image, 0, 0);
                    }
                    newList.Add(copy);
                }
                catch
                {
                    // Nếu ảnh bị Dispose, tạo layer trắng thay thế để không sập app
                    newList.Add(new Layer(l.Name, canvas.Width, canvas.Height));
                }
            }
            return newList;
        }
        private void ClearStack(Stack<List<Layer>> stack)
        {
            while (stack.Count > 0)
            {
                var list = stack.Pop();
                foreach (var l in list)
                {
                    l.Dispose();
                }
            }
        }

        // FIX Bug #17: Helper để dispose + clear undoList
        private void ClearUndoList()
        {
            foreach (var snapshot in undoList)
            {
                foreach (var l in snapshot)
                {
                    l.Dispose();
                }
            }
            undoList.Clear();
        }

        // Thanh_end

        // Designer
        private void btnLine_Click(object sender, EventArgs e) => currentTool = Tool.Line;
        private void btnWuLine_Click(object sender, EventArgs e) => currentTool = Tool.WuLine;
        private void btnRect_Click(object sender, EventArgs e) => currentTool = Tool.Rectangle;
        private void btnCircle_Click(object sender, EventArgs e) => currentTool = Tool.Circle;
        private void btnTriangle_Click(object sender, EventArgs e) => currentTool = Tool.Triangle;
        private void btnEraser_Click(object sender, EventArgs e) => currentTool = Tool.Eraser;
        private void btnStar_Click(object sender, EventArgs e) => currentTool = Tool.Star;
        private void btnTrapezoid_Click(object sender, EventArgs e) => currentTool = Tool.Trapezoid;
        private void BttHexagon_Click(object sender, EventArgs e) => currentTool = Tool.Hexagon;
        private void BttArrows_Click(object sender, EventArgs e) => currentTool = Tool.Arrows;
        private void BttDiamond_Click(object sender, EventArgs e) => currentTool = Tool.Diamond;
        private void BttEllipse_Click(object sender, EventArgs e) => currentTool = Tool.Ellipse;
        private void BttBezier_Click(object sender, EventArgs e) => currentTool = Tool.Bezier;
        private void BttRoundedRectangle_Click(object sender, EventArgs e) => currentTool = Tool.RoundedRectangle;

        private void btnColor_Click(object sender, EventArgs e)
        {
            if (colorDialog1.ShowDialog() == DialogResult.OK)
                pen.Color = colorDialog1.Color;
        }

        private void nudEraserSize_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                eraserSize = (int)nudEraserSize.Value;

            }
            catch { }
        }

        // MỚI: Handler cho nút Import Image
        private void btnImportImage_Click(object sender, EventArgs e)
        {
            // Tạo OpenFileDialog để chọn file ảnh
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Title = "Chọn ảnh để import";
                ofd.Filter = "Image Files (*.png;*.jpg;*.jpeg;*.bmp;*.gif)|*.png;*.jpg;*.jpeg;*.bmp;*.gif|All Files (*.*)|*.*";
                ofd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    SaveState();
                    try
                    {
                        // Load ảnh từ file
                        Bitmap loadedImage = new Bitmap(ofd.FileName);

                        // Nếu ảnh quá lớn, scale down để vừa với canvas
                        if (loadedImage.Width > canvas.Width * 0.8 || loadedImage.Height > canvas.Height * 0.8)
                        {
                            int maxWidth = (int)(canvas.Width * 0.8);
                            int maxHeight = (int)(canvas.Height * 0.8);
                            float scale = Math.Min((float)maxWidth / loadedImage.Width, (float)maxHeight / loadedImage.Height);
                            int newWidth = (int)(loadedImage.Width * scale);
                            int newHeight = (int)(loadedImage.Height * scale);

                            Bitmap resizedImage = new Bitmap(newWidth, newHeight);
                            using (Graphics g = Graphics.FromImage(resizedImage))
                            {
                                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                                g.DrawImage(loadedImage, 0, 0, newWidth, newHeight);
                            }
                            loadedImage.Dispose();
                            importedImage = resizedImage;
                        }
                        else
                        {
                            importedImage = loadedImage;
                        }

                        // MỚI: Hiển thị dialog xử lý ảnh chi tiết
                        using (ImageProcessingDialog processingDialog = new ImageProcessingDialog())
                        {
                            if (processingDialog.ShowDialog(this) == DialogResult.OK)
                            {
                                importedImage = ApplyImageProcessing(importedImage, processingDialog);
                            }
                        }

                        // Đặt vị trí ban đầu ở giữa canvas
                        importImagePos = new Point(
                            (canvas.Width - importedImage.Width) / 2,
                            (canvas.Height - importedImage.Height) / 2);

                        // MỚI: Tạo layer mới cho ảnh import thay vì vẽ vào layer hiện tại
                        // Điều này đảm bảo ảnh nằm trên cùng (phía trên các hình vẽ khác)
                        string imageLayerName = $"Image ({DateTime.Now:HH:mm:ss})";
                        Layer imageLayer = new Layer(imageLayerName, canvas.Width, canvas.Height);

                        // Vẽ ảnh vào layer mới
                        using (Graphics g = Graphics.FromImage(imageLayer.Image))
                        {
                            g.DrawImage(importedImage, importImagePos);
                        }

                        // FIX: Dispose importedImage after all uses are complete
                        importedImage?.Dispose();

                        // Thêm layer mới vào đầu danh sách (sẽ nằm trên cùng)
                        layers.Insert(0, imageLayer);

                        // DEBUG: In số layer hiện có
                        System.Diagnostics.Debug.WriteLine($"[btnImportImage_Click] Total layers in Form1.layers: {layers.Count}");
                        for (int i = 0; i < layers.Count; i++)
                        {
                            System.Diagnostics.Debug.WriteLine($"[btnImportImage_Click]   Layer {i}: {layers[i].Name}");
                        }

                        // Cập nhật LayerPanel để hiển thị layer mới
                        if (layerPanel != null)
                        {
                            System.Diagnostics.Debug.WriteLine($"[btnImportImage_Click] Before refresh - LayerPanel.GetLayerCount(): {layerPanel.GetLayerCount()}");
                            layerPanel.RefreshLayerList();
                            System.Diagnostics.Debug.WriteLine($"[btnImportImage_Click] After refresh - LayerPanel.GetLayerCount(): {layerPanel.GetLayerCount()}");
                        }

                        // Refresh UI
                        pictureBox1.Invalidate();
                        MessageBox.Show($"Ảnh đã được import vào layer '{imageLayerName}' thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Lỗi khi load ảnh: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        // MỚI: Handler cho TrackBar điều chỉnh độ trong suốt
        private void trackBarOpacity_ValueChanged(object sender, EventArgs e)
        {
            lineOpacity = trackBarOpacity.Value;
            // Cập nhật label để hiển thị giá trị độ trong suốt theo phần trăm
            int opacityPercent = (int)(lineOpacity / 255.0 * 100);
            lblOpacity.Text = $"Opacity: {opacityPercent}%";
            pictureBox1.Invalidate(); // Vẽ lại để hiển thị thay đổi
        }

        // MỚI: Handler cho nút Layers - bật/tắt layers panel
        private void btnLayers_Click(object sender, EventArgs e)
        {
            if (layerPanel != null)
            {
                layerPanel.Visible = !layerPanel.Visible;
                // Thay đổi màu nút để chỉ ra trạng thái
                btnLayers.BackColor = layerPanel.Visible ? Color.LightGreen : SystemColors.Control;
            }
        }
        //Thanh
        // Clone the current layers list (used by Undo/Redo)
        private List<Layer> CloneCurrentLayers()
        {
            // Defensive: if layers is null, return an empty list
            if (layers == null) return new List<Layer>();
            return CloneLayers(layers);
        }

        private void btnUndo_Click(object sender, EventArgs e)
        {
            Undo(); // gọi hàm Undo đã viết
        }

        private void btnRedo_Click(object sender, EventArgs e)
        {
            Redo(); // gọi hàm Redo đã viết
        }

        private void btnSmartSelect_Click(object sender, EventArgs e)
        {
            currentTool = Tool.SmartSelect;
        }

        // Save / Export

        private void ExecuteSaveAction(string title, string filter)
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Title = title;
                saveFileDialog.Filter = filter;
                saveFileDialog.DefaultExt = "png";

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        // 1. Đảm bảo tất cả Layer đã được gộp vào canvas tổng
                        RecomposeCanvas();

                        // 2. Xác định định dạng file dựa trên phần mở rộng
                        string ext = System.IO.Path.GetExtension(saveFileDialog.FileName).ToLower();
                        System.Drawing.Imaging.ImageFormat format = System.Drawing.Imaging.ImageFormat.Png;

                        if (ext == ".jpg" || ext == ".jpeg")
                            format = System.Drawing.Imaging.ImageFormat.Jpeg;
                        else if (ext == ".bmp")
                            format = System.Drawing.Imaging.ImageFormat.Bmp;

                        // 3. Thực hiện lưu
                        canvas.Save(saveFileDialog.FileName, format);

                        MessageBox.Show("Lưu file thành công!", "Thông báo",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Có lỗi khi lưu file: " + ex.Message, "Lỗi",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
        
        
        private void btnSave_Click(object sender, EventArgs e)
        {
            ExecuteSaveAction("Lưu hình ảnh", "PNG|*.png|JPEG|*.jpg|Bitmap|*.bmp");
        }
        private void btnExport_Click(object sender, EventArgs e)
        {
            ExecuteSaveAction("Xuất hình ảnh", "PNG Image|*.png");
        }

        
    }
}