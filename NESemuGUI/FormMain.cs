using Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NESemuGUI
{
    public partial class FormMain : Form
    {
        public Boolean btnA;        public Keys btnA_key;
        public Boolean btnB;
        public Boolean btnSelect;
        public Boolean btnStart;
        public Boolean btnUp;
        public Boolean btnDown;
        public Boolean btnLeft;
        public Boolean btnRight;
        public int fps;

        public bool pause = false;
        public bool reset = false;

        public GLView GLView;

        public FormPPUViewer FormPPUViewer;

        public FormINESHeader FormINESHeader;

        public Boolean debug = false;

        public FormMain()
        {
            InitializeComponent();

            btnA = false;
            btnB = false;
            btnSelect = false;
            btnStart = false;
            btnUp = false;
            btnDown = false;
            btnLeft = false;
            btnRight = false;
            //fps = 0;

            GLView = glView1;
            FormPPUViewer = new FormPPUViewer();
            FormINESHeader = new FormINESHeader();

            FormPPUViewer.glViewPatternTable0.Context.defaultZoom = 2;
            FormPPUViewer.glViewPatternTable1.Context.defaultZoom = 2;
            FormPPUViewer.glViewNametable.Context.defaultZoom = 1.5f;
            FormPPUViewer.glViewPaletteTile0.Context.defaultZoom = 32;
            FormPPUViewer.glViewPaletteTile1.Context.defaultZoom = 32;
            FormPPUViewer.glViewPaletteTile2.Context.defaultZoom = 32;
            FormPPUViewer.glViewPaletteTile3.Context.defaultZoom = 32;
            FormPPUViewer.glViewPaletteSprite0.Context.defaultZoom = 32;
            FormPPUViewer.glViewPaletteSprite1.Context.defaultZoom = 32;
            FormPPUViewer.glViewPaletteSprite2.Context.defaultZoom = 32;
            FormPPUViewer.glViewPaletteSprite3.Context.defaultZoom = 32;
        }

        public void UpdateFps(int _fps)
        {
            if (_fps == 61)
                _fps = 60;
            Byte performance = (Byte)((100 * _fps) / 60);
            fpsInfo = "FPS: " + _fps + " (" + performance + "%)" ;
            UpdateTitle(TITLE.FPS, fpsInfo);
        }

        public void UpdateName(string _path)
        {
            romInfo = Path.GetFileName(_path);
            UpdateTitle(TITLE.ROM, romInfo);
        }

        string screenInfo = "";
        string romInfo = "";
        string fpsInfo = "";

        private void UpdateTitle(TITLE t, string data)
        {
            switch (t)
            {
                case TITLE.VP:
                    screenInfo = data;
                    break;
                case TITLE.ROM:
                    romInfo = data;
                    break;
                case TITLE.FPS:
                    fpsInfo = data;
                    break;
            }

            //Text = romInfo + " " + screenInfo + " " + fpsInfo;
            if (!IsDisposed && IsHandleCreated)
            {
                // So basically my understing is that it cant update Text because the thread above this thread does not wait
                // for this Text update
                // you may wonder why does it even wait, i donno :)
                // i mean now it makes sense where there is possibility that it cant execute this fuction before handle being created 
                BeginInvoke(new Action(() =>
                {
                    this.Text = romInfo + "  " + screenInfo + "  " + fpsInfo;
                }));
            }
            
        }

        enum TITLE
        {
            VP,
            ROM,
            FPS
        }

        //public void UpdateView(ref uint[] data)
        //{
        //    glView1.Context.frameWidth = 256;
        //    glView1.Context.frameHeight = 240;
        //    glView1.Context.frameBuffer = data;
        //    glView1.Invalidate();
        //}

        //public void UpdateDebugViewPatternTable(ref uint[] data)
        //{
        //    FormPPUViewer.glViewPatternTable0.Context.frameWidth = 128;
        //    FormPPUViewer.glViewPatternTable0.Context.frameHeight = 128;
        //    FormPPUViewer.glViewPatternTable0.Context.defaultZoom = 2;
        //    FormPPUViewer.glViewPatternTable0.Context.frameBuffer = data;
        //    FormPPUViewer.glViewPatternTable0.Invalidate();

        //    FormPPUViewer.glViewPatternTable1.Context.frameWidth = 128;
        //    FormPPUViewer.glViewPatternTable1.Context.frameHeight = 128;
        //    FormPPUViewer.glViewPatternTable1.Context.defaultZoom = 2;
        //    FormPPUViewer.glViewPatternTable1.Context.frameBuffer = data2;
        //    FormPPUViewer.glViewPatternTable1.Invalidate();

        //    if(!FormPPUViewer.Visible){
        //        debug = false;
        //    }
        //}

        //public void UpdateDebugViewNameTable(ref uint[] data, int xScroll, int yScroll, bool mirroringType)
        //{
        //    FormPPUViewer.glViewNametable.Context.frameWidth = 512;
        //    FormPPUViewer.glViewNametable.Context.frameHeight = 480;
        //    FormPPUViewer.glViewNametable.Context.defaultZoom = (float)1.5;
        //    FormPPUViewer.glViewNametable.Context.frameBuffer = data;
        //    FormPPUViewer.glViewNametable.Context.xScroll = (short)xScroll;
        //    FormPPUViewer.glViewNametable.Context.yScroll = (short)yScroll;
        //    FormPPUViewer.glViewNametable.Invalidate();
        //    FormPPUViewer.UpdateMirroringType(mirroringType);
        //}

        //public void UpdateDebugViewPalette(TPixmap[] palettes)
        //{
        //    FormPPUViewer.glViewPaletteTile0.FrameBuffer = palettes[0];
        //    FormPPUViewer.glViewPaletteTile1.FrameBuffer = palettes[1];
        //    FormPPUViewer.glViewPaletteTile2.FrameBuffer = palettes[2];
        //    FormPPUViewer.glViewPaletteTile3.FrameBuffer = palettes[3];
        //    FormPPUViewer.glViewPaletteSprite0.FrameBuffer = palettes[4];
        //    FormPPUViewer.glViewPaletteSprite1.FrameBuffer = palettes[5];
        //    FormPPUViewer.glViewPaletteSprite2.FrameBuffer = palettes[6];
        //    FormPPUViewer.glViewPaletteSprite3.FrameBuffer = palettes[7];
        //}

        private void fileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //UInt32[] frame = new UInt32[256 * 240];


            //Bitmap img = new Bitmap("canvasTest.bmp");
            //Color pixel = img.GetPixel(255, 239);

            //for (int y = 239; y >= 0; y--)
            //{
            //    for (int x = 0; x < 256; x++)
            //    {
            //        var temp = Math.Abs(y - 239);
            //        pixel = img.GetPixel(x, temp);

            //        frame[x + (y * 256)] = ((UInt32)pixel.R << 24) + ((UInt32)pixel.G << 16) + ((UInt32)pixel.B << 8) + (UInt32)0x8F;
            //    }
            //}

            //UpdateView(ref frame);
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            //this.Text = "AS:" + 1.16;

            Control control = (Control)sender;

            glView1.Width = control.Width - 18;
            glView1.Height = (glView1.Width * 15) / 16;
            control.Height = glView1.Height + 78;

            //control.Height = (int)Math.Round(((float)control.Width * 159)/137); ;

            //this.Text += " GUI:" +  control.Width + "x" + control.Height;
            //274 318
            //256 240
            // 18  78
            glView1.Width = this.Width - 18;
            glView1.Height = this.Height - 78;

            //UpdateTitle(TITLE.VP, "GUI: " + this.Width + "x" + this.Height + "  CANVAS: " + glView1.Width + "x" + glView1.Height);
            UpdateTitle(TITLE.VP, "RES: " + glView1.Width + "x" + glView1.Height);
            //this.Text = "GUI:" + this.Width + "x" + this.Height + " CANVAS:" + glView1.Width + "x" + glView1.Height + " FPS: " + fps;
        }

        private bool btnEscape = false;

        private void glView1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Z)
                btnA = true;

            if (e.KeyCode == Keys.X)
                btnB = true;

            if (e.KeyCode == Keys.S)
                btnSelect = true;

            if (e.KeyCode == Keys.D)
                btnStart = true;

            if (e.KeyCode == Keys.I)
                btnUp = true;

            if (e.KeyCode == Keys.K)
                btnDown = true;

            if (e.KeyCode == Keys.J)
                btnLeft = true;

            if (e.KeyCode == Keys.L)
                btnRight = true;

            if (e.KeyCode == Keys.Escape)
            {
                if (!btnEscape)
                {
                    btnEscape = !btnEscape;
                    Pause();
                }
            }

            //MessageBox.Show(e.KeyCode.ToString(), "Key", MessageBoxButtons.OK);
        }

        private void glView1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Z)
                btnA = false;

            if (e.KeyCode == Keys.X)
                btnB = false;

            if (e.KeyCode == Keys.S)
                btnSelect = false;

            if (e.KeyCode == Keys.D)
                btnStart = false;

            if (e.KeyCode == Keys.I)
                btnUp = false;

            if (e.KeyCode == Keys.K)
                btnDown = false;

            if (e.KeyCode == Keys.J)
                btnLeft = false;

            if (e.KeyCode == Keys.L)
                btnRight = false;

            if (e.KeyCode == Keys.Escape)
            {
                btnEscape = !btnEscape;
            }
        }

        private void Pause()
        {
            if(pause)
            {
                pauseToolStripMenuItem.Text = "Pause";
            } else
            {
                pauseToolStripMenuItem.Text = "Resume";
            }
            pause = !pause;
        }

        private void pPUViewerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormPPUViewer.Show();
            debug = true;
        }

        private void pauseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Pause();
        }

        private void resetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            reset = true;
        }

        public bool newRomFile = false;
        public string romPath = "";
        

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = Path.GetDirectoryName(romPath);
            openFileDialog.Filter = "NES Roms (*.nes)|*.nes|All files (*.*)|*.*";
            openFileDialog.FilterIndex = 1;
            openFileDialog.RestoreDirectory = true;

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                //Get the path of specified file
                var filePath = openFileDialog.FileName;

                MessageBox.Show("File Content at path: " + filePath, "Load ROM", MessageBoxButtons.OK);

                romPath = filePath;
                newRomFile = true;
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void iNESHeaderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormINESHeader.Show();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormAbout about = new FormAbout();
            about.ShowDialog();
        }

        private void toolStripMenuItemControlls_Click(object sender, EventArgs e)
        {
            FormHelp help = new FormHelp();
            help.Show();
        }
    }
}
