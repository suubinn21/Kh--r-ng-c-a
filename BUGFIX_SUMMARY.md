# PaintLike 0.0.9 - Bug Fix Summary

## Vấn đề được xác định và sửa

### 1. **Zoom Layer Canvas Không Hoạt động Đúng** (BUG FIX)
**Vấn đề:** 
- Khi phóng to/thu nhỏ canvas bằng mouse wheel, layer bitmap không zoom theo tỷ lệ
- Preview hình vẽ bị lệch vị trí khi zoom khác 1.0x

**Nguyên nhân:**
- `GetCanvasPoint()` sử dụng integer division mà không làm tròn, gây mất độ chính xác tọa độ
- `DrawPreview()` không điều chỉnh độ dày pen (pen width) theo zoom scale
- Eraser indicator không điều chỉnh theo zoom

**Cách sửa:**
- Thay đổi `GetCanvasPoint()` để sử dụng `Math.Round()`:
  ```csharp
  return new Point((int)Math.Round(screenPoint.X / zoomScale), (int)Math.Round(screenPoint.Y / zoomScale));
  ```
- Thêm `previewPenWidth = Math.Max(pen.Width / zoomScale, 1f)` vào `DrawPreview()`
- Điều chỉnh độ dày pen và dashPen trong `pictureBox1_Paint()` theo `zoomScale`

**File thay đổi:** `PaintLike/Form1.cs`

---

### 2. **Toggle Invisible Checkbox Chỉ Xóa Một Góc Canvas** (BUG FIX)
**Vấn đề:**
- Khi ấn checkbox invisible để ẩn layer, chỉ một góc bên trái canvas bị xóa thay vì cả layer
- Canvas không render đúng khi toggle visibility

**Nguyên nhân:**
- `LayerPanel_LayersChanged()` được gọi khi visibility thay đổi (event `VisibilityChanged` trong `LayerItem.cs`)
- Nhưng nó không gọi `RecomposeCanvas()` để vẽ lại, chỉ gọi `pictureBox1.Invalidate()`
- `RecomposeCanvas()` không được thực thi khi visibility toggle

**Cách sửa:**
- Cập nhật `LayerPanel_LayersChanged()` để gọi `RecomposeCanvas()`:
  ```csharp
  private void LayerPanel_LayersChanged(object? sender, EventArgs e)
  {
      if (layerPanel != null)
          currentLayerIndex = layerPanel.GetCurrentLayerIndex();
      
      // FIX: Gộp các layer lại để cập nhật canvas khi layer ẩn/hiện
      RecomposeCanvas();
      pictureBox1.Invalidate();
  }
  ```
- Cải thiện `RecomposeCanvas()` thêm try-catch để debug

**File thay đổi:** `PaintLike/Form1.cs`

---

## Tổng kết

### ✅ Fixes đã áp dụng:
1. Coordinate precision khi zoom (sử dụng Math.Round)
2. Pen width scaling trong preview
3. Eraser indicator scaling theo zoom
4. RecomposeCanvas gọi khi visibility toggle
5. SmartSelect rectangle pen width scaling
6. Error handling trong RecomposeCanvas

### 🧪 Cách test:
1. **Test Zoom:**
   - Vẽ một hình trên canvas
   - Sử dụng mouse wheel để zoom in/out
   - Kiểm tra hình vẽ có scale đúng không
   - Preview hình khi kéo chuột phải ghi lại tọa độ đúng

2. **Test Visibility Toggle:**
   - Tạo 2-3 layer
   - Vẽ các hình khác nhau trên mỗi layer
   - Ấn checkbox "Visible" để ẩn/hiện layer
   - Kiểm tra canvas render đúng mà không bị xóa chỉ một góc

3. **Test Combined:**
   - Vẽ hình khi zoom 1.5x
   - Toggle layer invisible
   - Thu nhỏ zoom về 0.5x
   - Kiểm tra tất cả hoạt động đúng

---

## Files Modified
- `PaintLike/Form1.cs`:
  - `GetCanvasPoint()` - Thêm Math.Round()
  - `pictureBox1_Paint()` - Điều chỉnh pen width theo zoom
  - `DrawPreview()` - Thêm previewPenWidth calculation
  - `LayerPanel_LayersChanged()` - Thêm RecomposeCanvas() call
  - `RecomposeCanvas()` - Thêm try-catch

---

**Version:** PaintLike 0.0.9  
**Date:** 2026-04-11  
**Status:** ✅ Build Successful
