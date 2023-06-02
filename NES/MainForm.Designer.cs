
namespace NES
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.FrameTimer = new System.Windows.Forms.Timer(this.components);
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.systemToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.paletteMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.loadToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.regionMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.nTSCToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pALToolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.multiRegionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dendyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.resetToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.pauseToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.debugToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pPUMemoryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ScreenBox = new NESemuGUI.GLView();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // FrameTimer
            // 
            this.FrameTimer.Interval = 15;
            this.FrameTimer.Tick += new System.EventHandler(this.FrameTimer_Tick);
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.systemToolStripMenuItem,
            this.debugToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(855, 28);
            this.menuStrip1.TabIndex = 1;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(46, 24);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.Size = new System.Drawing.Size(128, 26);
            this.openToolStripMenuItem.Text = "Open";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
            // 
            // systemToolStripMenuItem
            // 
            this.systemToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.paletteMenu,
            this.regionMenuItem,
            this.toolStripMenuItem2,
            this.toolStripMenuItem1,
            this.resetToolStripMenuItem1,
            this.pauseToolStripMenuItem1});
            this.systemToolStripMenuItem.Name = "systemToolStripMenuItem";
            this.systemToolStripMenuItem.Size = new System.Drawing.Size(70, 24);
            this.systemToolStripMenuItem.Text = "System";
            // 
            // paletteMenu
            // 
            this.paletteMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.loadToolStripMenuItem,
            this.saveToolStripMenuItem});
            this.paletteMenu.Name = "paletteMenu";
            this.paletteMenu.Size = new System.Drawing.Size(213, 26);
            this.paletteMenu.Text = "Palette";
            // 
            // loadToolStripMenuItem
            // 
            this.loadToolStripMenuItem.Name = "loadToolStripMenuItem";
            this.loadToolStripMenuItem.Size = new System.Drawing.Size(125, 26);
            this.loadToolStripMenuItem.Text = "Load";
            // 
            // saveToolStripMenuItem
            // 
            this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            this.saveToolStripMenuItem.Size = new System.Drawing.Size(125, 26);
            this.saveToolStripMenuItem.Text = "Save";
            this.saveToolStripMenuItem.Click += new System.EventHandler(this.saveToolStripMenuItem_Click);
            // 
            // regionMenuItem
            // 
            this.regionMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.nTSCToolStripMenuItem,
            this.pALToolStripMenuItem2,
            this.multiRegionToolStripMenuItem,
            this.dendyToolStripMenuItem});
            this.regionMenuItem.Name = "regionMenuItem";
            this.regionMenuItem.Size = new System.Drawing.Size(213, 26);
            this.regionMenuItem.Text = "TV Mode (Region)";
            // 
            // nTSCToolStripMenuItem
            // 
            this.nTSCToolStripMenuItem.Name = "nTSCToolStripMenuItem";
            this.nTSCToolStripMenuItem.Size = new System.Drawing.Size(173, 26);
            this.nTSCToolStripMenuItem.Text = "NTSC";
            this.nTSCToolStripMenuItem.Click += new System.EventHandler(this.regionToolStripMenuItem_Click);
            // 
            // pALToolStripMenuItem2
            // 
            this.pALToolStripMenuItem2.Name = "pALToolStripMenuItem2";
            this.pALToolStripMenuItem2.Size = new System.Drawing.Size(173, 26);
            this.pALToolStripMenuItem2.Text = "PAL";
            this.pALToolStripMenuItem2.Click += new System.EventHandler(this.regionToolStripMenuItem_Click);
            // 
            // multiRegionToolStripMenuItem
            // 
            this.multiRegionToolStripMenuItem.Name = "multiRegionToolStripMenuItem";
            this.multiRegionToolStripMenuItem.Size = new System.Drawing.Size(173, 26);
            this.multiRegionToolStripMenuItem.Text = "MultiRegion";
            // 
            // dendyToolStripMenuItem
            // 
            this.dendyToolStripMenuItem.Name = "dendyToolStripMenuItem";
            this.dendyToolStripMenuItem.Size = new System.Drawing.Size(173, 26);
            this.dendyToolStripMenuItem.Text = "Dendy";
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(213, 26);
            this.toolStripMenuItem2.Text = "Input Keys Map";
            this.toolStripMenuItem2.Click += new System.EventHandler(this.toolStripMenuItem2_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(210, 6);
            // 
            // resetToolStripMenuItem1
            // 
            this.resetToolStripMenuItem1.Name = "resetToolStripMenuItem1";
            this.resetToolStripMenuItem1.Size = new System.Drawing.Size(213, 26);
            this.resetToolStripMenuItem1.Text = "Reset";
            this.resetToolStripMenuItem1.Click += new System.EventHandler(this.resetToolStripMenuItem1_Click);
            // 
            // pauseToolStripMenuItem1
            // 
            this.pauseToolStripMenuItem1.Name = "pauseToolStripMenuItem1";
            this.pauseToolStripMenuItem1.Size = new System.Drawing.Size(213, 26);
            this.pauseToolStripMenuItem1.Text = "Pause";
            this.pauseToolStripMenuItem1.Click += new System.EventHandler(this.pauseToolStripMenuItem1_Click);
            // 
            // debugToolStripMenuItem
            // 
            this.debugToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.pPUMemoryToolStripMenuItem});
            this.debugToolStripMenuItem.Name = "debugToolStripMenuItem";
            this.debugToolStripMenuItem.Size = new System.Drawing.Size(68, 24);
            this.debugToolStripMenuItem.Text = "Debug";
            // 
            // pPUMemoryToolStripMenuItem
            // 
            this.pPUMemoryToolStripMenuItem.Name = "pPUMemoryToolStripMenuItem";
            this.pPUMemoryToolStripMenuItem.Size = new System.Drawing.Size(177, 26);
            this.pPUMemoryToolStripMenuItem.Text = "PPU Memory";
            this.pPUMemoryToolStripMenuItem.Click += new System.EventHandler(this.pPUMemoryViewToolStripMenuItem_Click);
            // 
            // ScreenBox
            // 
            this.ScreenBox.AutoSize = true;
            this.ScreenBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ScreenBox.FrameBuffer = null;
            this.ScreenBox.Location = new System.Drawing.Point(0, 28);
            this.ScreenBox.Name = "ScreenBox";
            this.ScreenBox.Size = new System.Drawing.Size(855, 686);
            this.ScreenBox.TabIndex = 2;
            this.ScreenBox.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.ScreenBox_PreviewKeyDown);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(855, 714);
            this.Controls.Add(this.ScreenBox);
            this.Controls.Add(this.menuStrip1);
            this.KeyPreview = true;
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "MainForm";
            this.Text = "NES Emulator";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.MainForm_KeyDown);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.MainForm_KeyUp);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Timer FrameTimer;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private NESemuGUI.GLView ScreenBox;
        private System.Windows.Forms.ToolStripMenuItem systemToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem regionMenuItem;
        private System.Windows.Forms.ToolStripMenuItem nTSCToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pALToolStripMenuItem2;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem resetToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem pauseToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem debugToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pPUMemoryToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem paletteMenu;
        private System.Windows.Forms.ToolStripMenuItem loadToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem multiRegionToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem dendyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem2;
    }
}

