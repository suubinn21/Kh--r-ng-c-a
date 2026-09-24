
namespace PaintLike
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;
        
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            pictureBox1 = new PictureBox();
            panelTools = new Panel();
            btnExport = new Button();
            btnSave = new Button();
            BttRoundedRectangle = new Button();
            BttBezier = new Button();
            BttEllipse = new Button();
            BttArrows = new Button();
            BttDiamond = new Button();
            BttHexagon = new Button();
            btnSmartSelect = new Button();
            btnRedo = new Button();
            btnUndo = new Button();
            btnTrapezoid = new Button();
            btnStar = new Button();
            nudEraserSize = new NumericUpDown();
            btnTriangle = new Button();
            btnCircle = new Button();
            btnRect = new Button();
            btnLine = new Button();
            btnEraser = new Button();
            btnColor = new Button();
            btnImportImage = new Button();
            btnLayers = new Button();
            trackBarOpacity = new TrackBar();
            lblOpacity = new Label();
            colorDialog1 = new ColorDialog();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            panelTools.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)nudEraserSize).BeginInit();
            ((System.ComponentModel.ISupportInitialize)trackBarOpacity).BeginInit();
            SuspendLayout();
            // 
            // pictureBox1
            // 
            pictureBox1.BackColor = Color.White;
            pictureBox1.Dock = DockStyle.Fill;
            pictureBox1.Location = new Point(0, 110);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(1079, 490);
            pictureBox1.TabIndex = 0;
            pictureBox1.TabStop = false;
            pictureBox1.Paint += pictureBox1_Paint;
            pictureBox1.MouseDown += pictureBox1_MouseDown;
            pictureBox1.MouseMove += pictureBox1_MouseMove;
            pictureBox1.MouseUp += pictureBox1_MouseUp;
            // 
            // panelTools
            // 
            panelTools.BackColor = Color.FromArgb(230, 230, 230);
            panelTools.Controls.Add(btnExport);
            panelTools.Controls.Add(btnSave);
            panelTools.Controls.Add(BttRoundedRectangle);
            panelTools.Controls.Add(BttBezier);
            panelTools.Controls.Add(BttEllipse);
            panelTools.Controls.Add(BttArrows);
            panelTools.Controls.Add(BttDiamond);
            panelTools.Controls.Add(BttHexagon);
            panelTools.Controls.Add(btnSmartSelect);
            panelTools.Controls.Add(btnRedo);
            panelTools.Controls.Add(btnUndo);
            panelTools.Controls.Add(btnTrapezoid);
            panelTools.Controls.Add(btnStar);
            panelTools.Controls.Add(nudEraserSize);
            panelTools.Controls.Add(btnTriangle);
            panelTools.Controls.Add(btnCircle);
            panelTools.Controls.Add(btnRect);
            panelTools.Controls.Add(btnLine);
            panelTools.Controls.Add(btnEraser);
            panelTools.Controls.Add(btnColor);
            panelTools.Controls.Add(btnImportImage);
            panelTools.Controls.Add(btnLayers);
            panelTools.Controls.Add(trackBarOpacity);
            panelTools.Controls.Add(lblOpacity);
            panelTools.Dock = DockStyle.Top;
            panelTools.Location = new Point(0, 0);
            panelTools.Name = "panelTools";
            panelTools.Size = new Size(1079, 110);
            panelTools.TabIndex = 1;
            // 
            // btnExport
            // 
            btnExport.Location = new Point(628, 43);
            btnExport.Name = "btnExport";
            btnExport.Size = new Size(39, 27);
            btnExport.TabIndex = 122;
            btnExport.Text = "📥";
            btnExport.UseVisualStyleBackColor = false;
            btnExport.Click += btnExport_Click;
            // 
            // btnSave
            // 
            btnSave.Location = new Point(673, 12);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(42, 28);
            btnSave.TabIndex = 121;
            btnSave.Text = "💾";
            btnSave.UseVisualStyleBackColor = false;
            btnSave.Click += btnSave_Click;
            // 
            // BttRoundedRectangle
            // 
            BttRoundedRectangle.Location = new Point(305, 60);
            BttRoundedRectangle.Name = "BttRoundedRectangle";
            BttRoundedRectangle.Size = new Size(53, 47);
            BttRoundedRectangle.TabIndex = 120;
            BttRoundedRectangle.Text = "Rounded Rectangle";
            BttRoundedRectangle.UseVisualStyleBackColor = false;
            BttRoundedRectangle.Click += BttRoundedRectangle_Click;
            // 
            // BttBezier
            // 
            BttBezier.Location = new Point(246, 60);
            BttBezier.Name = "BttBezier";
            BttBezier.Size = new Size(53, 47);
            BttBezier.TabIndex = 119;
            BttBezier.Text = "Bezier";
            BttBezier.UseVisualStyleBackColor = false;
            BttBezier.Click += BttBezier_Click;
            // 
            // BttEllipse
            // 
            BttEllipse.Location = new Point(305, 10);
            BttEllipse.Name = "BttEllipse";
            BttEllipse.Size = new Size(53, 44);
            BttEllipse.TabIndex = 118;
            BttEllipse.Text = "ellipse";
            BttEllipse.UseVisualStyleBackColor = false;
            BttEllipse.Click += BttEllipse_Click;
            // 
            // BttArrows
            // 
            BttArrows.Location = new Point(187, 60);
            BttArrows.Name = "BttArrows";
            BttArrows.Size = new Size(53, 47);
            BttArrows.TabIndex = 117;
            BttArrows.Text = "Arrows";
            BttArrows.UseVisualStyleBackColor = false;
            BttArrows.Click += BttArrows_Click;
            // 
            // BttDiamond
            // 
            BttDiamond.Location = new Point(246, 9);
            BttDiamond.Name = "BttDiamond";
            BttDiamond.Size = new Size(53, 47);
            BttDiamond.TabIndex = 116;
            BttDiamond.Text = "Diamond";
            BttDiamond.UseVisualStyleBackColor = false;
            BttDiamond.Click += BttDiamond_Click;
            // 
            // BttHexagon
            // 
            BttHexagon.Location = new Point(128, 60);
            BttHexagon.Name = "BttHexagon";
            BttHexagon.Size = new Size(53, 47);
            BttHexagon.TabIndex = 115;
            BttHexagon.Text = "Hexagon";
            BttHexagon.UseVisualStyleBackColor = false;
            BttHexagon.Click += BttHexagon_Click;
            // 
            // btnSmartSelect
            // 
            btnSmartSelect.Location = new Point(364, 44);
            btnSmartSelect.Name = "btnSmartSelect";
            btnSmartSelect.Size = new Size(56, 26);
            btnSmartSelect.TabIndex = 114;
            btnSmartSelect.Text = "✂️";
            btnSmartSelect.UseVisualStyleBackColor = true;
            btnSmartSelect.Click += btnSmartSelect_Click;
            // 
            // btnRedo
            // 
            btnRedo.Location = new Point(398, 10);
            btnRedo.Name = "btnRedo";
            btnRedo.Size = new Size(26, 28);
            btnRedo.TabIndex = 113;
            btnRedo.Text = "↷";
            btnRedo.UseVisualStyleBackColor = true;
            btnRedo.Click += btnRedo_Click;
            // 
            // btnUndo
            // 
            btnUndo.Location = new Point(364, 10);
            btnUndo.Name = "btnUndo";
            btnUndo.Size = new Size(28, 28);
            btnUndo.TabIndex = 112;
            btnUndo.Text = "↶";
            btnUndo.UseVisualStyleBackColor = true;
            btnUndo.Click += btnUndo_Click;
            // 
            // btnTrapezoid
            // 
            btnTrapezoid.Location = new Point(187, 10);
            btnTrapezoid.Name = "btnTrapezoid";
            btnTrapezoid.Size = new Size(53, 47);
            btnTrapezoid.TabIndex = 107;
            btnTrapezoid.Text = "Hình Thang ";
            btnTrapezoid.Click += btnTrapezoid_Click;
            // 
            // btnStar
            // 
            btnStar.Location = new Point(69, 9);
            btnStar.Name = "btnStar";
            btnStar.Size = new Size(53, 47);
            btnStar.TabIndex = 0;
            btnStar.Text = "☆";
            btnStar.Click += btnStar_Click;
            // 
            // nudEraserSize
            // 
            nudEraserSize.Location = new Point(628, 70);
            nudEraserSize.Maximum = new decimal(new int[] { 300, 0, 0, 0 });
            nudEraserSize.Minimum = new decimal(new int[] { 5, 0, 0, 0 });
            nudEraserSize.Name = "nudEraserSize";
            nudEraserSize.Size = new Size(57, 27);
            nudEraserSize.TabIndex = 100;
            nudEraserSize.Value = new decimal(new int[] { 100, 0, 0, 0 });
            nudEraserSize.ValueChanged += nudEraserSize_ValueChanged;
            // 
            // btnTriangle
            // 
            btnTriangle.Location = new Point(69, 60);
            btnTriangle.Name = "btnTriangle";
            btnTriangle.Size = new Size(53, 47);
            btnTriangle.TabIndex = 101;
            btnTriangle.Text = "Triangle";
            btnTriangle.Click += btnTriangle_Click;
            // 
            // btnCircle
            // 
            btnCircle.Location = new Point(128, 9);
            btnCircle.Name = "btnCircle";
            btnCircle.Size = new Size(53, 47);
            btnCircle.TabIndex = 102;
            btnCircle.Text = "〇";
            btnCircle.Click += btnCircle_Click;
            // 
            // btnRect
            // 
            btnRect.Location = new Point(10, 60);
            btnRect.Name = "btnRect";
            btnRect.Size = new Size(53, 47);
            btnRect.TabIndex = 103;
            btnRect.Text = "Rectangle";
            btnRect.Click += btnRect_Click;
            // 
            // btnLine
            // 
            btnLine.Location = new Point(10, 9);
            btnLine.Name = "btnLine";
            btnLine.Size = new Size(53, 47);
            btnLine.TabIndex = 104;
            btnLine.Text = "〵";
            btnLine.Click += btnLine_Click;
            // 
            // btnEraser
            // 
            btnEraser.Location = new Point(508, 60);
            btnEraser.Name = "btnEraser";
            btnEraser.Size = new Size(114, 44);
            btnEraser.TabIndex = 105;
            btnEraser.Text = "Eraser";
            btnEraser.Click += btnEraser_Click;
            // 
            // btnColor
            // 
            btnColor.Location = new Point(508, 12);
            btnColor.Name = "btnColor";
            btnColor.Size = new Size(114, 44);
            btnColor.TabIndex = 106;
            btnColor.Text = "Color";
            btnColor.Click += btnColor_Click;
            // 
            // btnImportImage
            // 
            btnImportImage.Location = new Point(628, 12);
            btnImportImage.Name = "btnImportImage";
            btnImportImage.Size = new Size(39, 28);
            btnImportImage.TabIndex = 108;
            btnImportImage.Text = "📁 ";
            btnImportImage.Click += btnImportImage_Click;
            // 
            // btnLayers
            // 
            btnLayers.Location = new Point(750, 12);
            btnLayers.Name = "btnLayers";
            btnLayers.Size = new Size(85, 44);
            btnLayers.TabIndex = 111;
            btnLayers.Text = "Layers";
            btnLayers.Click += btnLayers_Click;
            // 
            // trackBarOpacity
            // 
            trackBarOpacity.Location = new Point(832, 33);
            trackBarOpacity.Maximum = 255;
            trackBarOpacity.Name = "trackBarOpacity";
            trackBarOpacity.Size = new Size(150, 56);
            trackBarOpacity.TabIndex = 109;
            trackBarOpacity.Value = 255;
            trackBarOpacity.ValueChanged += trackBarOpacity_ValueChanged;
            // 
            // lblOpacity
            // 
            lblOpacity.AutoSize = true;
            lblOpacity.Location = new Point(842, 10);
            lblOpacity.Name = "lblOpacity";
            lblOpacity.Size = new Size(103, 20);
            lblOpacity.TabIndex = 110;
            lblOpacity.Text = "Opacity: 100%";
            // 
            // Form1
            // 
            ClientSize = new Size(1079, 600);
            Controls.Add(pictureBox1);
            Controls.Add(panelTools);
            Name = "Form1";
            Text = "Paint Like - Improved UI";
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            panelTools.ResumeLayout(false);
            panelTools.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)nudEraserSize).EndInit();
            ((System.ComponentModel.ISupportInitialize)trackBarOpacity).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Panel panelTools;
        private System.Windows.Forms.Button btnLine;
        private System.Windows.Forms.Button btnRect;
        private System.Windows.Forms.Button btnCircle;
        private System.Windows.Forms.Button btnTriangle;
        private System.Windows.Forms.Button btnEraser;
        private System.Windows.Forms.Button btnColor;
        private System.Windows.Forms.Button btnStar;
        private System.Windows.Forms.NumericUpDown nudEraserSize; // khai báo NumericUpDown cho eraser size
        private System.Windows.Forms.ColorDialog colorDialog1;
        private Button btnTrapezoid;
        private System.Windows.Forms.Button btnImportImage; // MỚI: Nút Import Image
        private System.Windows.Forms.Button btnLayers; // MỚI: Nút Layers
        private System.Windows.Forms.TrackBar trackBarOpacity; // MỚI: TrackBar điều chỉnh độ trong suốt
        private System.Windows.Forms.Label lblOpacity; // MỚI: Label hiển thị độ trong suốt
        private Button btnRedo;
        private Button btnUndo;
        private Button btnSmartSelect;
        private Button BttHexagon;
        private Button BttDiamond;
        private Button BttArrows;
        private Button BttEllipse;
        private Button BttBezier;
        private Button BttRoundedRectangle;
        private Button btnExport;
        private Button btnSave;
    }
}