# 🎨 Hướng Dẫn Xử Lý Ảnh - PaintLike

## ✨ Các Tính Năng Xử Lý Ảnh Mới

### 1. **Dialog Xử Lý Ảnh Toàn Diện** 
Khi bạn nhấn nút **Import Image**, ứng dụng sẽ hiển thị một dialog cho phép bạn chọn các tùy chọn xử lý ảnh trước khi import vào canvas.

#### **Tab 1: Khử Răng Cưa (Anti-Aliasing)**
Khử răng cưa là quá trình làm mịn các cạnh không đều của ảnh raster.

**Các phương pháp:**
- **Không xử lý** (mặc định) - Giữ nguyên ảnh gốc
- **Khử răng cưa đơn giản** - Sử dụng Bilateral Filter + Morphological Opening
  - Phù hợp: Đối với hầu hết các loại ảnh
  - Tốc độ: Nhanh
  
- **Khử răng cưa nâng cao** - Bilateral Filter + Open + Close + Median Blur
  - Phù hợp: Ảnh có nhiều tiếng ồn hoặc nhiễu
  - Tốc độ: Trung bình
  - Kết quả: Tốt hơn, nhưng mất chi tiết hơn
  
- **Khử răng cưa cho đường cong** - Gaussian Blur + Bilateral + Close
  - Phù hợp: Ảnh có nhiều đường cong hoặc hình tròn
  - Tốc độ: Nhanh
  - Kết quả: Giữ chi tiết tốt hơn

#### **Tab 2: Các Bộ Lọc Khác**
Bạn có thể kombinovat các bộ lọc sau:

**1. Điều chỉnh Tương Phản (Contrast)**
```
Giá trị: 50% - 300% (mặc định 100%)
- 50% = Giảm tương phản (ảnh nhạt hơn)
- 100% = Không thay đổi
- 300% = Tăng tương phản cao (ảnh sậm hơn)
```

**2. Làm Sắc Nét (Sharpen)**
```
Độ mạnh: 1 - 5 (mặc định 1)
- 1 = Nhẹ nhàng
- 5 = Rất mạnh
```

**3. Làm Mờ (Blur)**
```
Độ mạnh: 0 - 10 (mặc định 0 = không blur)
- 0 = Không áp dụng
- 5 = Làm mờ vừa phải
- 10 = Rất mờ
```

**4. Điều chỉnh Độ Sáng (Brightness)**
```
Giá trị: -100 - 100 (mặc định 0)
- Âm = Tối hơn
- 0 = Không thay đổi
- Dương = Sáng hơn
```

---

## 🚀 Cách Sử Dụng

### **Bước 1: Mở File Ảnh**
1. Nhấn nút **📁 Import** ở thanh công cụ
2. Chọn file ảnh (PNG, JPG, BMP, GIF)

### **Bước 2: Chọn Tùy Chọn Xử Lý**
Dialog sẽ xuất hiện với 2 tab:
- **Tab 1 (Khử Răng Cưa)**: Chọn phương pháp khử răng cưa
- **Tab 2 (Bộ Lọc)**: Tích chọn các bộ lọc bổ sung + điều chỉnh giá trị

### **Bước 3: Áp Dụng**
- Nhấn **"Áp Dụng"** để áp dụng các xử lý đã chọn
- Nhấn **"Hủy"** để hủy import

### **Bước 4: Kết Quả**
- Ảnh sẽ được thêm vào một layer mới
- Layer này sẽ hiển thị ở trên cùng trong bảng Layers
- Bạn có thể tiếp tục vẽ thêm hoặc chỉnh sửa

---

## 💡 Các Ví Dụ Thực Tế

### **Ví Dụ 1: Ảnh Chụp Từ Camera (Có Nhiễu)**
```
Khử Răng Cưa: Khử răng cưa nâng cao ✓
Tương Phản: 120% (tăng một chút)
Làm Sắc Nét: Có (độ mạnh 2)
Brightness: 0 (giữ nguyên)
```

### **Ví Dụ 2: Ảnh Vẽ Tay (Đường Khó)**
```
Khử Răng Cưa: Khử răng cưa cho đường cong ✓
Tương Phản: 100% (không đổi)
Làm Sắc Nét: Có (độ mạnh 1)
Blur: 0 (không blur)
```

### **Ví Dụ 3: Ảnh Tối, Cần Làm Sáng**
```
Khử Răng Cưa: Không xử lý
Brightness: +30 (sáng hơn)
Tương Phản: 110%
```

### **Ví Dụ 4: Ảnh Sáng Quá, Cần Làm Tối**
```
Khử Răng Cưa: Khử răng cưa đơn giản ✓
Brightness: -20 (tối hơn)
Tương Phản: 90%
```

---

## ⚙️ Các Phương Pháp Xử Lý Chi Tiết

### **1. Bilateral Filter**
```
Tác dụng: Làm mìn ảnh nhưng giữ lại cạnh sắc nét
Công dụng: Khử nhiễu mà không làm mất chi tiết
```

### **2. Morphological Operations**
```
- Opening: Loại bỏ các vật thể nhỏ (nhiễu)
- Closing: Lấp đầy các lỗ nhỏ
- Close: Kết hợp cả hai
```

### **3. Median Blur**
```
Tác dụng: Loại bỏ các điểm ngoại lệ
Công dụng: Giảm salt-and-pepper noise
```

### **4. Gaussian Blur**
```
Tác dụng: Làm mịn kích thước lớn
Công dụng: Giảm chi tiết nhưng giữ hình dáng chính
```

---

## 🎯 Mẹo & Thủ Thuật

1. **Kết hợp nhiều bộ lọc**
   - Khử răng cưa + Sharpen = Làm sắc nét cạnh mịn
   - Khử răng cưa + Blur = Làm mịn thêm nếu cần

2. **Điều chỉnh Tương Phản trước Độ Sáng**
   - Tương phản trước làm ảnh chi tiết hơn
   - Độ sáng sau để điều chỉnh độ sáng cuối cùng

3. **Thử từng bộ lọc**
   - Bắt đầu với khử răng cưa đơn giản nếu bạn không chắc
   - Nếu cần chất lượng cao, thử khử răng cưa nâng cao

4. **Lưu Undo nếu không hài lòng**
   - Bạn có thể import lại ảnh với các tùy chọn khác

---

## 🔧 Có Vấn Đề?

### **Ảnh bị mờ quá mức**
→ Giảm độ mạnh Blur hoặc không dùng Blur

### **Ảnh không rõ ràng**
→ Tăng Tương Phản hoặc thêm Sharpen

### **Ảnh quá sáng/tối**
→ Điều chỉnh Brightness hoặc Tương Phản

### **Ảnh vẫn có nhiễu**
→ Sử dụng "Khử răng cưa nâng cao" thay vì "đơn giản"

---

## 📝 Ghi Chú Kỹ Thuật

- Tất cả xử lý ảnh sử dụng **OpenCV Sharp** (OpenCvSharp)
- Ảnh được xử lý **trước khi** thêm vào layer
- Các layer có thể có độ trong suốt (Opacity) riêng
- Xử lý được áp dụng **một lần** khi import, không ảnh hưởng hiệu suất sau đó

---

## 📚 Thêm Thông Tin

Xem file `ANTIALIASING_GUIDE.md` và `ANTIALIASING_README.md` để biết thêm chi tiết về khử răng cưa.
