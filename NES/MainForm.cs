using Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

namespace NES
{
    public partial class MainForm : Form
    {
        string ROMfilename = ""; //"zelda2_mapper1.nes";
        string ROMpath;
        Bus Bus;
        int FrameCount;
        Stopwatch Watch = new Stopwatch();
        FormPPUViewer PPUviewer;
        bool Debug;

        [Flags]
        enum ControllerBtn { 
            Right = 0x01, 
            Left = 0x02, 
            Down = 0x04, 
            Up = 0x08, 
            Start = 0x10, 
            Select = 0x20, 
            B = 0x40, 
            A = 0x80
        };
        public MainForm()
        {
            InitializeComponent();
            DoubleBuffered = true;
            PPUviewer = new FormPPUViewer();
            ROMpath = Application.StartupPath;
            Bus = new Bus();
            Run();
        }

        public void Run()
        {
            FrameTimer.Stop();
            if (ROMfilename != "")
            {
                Bus.LoadCartridge(ROMpath);
                if (!Bus.ImageValid)
                    MessageBox.Show("Invalid image!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                else
                {
                    Watch.Start();
                    FrameTimer.Start();
                }
            }
        }

        //public bool IsOutputFrame;
        private void timer1_Tick(object sender, EventArgs e)
        {
            //for (int i = 0; i < 2; i++)
            //{
            //    Bus.FrameLoop();
            //    FrameCount++;
            //    if (IsOutputFrame)
            //        ScreenImage = Bus.PPU.Screen.Image;
            //    IsOutputFrame = !IsOutputFrame;
            //}
            //IsOutputFrame = !IsOutputFrame;

            Bus.FrameLoop();
            FrameCount++;
            //if (IsOutputFrame)
            //ScreenImage = Bus.PPU.Screen.Image;
            ScreenBox.FrameBuffer = Bus.PPU.Screen;
            Invalidate();
            if (Watch.ElapsedMilliseconds >= 1000)
            {
                var fpsInfo = FrameCount.ToString() + " FPS";
                var romInfo = Path.GetFileName(ROMpath);
                Text = romInfo + "  " + fpsInfo;
                FrameCount = 0;
                Watch.Restart();
            }
            if (Debug)
            {
                PPUviewer.PatternTable0.Image = Bus.PPU.GetPatternTableMap(0, 0).Image;
                PPUviewer.PatternTable1.Image = Bus.PPU.GetPatternTableMap(1, 0).Image;
                var mirrorType = Bus.Mapper.Mirroring;
                PPUviewer.labelMirroringType.Text = mirrorType.ToString();
                PPUviewer.ScrollXBox.Text = Bus.PPU.Scroll.X.ToString();
                PPUviewer.ScrollYBox.Text = Bus.PPU.Scroll.Y.ToString();
                TPixmap fourScreens;
                if (mirrorType == VRAMmirroring.Vertical)
                {
                    var twoScreens = Bus.PPU.GetNameTableMap(0).HorzCat(Bus.PPU.GetNameTableMap(1));
                    fourScreens = twoScreens.VertCat(twoScreens);
                }
                else
                {
                    var twoScreens = Bus.PPU.GetNameTableMap(0).VertCat(Bus.PPU.GetNameTableMap(1));
                    fourScreens = twoScreens.HorzCat(twoScreens);
                }
                PPUviewer.Nametable.Image = fourScreens.Image;
                var palettes = new TPixmap[8];
                for (var i = 0; i < 8; i++)
                {
                    var pal = new TPixmap(4, 1);
                    for (var col = 0; col < 4; col++)
                        pal.Pixels[col] = Bus.PPU.GetColorFromPalette(i, col);
                    palettes[i] = pal;
                }
                PPUviewer.PaletteTile0.Image = palettes[0].Image;
                PPUviewer.PaletteTile1.Image = palettes[1].Image;
                PPUviewer.PaletteTile2.Image = palettes[2].Image;
                PPUviewer.PaletteTile3.Image = palettes[3].Image;
                PPUviewer.PaletteSprite0.Image = palettes[4].Image;
                PPUviewer.PaletteSprite1.Image = palettes[5].Image;
                PPUviewer.PaletteSprite2.Image = palettes[6].Image;
                PPUviewer.PaletteSprite3.Image = palettes[7].Image;
                PPUviewer.Show();
                Debug = false;
            }
        }

        void Pause()
        {
            FrameTimer.Enabled = !FrameTimer.Enabled;
        }

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Z: Bus.controller[0] |= (int)ControllerBtn.A; break;
                case Keys.X: Bus.controller[0] |= (int)ControllerBtn.B; break;
                case Keys.S: Bus.controller[0] |= (int)ControllerBtn.Select; break;
                case Keys.D: Bus.controller[0] |= (int)ControllerBtn.Start; break;
                case Keys.I: Bus.controller[0] |= (int)ControllerBtn.Up; break;
                case Keys.K: Bus.controller[0] |= (int)ControllerBtn.Down; break;
                case Keys.J: Bus.controller[0] |= (int)ControllerBtn.Left; break;
                case Keys.L: Bus.controller[0] |= (int)ControllerBtn.Right; break;
                case Keys.Escape: Pause(); break;
            }

        }

        private void MainForm_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Z: Bus.controller[0] ^= (int)ControllerBtn.A; break;
                case Keys.X: Bus.controller[0] ^= (int)ControllerBtn.B; break;
                case Keys.S: Bus.controller[0] ^= (int)ControllerBtn.Select; break;
                case Keys.D: Bus.controller[0] ^= (int)ControllerBtn.Start; break;
                case Keys.I: Bus.controller[0] ^= (int)ControllerBtn.Up; break;
                case Keys.K: Bus.controller[0] ^= (int)ControllerBtn.Down; break;
                case Keys.J: Bus.controller[0] ^= (int)ControllerBtn.Left; break;
                case Keys.L: Bus.controller[0] ^= (int)ControllerBtn.Right; break;
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = Path.GetDirectoryName(ROMpath);
            openFileDialog.Filter = "NES Roms (*.nes)|*.nes|All files (*.*)|*.*";
            openFileDialog.FilterIndex = 1;
            openFileDialog.RestoreDirectory = true;
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                var filePath = openFileDialog.FileName;
                MessageBox.Show("File Content at path: " + filePath, "Load ROM", MessageBoxButtons.OK);
                ROMpath = filePath;
                ROMfilename = Path.GetFileName(ROMpath);
                Run();
            }
        }

        private void pPUMemoryViewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Debug = true;
        }

        private void pauseToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Pause();
        }

        private void regionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var menuItem = (ToolStripMenuItem)sender;
            Bus.SystemType = (SystemType)regionMenuItem.DropDownItems.IndexOf(menuItem);
            foreach (var subMenu in regionMenuItem.DropDownItems)
                ((ToolStripMenuItem)subMenu).Checked = false;
            menuItem.Checked = true;
        }

        private void resetToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Bus.Reset();
        }
    }
}
