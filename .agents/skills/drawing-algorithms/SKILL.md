---
name: drawing-algorithms
description: >-
  Hướng dẫn triển khai và tối ưu các thuật toán vẽ trong PaintLike:
  Wu anti-aliased line drawing, true alpha blending, per-pixel transparency,
  và refactor LockBits pattern. Kích hoạt khi user yêu cầu sửa/nâng cấp
  WuLineDrawer, BitmapBuffer, alpha blending, anti-aliasing, hoặc pixel ops.
---

# Drawing Algorithms — PaintLike Skill

Skill này chứa kiến thức chuyên sâu về các thuật toán đồ hoạ đang dùng trong
[`WuLineDrawer.cs`](../../PaintLike/WuLineDrawer.cs) và
[`BitmapBuffer.cs`](../../PaintLike/BitmapBuffer.cs).

---

## 1. Wu's Anti-Aliased Line Algorithm

### Nguyên lý
Wu line vẽ mỗi pixel với **hai điểm** (trên và dưới đường thẳng lý tưởng),
cường độ alpha tỷ lệ nghịch với khoảng cách fractional:

```
intensity(upper) = 1 - frac(y)
intensity(lower) = frac(y)
```

### Vấn đề hiện tại (WuLineDrawer.cs)
- `DrawThickLine`, `DrawThickPolygon`, `DrawThickCircle` gọi hàm 1px trong vòng lặp
- Mỗi lần gọi → 1 lần `BitmapBuffer` constructor → `LockBits` + `UnlockBits`
- Vẽ nét dày 15px = **lock/unlock 15 lần** liên tiếp

### Fix: Refactor nhận BitmapBuffer làm tham số

```csharp
// ❌ TRƯỚC — lock lặp lại
public static void DrawThickLine(Bitmap bmp, Point p1, Point p2, Color color, float thickness)
{
    for (int i = -(int)(thickness/2); i <= (int)(thickness/2); i++)
    {
        DrawLine(bmp, offset_p1, offset_p2, color); // mỗi lần lock/unlock
    }
}

// ✅ SAU — lock 1 lần duy nhất
public static void DrawThickLine(Bitmap bmp, Point p1, Point p2, Color color, float thickness)
{
    using var buf = new BitmapBuffer(bmp); // lock 1 lần
    for (int i = -(int)(thickness/2); i <= (int)(thickness/2); i++)
    {
        DrawLineOnBuffer(buf, offset_p1, offset_p2, color); // dùng buffer trực tiếp
    }
} // unlock khi ra khỏi using

// Hàm internal nhận BitmapBuffer thay vì Bitmap
private static void DrawLineOnBuffer(BitmapBuffer buf, Point p1, Point p2, Color color)
{
    // ... Wu algorithm trực tiếp trên buf.SetPixel / buf.Blend
}
```

---

## 2. True Alpha Blending trong BitmapBuffer.Blend()

### Vấn đề hiện tại (BitmapBuffer.cs ~dòng 126-138)
`Blend()` hardcode alpha = 255 → vẽ trên nền trong suốt tạo viền đặc (halo).

```csharp
// ❌ HIỆN TẠI — alpha bị mất
return Color.FromArgb(255, r, g, b);
```

### Fix: Porter-Duff "Source Over" compositing

```csharp
// ✅ FIX — true alpha blending (Porter-Duff Source Over)
public Color Blend(Color src, Color dst, float alpha)
{
    // alpha = cường độ anti-alias (0.0 → 1.0)
    int srcA = (int)(src.A * alpha);   // alpha nguồn sau scale
    int dstA = dst.A;

    // Porter-Duff Source Over
    int outA = srcA + dstA * (255 - srcA) / 255;
    if (outA == 0) return Color.Transparent;

    int outR = (src.R * srcA + dst.R * dstA * (255 - srcA) / 255) / outA;
    int outG = (src.G * srcA + dst.G * dstA * (255 - srcA) / 255) / outA;
    int outB = (src.B * srcA + dst.B * dstA * (255 - srcA) / 255) / outA;

    return Color.FromArgb(
        Math.Clamp(outA, 0, 255),
        Math.Clamp(outR, 0, 255),
        Math.Clamp(outG, 0, 255),
        Math.Clamp(outB, 0, 255)
    );
}
```

