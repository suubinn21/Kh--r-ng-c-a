# TODO - Kế Hoạch Sửa Lỗi PaintLike (Bug Fix Plan)

## ✅ HOÀN THÀNH (7/12)
## ⏳ ĐANG LÀM (0/12) 
## 🔄 CHƯA LÀM (5/12)

### GIAI ĐOẠN 1: SỬA LỖI NGUY HÍEM (CRITICAL FIXES) ✅ **COMPLETE**
✅ 1.1-1.6: Empty catches → MessageBox logging, null checks, index bounds, dispose importedImage

### GIAI ĐOẠN 2: AN TOÀN BỘ NHỚ (MEMORY SAFETY) **66%**
✅ 2.1 Layer implements IDisposable (Dispose pattern + finalizer)

✅ **2.2** Form1_Resize(): using{} Graphics + oldCanvas.Dispose()

✅ **2.3** Undo/Redo: layer.Dispose() in SaveState/ClearStack/Redo/Undo

### GIAI ĐOẠN 3: TỐI ƯU HIỆU SUẤT (PERF) **0%**
- [ ] 3.1 ImageProcessor pixel loops → OpenCV Mat
- [ ] 3.2 WuLineDrawer LockBits perf upgrade
- [ ] 3.3 Cache opacity ColorMatrix Paint event

### GIAI ĐOẠN 4: HOÀN THIỆN (POLISH) **0%**
- [ ] 4.1 Magic number consts
- [ ] 4.2 Remove duplicate GetRect/GetRectangle
- [ ] 4.3 Final RAM/CPU stress test

**Tiến độ tổng**: ~58% | **Compile**: PASS ✅ | **Est time left**: 1-2 giờ  
**Next**: 2.3 Undo/Redo memory + Phase 3 perf!

**Test cmd**: `dotnet run --project PaintLike/PaintLike.csproj` → Import 5 ảnh, undo/resize → No crashes/leaks!


