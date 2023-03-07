using Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace NES
{
    public partial class MainForm : Form
    {
        string NESpath = "/demo.nes";
        string RomPath;
        Bus Bus;
        Bitmap ScreenImage;
        int FrameCount;
        Stopwatch Watch = Stopwatch.StartNew();
        FormPPUViewer PPUviewer;
        bool Debug;
        public MainForm()
        {
            InitializeComponent();
            PPUviewer = new FormPPUViewer();
            RomPath = Application.StartupPath + NESpath;
            Run();
        }

        public void Run()
        {
            Cartridge Cartridge = new Cartridge(RomPath);
            //threads.form.FormINESHeader.BinRomHeader = Cartridge.BinHeader;
            if (!Cartridge.ImageValid())
                MessageBox.Show("Invalid image!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Bus = new Bus(ref Cartridge);
            Bus.Reset();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            do
            { 
                Bus.Clock();
            } while (!Bus.PPU.FrameComplete);
            Bus.PPU.FrameComplete = false;
            FrameCount++;
            if (Watch.ElapsedMilliseconds >= 1000)
            {
                var fpsInfo = FrameCount.ToString() + " FPS";
                var romInfo = Path.GetFileName(RomPath);
                Text = romInfo + "  " + fpsInfo;
                FrameCount = 0;
                Watch.Restart();
            }
            ScreenImage = Bus.PPU.Screen.Image;
            ScreenBox.Invalidate();
            if (Debug)
            {
                PPUviewer.PatternTable0.Image = Bus.PPU.GetPattern(0, 0).Image;
                PPUviewer.PatternTable1.Image = Bus.PPU.GetPattern(1, 0).Image;
                var mirrorType = Bus.Cartridge.Mirror();
                PPUviewer.labelMirroringType.Text = mirrorType.ToString();
                PPUviewer.ScrollXBox.Text = Bus.PPU.Scroll.X.ToString();
                PPUviewer.ScrollYBox.Text = Bus.PPU.Scroll.Y.ToString();
                TPixmap fourScreens;
                if (mirrorType == MIRROR.VERTICAL)
                {
                    var twoScreens = Bus.PPU.GetNameTable(0).HorzCat(Bus.PPU.GetNameTable(1));
                    fourScreens = twoScreens.VertCat(twoScreens);
                }
                else
                {
                    var twoScreens = Bus.PPU.GetNameTable(0).VertCat(Bus.PPU.GetNameTable(1));
                    fourScreens = twoScreens.HorzCat(twoScreens);
                }
                PPUviewer.Nametable.Image = fourScreens.Image;
                var palettes = new TPixmap[8];
                for (var i = 0; i < 8; i++)
                {
                    var pal = new TPixmap(4, 1);
                    for (var col = 0; col < 4; col++)
                        pal.Pixels[col] = Bus.PPU.GetColourFromPaletteRam(i, col);
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

        private void ScreenBox_Paint(object sender, PaintEventArgs e)
        {
            if (ScreenImage != null)
            {
                e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                e.Graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
                e.Graphics.DrawImage(ScreenImage, ScreenBox.ClientRectangle);
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = Path.GetDirectoryName(RomPath);
            openFileDialog.Filter = "NES Roms (*.nes)|*.nes|All files (*.*)|*.*";
            openFileDialog.FilterIndex = 1;
            openFileDialog.RestoreDirectory = true;

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                var filePath = openFileDialog.FileName;
                MessageBox.Show("File Content at path: " + filePath, "Load ROM", MessageBoxButtons.OK);
                RomPath = filePath;
                Run();
            }
        }

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Z: Bus.controller[0] |= 0x80; break;
                case Keys.X: Bus.controller[0] |= 0x40; break;
                case Keys.S: Bus.controller[0] |= 0x20; break;
                case Keys.D: Bus.controller[0] |= 0x10; break;
                case Keys.I: Bus.controller[0] |= 0x08; break;
                case Keys.K: Bus.controller[0] |= 0x04; break;
                case Keys.J: Bus.controller[0] |= 0x02; break;
                case Keys.L: Bus.controller[0] |= 0x01; break;
                case Keys.Escape: Pause(); break;
            }

        }

        void Pause()
        {

        }

        private void MainForm_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Z: Bus.controller[0] ^= 0x80; break;
                case Keys.X: Bus.controller[0] ^= 0x40; break;
                case Keys.S: Bus.controller[0] ^= 0x20; break;
                case Keys.D: Bus.controller[0] ^= 0x10; break;
                case Keys.I: Bus.controller[0] ^= 0x08; break;
                case Keys.K: Bus.controller[0] ^= 0x04; break;
                case Keys.J: Bus.controller[0] ^= 0x02; break;
                case Keys.L: Bus.controller[0] ^= 0x01; break;
                case Keys.Escape: Pause(); break;
            }
        }

        private void pPUMemoryViewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Debug = true;
        }
    }
}