### Kết quả
| Trước | Sau |
|-------|-----|
| Viền trắng/đen đặc quanh nét vẽ | Nét mờ dần tự nhiên trên mọi nền |
| AA không hoạt động trên nền trong suốt | AA đúng trên cả ARGB layer |

---

## 3. Pre-multiplied Alpha vs Straight Alpha

PaintLike dùng **straight alpha** (GDI+ default). Khi blend nhiều lần:

```
// Straight alpha — dùng khi làm việc với GDI+ Bitmap trực tiếp
outColor = src * srcAlpha + dst * (1 - srcAlpha)

// Pre-multiplied — dùng nếu chuyển sang Direct2D/WPF sau này
// Cần convert: premul.R = straight.R * straight.A / 255
```

> [!IMPORTANT]
> Giữ nguyên **straight alpha** cho BitmapBuffer để tương thích GDI+.
> Chỉ chuyển pre-multiplied nếu migrate sang Direct2D.

---

## 4. LockBits Pattern Chuẩn

```csharp
// Pattern chuẩn cho pixel-level access trong WuLineDrawer / BitmapBuffer
public sealed class BitmapBuffer : IDisposable
{
    private readonly Bitmap _bmp;
    private BitmapData _data;
    private bool _locked;

    public BitmapBuffer(Bitmap bmp)
    {
        _bmp = bmp;
        _data = bmp.LockBits(
            new Rectangle(0, 0, bmp.Width, bmp.Height),
            ImageLockMode.ReadWrite,
            PixelFormat.Format32bppArgb);
        _locked = true;
    }

    public unsafe void SetPixel(int x, int y, Color c)
    {
        if (x < 0 || y < 0 || x >= _bmp.Width || y >= _bmp.Height) return;
        int* ptr = (int*)(_data.Scan0 + y * _data.Stride + x * 4);
        *ptr = c.ToArgb();
    }

    public unsafe Color GetPixel(int x, int y)
    {
        if (x < 0 || y < 0 || x >= _bmp.Width || y >= _bmp.Height)
            return Color.Transparent;
        int* ptr = (int*)(_data.Scan0 + y * _data.Stride + x * 4);
        return Color.FromArgb(*ptr);
    }

    public void Dispose()
    {
        if (_locked)
        {
            _bmp.UnlockBits(_data);
            _locked = false;
        }
    }
}
```

> [!TIP]
> Dùng `unsafe` + pointer trực tiếp nhanh hơn `Marshal.ReadInt32` khoảng 2-3x.
> Thêm `<AllowUnsafeBlocks>true</AllowUnsafeBlocks>` vào `.csproj`.

---

## 5. Quy trình tối ưu WuLineDrawer

Thứ tự thực hiện:

1. **Refactor internal helpers** — tạo overload `DrawWuLine(BitmapBuffer buf, ...)` nhận buffer thay Bitmap
2. **Wrap public API** — public methods tạo `BitmapBuffer` 1 lần rồi gọi internal
3. **Fix Blend()** — apply Porter-Duff alpha (mục 2)
4. **Test** — vẽ đường dày 20px trên layer trong suốt → không có halo
5. **Benchmark** — so sánh time trước/sau với `Stopwatch`

---

## 6. References

- [Wu, X. (1991). An efficient antialiasing technique. SIGGRAPH](https://en.wikipedia.org/wiki/Xiaolin_Wu%27s_line_algorithm)
- [Porter, T. & Duff, T. (1984). Compositing digital images](https://en.wikipedia.org/wiki/Alpha_compositing)
- [MSDN: LockBits documentation](https://learn.microsoft.com/en-us/dotnet/api/system.drawing.bitmap.lockbits)
