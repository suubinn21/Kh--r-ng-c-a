# 🎨 Thuật Toán Khử Răng Cưa (Anti-Aliasing) - PaintLike

Dự án nghiên cứu và hiện thực hóa các **thuật toán khử răng cưa (Anti-Aliasing)** cùng các kỹ thuật tối ưu hóa kết xuất đồ họa (Graphics Rendering Optimization) và xử lý ảnh số, được xây dựng trên nền tảng **C# (.NET 8 Windows Forms)** kết hợp cùng thư viện **OpenCVSharp4**.

---

## ✨ Điểm nổi bật & Tính năng chính

### 1. Thuật toán vẽ khử răng cưa Xiaolin Wu (Wu's Algorithm)
- Tự cài đặt thuật toán **Xiaolin Wu** cho phép vẽ đường thẳng, đường cong Bézier, hình tròn, ellipse và đa giác với độ mịn cao.
- Cơ chế pha trộn màu (**True Alpha Blending**) tính toán cường độ phủ pixel chính xác, loại bỏ hoàn toàn hiện tượng bậc thang (jagged edges).

### 2. Tối ưu hóa kết xuất bộ nhớ (LockBits & BitmapBuffer)
- Thay thế hoàn toàn cơ chế `GetPixel` / `SetPixel` mặc định của GDI+ bằng việc thao tác trực tiếp trên mảng byte thông qua `Bitmap.LockBits`.
- Tăng tốc độ vẽ và xử lý đồ họa từ **10x đến 100x**, mang lại trải nghiệm vẽ mượt mà theo thời gian thực (real-time).

### 3. Hậu xử lý ảnh với OpenCVSharp4
- Tích hợp bộ xử lý ảnh `ImageProcessor` cung cấp 4 chế độ khử răng cưa cho ảnh import:
  - **RemoveAliasing (Cơ bản)**: Xử lý nhanh, cân bằng tốt giữa hiệu năng và chất lượng.
  - **RemoveAliasingAdvanced (Nâng cao)**: Giữ chi tiết sắc nét, khử răng cưa mạnh mẽ.
  - **RemoveAliasingForCurves**: Tối ưu chuyên biệt cho đường viền cong và hình tròn.
  - **RemoveAliasingWithStrength**: Cho phép tinh chỉnh cường độ lọc tùy biến.

### 4. Ứng dụng đồ họa PaintLike
- Quản lý vẽ theo cơ chế đa lớp (**Layer System**) tương tự các phần mềm đồ họa chuyên nghiệp.
- Hỗ trợ đầy đủ bộ công cụ vẽ hình học, cọ vẽ, bút vẽ và bảng điều khiển trực quan.

---

## 🛠️ Công nghệ sử dụng

- **Ngôn ngữ**: C# 12
- **Framework**: .NET 8.0 (Windows Forms)
- **Thư viện chính**:
  - `OpenCvSharp4` (4.13.x) & `OpenCvSharp4.Extensions`
  - `OpenCvSharp4.runtime.win`
  - GDI+ (Graphics, Bitmap, LockBits)

---


## 🚀 Hướng dẫn cài đặt & Chạy dự án

1. **Yêu cầu hệ thống**:
   - Visual Studio 2022 trở lên (.NET 8 SDK).
   - Hệ điều hành Windows 10/11.

2. **Cài đặt & Thực thi**:
   ```bash
   # Clone repository về máy
   git clone https://github.com/suubinn21/Thu-t-to-n-ki-m-kh-r-ng-c-a-.git

   # Mở file solution
   PaintLike.sln
   
## 📚 **Nguồn gốc thuật toán & Tài liệu tham khảo**
- Thuật toán khử răng cưa Xiaolin Wu (Wu's Line Algorithm):

1. **Tác giả phát minh: GS. Xiaolin Wu (1991).**
- Bài báo khoa học: "An Efficient Antialiasing Technique", ACM SIGGRAPH Computer Graphics, Tập 25, Số 4, tr. 143–152.
- Mở rộng cho đường tròn / ellipse: "Fast Anti-Aliased Circle Generation", xuất bản trong cuốn Graphics Gems II (Academic Press, 1991).
2. **Kỹ thuật tối ưu bộ nhớ đồ họa GDI+ (Direct Pixel Manipulation):**

- Sử dụng phương thức System.Drawing.Bitmap.LockBits kết hợp System.Runtime.InteropServices.Marshal.Copy để truy cập trực tiếp bộ nhớ đệm pixel (Pixel Buffer), loại bỏ chi phí context-switch của các hàm đồ họa cấp cao.

3. **Xử lý ảnh số & Lọc làm mịn (Image Filtering):**
Thư viện mã nguồn mở thị giác máy tính OpenCV & wrapper OpenCvSharp4 (Gaussian Blur, Bilateral Filter, Edge-Preserving Filter).
