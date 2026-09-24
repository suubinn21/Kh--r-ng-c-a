using System;
using System.Drawing;
using System.Windows.Forms;
using OpenCvSharp;
using OpenCvSharp.Extensions;

namespace PaintLike
{
    /// <summary>
    /// Lớp xử lý ảnh sử dụng OpenCV
    /// </summary>
    public class ImageProcessor
    {
        /// <summary>
        /// Khử răng cưa (anti-aliasing) đơn giản cho ảnh
        /// </summary>
        /// <param name="image">Ảnh input</param>
        /// <returns>Ảnh sau khi khử răng cưa</returns>
        public static Bitmap RemoveAliasing(Bitmap image)
        {
            try
            {
                // Chuyển Bitmap sang Mat của OpenCV
                Mat srcMat = BitmapConverter.ToMat(image);
                Mat dstMat = new Mat();

                // Áp dụng Bilateral Filter - bộ lọc tuyệt vời để khử nhiễu mà giữ lại cạnh
                // d=5: kích thước vùng lân cận
                // sigmaColor=75: độ lớn của bộ lọc trong không gian màu
                // sigmaSpace=75: độ lớn của bộ lọc trong không gian hình học
                Cv2.BilateralFilter(srcMat, dstMat, 5, 75, 75);

                // Áp dụng morphological opening để loại bỏ nhiễu nhỏ
                Mat kernel = Cv2.GetStructuringElement(MorphShapes.Ellipse, new OpenCvSharp.Size(3, 3));
                Mat openedMat = new Mat();
                Cv2.MorphologyEx(dstMat, openedMat, MorphTypes.Open, kernel, new OpenCvSharp.Point(-1, -1), 1);

                // Chuyển Mat trở lại Bitmap
                Bitmap resultImage = BitmapConverter.ToBitmap(openedMat);

                // Giải phóng tài nguyên
                srcMat.Dispose();
                dstMat.Dispose();
                kernel.Dispose();
                openedMat.Dispose();

                return resultImage;
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(
                    "Lỗi khi xử lý khử răng cưa (đơn giản): " + ex.Message,
                    "Lỗi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return image;
            }
        }

        /// <summary>
        /// Khử răng cưa nâng cao cho ảnh (sử dụng nhiều bộ lọc kết hợp)
        /// </summary>
        /// <param name="image">Ảnh input</param>
        /// <returns>Ảnh sau khi khử răng cưa nâng cao</returns>
        public static Bitmap RemoveAliasingAdvanced(Bitmap image)
        {
            try
            {
                Mat srcMat = BitmapConverter.ToMat(image);
                Mat dstMat = new Mat();

                // Bước 1: Bilateral Filter - khử nhiễu mà giữ cạnh sắc nét
                Cv2.BilateralFilter(srcMat, dstMat, 7, 100, 100);

                // Bước 2: Morphological Opening - loại bỏ các đối tượng nhỏ
                Mat kernel = Cv2.GetStructuringElement(MorphShapes.Ellipse, new OpenCvSharp.Size(5, 5));
                Mat morphMat = new Mat();
                Cv2.MorphologyEx(dstMat, morphMat, MorphTypes.Open, kernel);

                // Bước 3: Morphological Closing - lấp đầy các lỗ nhỏ
                Mat closedMat = new Mat();
                Cv2.MorphologyEx(morphMat, closedMat, MorphTypes.Close, kernel);

                // Bước 4: Median Blur - làm mịn thêm (hiệu quả với các đường cong)
                Mat medianMat = new Mat();
                Cv2.MedianBlur(closedMat, medianMat, 5);

                // Chuyển về Bitmap
                Bitmap resultImage = BitmapConverter.ToBitmap(medianMat);

                // Giải phóng tài nguyên
                srcMat.Dispose();
                dstMat.Dispose();
                kernel.Dispose();
                morphMat.Dispose();
                closedMat.Dispose();
                medianMat.Dispose();

                return resultImage;
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(
                    "Lỗi khi xử lý khử răng cưa (nâng cao): " + ex.Message,
                    "Lỗi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return image;
            }
        }

        /// <summary>
        /// Khử răng cưa tối ưu cho các đường cong (curves)
        /// </summary>
        /// <param name="image">Ảnh input</param>
        /// <returns>Ảnh sau xử lý</returns>
        public static Bitmap RemoveAliasingForCurves(Bitmap image)
        {
            try
            {
                Mat srcMat = BitmapConverter.ToMat(image);
                Mat dstMat = new Mat();

                // Gaussian Blur - tốt cho làm mịn đường cong
                Cv2.GaussianBlur(srcMat, dstMat, new OpenCvSharp.Size(3, 3), 1.0);

                // Bilateral Filter - giữ lại chi tiết
                Mat bilateralMat = new Mat();
                Cv2.BilateralFilter(dstMat, bilateralMat, 5, 50, 50);

                // Edge-Preserving Filter (Morphological operation)
                Mat kernel = Cv2.GetStructuringElement(MorphShapes.Ellipse, new OpenCvSharp.Size(3, 3));
                Mat resultMat = new Mat();
                Cv2.MorphologyEx(bilateralMat, resultMat, MorphTypes.Close, kernel);

                Bitmap resultImage = BitmapConverter.ToBitmap(resultMat);

                srcMat.Dispose();
                dstMat.Dispose();
                bilateralMat.Dispose();
                kernel.Dispose();
                resultMat.Dispose();

                return resultImage;
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(
                    "Lỗi khi xử lý khử răng cưa (curves): " + ex.Message,
                    "Lỗi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return image;
            }
        }

        /// <summary>
        /// Khử răng cưa với độ mạnh có thể điều chỉnh
        /// </summary>
        /// <param name="image">Ảnh input</param>
        /// <param name="strength">Độ mạnh của bộ lọc (1-10, mặc định 5)</param>
        /// <returns>Ảnh sau xử lý</returns>
        public static Bitmap RemoveAliasingWithStrength(Bitmap image, int strength = 5)
        {
            try
            {
                // Điều chỉnh strength nằm trong khoảng 1-10
                strength = Math.Max(1, Math.Min(10, strength));

                Mat srcMat = BitmapConverter.ToMat(image);
                Mat dstMat = new Mat();

                // Tính toán tham số dựa trên strength
                int kernelSize = 3 + (strength - 1) * 2; // 3, 5, 7, 9, ... 21
                int sigmaColor = 50 + (strength - 1) * 5; // 50, 55, 60, ... 95
                int sigmaSpace = sigmaColor;

                // Áp dụng Bilateral Filter với tham số động
                Cv2.BilateralFilter(srcMat, dstMat, kernelSize, sigmaColor, sigmaSpace);

                // Morphological Opening với kernel size động
                Mat kernel = Cv2.GetStructuringElement(MorphShapes.Ellipse, new OpenCvSharp.Size(strength, strength));
                Mat resultMat = new Mat();
                Cv2.MorphologyEx(dstMat, resultMat, MorphTypes.Open, kernel);

                Bitmap resultImage = BitmapConverter.ToBitmap(resultMat);

                srcMat.Dispose();
                dstMat.Dispose();
                kernel.Dispose();
                resultMat.Dispose();

                return resultImage;
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(
                    "Lỗi khi xử lý khử răng cưa (tùy chỉnh): " + ex.Message,
                    "Lỗi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return image;
            }
        }

        /// <summary>
        /// Điều chỉnh độ sáng của ảnh
        /// </summary>
        /// <param name="image">Ảnh input</param>
        /// <param name="brightness">Độ sáng (-100 to 100, 0 là không thay đổi)</param>
        /// <returns>Ảnh sau xử lý</returns>
        public static Bitmap AdjustBrightness(Bitmap image, int brightness)
        {
            try
            {
                brightness = Math.Max(-100, Math.Min(100, brightness));

                Bitmap result = new Bitmap(image.Width, image.Height);
                
                for (int y = 0; y < image.Height; y++)
                {
                    for (int x = 0; x < image.Width; x++)
                    {
                        Color pixel = image.GetPixel(x, y);
                        
                        int r = Math.Max(0, Math.Min(255, pixel.R + brightness));
                        int g = Math.Max(0, Math.Min(255, pixel.G + brightness));
                        int b = Math.Max(0, Math.Min(255, pixel.B + brightness));
                        
                        result.SetPixel(x, y, Color.FromArgb(pixel.A, r, g, b));
                    }
                }
                
                return result;
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(
                    "Lỗi khi điều chỉnh độ sáng: " + ex.Message,
                    "Lỗi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return image;
            }
        }

        /// <summary>
        /// Điều chỉnh độ tương phản của ảnh
        /// </summary>
        /// <param name="image">Ảnh input</param>
        /// <param name="contrast">Độ tương phản (0.5 to 3.0, 1.0 là không thay đổi)</param>
        /// <returns>Ảnh sau xử lý</returns>
        public static Bitmap AdjustContrast(Bitmap image, double contrast)
        {
            try
            {
                contrast = Math.Max(0.5, Math.Min(3.0, contrast));

                Bitmap result = new Bitmap(image.Width, image.Height);
                // FIX Bug #12: Công thức contrast đúng cho miền 0.5-3.0
                // contrast = 1.0 → không thay đổi, < 1.0 → giảm, > 1.0 → tăng
                double factor = contrast;

                for (int y = 0; y < image.Height; y++)
                {
                    for (int x = 0; x < image.Width; x++)
                    {
                        Color pixel = image.GetPixel(x, y);

                        int r = Math.Max(0, Math.Min(255, (int)(factor * (pixel.R - 128) + 128)));
                        int g = Math.Max(0, Math.Min(255, (int)(factor * (pixel.G - 128) + 128)));
                        int b = Math.Max(0, Math.Min(255, (int)(factor * (pixel.B - 128) + 128)));

                        result.SetPixel(x, y, Color.FromArgb(pixel.A, r, g, b));
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(
                    "Lỗi khi điều chỉnh độ tương phản: " + ex.Message,
                    "Lỗi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return image;
            }
        }

        /// <summary>
        /// Áp dụng Blur (làm mờ) cho ảnh
        /// </summary>
        /// <param name="image">Ảnh input</param>
        /// <param name="strength">Độ mạnh (1-10)</param>
        /// <returns>Ảnh sau xử lý</returns>
        public static Bitmap ApplyBlur(Bitmap image, int strength = 3)
        {
            try
            {
                strength = Math.Max(1, Math.Min(10, strength));

                Mat srcMat = BitmapConverter.ToMat(image);
                Mat dstMat = new Mat();

                int kernelSize = 1 + (strength - 1) * 2; // 1, 3, 5, 7, ...
                Cv2.Blur(srcMat, dstMat, new OpenCvSharp.Size(kernelSize, kernelSize));

                Bitmap resultImage = BitmapConverter.ToBitmap(dstMat);

                srcMat.Dispose();
                dstMat.Dispose();

                return resultImage;
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(
                    "Lỗi khi áp dụng Blur: " + ex.Message,
                    "Lỗi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return image;
            }
        }

        /// <summary>
        /// Áp dụng Gaussian Blur (làm mờ Gaussian)
        /// </summary>
        /// <param name="image">Ảnh input</param>
        /// <param name="strength">Độ mạnh (1-10)</param>
        /// <returns>Ảnh sau xử lý</returns>
        public static Bitmap ApplyGaussianBlur(Bitmap image, int strength = 3)
        {
            try
            {
                strength = Math.Max(1, Math.Min(10, strength));

                Mat srcMat = BitmapConverter.ToMat(image);
                Mat dstMat = new Mat();

                int kernelSize = 1 + (strength - 1) * 2; // 1, 3, 5, 7, ...
                Cv2.GaussianBlur(srcMat, dstMat, new OpenCvSharp.Size(kernelSize, kernelSize), 0);

                Bitmap resultImage = BitmapConverter.ToBitmap(dstMat);

                srcMat.Dispose();
                dstMat.Dispose();

                return resultImage;
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(
                    "Lỗi khi áp dụng Gaussian Blur: " + ex.Message,
                    "Lỗi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return image;
            }
        }

        /// <summary>
        /// Chuyển ảnh sang Grayscale (đen trắng)
        /// </summary>
        /// <param name="image">Ảnh input</param>
        /// <returns>Ảnh đen trắng</returns>
        public static Bitmap ConvertToGrayscale(Bitmap image)
        {
            try
            {
                Mat srcMat = BitmapConverter.ToMat(image);
                Mat dstMat = new Mat();

                Cv2.CvtColor(srcMat, dstMat, ColorConversionCodes.BGR2GRAY);
                // Chuyển lại sang BGR để giữ định dạng
                Mat rgbMat = new Mat();
                Cv2.CvtColor(dstMat, rgbMat, ColorConversionCodes.GRAY2BGR);

                Bitmap resultImage = BitmapConverter.ToBitmap(rgbMat);

                srcMat.Dispose();
                dstMat.Dispose();
                rgbMat.Dispose();

                return resultImage;
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(
                    "Lỗi khi chuyển sang Grayscale: " + ex.Message,
                    "Lỗi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return image;
            }
        }

        /// <summary>
        /// Áp dụng Edge Detection (phát hiện cạnh)
        /// </summary>
        /// <param name="image">Ảnh input</param>
        /// <returns>Ảnh sau khi phát hiện cạnh</returns>
        public static Bitmap DetectEdges(Bitmap image)
        {
            try
            {
                Mat srcMat = BitmapConverter.ToMat(image);
                Mat grayMat = new Mat();
                Mat edgesMat = new Mat();

                // Chuyển sang grayscale
                Cv2.CvtColor(srcMat, grayMat, ColorConversionCodes.BGR2GRAY);

                // Áp dụng Canny Edge Detection
                Cv2.Canny(grayMat, edgesMat, 100, 200);

                // Chuyển lại sang BGR
                Mat resultMat = new Mat();
                Cv2.CvtColor(edgesMat, resultMat, ColorConversionCodes.GRAY2BGR);

                Bitmap resultImage = BitmapConverter.ToBitmap(resultMat);

                srcMat.Dispose();
                grayMat.Dispose();
                edgesMat.Dispose();
                resultMat.Dispose();

                return resultImage;
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(
                    "Lỗi khi phát hiện cạnh: " + ex.Message,
                    "Lỗi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return image;
            }
        }

        /// <summary>
        /// Áp dụng Sharpen (làm sắc nét ảnh)
        /// </summary>
        /// <param name="image">Ảnh input</param>
        /// <param name="strength">Độ mạnh (1-5)</param>
        /// <returns>Ảnh sau xử lý</returns>
        public static Bitmap ApplySharpen(Bitmap image, int strength = 1)
        {
            try
            {
                strength = Math.Max(1, Math.Min(5, strength));

                Mat srcMat = BitmapConverter.ToMat(image);
                Mat resultMat = new Mat();

                // Tạo kernel sharpen
                float[,] kernelData = new float[,]
                {
                    { 0, -1, 0 },
                    { -1, 4 + strength, -1 },
                    { 0, -1, 0 }
                };

                Mat kernel = Mat.FromArray(kernelData);
                Cv2.Filter2D(srcMat, resultMat, -1, kernel);

                Bitmap resultImage = BitmapConverter.ToBitmap(resultMat);

                srcMat.Dispose();
                kernel.Dispose();
                resultMat.Dispose();

                return resultImage;
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(
                    "Lỗi khi làm sắc nét ảnh: " + ex.Message,
                    "Lỗi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return image;
            }
        }

        /// <summary>
        /// Đảo ngược màu sắc (Invert)
        /// </summary>
        /// <param name="image">Ảnh input</param>
        /// <returns>Ảnh đảo ngược</returns>
        public static Bitmap InvertColors(Bitmap image)
        {
            try
            {
                Mat srcMat = BitmapConverter.ToMat(image);
                Mat dstMat = new Mat();

                Cv2.BitwiseNot(srcMat, dstMat);

                Bitmap resultImage = BitmapConverter.ToBitmap(dstMat);

                srcMat.Dispose();
                dstMat.Dispose();

                return resultImage;
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(
                    "Lỗi khi đảo ngược màu: " + ex.Message,
                    "Lỗi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return image;
            }
        }

        /// <summary>
        /// Áp dụng Sepia tone (tông màu sepia cổ điển)
        /// </summary>
        /// <param name="image">Ảnh input</param>
        /// <returns>Ảnh với tông Sepia</returns>
        public static Bitmap ApplySepia(Bitmap image)
        {
            try
            {
                Bitmap result = new Bitmap(image.Width, image.Height);

                for (int y = 0; y < image.Height; y++)
                {
                    for (int x = 0; x < image.Width; x++)
                    {
                        Color pixel = image.GetPixel(x, y);

                        // Công thức Sepia
                        int r = Math.Min(255, (int)(pixel.R * 0.393 + pixel.G * 0.769 + pixel.B * 0.189));
                        int g = Math.Min(255, (int)(pixel.R * 0.349 + pixel.G * 0.686 + pixel.B * 0.168));
                        int b = Math.Min(255, (int)(pixel.R * 0.272 + pixel.G * 0.534 + pixel.B * 0.131));

                        result.SetPixel(x, y, Color.FromArgb(pixel.A, r, g, b));
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(
                    "Lỗi khi áp dụng Sepia: " + ex.Message,
                    "Lỗi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return image;
            }
        }

        /// <summary>
        /// Tăng độ bão hòa màu sắc
        /// </summary>
        /// <param name="image">Ảnh input</param>
        /// <param name="saturation">Độ bão hòa (0.0 to 2.0, 1.0 là không thay đổi)</param>
        /// <returns>Ảnh sau xử lý</returns>
        public static Bitmap AdjustSaturation(Bitmap image, double saturation)
        {
            try
            {
                saturation = Math.Max(0.0, Math.Min(2.0, saturation));

                Bitmap result = new Bitmap(image.Width, image.Height);

                for (int y = 0; y < image.Height; y++)
                {
                    for (int x = 0; x < image.Width; x++)
                    {
                        Color pixel = image.GetPixel(x, y);

                        // Chuyển RGB sang HSV
                        float h, s, v;
                        RgbToHsv(pixel.R, pixel.G, pixel.B, out h, out s, out v);

                        // Điều chỉnh saturation
                        s = (float)Math.Min(1.0, s * saturation);

                        // Chuyển lại sang RGB
                        Color newColor = HsvToRgb(h, s, v);
                        result.SetPixel(x, y, Color.FromArgb(pixel.A, newColor.R, newColor.G, newColor.B));
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(
                    "Lỗi khi điều chỉnh bão hòa: " + ex.Message,
                    "Lỗi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return image;
            }
        }
        //Thanh start : Khử nền bằng thuật toán GrabCut OpenCV
        public static Bitmap RemoveBackgroundGrabCut(Bitmap sourceBmp, Rectangle rect)
        {
            // Sử dụng khối using cho tất cả các đối tượng Mat để giải phóng bộ nhớ tự động
            using (Mat src = sourceBmp.ToMat())
            using (Mat mask = new Mat())
            using (Mat bgModel = new Mat())
            using (Mat fgModel = new Mat())
            using (Mat foregroundMask = new Mat())
            using (Mat finalMask = new Mat())
            using (Mat result = new Mat(src.Size(), MatType.CV_8UC4, new Scalar(0, 0, 0, 0)))
            using (Mat srcBGRA = new Mat())
            {
                // 1. Chuyển đổi kênh màu nếu cần (GrabCut yêu cầu 3 kênh BGR)
                if (src.Type() == MatType.CV_8UC4)
                {
                    Cv2.CvtColor(src, src, ColorConversionCodes.BGRA2BGR);
                }

                // 2. Định nghĩa vùng quan tâm (ROI)
                Rect roi = new Rect(rect.X, rect.Y, rect.Width, rect.Height);

                // 3. Thực thi GrabCut (với 5 vòng lặp để cân bằng giữa độ chính xác và tốc độ)
                Cv2.GrabCut(src, mask, roi, bgModel, fgModel, 5, GrabCutModes.InitWithRect);

                // 4. Xử lý mặt nạ (Mask)
                // Lấy các pixel chắc chắn là tiền cảnh (FGD) và có thể là tiền cảnh (PR_FGD)
                Cv2.Compare(mask, Scalar.All((double)GrabCutClasses.PR_FGD), foregroundMask, CmpTypes.EQ);
                Cv2.Compare(mask, Scalar.All((double)GrabCutClasses.FGD), finalMask, CmpTypes.EQ);

                // Gộp hai mặt nạ lại
                Cv2.BitwiseOr(foregroundMask, finalMask, finalMask);

                // 5. Tạo ảnh kết quả có nền trong suốt
                Cv2.CvtColor(src, srcBGRA, ColorConversionCodes.BGR2BGRA);
                srcBGRA.CopyTo(result, finalMask);

                // 6. Chuyển đổi ngược lại Bitmap để hiển thị trên UI
                return result.ToBitmap();
            } // Tất cả các đối tượng Mat (src, mask,...) sẽ được giải phóng hoàn toàn tại đây
        }
        /// <summary>
        /// Chuyển RGB sang HSV
        /// </summary>
        private static void RgbToHsv(int r, int g, int b, out float h, out float s, out float v)
        {
            float rf = r / 255.0f;
            float gf = g / 255.0f;
            float bf = b / 255.0f;

            float max = Math.Max(rf, Math.Max(gf, bf));
            float min = Math.Min(rf, Math.Min(gf, bf));
            float delta = max - min;

            // Tính V
            v = max;

            // Tính S
            s = max == 0 ? 0 : delta / max;

            // Tính H
            if (delta == 0)
            {
                h = 0;
            }
            else if (max == rf)
            {
                h = 60 * (((gf - bf) / delta) % 6);
            }
            else if (max == gf)
            {
                h = 60 * ((bf - rf) / delta + 2);
            }
            else
            {
                h = 60 * ((rf - gf) / delta + 4);
            }

            if (h < 0) h += 360;
        }

        /// <summary>
        /// Chuyển HSV sang RGB
        /// </summary>
        private static Color HsvToRgb(float h, float s, float v)
        {
            float c = v * s;
            float x = c * (1 - Math.Abs((h / 60) % 2 - 1));
            float m = v - c;

            float rf, gf, bf;

            if (h < 60)
            {
                rf = c;
                gf = x;
                bf = 0;
            }
            else if (h < 120)
            {
                rf = x;
                gf = c;
                bf = 0;
            }
            else if (h < 180)
            {
                rf = 0;
                gf = c;
                bf = x;
            }
            else if (h < 240)
            {
                rf = 0;
                gf = x;
                bf = c;
            }
            else if (h < 300)
            {
                rf = x;
                gf = 0;
                bf = c;
            }
            else
            {
                rf = c;
                gf = 0;
                bf = x;
            }

            int r = (int)((rf + m) * 255);
            int g = (int)((gf + m) * 255);
            int b = (int)((bf + m) * 255);

            return Color.FromArgb(r, g, b);
        }
    }
}
