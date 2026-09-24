using System;
using System.Drawing;

namespace PaintLike
{
    /// <summary>
    /// WuLineDrawer: High-performance graphics rendering with LockBits optimization
    /// Sử dụng BitmapBuffer để truy cập pixel nhanh và con trỏ unsafe cho hiệu suất tối đa
    /// </summary>
    public class WuLineDrawer
    {
        // ==========================================
        // PHẦN 1: CÁC HÀM CỐT LÕI (PLOT & BLEND)
        // ==========================================

        /// <summary>
        /// Plot pixel tại (x, y) với độ sáng (anti-aliasing)
        /// Sử dụng BitmapBuffer cho hiệu suất cao
        /// </summary>
        private static void Plot(BitmapBuffer buffer, int x, int y, double brightness, Color color)
        {
            if (buffer == null || !buffer.IsLocked) return;
            if (x < 0 || x >= buffer.Width || y < 0 || y >= buffer.Height) return;

            int alpha = (int)(brightness * 255);
            alpha = Math.Min(255, Math.Max(0, alpha));

            Color backColor = buffer.GetPixel(x, y);
            Color finalColor = BitmapBuffer.Blend(color, backColor, alpha);
            buffer.SetPixel(x, y, finalColor);
        }

        private static int IPart(double x) => (int)Math.Floor(x);
        private static int Round(double x) => (int)Math.Round(x);
        private static double FPart(double x) => x - Math.Floor(x);
        private static double RFPart(double x) => 1.0 - FPart(x);

        
        // ==========================================
        // PHẦN 2: VẼ CÁC HÌNH DẠO HÀM
        // ==========================================

        /// <summary>
        /// Vẽ đường thẳng khử răng cưa (Wu's Line Algorithm)
        /// Sử dụng BitmapBuffer để tối ưu hiệu suất
        /// </summary>
        public static void DrawLine(Bitmap bmp, int x0, int y0, int x1, int y1, Color color)
        {
            using (BitmapBuffer buffer = new BitmapBuffer(bmp))
            {
                buffer.Lock();
                DrawLineOptimized(buffer, x0, y0, x1, y1, color);
                buffer.Unlock();
            }
        }

        private static void DrawLineOptimized(BitmapBuffer buffer, int x0, int y0, int x1, int y1, Color color)
        {
            bool steep = Math.Abs(y1 - y0) > Math.Abs(x1 - x0);

            if (steep) { (x0, y0) = (y0, x0); (x1, y1) = (y1, x1); }
            if (x0 > x1) { (x0, x1) = (x1, x0); (y0, y1) = (y1, y0); }

            double dx = x1 - x0;
            double dy = y1 - y0;
            double gradient = (dx == 0) ? 1.0 : dy / dx;

            int xend = Round(x0);
            double yend = y0 + gradient * (xend - x0);
            double xgap = RFPart(x0 + 0.5);
            int xpxl1 = xend;
            int ypxl1 = IPart(yend);

            if (steep)
            {
                Plot(buffer, ypxl1, xpxl1, RFPart(yend) * xgap, color);
                Plot(buffer, ypxl1 + 1, xpxl1, FPart(yend) * xgap, color);
            }
            else
            {
                Plot(buffer, xpxl1, ypxl1, RFPart(yend) * xgap, color);
                Plot(buffer, xpxl1, ypxl1 + 1, FPart(yend) * xgap, color);
            }

            double intery = yend + gradient;

            xend = Round(x1);
            yend = y1 + gradient * (xend - x1);
            xgap = FPart(x1 + 0.5);
            int xpxl2 = xend;
            int ypxl2 = IPart(yend);

            if (steep)
            {
                Plot(buffer, ypxl2, xpxl2, RFPart(yend) * xgap, color);
                Plot(buffer, ypxl2 + 1, xpxl2, FPart(yend) * xgap, color);
            }
            else
            {
                Plot(buffer, xpxl2, ypxl2, RFPart(yend) * xgap, color);
                Plot(buffer, xpxl2, ypxl2 + 1, FPart(yend) * xgap, color);
            }

            if (steep)
            {
                for (int x = xpxl1 + 1; x < xpxl2; x++)
                {
                    Plot(buffer, IPart(intery), x, RFPart(intery), color);
                    Plot(buffer, IPart(intery) + 1, x, FPart(intery), color);
                    intery += gradient;
                }
            }
            else
            {
                for (int x = xpxl1 + 1; x < xpxl2; x++)
                {
                    Plot(buffer, x, IPart(intery), RFPart(intery), color);
                    Plot(buffer, x, IPart(intery) + 1, FPart(intery), color);
                    intery += gradient;
                }
            }
        }

       
        /// Vẽ đường thẳng có độ dày (Phóng to nét vẽ)
        public static void DrawThickLine(Bitmap bmp, int x0, int y0, int x1, int y1, Color color, int thickness)
        {
            if (thickness <= 1)
            {
                DrawLine(bmp, x0, y0, x1, y1, color);
                return;
            }

            int dx = x1 - x0;
            int dy = y1 - y0;
            bool isSteep = Math.Abs(dy) > Math.Abs(dx);
            int halfWidth = thickness / 2;

            // Vẽ nhiều đường song song để tạo độ dày
            for (int i = -halfWidth; i <= halfWidth; i++)
            {
                if (isSteep)
                    DrawLine(bmp, x0 + i, y0, x1 + i, y1, color);
                else
                    DrawLine(bmp, x0, y0 + i, x1, y1 + i, color);
            }
        }

