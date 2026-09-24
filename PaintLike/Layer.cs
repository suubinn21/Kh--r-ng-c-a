using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace PaintLike
{
    /// <summary>
    /// Represents a single layer in the drawing application
    /// Mô tả một layer đơn trong ứng dụng vẽ
    /// </summary>
    public class Layer : IDisposable
    {
        /// <summary>
        /// Name of the layer
        /// Tên của layer
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Bitmap containing the layer's image data
        /// Bitmap chứa dữ liệu hình ảnh của layer
        /// </summary>
        public Bitmap Image { get; set; }

        /// <summary>
        /// Whether the layer is visible or hidden
        /// Kiểm soát khả năng hiển thị của layer
        /// </summary>
        public bool IsVisible { get; set; }

        /// <summary>
        /// Opacity level (0-255) where 255 is fully opaque
        /// Mức độ trong suốt (0-255) với 255 là hoàn toàn mờ
        /// </summary>
        public int Opacity { get; set; }

        /// <summary>
        /// Constructor to create a new layer
        /// Tạo một layer mới
        /// </summary>
        public Layer(string name, int width, int height)
        {
            Name = name;
            // Use Format32bppArgb for transparency support
            // Dùng Format32bppArgb để hỗ trợ tính trong suốt
            Image = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            IsVisible = true;
            Opacity = 255; // Fully opaque by default
            
            // Clear to transparent
            // Xóa để trong suốt
            using (Graphics g = Graphics.FromImage(Image))
            {
                g.Clear(Color.Transparent);
            }
        }

        /// <summary>
        /// Clone the layer
        /// Sao chép layer
        /// </summary>
        public Layer Clone()
        {
            Layer newLayer = new Layer(Name + " Copy", Image.Width, Image.Height);
            newLayer.Opacity = Opacity;
            newLayer.IsVisible = IsVisible;
            
            using (Graphics g = Graphics.FromImage(newLayer.Image))
            {
                g.DrawImage(Image, 0, 0);
            }
            
            return newLayer;
        }

        ~Layer()
        {
            Dispose(false);
        }

        /// <summary>
        /// Implements IDisposable.Dispose()
        /// Thực hiện IDisposable.Dispose()
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Image?.Dispose();
            }
        }

        /// <summary>
        /// Áp dụng bộ lọc khử răng cưa lên layer
        /// </summary>
        public void ApplyAntiAliasing(string mode = "simple")
        {
            try
            {
                Bitmap processed = mode switch
                {
                    "advanced" => ImageProcessor.RemoveAliasingAdvanced(Image),
                    "curves" => ImageProcessor.RemoveAliasingForCurves(Image),
                    _ => ImageProcessor.RemoveAliasing(Image)
                };

                Image.Dispose();
                Image = processed;
            }
            catch
            {
                // Giữ ảnh gốc nếu lỗi
            }
        }

        /// <summary>
        /// Điều chỉnh độ sáng của layer
        /// </summary>
        /// <param name="brightness">Độ sáng (-100 to 100)</param>
        public void ApplyBrightness(int brightness)
        {
            try
            {
                Bitmap processed = ImageProcessor.AdjustBrightness(Image, brightness);
                Image.Dispose();
                Image = processed;
            }
            catch
            {
                // Giữ ảnh gốc nếu lỗi
            }
        }

        /// <summary>
        /// Điều chỉnh độ tương phản của layer
        /// </summary>
        /// <param name="contrast">Độ tương phản (0.5 to 3.0)</param>
        public void ApplyContrast(double contrast)
        {
            try
            {
                Bitmap processed = ImageProcessor.AdjustContrast(Image, contrast);
                Image.Dispose();
                Image = processed;
            }
            catch
            {
                // Giữ ảnh gốc nếu lỗi
            }
        }

        /// <summary>
        /// Áp dụng Blur vào layer
        /// </summary>
        /// <param name="strength">Độ mạnh (1-10)</param>
        public void ApplyBlur(int strength = 3)
        {
            try
            {
                Bitmap processed = ImageProcessor.ApplyBlur(Image, strength);
                Image.Dispose();
                Image = processed;
            }
            catch
            {
                // Giữ ảnh gốc nếu lỗi
            }
        }

        /// <summary>
        /// Áp dụng Gaussian Blur vào layer
        /// </summary>
        /// <param name="strength">Độ mạnh (1-10)</param>
        public void ApplyGaussianBlur(int strength = 3)
        {
            try
            {
                Bitmap processed = ImageProcessor.ApplyGaussianBlur(Image, strength);
                Image.Dispose();
                Image = processed;
            }
            catch
            {
                // Giữ ảnh gốc nếu lỗi
            }
        }

        /// <summary>
        /// Chuyển layer sang Grayscale
        /// </summary>
        public void ConvertToGrayscale()
        {
            try
            {
                Bitmap processed = ImageProcessor.ConvertToGrayscale(Image);
                Image.Dispose();
                Image = processed;
            }
            catch
            {
                // Giữ ảnh gốc nếu lỗi
            }
        }

        /// <summary>
        /// Phát hiện cạnh trong layer
        /// </summary>
        public void DetectEdges()
        {
            try
            {
                Bitmap processed = ImageProcessor.DetectEdges(Image);
                Image.Dispose();
                Image = processed;
            }
            catch
            {
                // Giữ ảnh gốc nếu lỗi
            }
        }

        /// <summary>
        /// Làm sắc nét layer
        /// </summary>
        /// <param name="strength">Độ mạnh (1-5)</param>
        public void ApplySharpen(int strength = 1)
        {
            try
            {
                Bitmap processed = ImageProcessor.ApplySharpen(Image, strength);
                Image.Dispose();
                Image = processed;
            }
            catch
            {
                // Giữ ảnh gốc nếu lỗi
            }
        }

        /// <summary>
        /// Đảo ngược màu sắc của layer
        /// </summary>
        public void InvertColors()
        {
            try
            {
                Bitmap processed = ImageProcessor.InvertColors(Image);
                Image.Dispose();
                Image = processed;
            }
            catch
            {
                // Giữ ảnh gốc nếu lỗi
            }
        }

        /// <summary>
        /// Áp dụng tông Sepia vào layer
        /// </summary>
        public void ApplySepia()
        {
            try
            {
                Bitmap processed = ImageProcessor.ApplySepia(Image);
                Image.Dispose();
                Image = processed;
            }
            catch
            {
                // Giữ ảnh gốc nếu lỗi
            }
        }

        /// <summary>
        /// Điều chỉnh bão hòa màu sắc của layer
        /// </summary>
        /// <param name="saturation">Độ bão hòa (0.0 to 2.0)</param>
        public void AdjustSaturation(double saturation)
        {
            try
            {
                Bitmap processed = ImageProcessor.AdjustSaturation(Image, saturation);
                Image.Dispose();
                Image = processed;
            }
            catch
            {
                // Giữ ảnh gốc nếu lỗi
            }
        }
    }
}
