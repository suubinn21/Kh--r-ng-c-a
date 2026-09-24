using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace PaintLike
{
    /// <summary>
    /// BitmapBuffer: High-performance bitmap pixel access using LockBits
    /// Cung cấp truy cập hiệu suất cao vào pixel bitmap bằng LockBits và con trỏ
    /// </summary>
    public class BitmapBuffer : IDisposable
    {
        private Bitmap bitmap;
        private BitmapData? bitmapData;
        private IntPtr pixelDataPtr;
        private byte[]? pixelBuffer;
        private int width;
        private int height;
        private int stride;
        private PixelFormat pixelFormat;
        private bool isLocked = false;

        public int Width => width;
        public int Height => height;
        public int Stride => stride;
        public bool IsLocked => isLocked;

        public BitmapBuffer(Bitmap bmp)
        {
            bitmap = bmp ?? throw new ArgumentNullException(nameof(bmp));
            width = bitmap.Width;
            height = bitmap.Height;
            pixelFormat = bitmap.PixelFormat;
        }

        /// <summary>
        /// Lock bitmap để truy cập trực tiếp vào pixel data
        /// </summary>
        public void Lock()
        {
            if (isLocked) return;

            Rectangle rect = new Rectangle(0, 0, width, height);
            bitmapData = bitmap.LockBits(rect, ImageLockMode.ReadWrite, pixelFormat);
            
            pixelDataPtr = bitmapData.Scan0;
            stride = bitmapData.Stride;
            
            // Tạo buffer để lưu trữ dữ liệu pixel
            int bufferSize = Math.Abs(stride) * height;
            pixelBuffer = new byte[bufferSize];
            Marshal.Copy(pixelDataPtr, pixelBuffer, 0, bufferSize);
            
            isLocked = true;
        }

        /// <summary>
        /// Unlock bitmap và ghi lại dữ liệu pixel
        /// </summary>
        public void Unlock()
        {
            if (!isLocked) return;

            if (pixelBuffer != null && bitmapData != null)
            {
                Marshal.Copy(pixelBuffer, 0, pixelDataPtr, pixelBuffer.Length);
            }

            if (bitmapData != null)
            {
                bitmap.UnlockBits(bitmapData);
                bitmapData = null;
            }

            isLocked = false;
        }

        /// <summary>
        /// Lấy màu pixel tại vị trí (x, y)
        /// </summary>
        public Color GetPixel(int x, int y)
        {
            if (x < 0 || x >= width || y < 0 || y >= height)
                return Color.Transparent;

            if (!isLocked) throw new InvalidOperationException("BitmapBuffer must be locked before accessing pixels");
            if (pixelBuffer == null) throw new InvalidOperationException("Pixel buffer not initialized");

            int index = y * stride + x * 4;
            if (index + 3 >= pixelBuffer.Length)
                return Color.Transparent;

            byte b = pixelBuffer[index];
            byte g = pixelBuffer[index + 1];
            byte r = pixelBuffer[index + 2];
            byte a = pixelBuffer[index + 3];

            return Color.FromArgb(a, r, g, b);
        }

        /// <summary>
        /// Đặt màu pixel tại vị trí (x, y)
        /// </summary>
        public void SetPixel(int x, int y, Color color)
        {
            if (x < 0 || x >= width || y < 0 || y >= height)
                return;

            if (!isLocked) throw new InvalidOperationException("BitmapBuffer must be locked before accessing pixels");
            if (pixelBuffer == null) throw new InvalidOperationException("Pixel buffer not initialized");

            int index = y * stride + x * 4;
            if (index + 3 >= pixelBuffer.Length)
                return;

            pixelBuffer[index] = color.B;
            pixelBuffer[index + 1] = color.G;
            pixelBuffer[index + 2] = color.R;
            pixelBuffer[index + 3] = color.A;
        }

        /// <summary>
        /// Blend hai màu với alpha
        /// </summary>
        public static Color Blend(Color fore, Color back, int alpha)
        {
            if (alpha >= 255) return fore;
            if (alpha <= 0) return back;

            float a = alpha / 255f;
            float invA = 1.0f - a;

            int r = (int)((fore.R * a) + (back.R * invA));
            int g = (int)((fore.G * a) + (back.G * invA));
            int b = (int)((fore.B * a) + (back.B * invA));

            return Color.FromArgb(255, r, g, b);
        }

        /// <summary>
        /// Clear bitmap với một màu
        /// </summary>
        public void Clear(Color color)
        {
            if (!isLocked) throw new InvalidOperationException("BitmapBuffer must be locked before clearing");
            if (pixelBuffer == null) throw new InvalidOperationException("Pixel buffer not initialized");

            for (int i = 0; i < pixelBuffer.Length; i += 4)
            {
                pixelBuffer[i] = color.B;
                pixelBuffer[i + 1] = color.G;
                pixelBuffer[i + 2] = color.R;
                pixelBuffer[i + 3] = color.A;
            }
        }

        public void Dispose()
        {
            Unlock();
            pixelBuffer = null;
        }
    }
}