        /// Vẽ hình chữ nhật
       
        public static void DrawRectangle(Bitmap bmp, Rectangle rect, Color color)
        {
            DrawLine(bmp, rect.Left, rect.Top, rect.Right, rect.Top, color);
            DrawLine(bmp, rect.Right, rect.Top, rect.Right, rect.Bottom, color);
            DrawLine(bmp, rect.Right, rect.Bottom, rect.Left, rect.Bottom, color);
            DrawLine(bmp, rect.Left, rect.Bottom, rect.Left, rect.Top, color);
        }

        
        /// thêm độ dàu cho hình chữ nhật
        public static void DrawThickRectangle(Bitmap bmp, Rectangle rect, Color color, int thickness)
        {
          DrawThickLine(bmp, rect.Left, rect.Top, rect.Right, rect.Top, color, thickness);
            DrawThickLine(bmp, rect.Right, rect.Top, rect.Right, rect.Bottom, color, thickness);
            DrawThickLine(bmp, rect.Right, rect.Bottom, rect.Left, rect.Bottom, color, thickness);
            DrawThickLine(bmp, rect.Left, rect.Bottom, rect.Left, rect.Top, color, thickness);
        }
        
      
        /// Vẽ đa giác (Dùng cho Tam giác, Ngôi sao, Hình thang)
        public static void DrawPolygon(Bitmap bmp, Point[] points, Color color)
        {
            if (points.Length < 2) return;
            for (int i = 0; i < points.Length - 1; i++)
            {
                DrawLine(bmp, points[i].X, points[i].Y, points[i + 1].X, points[i + 1].Y, color);
            }
            // Nối điểm cuối về đầu
            DrawLine(bmp, points[points.Length - 1].X, points[points.Length - 1].Y, points[0].X, points[0].Y, color);
        }
        /// Vẽ đa giác có độ dày (Dùng cho Tam giác, Ngôi sao, Hình thang)
        public static void DrawThickPolygon(Bitmap bmp, Point[] points, Color color, int thickness)
        {
            if (points.Length < 2) return;
            for (int i = 0; i < points.Length - 1; i++)
            {
                DrawThickLine(bmp, points[i].X, points[i].Y, points[i + 1].X, points[i + 1].Y, color, thickness);
            }
            // Nối điểm cuối về đầu
            DrawThickLine(bmp, points[points.Length - 1].X, points[points.Length - 1].Y, points[0].X, points[0].Y, color, thickness);
        }


        // ==========================================
        // PHẦN 3: VẼ HÌNH TRÒN & ELLIPSE
        // ==========================================

