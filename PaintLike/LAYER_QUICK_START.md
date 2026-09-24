# Layer Control Panel - Quick Start Guide

## What's New?

Your Paint application now has a professional **Layer Control Panel** located at the bottom-right of the screen. This allows you to work with multiple transparent layers, just like Photoshop or GIMP!

---

## Getting Started

### 1. Launch the Application
When you start the app, you'll see:
- The layer panel in the bottom-right corner
- One default layer named "Background" containing your current canvas

### 2. Understanding the Layer Panel

```
┌─────────────────────────────────────┐
│            LAYERS                   │
├─────────────────────────────────────┤
│ [✓] Background  Op: 255             │
│                                     │
├─────────────────────────────────────┤
│ [Add Layer] [Delete] [Up ↑] [Down ↓]│
│ [Rename]   [Opacity] [Duplicate]    │
└─────────────────────────────────────┘
```

- **Checkbox (✓)** - Toggle layer visibility on/off
- **Layer Name** - Click to select/edit layer
- **Op: 255** - Current opacity (0=transparent, 255=opaque)

---

## Basic Operations

### Create a New Layer
1. Click **"Add Layer"** button
2. A new transparent layer appears at the top
3. It automatically becomes the active (selected) layer
4. Start drawing on this new layer!

**Result:**
- New layer floats on top of existing layers
- You can switch between layers by clicking their names
- Drawing only affects the currently selected layer

### Switch Between Layers
1. Click on any layer name in the list
2. It highlights with a blue background (selected)
3. All drawing operations now target this layer
4. Use the coordinates display to see where you're drawing

### Delete a Layer
1. Click on the layer you want to delete
2. Click **"Delete"** button
3. Layer is removed (can't undo!)

**Important:** You must keep at least 1 layer (background required)

### Change Layer Order (Z-Order)
Use **"Up ↑"** and **"Down ↓"** buttons to reorder layers:

**Before:**
```
[✓] Sketch      ← Currently selected
[✓] Colors
[✓] Background
```

**After clicking "Down ↓":**
```
[✓] Colors      ← Moved down
[✓] Sketch      ← Now below Colors
[✓] Background
```

This changes which layer appears on top in the final image.

---

## Advanced Features

### Adjust Layer Opacity
1. Click on a layer to select it
2. Click **"Opacity"** button
3. A slider appears (0 = invisible, 255 = fully opaque)
4. Drag the slider to adjust transparency
5. Click OK to apply

**Effect:** Makes the layer semi-transparent, allowing layers below to show through.

### Rename a Layer
1. Select a layer
2. Click **"Rename"** button
3. Type new name (e.g., "Shadow", "Highlights", "Final Details")
4. Click OK

**Tip:** Use descriptive names to organize your artwork!

### Duplicate a Layer
1. Select a layer to copy
2. Click **"Duplicate"** button
3. A copy appears above the original with " Copy" suffix

**Uses:**
- Create backups before experimenting
- Build variations of the same element
- Test different colors/effects

---

## Drawing on Layers

### Regular Drawing Tools (Line, Shapes)
1. Select desired tool from toolbar
2. Select the layer you want to draw on
3. Draw as normal (appears only on that layer)
4. Drawing is visible through semi-transparent layers below

### Eraser on Layers
1. Select **Eraser** tool
2. Select the layer you want to erase
3. Drag to erase (creates transparency, not white!)
4. Underlying layers show through erased areas

**Key Difference:** Unlike single-layer mode, eraser now creates true transparency, not white.

### Using Opacity While Drawing
1. Adjust the **"Opacity"** slider at top (controls pen opacity)
2. Draw on layers - strokes will be semi-transparent
3. This is DIFFERENT from layer opacity (this is stroke opacity)

---

## Workflow Examples

### Example 1: Colored Sketch
```
Step 1: Create "Sketch" layer → Draw initial outline with Line tool
Step 2: Create "Colors" layer → Below Sketch, use Rectangle/Circle for shapes
Step 3: Set Sketch opacity to 50% → Makes pencil marks semi-transparent
Step 4: Create "Details" layer → On top, add fine details
Result: Professional multi-layer artwork!
```

### Example 2: Compare Versions
```
Step 1: Create "Version A" layer → Draw design A
Step 2: Duplicate → Get "Version A Copy"
Step 3: Rename duplicate to "Version B" → Modify this version
Step 4: Toggle visibility (checkbox) → Compare which version looks better!
```

### Example 3: Dark Mode Effects
```
Step 1: Create "Shadow" layer → Draw dark shapes
Step 2: Set Shadow opacity to 30% → Semi-transparent shadow
Step 3: Move Shadow below other layers → Shadows appear behind content
Result: Realistic shadow effects!
```

---

## Tips & Tricks

✅ **Save Often** - Export your artwork regularly to avoid data loss

✅ **Name Your Layers** - Use clear names like "Background", "Shadows", "Highlights"

✅ **Group Related Content** - Keep similar elements on same layer for easy editing

✅ **Use Opacity Strategically** - Combine semi-transparent layers for depth

✅ **Test Before Committing** - Duplicate layers before major changes, delete copy if unhappy

✅ **Keep Layer Count Reasonable** - Too many layers slows down the app (stay under 20)

---

## Troubleshooting

### My drawing doesn't appear!
- ✓ Check that layer is **visible** (checkbox should be checked ✓)
- ✓ Check that you're drawing on the **correct layer** (layer should be highlighted blue)
- ✓ Check **layer opacity** - if set to 0, layer is completely invisible

### Eraser makes things white, not transparent!
- This happens if drawing was on an older layer created before transparency support
- **Solution:** Duplicate current layer (copy will have transparency support)

### Application is slow!
- Close unnecessary programs to free memory
- Reduce number of layers (delete unused ones)
- Each layer uses memory (~1.9 MB for 800×600 image)

### I accidentally deleted a layer!
- **Undo** doesn't work for layer deletion (limitation of current version)
- **Solution:** Reload the saved file without saving this session

---

## Keyboard Shortcuts (Future Enhancement Ideas)

These aren't implemented yet, but could be added:
- **Ctrl+Shift+N** - New Layer
- **Ctrl+Shift+D** - Duplicate Layer
- **Tab** - Toggle Layer Panel visibility
- **Ctrl+[** - Select Previous Layer
- **Ctrl+]** - Select Next Layer
- **Ctrl+H** - Toggle Layer Visibility

---

## What You Can Now Do!

✨ **Multi-Layer Artwork**
- Organize complex drawings into logical layers
- Adjust each layer independently

✨ **Non-Destructive Editing**
- Eraser creates transparency, not destruction
- Edit layers without affecting others

✨ **Professional Workflow**
- Sketch → Color → Details on separate layers
- Toggle visibility to compare

✨ **Creative Effects**
- Layer transparency creates depth and shadows
- Duplicate layers for variations

---

## Need Help?

Refer to **LAYER_SYSTEM_DOCUMENTATION.md** for technical details about:
- How layers are composited (combined)
- Architecture of the layer system
- Implementation details

---

**Happy Painting with Layers! 🎨**
