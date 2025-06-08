
namespace NES
{
    partial class FormPPUViewer
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.PatternTable1 = new System.Windows.Forms.PictureBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.Nametable = new System.Windows.Forms.PictureBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.labelMirroringType = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label13 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.PaletteSprite3 = new System.Windows.Forms.PictureBox();
            this.PaletteSprite2 = new System.Windows.Forms.PictureBox();
            this.PaletteTile3 = new System.Windows.Forms.PictureBox();
            this.PaletteTile2 = new System.Windows.Forms.PictureBox();
            this.PaletteSprite1 = new System.Windows.Forms.PictureBox();
            this.PaletteSprite0 = new System.Windows.Forms.PictureBox();
            this.PaletteTile1 = new System.Windows.Forms.PictureBox();
            this.PaletteTile0 = new System.Windows.Forms.PictureBox();
            this.checkBoxShowPpuScrollOverlay = new System.Windows.Forms.CheckBox();
            this.checkBoxShowTileGrid = new System.Windows.Forms.CheckBox();
            this.checkBoxShowAttributeGrid = new System.Windows.Forms.CheckBox();
            this.label5 = new System.Windows.Forms.Label();
            this.ScrollXBox = new System.Windows.Forms.Label();
            this.ScrollYBox = new System.Windows.Forms.Label();
            this.PatternTable0 = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.PatternTable1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Nametable)).BeginInit();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PaletteSprite3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.PaletteSprite2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.PaletteTile3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.PaletteTile2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.PaletteSprite1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.PaletteSprite0)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.PaletteTile1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.PaletteTile0)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.PatternTable0)).BeginInit();
            this.SuspendLayout();
            // 
            // PatternTable1
            // 
            this.PatternTable1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.PatternTable1.Location = new System.Drawing.Point(1048, 32);
            this.PatternTable1.Name = "PatternTable1";
            this.PatternTable1.Size = new System.Drawing.Size(256, 256);
            this.PatternTable1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.PatternTable1.TabIndex = 1;
            this.PatternTable1.TabStop = false;
            this.PatternTable1.Paint += new System.Windows.Forms.PaintEventHandler(this.PictureBoxPaint);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(786, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(106, 20);
            this.label1.TabIndex = 2;
            this.label1.Text = "Pattern Table 0";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(1048, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(106, 20);
            this.label2.TabIndex = 3;
            this.label2.Text = "Pattern Table 1";
            // 
            // Nametable
            // 
            this.Nametable.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Nametable.Location = new System.Drawing.Point(12, 32);
            this.Nametable.Name = "Nametable";
            this.Nametable.Size = new System.Drawing.Size(768, 720);
            this.Nametable.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.Nametable.TabIndex = 4;
            this.Nametable.TabStop = false;
            this.Nametable.Paint += new System.Windows.Forms.PaintEventHandler(this.PictureBoxPaint);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 9);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(89, 20);
            this.label3.TabIndex = 5;
            this.label3.Text = "Nametables";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 760);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(109, 20);
            this.label4.TabIndex = 6;
            this.label4.Text = "Mirroring Type:";
            // 
            // labelMirroringType
            // 
            this.labelMirroringType.AutoSize = true;
            this.labelMirroringType.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.labelMirroringType.Location = new System.Drawing.Point(127, 760);
            this.labelMirroringType.Name = "labelMirroringType";
            this.labelMirroringType.Size = new System.Drawing.Size(82, 20);
            this.labelMirroringType.TabIndex = 7;
            this.labelMirroringType.Text = "Horizontal";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label13);
            this.groupBox1.Controls.Add(this.label12);
            this.groupBox1.Controls.Add(this.label11);
            this.groupBox1.Controls.Add(this.label10);
            this.groupBox1.Controls.Add(this.label9);
            this.groupBox1.Controls.Add(this.label8);
            this.groupBox1.Controls.Add(this.label7);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.PaletteSprite3);
            this.groupBox1.Controls.Add(this.PaletteSprite2);
            this.groupBox1.Controls.Add(this.PaletteTile3);
            this.groupBox1.Controls.Add(this.PaletteTile2);
            this.groupBox1.Controls.Add(this.PaletteSprite1);
            this.groupBox1.Controls.Add(this.PaletteSprite0);
            this.groupBox1.Controls.Add(this.PaletteTile1);
            this.groupBox1.Controls.Add(this.PaletteTile0);
            this.groupBox1.Location = new System.Drawing.Point(786, 294);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(518, 271);
            this.groupBox1.TabIndex = 8;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Palette";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(262, 197);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(60, 20);
            this.label13.TabIndex = 15;
            this.label13.Text = "Sprite 3";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(262, 139);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(60, 20);
            this.label12.TabIndex = 14;
            this.label12.Text = "Sprite 2";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(262, 81);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(60, 20);
            this.label11.TabIndex = 13;
            this.label11.Text = "Sprite 1";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(262, 23);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(60, 20);
            this.label10.TabIndex = 9;
            this.label10.Text = "Sprite 0";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(6, 197);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(45, 20);
            this.label9.TabIndex = 8;
            this.label9.Text = "Tile 3";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(6, 139);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(45, 20);
            this.label8.TabIndex = 7;
            this.label8.Text = "Tile 2";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(6, 81);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(45, 20);
            this.label7.TabIndex = 2;
            this.label7.Text = "Tile 1";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(6, 23);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(45, 20);
            this.label6.TabIndex = 0;
            this.label6.Text = "Tile 0";
            // 
            // PaletteSprite3
            // 
            this.PaletteSprite3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.PaletteSprite3.Location = new System.Drawing.Point(262, 220);
            this.PaletteSprite3.Name = "PaletteSprite3";
            this.PaletteSprite3.Size = new System.Drawing.Size(128, 32);
            this.PaletteSprite3.TabIndex = 12;
            this.PaletteSprite3.TabStop = false;
            this.PaletteSprite3.Paint += new System.Windows.Forms.PaintEventHandler(this.PictureBoxPaint);
            // 
            // PaletteSprite2
            // 
            this.PaletteSprite2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.PaletteSprite2.Location = new System.Drawing.Point(262, 162);
            this.PaletteSprite2.Name = "PaletteSprite2";
            this.PaletteSprite2.Size = new System.Drawing.Size(128, 32);
            this.PaletteSprite2.TabIndex = 11;
            this.PaletteSprite2.TabStop = false;
            this.PaletteSprite2.Paint += new System.Windows.Forms.PaintEventHandler(this.PictureBoxPaint);
            // 
            // PaletteTile3
            // 
            this.PaletteTile3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.PaletteTile3.Location = new System.Drawing.Point(6, 220);
            this.PaletteTile3.Name = "PaletteTile3";
            this.PaletteTile3.Size = new System.Drawing.Size(128, 32);
            this.PaletteTile3.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.PaletteTile3.TabIndex = 10;
            this.PaletteTile3.TabStop = false;
            this.PaletteTile3.Paint += new System.Windows.Forms.PaintEventHandler(this.PictureBoxPaint);
            // 
            // PaletteTile2
            // 
            this.PaletteTile2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.PaletteTile2.Location = new System.Drawing.Point(6, 162);
            this.PaletteTile2.Name = "PaletteTile2";
            this.PaletteTile2.Size = new System.Drawing.Size(128, 32);
            this.PaletteTile2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.PaletteTile2.TabIndex = 6;
            this.PaletteTile2.TabStop = false;
            this.PaletteTile2.Paint += new System.Windows.Forms.PaintEventHandler(this.PictureBoxPaint);
            // 
            // PaletteSprite1
            // 
            this.PaletteSprite1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.PaletteSprite1.Location = new System.Drawing.Point(262, 104);
            this.PaletteSprite1.Name = "PaletteSprite1";
            this.PaletteSprite1.Size = new System.Drawing.Size(128, 32);
            this.PaletteSprite1.TabIndex = 5;
            this.PaletteSprite1.TabStop = false;
            this.PaletteSprite1.Paint += new System.Windows.Forms.PaintEventHandler(this.PictureBoxPaint);
            // 
            // PaletteSprite0
            // 
            this.PaletteSprite0.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.PaletteSprite0.Location = new System.Drawing.Point(262, 46);
            this.PaletteSprite0.Name = "PaletteSprite0";
            this.PaletteSprite0.Size = new System.Drawing.Size(128, 32);
            this.PaletteSprite0.TabIndex = 4;
            this.PaletteSprite0.TabStop = false;
            this.PaletteSprite0.Paint += new System.Windows.Forms.PaintEventHandler(this.PictureBoxPaint);
            // 
            // PaletteTile1
            // 
            this.PaletteTile1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.PaletteTile1.Location = new System.Drawing.Point(6, 104);
            this.PaletteTile1.Name = "PaletteTile1";
            this.PaletteTile1.Size = new System.Drawing.Size(128, 32);
            this.PaletteTile1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.PaletteTile1.TabIndex = 3;
            this.PaletteTile1.TabStop = false;
            this.PaletteTile1.Paint += new System.Windows.Forms.PaintEventHandler(this.PictureBoxPaint);
            // 
            // PaletteTile0
            // 
            this.PaletteTile0.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.PaletteTile0.Location = new System.Drawing.Point(6, 46);
            this.PaletteTile0.Name = "PaletteTile0";
            this.PaletteTile0.Size = new System.Drawing.Size(128, 32);
            this.PaletteTile0.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.PaletteTile0.TabIndex = 1;
            this.PaletteTile0.TabStop = false;
            this.PaletteTile0.Paint += new System.Windows.Forms.PaintEventHandler(this.PictureBoxPaint);
            // 
            // checkBoxShowPpuScrollOverlay
            // 
            this.checkBoxShowPpuScrollOverlay.AutoSize = true;
            this.checkBoxShowPpuScrollOverlay.Location = new System.Drawing.Point(786, 572);
            this.checkBoxShowPpuScrollOverlay.Name = "checkBoxShowPpuScrollOverlay";
            this.checkBoxShowPpuScrollOverlay.Size = new System.Drawing.Size(192, 24);
            this.checkBoxShowPpuScrollOverlay.TabIndex = 9;
            this.checkBoxShowPpuScrollOverlay.Text = "Show PPU Scroll Overlay";
            this.checkBoxShowPpuScrollOverlay.UseVisualStyleBackColor = true;
            this.checkBoxShowPpuScrollOverlay.CheckedChanged += new System.EventHandler(this.checkBoxShowPpuScrollOverlay_CheckedChanged);
            // 
            // checkBoxShowTileGrid
            // 
            this.checkBoxShowTileGrid.AutoSize = true;
            this.checkBoxShowTileGrid.Location = new System.Drawing.Point(786, 603);
            this.checkBoxShowTileGrid.Name = "checkBoxShowTileGrid";
            this.checkBoxShowTileGrid.Size = new System.Drawing.Size(127, 24);
            this.checkBoxShowTileGrid.TabIndex = 10;
            this.checkBoxShowTileGrid.Text = "Show Tile Grid";
            this.checkBoxShowTileGrid.UseVisualStyleBackColor = true;
            this.checkBoxShowTileGrid.CheckedChanged += new System.EventHandler(this.checkBoxShowTileGrid_CheckedChanged);
            // 
            // checkBoxShowAttributeGrid
            // 
            this.checkBoxShowAttributeGrid.AutoSize = true;
            this.checkBoxShowAttributeGrid.Location = new System.Drawing.Point(786, 634);
            this.checkBoxShowAttributeGrid.Name = "checkBoxShowAttributeGrid";
            this.checkBoxShowAttributeGrid.Size = new System.Drawing.Size(162, 24);
            this.checkBoxShowAttributeGrid.TabIndex = 11;
            this.checkBoxShowAttributeGrid.Text = "Show Attribute Grid";
            this.checkBoxShowAttributeGrid.UseVisualStyleBackColor = true;
            this.checkBoxShowAttributeGrid.CheckedChanged += new System.EventHandler(this.checkBoxShowAttributeGrid_CheckedChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(263, 760);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(87, 20);
            this.label5.TabIndex = 12;
            this.label5.Text = "Scroll (X, Y):";
            // 
            // ScrollXBox
            // 
            this.ScrollXBox.AutoSize = true;
            this.ScrollXBox.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.ScrollXBox.Location = new System.Drawing.Point(365, 760);
            this.ScrollXBox.Name = "ScrollXBox";
            this.ScrollXBox.Size = new System.Drawing.Size(18, 20);
            this.ScrollXBox.TabIndex = 13;
            this.ScrollXBox.Text = "0";
            // 
            // ScrollYBox
            // 
            this.ScrollYBox.AutoSize = true;
            this.ScrollYBox.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.ScrollYBox.Location = new System.Drawing.Point(404, 760);
            this.ScrollYBox.Name = "ScrollYBox";
            this.ScrollYBox.Size = new System.Drawing.Size(18, 20);
            this.ScrollYBox.TabIndex = 14;
            this.ScrollYBox.Text = "0";
            // 
            // PatternTable0
            // 
            this.PatternTable0.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.PatternTable0.Location = new System.Drawing.Point(786, 32);
            this.PatternTable0.Name = "PatternTable0";
            this.PatternTable0.Size = new System.Drawing.Size(256, 256);
            this.PatternTable0.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.PatternTable0.TabIndex = 0;
            this.PatternTable0.TabStop = false;
            this.PatternTable0.Paint += new System.Windows.Forms.PaintEventHandler(this.PictureBoxPaint);
            // 
            // FormPPUViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1315, 789);
            this.Controls.Add(this.Nametable);
            this.Controls.Add(this.PatternTable0);
            this.Controls.Add(this.PatternTable1);
            this.Controls.Add(this.ScrollYBox);
            this.Controls.Add(this.ScrollXBox);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.checkBoxShowAttributeGrid);
            this.Controls.Add(this.checkBoxShowTileGrid);
            this.Controls.Add(this.checkBoxShowPpuScrollOverlay);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.labelMirroringType);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Name = "FormPPUViewer";
            this.Text = "PPU Viewer";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormPPUViewer_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.PatternTable1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.Nametable)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PaletteSprite3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.PaletteSprite2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.PaletteTile3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.PaletteTile2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.PaletteSprite1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.PaletteSprite0)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.PaletteTile1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.PaletteTile0)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.PatternTable0)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        public System.Windows.Forms.PictureBox PatternTable0;
        public System.Windows.Forms.PictureBox PatternTable1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        public System.Windows.Forms.PictureBox Nametable;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        public System.Windows.Forms.Label labelMirroringType;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label11;
        public System.Windows.Forms.PictureBox PaletteSprite3;
        public System.Windows.Forms.PictureBox PaletteSprite2;
        public System.Windows.Forms.PictureBox PaletteTile3;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label8;
        public System.Windows.Forms.PictureBox PaletteTile2;
        public System.Windows.Forms.PictureBox PaletteSprite1;
        public System.Windows.Forms.PictureBox PaletteSprite0;
        public System.Windows.Forms.PictureBox PaletteTile1;
        private System.Windows.Forms.Label label7;
        public System.Windows.Forms.PictureBox PaletteTile0;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.CheckBox checkBoxShowPpuScrollOverlay;
        private System.Windows.Forms.CheckBox checkBoxShowTileGrid;
        private System.Windows.Forms.CheckBox checkBoxShowAttributeGrid;
        private System.Windows.Forms.Label label5;
        public System.Windows.Forms.Label ScrollXBox;
        public System.Windows.Forms.Label ScrollYBox;
    }
}