        private static void PlotCirclePoints(BitmapBuffer buffer, int cx, int cy, int x, int y, double alpha, Color color)
        {
            Plot(buffer, cx + x, cy + y, alpha, color);
            Plot(buffer, cx + x, cy - y, alpha, color);
            Plot(buffer, cx - x, cy + y, alpha, color);
            Plot(buffer, cx - x, cy - y, alpha, color);
            Plot(buffer, cx + y, cy + x, alpha, color);
            Plot(buffer, cx + y, cy - x, alpha, color);
            Plot(buffer, cx - y, cy + x, alpha, color);
            Plot(buffer, cx - y, cy - x, alpha, color);
        }

        /// <summary>
        /// Vẽ hình tròn với anti-aliasing
        /// </summary>
        public static void DrawCircle(Bitmap bmp, int cx, int cy, int radius, Color color)
        {
            using (BitmapBuffer buffer = new BitmapBuffer(bmp))
            {
                buffer.Lock();
                DrawCircleOptimized(buffer, cx, cy, radius, color);
                buffer.Unlock();
            }
        }

        private static void DrawCircleOptimized(BitmapBuffer buffer, int cx, int cy, int radius, Color color)
        {
            int x = 0;
            double y = radius;
            PlotCirclePoints(buffer, cx, cy, 0, radius, 1.0, color);

            while (x <= y)
            {
                x++;
                y = Math.Sqrt(radius * radius - x * x);
                double alpha = y - Math.Floor(y);
                PlotCirclePoints(buffer, cx, cy, x, (int)Math.Floor(y), 1.0 - alpha, color);
                PlotCirclePoints(buffer, cx, cy, x, (int)Math.Floor(y) + 1, alpha, color);
            }
        }
        //vẽ độ dày cho hình tròn
         public static void DrawThickCircle(Bitmap bmp, int cx, int cy, int radius, Color color, int thickness)
        {
            if (thickness <= 1)
            {
                DrawCircle(bmp, cx, cy, radius, color);
                return;
            }

            int halfWidth = thickness / 2;
            // Vẽ nhiều circle lồng nhau để tạo độ dày
            for (int i = -halfWidth; i <= halfWidth; i++)
            {
                int newRadius = Math.Max(1, radius + i);
                DrawCircle(bmp, cx, cy, newRadius, color);
            }
        }

        
        // ==========================================
        // PHẦN 4: VẼ CÁC HÌNH DẠNG NÂNG CAO
        // ==========================================

        // Helper: Calculate hexagon points
        private static Point[] GetHexagonPoints(Rectangle rect)
        {
            double cx = rect.X + rect.Width / 2.0;
            double cy = rect.Y + rect.Height / 2.0;
            double radius = Math.Min(rect.Width, rect.Height) / 2.0;
            Point[] points = new Point[6];
            
            for (int i = 0; i < 6; i++)
            {
                double angle = Math.PI / 3 * i - Math.PI / 6;
                double x = cx + radius * Math.Cos(angle);
                double y = cy + radius * Math.Sin(angle);
                points[i] = new Point((int)x, (int)y);
            }
            return points;
        }

        // LỤC GIÁC - Dùng thuật toán vẽ đa giác dựa trên đường thẳng
        public static void DrawHexagon(Bitmap bmp, Rectangle rect, Color color)
            => DrawPolygon(bmp, GetHexagonPoints(rect), color);

        public static void DrawThickHexagon(Bitmap bmp, Rectangle rect, Color color, int thickness)
            => DrawThickPolygon(bmp, GetHexagonPoints(rect), color, thickness);

        // Helper: Calculate diamond points
        private static Point[] GetDiamondPoints(Rectangle rect)
        {
            return new Point[]
            {
                new Point(rect.X + rect.Width / 2, rect.Top),
                new Point(rect.Right, rect.Y + rect.Height / 2),
                new Point(rect.X + rect.Width / 2, rect.Bottom),
                new Point(rect.Left, rect.Y + rect.Height / 2)
            };
        }

        // KIM CƯƠNG - Dùng thuật toán vẽ đa giác 4 cạnh
        public static void DrawDiamond(Bitmap bmp, Rectangle rect, Color color)
            => DrawPolygon(bmp, GetDiamondPoints(rect), color);

        public static void DrawThickDiamond(Bitmap bmp, Rectangle rect, Color color, int thickness)
            => DrawThickPolygon(bmp, GetDiamondPoints(rect), color, thickness);

