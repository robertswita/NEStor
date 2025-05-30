﻿
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
            components = new System.ComponentModel.Container();
            FrameTimer1 = new System.Windows.Forms.Timer(components);
            menuStrip1 = new System.Windows.Forms.MenuStrip();
            fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripMenuItem3 = new System.Windows.Forms.ToolStripSeparator();
            batchSeparateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            systemToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            paletteMenu = new System.Windows.Forms.ToolStripMenuItem();
            loadToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            regionMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            nTSCToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            pALToolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            multiRegionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            dendyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            soundToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            resetToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            pauseToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            debugToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            pPUMemoryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            ScreenBox = new GLView();
            FrameTimer2 = new System.Windows.Forms.Timer(components);
            FrameTimer3 = new System.Windows.Forms.Timer(components);
            menuStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // FrameTimer1
            // 
            FrameTimer1.Interval = 50;
            FrameTimer1.Tick += FrameTimer_Tick;
            // 
            // menuStrip1
            // 
            menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { fileToolStripMenuItem, systemToolStripMenuItem, debugToolStripMenuItem });
            menuStrip1.Location = new System.Drawing.Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new System.Drawing.Size(855, 28);
            menuStrip1.TabIndex = 1;
            menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { openToolStripMenuItem, toolStripMenuItem3, batchSeparateToolStripMenuItem });
            fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            fileToolStripMenuItem.Size = new System.Drawing.Size(46, 24);
            fileToolStripMenuItem.Text = "File";
            // 
            // openToolStripMenuItem
            // 
            openToolStripMenuItem.Name = "openToolStripMenuItem";
            openToolStripMenuItem.Size = new System.Drawing.Size(247, 26);
            openToolStripMenuItem.Text = "Open";
            openToolStripMenuItem.Click += openToolStripMenuItem_Click;
            // 
            // toolStripMenuItem3
            // 
            toolStripMenuItem3.Name = "toolStripMenuItem3";
            toolStripMenuItem3.Size = new System.Drawing.Size(244, 6);
            // 
            // batchSeparateToolStripMenuItem
            // 
            batchSeparateToolStripMenuItem.Name = "batchSeparateToolStripMenuItem";
            batchSeparateToolStripMenuItem.Size = new System.Drawing.Size(247, 26);
            batchSeparateToolStripMenuItem.Text = "Organize by mapper ID";
            batchSeparateToolStripMenuItem.Click += batchSeparateToolStripMenuItem_Click;
            // 
            // systemToolStripMenuItem
            // 
            systemToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { paletteMenu, regionMenuItem, soundToolStripMenuItem, toolStripMenuItem2, toolStripMenuItem1, resetToolStripMenuItem1, pauseToolStripMenuItem1 });
            systemToolStripMenuItem.Name = "systemToolStripMenuItem";
            systemToolStripMenuItem.Size = new System.Drawing.Size(70, 24);
            systemToolStripMenuItem.Text = "System";
            // 
            // paletteMenu
            // 
            paletteMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { loadToolStripMenuItem, saveToolStripMenuItem });
            paletteMenu.Name = "paletteMenu";
            paletteMenu.Size = new System.Drawing.Size(213, 26);
            paletteMenu.Text = "Palette";
            // 
            // loadToolStripMenuItem
            // 
            loadToolStripMenuItem.Name = "loadToolStripMenuItem";
            loadToolStripMenuItem.Size = new System.Drawing.Size(125, 26);
            loadToolStripMenuItem.Text = "Load";
            // 
            // saveToolStripMenuItem
            // 
            saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            saveToolStripMenuItem.Size = new System.Drawing.Size(125, 26);
            saveToolStripMenuItem.Text = "Save";
            saveToolStripMenuItem.Click += saveToolStripMenuItem_Click;
            // 
            // regionMenuItem
            // 
            regionMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { nTSCToolStripMenuItem, pALToolStripMenuItem2, multiRegionToolStripMenuItem, dendyToolStripMenuItem });
            regionMenuItem.Name = "regionMenuItem";
            regionMenuItem.Size = new System.Drawing.Size(213, 26);
            regionMenuItem.Text = "TV Mode (Region)";
            // 
            // nTSCToolStripMenuItem
            // 
            nTSCToolStripMenuItem.Name = "nTSCToolStripMenuItem";
            nTSCToolStripMenuItem.Size = new System.Drawing.Size(173, 26);
            nTSCToolStripMenuItem.Text = "NTSC";
            nTSCToolStripMenuItem.Click += regionToolStripMenuItem_Click;
            // 
            // pALToolStripMenuItem2
            // 
            pALToolStripMenuItem2.Name = "pALToolStripMenuItem2";
            pALToolStripMenuItem2.Size = new System.Drawing.Size(173, 26);
            pALToolStripMenuItem2.Text = "PAL";
            pALToolStripMenuItem2.Click += regionToolStripMenuItem_Click;
            // 
            // multiRegionToolStripMenuItem
            // 
            multiRegionToolStripMenuItem.Name = "multiRegionToolStripMenuItem";
            multiRegionToolStripMenuItem.Size = new System.Drawing.Size(173, 26);
            multiRegionToolStripMenuItem.Text = "MultiRegion";
            // 
            // dendyToolStripMenuItem
            // 
            dendyToolStripMenuItem.Name = "dendyToolStripMenuItem";
            dendyToolStripMenuItem.Size = new System.Drawing.Size(173, 26);
            dendyToolStripMenuItem.Text = "Dendy";
            // 
            // soundToolStripMenuItem
            // 
            soundToolStripMenuItem.Checked = true;
            soundToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            soundToolStripMenuItem.Name = "soundToolStripMenuItem";
            soundToolStripMenuItem.Size = new System.Drawing.Size(213, 26);
            soundToolStripMenuItem.Text = "Sound";
            soundToolStripMenuItem.Click += soundToolStripMenuItem_Click;
            // 
            // toolStripMenuItem2
            // 
            toolStripMenuItem2.Name = "toolStripMenuItem2";
            toolStripMenuItem2.Size = new System.Drawing.Size(213, 26);
            toolStripMenuItem2.Text = "Input Keys Map";
            toolStripMenuItem2.Click += toolStripMenuItem2_Click;
            // 
            // toolStripMenuItem1
            // 
            toolStripMenuItem1.Name = "toolStripMenuItem1";
            toolStripMenuItem1.Size = new System.Drawing.Size(210, 6);
            // 
            // resetToolStripMenuItem1
            // 
            resetToolStripMenuItem1.Name = "resetToolStripMenuItem1";
            resetToolStripMenuItem1.Size = new System.Drawing.Size(213, 26);
            resetToolStripMenuItem1.Text = "Reset";
            resetToolStripMenuItem1.Click += resetToolStripMenuItem1_Click;
            // 
            // pauseToolStripMenuItem1
            // 
            pauseToolStripMenuItem1.Name = "pauseToolStripMenuItem1";
            pauseToolStripMenuItem1.Size = new System.Drawing.Size(213, 26);
            pauseToolStripMenuItem1.Text = "Pause";
            pauseToolStripMenuItem1.Click += pauseToolStripMenuItem1_Click;
            // 
            // debugToolStripMenuItem
            // 
            debugToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { pPUMemoryToolStripMenuItem });
            debugToolStripMenuItem.Name = "debugToolStripMenuItem";
            debugToolStripMenuItem.Size = new System.Drawing.Size(68, 24);
            debugToolStripMenuItem.Text = "Debug";
            // 
            // pPUMemoryToolStripMenuItem
            // 
            pPUMemoryToolStripMenuItem.Name = "pPUMemoryToolStripMenuItem";
            pPUMemoryToolStripMenuItem.Size = new System.Drawing.Size(177, 26);
            pPUMemoryToolStripMenuItem.Text = "PPU Memory";
            pPUMemoryToolStripMenuItem.Click += pPUMemoryViewToolStripMenuItem_Click;
            // 
            // ScreenBox
            // 
            ScreenBox.AutoSize = true;
            ScreenBox.Dock = System.Windows.Forms.DockStyle.Fill;
            ScreenBox.Location = new System.Drawing.Point(0, 28);
            ScreenBox.Name = "ScreenBox";
            ScreenBox.Size = new System.Drawing.Size(855, 686);
            ScreenBox.TabIndex = 2;
            ScreenBox.PreviewKeyDown += ScreenBox_PreviewKeyDown;
            // 
            // FrameTimer2
            // 
            FrameTimer2.Interval = 50;
            FrameTimer2.Tick += FrameTimer_Tick;
            // 
            // FrameTimer3
            // 
            FrameTimer3.Interval = 50;
            FrameTimer3.Tick += FrameTimer_Tick;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(855, 714);
            Controls.Add(ScreenBox);
            Controls.Add(menuStrip1);
            KeyPreview = true;
            MainMenuStrip = menuStrip1;
            Name = "MainForm";
            Text = "NEStor NES Emulator";
            FormClosing += MainForm_FormClosing;
            KeyDown += MainForm_KeyDown;
            KeyUp += MainForm_KeyUp;
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Timer FrameTimer1;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private NES.GLView ScreenBox;
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
        private System.Windows.Forms.ToolStripMenuItem soundToolStripMenuItem;
        private System.Windows.Forms.Timer FrameTimer2;
        private System.Windows.Forms.Timer FrameTimer3;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem3;
        private System.Windows.Forms.ToolStripMenuItem batchSeparateToolStripMenuItem;
    }
}

