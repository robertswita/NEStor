using Common;
using NEStor;
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
        string ROMfilename = "";
        string ROMpath;
        Bus Bus;
        int FrameCount;
        Stopwatch Watch = new Stopwatch();
        FormPPUViewer PPUviewer;
        bool Debug;
        int[] KeyMap = new int[256];
        bool IsRunning;
        long NextTick;
        int FPS;
        long FrameTime;
        //MmTimer PlayTimer = new MmTimer();

        //protected override void WndProc(ref Message m)
        //{
        //    base.WndProc(ref m);
        //    if (IsEnabled)
        //    {
        //        var startTick = Watch.ElapsedMilliseconds;
        //        if (startTick > NextTick)
        //        {
        //            FrameTime += startTick;
        //            //FPS = 1000 / (int)(startTick);
        //            FrameTimer_Tick(this, null);
        //            var stopTick = Watch.ElapsedMilliseconds;
        //            NextTick = 23 - stopTick;
        //        }
        //        Invalidate();
        //    }
        //}

        [Flags]
        enum InputBtn
        {
            A = 0x01,
            B = 0x02,
            Select = 0x04,
            Start = 0x08,
            Up = 0x10,
            Down = 0x20,
            Left = 0x40,
            Right = 0x80,
        };
        public MainForm()
        {
            InitializeComponent();
            //Application.Idle += FrameTimer1_Tick;
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
            //DoubleBuffered = true;
            PPUviewer = new FormPPUViewer();
            ROMpath = Application.StartupPath + ROMfilename;
            Reset();
            ScreenBox.FrameBuffer = Bus.Ppu.Screen;
            KeyMap[(int)Keys.Right] = 1;
            KeyMap[(int)Keys.Left] = 2;
            KeyMap[(int)Keys.Down] = 3;
            KeyMap[(int)Keys.Up] = 4;
            KeyMap[(int)Keys.Enter] = 5;
            KeyMap[(int)Keys.Space] = 6;
            KeyMap[(int)Keys.ControlKey] = 7;
            KeyMap[(int)Keys.ShiftKey] = 8;
            KeyMap[(int)Keys.Escape] = 9;
            FrameTimer1.Interval = 33;
            FrameTimer2.Interval = 33;
            FrameTimer3.Interval = 33;
            //PlayTimer.Interval = 15;
            //PlayTimer.Elapsed += FrameTimer_Tick;
        }

        public void Reset()
        {
            IsEnabled = false;
            if (Bus != null)
                Bus.Apu.WaveOut.Close();
            Bus = new Bus();
            if (ROMfilename != "")
            {
                Bus.LoadCartridge(ROMpath);
                //Bus.Cpu.TestInstructionsLength();
                Bus.Apu.Enabled = soundToolStripMenuItem.Checked;
                if (!Bus.ImageValid)
                    MessageBox.Show("Invalid image!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                else
                {
                    regionMenuItem.DropDownItems[(int)Bus.SystemType].PerformClick();
                    Watch.Start();
                    IsEnabled = true;
                    Invalidate();
                    FrameTimer1.Start();
                    System.Threading.Thread.Sleep(FrameTimer1.Interval / 3);
                    FrameTimer2.Start();
                    System.Threading.Thread.Sleep(FrameTimer1.Interval / 3);
                    FrameTimer3.Start();
                }
            }
        }

        private void FrameTimer_Tick(object sender, EventArgs e)
        {
            Bus.FrameLoop();
            //Bus.FrameLoop();
            //Bus.FrameLoop();
            ScreenBox.FrameBuffer = Bus.Ppu.Screen;
            ScreenBox.Invalidate();
            //ScreenBox.Context.DrawView();
            FrameCount++;
            //var delta = 500 * FrameCount / Watch.ElapsedMilliseconds - 30;
            if (Watch.ElapsedMilliseconds >= 1000)
            //if (FrameTime > 1000)
            {
                FrameTime = 0;
                //IsEnabled = false;
                var fpsInfo = FrameCount.ToString() + " FPS";
                var romInfo = Path.GetFileName(ROMpath);
                Text = romInfo + " (" + Bus.SystemType + ", MapperID = " + Bus.Mapper.Id.ToString() + ") " + fpsInfo;
                FrameCount = 0;
                //IsEnabled = true;
                Watch.Restart();

            }
            if (Debug)
            {
                PPUviewer.PatternTable0.Image = GetPatternTableMap(0, 0).Image;
                PPUviewer.PatternTable1.Image = GetPatternTableMap(1, 0).Image;
                var mirrorType = Bus.Mapper.Mirroring;
                PPUviewer.labelMirroringType.Text = mirrorType.ToString();
                PPUviewer.ScrollXBox.Text = Bus.Ppu.FrameScroll.X.ToString();
                PPUviewer.ScrollYBox.Text = Bus.Ppu.FrameScroll.Y.ToString();
                TPixmap fourScreens;
                if (mirrorType == MirrorType.Vertical)
                {
                    var twoScreens = GetNameTableMap(0).HorzCat(GetNameTableMap(1));
                    fourScreens = twoScreens.VertCat(twoScreens);
                }
                else
                {
                    var twoScreens = GetNameTableMap(0).VertCat(GetNameTableMap(1));
                    fourScreens = twoScreens.HorzCat(twoScreens);
                }
                PPUviewer.Nametable.Image = fourScreens.Image;
                var palettes = new TPixmap[8];
                for (var i = 0; i < palettes.Length; i++)
                {
                    var pal = new TPixmap(4, 1);
                    for (var col = 0; col < 4; col++)
                        pal.Pixels[col] = Bus.Ppu.GetPixel(i, col);
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

        bool isEnabled;
        bool IsEnabled
        {
            get { return isEnabled; }
            set
            {
                isEnabled = value;
                FrameTimer1.Enabled = isEnabled;
                FrameTimer2.Enabled = isEnabled;
                FrameTimer3.Enabled = isEnabled;
                //if (value)
                //    PlayTimer.Start();
                //else if (PlayTimer.IsRunning)
                //    PlayTimer.Stop();
                pauseToolStripMenuItem1.Checked = !IsEnabled;
            }
        }

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            switch (KeyMap[e.KeyValue])
            {
                case 9: pauseToolStripMenuItem1.PerformClick(); break;
                case 8: Bus.Controller[0] |= (int)InputBtn.A; break;
                case 7: Bus.Controller[0] |= (int)InputBtn.B; break;
                case 6: Bus.Controller[0] |= (int)InputBtn.Select; break;
                case 5: Bus.Controller[0] |= (int)InputBtn.Start; break;
                case 4: Bus.Controller[0] |= (int)InputBtn.Up; break;
                case 3: Bus.Controller[0] |= (int)InputBtn.Down; break;
                case 2: Bus.Controller[0] |= (int)InputBtn.Left; break;
                case 1: Bus.Controller[0] |= (int)InputBtn.Right; break;
            }
        }
        private void MainForm_KeyUp(object sender, KeyEventArgs e)
        {
            switch (KeyMap[e.KeyValue])
            {
                case 8: Bus.Controller[0] ^= (int)InputBtn.A; break;
                case 7: Bus.Controller[0] ^= (int)InputBtn.B; break;
                case 6: Bus.Controller[0] ^= (int)InputBtn.Select; break;
                case 5: Bus.Controller[0] ^= (int)InputBtn.Start; break;
                case 4: Bus.Controller[0] ^= (int)InputBtn.Up; break;
                case 3: Bus.Controller[0] ^= (int)InputBtn.Down; break;
                case 2: Bus.Controller[0] ^= (int)InputBtn.Left; break;
                case 1: Bus.Controller[0] ^= (int)InputBtn.Right; break;
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = Path.GetDirectoryName(ROMpath);
            openFileDialog.Filter = "NES Roms (*.nes)|*.nes|All files (*.*)|*.*";
            openFileDialog.FilterIndex = 1;
            openFileDialog.RestoreDirectory = true;
            IsEnabled = false;
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                var filePath = openFileDialog.FileName;
                //MessageBox.Show("File Content at path: " + filePath, "Load ROM", MessageBoxButtons.OK);
                ROMpath = filePath;
                ROMfilename = Path.GetFileName(ROMpath);
                Reset();
                //var s = new FileStream("dump.txt", FileMode.Create);
                //Bus.CPU.Writer = new StreamWriter(s);
            }
        }

        private void pPUMemoryViewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Debug = true;
        }

        private void pauseToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            IsEnabled = !IsEnabled;
        }

        private void regionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var menuItem = (ToolStripMenuItem)sender;
            var systemType = (SystemType)regionMenuItem.DropDownItems.IndexOf(menuItem);
            if (systemType != Bus.SystemType)
                Bus.SystemType = systemType;
            foreach (var subMenu in regionMenuItem.DropDownItems)
                ((ToolStripMenuItem)subMenu).Checked = false;
            menuItem.Checked = true;
        }

        private void resetToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Reset();
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var saveFileDialog = new SaveFileDialog();
            saveFileDialog.InitialDirectory = Path.GetDirectoryName(ROMpath);
            saveFileDialog.Filter = "Image files (PNG|*.png|Bitmap|*.bmp";
            saveFileDialog.FilterIndex = 0;
            saveFileDialog.RestoreDirectory = true;
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                var bmp = Bus.Ppu.GetPaletteMap().Image;
                bmp.Save(saveFileDialog.FileName);
            }
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            var inputMapDlg = new InputMapDlg();
            inputMapDlg.KeyMap = KeyMap;
            if (inputMapDlg.ShowDialog() == DialogResult.OK)
            {
                KeyMap = inputMapDlg.KeyMap;
            }
        }

        private void ScreenBox_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Left:
                case Keys.Right:
                case Keys.Up:
                case Keys.Down:
                case Keys.ShiftKey:
                case Keys.ControlKey:
                    e.IsInputKey = true;
                    break;
            }
        }

        /// <summary>
        /// Debug view of the specific pattern table for given palette
        /// </summary>
        /// <param name="idx"></param>
        /// <param name="palette"></param>
        /// <returns></returns>
        public TPixmap GetPatternTableMap(int tableIdx, byte palette)
        {
            var pattern = new TPixmap(128, 0);
            for (var y = 0; y < 16; y++)
            {
                var patternRow = new TPixmap(0, 8);
                for (var x = 0; x < 16; x++)
                {
                    var tileRow = new TileRow();
                    tileRow.ID = (y * 16 + x) | tableIdx << 8;
                    tileRow.Attribute = (byte)palette;
                    var tileMap = Bus.Ppu.GetTileMap(tileRow);
                    patternRow = patternRow.HorzCat(tileMap);
                }
                pattern = pattern.VertCat(patternRow);
            }
            return pattern;
        }

        /// <summary>
        /// Dump of the name table memory (information about the tiles is incomplete)
        /// </summary>
        /// <param name="idx"></param>
        /// <returns></returns>
        public TPixmap GetNameTableMap(int idx)
        {
            var nameTable = new TPixmap(256, 0);
            for (var y = 0; y < 30; y++)
            {
                var nameTableRow = new TPixmap(0, 8);
                for (var x = 0; x < 32; x++)
                {
                    var tile = Bus.Ppu.GetBgTile(idx, y, x);
                    var tileMap = Bus.Ppu.GetTileMap(tile);
                    nameTableRow = nameTableRow.HorzCat(tileMap);
                }
                nameTable = nameTable.VertCat(nameTableRow);
            }
            return nameTable;
        }

        private void soundToolStripMenuItem_Click(object sender, EventArgs e)
        {
            soundToolStripMenuItem.Checked = !soundToolStripMenuItem.Checked;
            Bus.Apu.Enabled = soundToolStripMenuItem.Checked;
        }

        //private void FrameTimer1_Tick(object sender, EventArgs e)
        //{
        //    if (IsEnabled)
        //    {
        //        var startTick = Watch.ElapsedMilliseconds;
        //        if (startTick > NextTick)
        //        {
        //            FrameTime += startTick;
        //            //FPS = 1000 / (int)(startTick);
        //            FrameTimer_Tick(this, null);
        //            var stopTick = Watch.ElapsedMilliseconds;
        //            NextTick = 23 - stopTick;
        //        }
        //        //Invalidate();
        //    }
        //}
    }
}