        // Helper: Calculate arrow points
        private static Point[] GetArrowPoints(Rectangle rect)
        {
            int arrowSize = Math.Min(rect.Width, rect.Height) / 3;
            return new Point[]
            {
                new Point(rect.X + rect.Width / 2, rect.Top),
                new Point(rect.Right - arrowSize, rect.Top + arrowSize),
                new Point(rect.X + rect.Width / 2 + arrowSize / 2, rect.Top + arrowSize),
                new Point(rect.X + rect.Width / 2 + arrowSize / 2, rect.Bottom),
                new Point(rect.X + rect.Width / 2 - arrowSize / 2, rect.Bottom),
                new Point(rect.X + rect.Width / 2 - arrowSize / 2, rect.Top + arrowSize),
                new Point(rect.Left + arrowSize, rect.Top + arrowSize)
            };
        }

        // MŨI TẾN - Dùng thuật toán vẽ đa giác 7 cạnh
        public static void DrawArrows(Bitmap bmp, Rectangle rect, Color color)
            => DrawPolygon(bmp, GetArrowPoints(rect), color);

        public static void DrawThickArrows(Bitmap bmp, Rectangle rect, Color color, int thickness)
            => DrawThickPolygon(bmp, GetArrowPoints(rect), color, thickness);

        // ELLIPSE - Dùng thuật toán vẽ ellipse dựa trên thuật toán tròn + Wu's antialiasing
        public static void DrawEllipse(Bitmap bmp, Rectangle rect, Color color)
        {
            using (BitmapBuffer buffer = new BitmapBuffer(bmp))
            {
                buffer.Lock();
                DrawEllipseOptimized(buffer, rect, color);
                buffer.Unlock();
            }
        }

        private static void DrawEllipseOptimized(BitmapBuffer buffer, Rectangle rect, Color color)
        {
            int a = rect.Width / 2;
            int b = rect.Height / 2;
            int cx = rect.X + a;
            int cy = rect.Y + b;

            int x = 0;
            double y = b;

            double d1 = (b * b) - (a * a * b) + (0.25 * a * a);

            while ((a * a * (y - 0.5)) > (b * b * (x + 1)))
            {
                if (d1 < 0)
                {
                    d1 += (2 * b * b * (2 * x + 3));
                }
                else
                {
                    d1 += (2 * b * b * (2 * x + 3)) - (4 * a * a * (y - 1));
                    y--;
                }
                x++;

                int y_int = (int)Math.Floor(y);
                double alpha = y - y_int;

                Plot(buffer, cx + x, cy + y_int, 1.0 - alpha, color);
                Plot(buffer, cx + x, cy + y_int + 1, alpha, color);
                Plot(buffer, cx - x, cy + y_int, 1.0 - alpha, color);
                Plot(buffer, cx - x, cy + y_int + 1, alpha, color);
                Plot(buffer, cx + x, cy - y_int, 1.0 - alpha, color);
                Plot(buffer, cx + x, cy - y_int - 1, alpha, color);
                Plot(buffer, cx - x, cy - y_int, 1.0 - alpha, color);
                Plot(buffer, cx - x, cy - y_int - 1, alpha, color);
            }

            double d2 = (b * b * (x + 0.5) * (x + 0.5)) + (a * a * (y - 1) * (y - 1)) - (a * a * b * b);

            while (y > 0)
            {
                if (d2 < 0)
                {
                    d2 += (2 * b * b * (2 * x + 2)) - (4 * a * a * (y - 1));
                    x++;
                }
                else
                {
                    d2 -= (4 * a * a * (y - 1));
                }
                y--;

                int y_int = (int)Math.Floor(y);
                double alpha = y - y_int;

                Plot(buffer, cx + x, cy + y_int, 1.0 - alpha, color);
                Plot(buffer, cx + x, cy + y_int + 1, alpha, color);
                Plot(buffer, cx - x, cy + y_int, 1.0 - alpha, color);
                Plot(buffer, cx - x, cy + y_int + 1, alpha, color);
                Plot(buffer, cx + x, cy - y_int, 1.0 - alpha, color);
                Plot(buffer, cx + x, cy - y_int - 1, alpha, color);
                Plot(buffer, cx - x, cy - y_int, 1.0 - alpha, color);
                Plot(buffer, cx - x, cy - y_int - 1, alpha, color);
            }
        }

        // FIX Bug #16: Offset đối xứng cả X lẫn Y
        public static void DrawThickEllipse(Bitmap bmp, Rectangle rect, Color color, int thickness)
        {
            if (thickness <= 1)
            {
                DrawEllipse(bmp, rect, color);
                return;
            }

            int halfWidth = thickness / 2;
            for (int i = -halfWidth; i <= halfWidth; i++)
            {
                Rectangle newRect = new Rectangle(
                    rect.X + i,                                        // FIX: offset X
                    rect.Y + i,                                        // FIX: offset Y đối xứng
                    Math.Max(1, rect.Width - Math.Abs(i) * 2),         // Thu nhỏ width
                    Math.Max(1, rect.Height - Math.Abs(i) * 2)         // FIX: Thu nhỏ height đối xứng
                );
                DrawEllipse(bmp, newRect, color);
            }
        }

        // BEZIER CURVE - Dùng thuật toán De Casteljau + Wu's antialiasing
        public static void DrawBezier(Bitmap bmp, Point p0, Point p1, Point p2, Point p3, Color color, int steps = 50)
        {
            Point prevPoint = p0;
            
            // Vẽ steps+1 điểm để bao gồm cả điểm cuối (t=1.0)
            for (int i = 0; i <= steps; i++)
            {
                double t = (i == steps) ? 1.0 : (double)i / steps;
                double t_inv = 1.0 - t;
                
                // De Casteljau's algorithm: B(t) = (1-t)³P₀ + 3(1-t)²tP₁ + 3(1-t)t²P₂ + t³P₃
                double coef0 = t_inv * t_inv * t_inv;
                double coef1 = 3 * t * t_inv * t_inv;
                double coef2 = 3 * t * t * t_inv;
                double coef3 = t * t * t;
                
                double x = (coef0 * p0.X) + (coef1 * p1.X) + (coef2 * p2.X) + (coef3 * p3.X);
                double y = (coef0 * p0.Y) + (coef1 * p1.Y) + (coef2 * p2.Y) + (coef3 * p3.Y);
                
                Point currentPoint = new Point((int)Math.Round(x), (int)Math.Round(y));
                
                // Vẽ đường nối từ điểm trước đến điểm hiện tại
                if (i > 0)
                {
                    DrawLine(bmp, prevPoint.X, prevPoint.Y, currentPoint.X, currentPoint.Y, color);
                }
                
                prevPoint = currentPoint;
            }
        }

        public static void DrawThickBezier(Bitmap bmp, Point p0, Point p1, Point p2, Point p3, Color color, int thickness, int steps = 50)
        {
            if (thickness <= 1)
            {
                DrawBezier(bmp, p0, p1, p2, p3, color, steps);
                return;
            }

            // Vẽ nhiều đường Bezier song song
            int halfWidth = thickness / 2;
            for (int i = -halfWidth; i <= halfWidth; i++)
            {
                DrawBezier(bmp, 
                    new Point(p0.X, p0.Y + i),
                    new Point(p1.X, p1.Y + i),
                    new Point(p2.X, p2.Y + i),
                    new Point(p3.X, p3.Y + i),
                    color, steps);
            }
        }

        // FIX Bug #18: Vẽ 1/4 cung tròn thay vì nguyên circle
        // Hàm helper vẽ quarter arc tại vị trí (cx, cy) với bán kính r
        private static void DrawQuarterArc(BitmapBuffer buffer, int cx, int cy, int radius, int quadrant, Color color)
        {
            int x = 0;
            double y = radius;

            while (x <= y)
            {
                double realY = Math.Sqrt((double)radius * radius - (double)x * x);
                double alpha = realY - Math.Floor(realY);
                int y_int = (int)Math.Floor(realY);

                switch (quadrant)
                {
                    case 0:
                        Plot(buffer, cx - x, cy - y_int, 1.0 - alpha, color);
                        Plot(buffer, cx - x, cy - y_int - 1, alpha, color);
                        Plot(buffer, cx - y_int, cy - x, 1.0 - alpha, color);
                        Plot(buffer, cx - y_int - 1, cy - x, alpha, color);
                        break;
                    case 1:
                        Plot(buffer, cx + x, cy - y_int, 1.0 - alpha, color);
                        Plot(buffer, cx + x, cy - y_int - 1, alpha, color);
                        Plot(buffer, cx + y_int, cy - x, 1.0 - alpha, color);
                        Plot(buffer, cx + y_int + 1, cy - x, alpha, color);
                        break;
                    case 2:
                        Plot(buffer, cx + x, cy + y_int, 1.0 - alpha, color);
                        Plot(buffer, cx + x, cy + y_int + 1, alpha, color);
                        Plot(buffer, cx + y_int, cy + x, 1.0 - alpha, color);
                        Plot(buffer, cx + y_int + 1, cy + x, alpha, color);
                        break;
                    case 3:
                        Plot(buffer, cx - x, cy + y_int, 1.0 - alpha, color);
                        Plot(buffer, cx - x, cy + y_int + 1, alpha, color);
                        Plot(buffer, cx - y_int, cy + x, 1.0 - alpha, color);
                        Plot(buffer, cx - y_int - 1, cy + x, alpha, color);
                        break;
                }

                x++;
                y = realY;
            }
        }

        // HÌNH CHỮ NHẬT BO GÓC - Kết hợp đường thẳng + 1/4 cung tròn
        public static void DrawRoundedRectangle(Bitmap bmp, Rectangle rect, int cornerRadius, Color color)
        {
            using (BitmapBuffer buffer = new BitmapBuffer(bmp))
            {
                buffer.Lock();
                DrawRoundedRectangleOptimized(buffer, rect, cornerRadius, color);
                buffer.Unlock();
            }
        }

        private static void DrawRoundedRectangleOptimized(BitmapBuffer buffer, Rectangle rect, int cornerRadius, Color color)
        {
            int maxRadius = Math.Min(rect.Width, rect.Height) / 2;
            int r = Math.Min(cornerRadius, maxRadius);

            if (r <= 0)
            {
                DrawLineOptimized(buffer, rect.Left, rect.Top, rect.Right, rect.Top, color);
                return;
            }

            DrawLineOptimized(buffer, rect.Left + r, rect.Top, rect.Right - r, rect.Top, color);
            DrawLineOptimized(buffer, rect.Right, rect.Top + r, rect.Right, rect.Bottom - r, color);
            DrawLineOptimized(buffer, rect.Right - r, rect.Bottom, rect.Left + r, rect.Bottom, color);
            DrawLineOptimized(buffer, rect.Left, rect.Bottom - r, rect.Left, rect.Top + r, color);

            DrawQuarterArc(buffer, rect.Left + r, rect.Top + r, r, 0, color);
            DrawQuarterArc(buffer, rect.Right - r, rect.Top + r, r, 1, color);
            DrawQuarterArc(buffer, rect.Right - r, rect.Bottom - r, r, 2, color);
            DrawQuarterArc(buffer, rect.Left + r, rect.Bottom - r, r, 3, color);
        }

        // FIX Bug #15: Offset đối xứng cho rounded rectangle
        public static void DrawThickRoundedRectangle(Bitmap bmp, Rectangle rect, int cornerRadius, Color color, int thickness)
        {
            if (thickness <= 1)
            {
                DrawRoundedRectangle(bmp, rect, cornerRadius, color);
                return;
            }

            int halfWidth = thickness / 2;
            for (int i = -halfWidth; i <= halfWidth; i++)
            {
                Rectangle offsetRect = new Rectangle(
                    rect.X + i,                                       // FIX: offset X đối xứng
                    rect.Y + i,                                       // FIX: offset Y đối xứng
                    Math.Max(1, rect.Width - Math.Abs(i) * 2),        // Thu width đều 2 bên
                    Math.Max(1, rect.Height - Math.Abs(i) * 2)        // FIX: Thu height đều 2 bên
                );
                DrawRoundedRectangle(bmp, offsetRect, Math.Max(1, cornerRadius - Math.Abs(i)), color);
            }
        }
    }
}