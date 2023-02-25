using System;
using Common;

namespace NESemuGUI
{
    public class GLContext
    {
        public GLView View;
        IntPtr HDC; //Windows handler (Device Context)
        IntPtr HRC; //OpenGL handler (Render Context)

        // Memory buffer for our NES frame
        public UInt32[] frameBuffer = new UInt32[256*240];

        // NES frame width and height
        public int frameWidth = 256;
        public int frameHeight = 240;

        // Zoom level (default is 0)
        public float defaultZoom = 0;

        // X and Y of the scrolling overlay
        public short xScroll = 0;
        public short yScroll = 0;

        // Nametable debug view
        public bool IsNametableViewer = false;

        // Whether to show the PPU's scrolling overlay
        public bool ShowPPUScrollOverlay = false;

        // Whether to show attribute grid
        public bool ShowAttributeGrid = false;

        // Whether to show tile grid
        public bool ShowTileGrid = false;

        public IntPtr Handle
        {
            get
            {
                if (HRC == IntPtr.Zero)
                {
                    var pfd = new Win32.PIXELFORMATDESCRIPTOR();
                    HDC = View.CreateGraphics().GetHdc();
                    var index = Win32.ChoosePixelFormat(HDC, pfd);
                    Win32.SetPixelFormat(HDC, index, pfd);
                    HRC = Win32.wglCreateContext(HDC);
                }
                return HRC;
            }
        }

        internal void DrawView()
        {
            if (Handle != IntPtr.Zero)
            {
                Win32.wglMakeCurrent(HDC, HRC);
                var vp = View.ClientRectangle;
                OpenGL.glViewport(vp.Left, vp.Top, vp.Width, vp.Height);
                //OpenGL.glScissor(vp.Left, vp.Top, vp.Width, vp.Height);
                //OpenGL.gluOrtho2D(0, frameWidth, frameHeight, 0);

                var bg = View.BackColor;
                OpenGL.glClearColor(bg.R / 255f, bg.G / 255f, bg.B / 255f, 1);
                OpenGL.glClear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);
                Init();
                DrawScene();
                Win32.SwapBuffers(HDC);
            }
        }

        public bool IsInited;
        internal void Init()
        {
            if (!IsInited)
            {

                //OpenGL.glEnable(OpenGL.GL_BLEND);
                //OpenGL.glBlendFunc(OpenGL.GL_SRC_ALPHA, OpenGL.GL_ONE_MINUS_SRC_ALPHA);

                OpenGL.glEnable(OpenGL.GL_DEPTH_TEST);
                //OpenGL.glEnable(OpenGL.GL_DEPTH_TEST);
                //OpenGL.glEnable(OpenGL.GL_CULL_FACE);
                //OpenGL.glPolygonMode(OpenGL.GL_FRONT_AND_BACK,OpenGL.GL_LINE);
                //OpenGL.glMatrixMode(OpenGL.GL_PROJECTION);
                IsInited = true;
            }
        }

        internal void DrawScene()
        {
            OpenGL.glLoadIdentity();
            //OpenGL.glOrtho(0, frameWidth, frameHeight, 0, -1, 1);
            //OpenGL.glEnable(OpenGL.GL_BLEND);
            //OpenGL.glBlendFunc(OpenGL.GL_SRC_ALPHA, OpenGL.GL_ONE_MINUS_SRC_ALPHA);
            //OpenGL.glEnable(OpenGL.GL_DEPTH_TEST);
            RenderImage(frameBuffer);
        }

        public void RenderImage(UInt32[] data)
        {
            //if(!OpenGL.glIsEnabled(OpenGL.GL_DEPTH_TEST))
            //{
            //    throw new Exception("depth test is not enabled");
            //}

            if (IsNametableViewer) {
                

                float x = xScroll;
                float y = yScroll;

                UInt32 color = 0xFF0000ff;

                if (ShowPPUScrollOverlay) {
                    DrawRectangle(color, x, y, 256, 240);

                    if (x + 256 >= 512)
                    {
                        DrawRectangle(color, 0, y, x - 256, 240);
                    }
                    if (y + 240 >= 480)
                    {
                        DrawRectangle(color, x, 0, 256, y - 240);
                    }
                    if (x + 256 >= 512 && y + 240 >= 480)
                    {
                        DrawRectangle(color, 0, 0, x - 256, y - 240);
                    }
                }

                if (ShowAttributeGrid)
                {
                    color = 0x5082FAb4;
                    for (int i = 0; i < 32; i++)
                    {
                        DrawLine(color, i * 16, 0, i * 16, 479);
                    }
                    for (int i = 0; i < 30; i++)
                    {
                        DrawLine(color, 0, i * 16 - 1, 511, i * 16 - 1); // ???
                    }
                }

                if (ShowTileGrid)
                {
                    color = 0xF06478b4;
                    for (int i = 0; i < 64; i++)
                    {
                        DrawLine(color, i * 8, 0, i * 8, 479);
                    }
                    for (int i = 0; i < 60; i++)
                    {
                        DrawLine(color, 0, i * 8 - 1, 511, i * 8 - 1); // ???
                    }
                }
            }
            //OpenGL.glDepthMask(false);

            if (defaultZoom > 0)
            {
                OpenGL.glPixelZoom(defaultZoom, -defaultZoom);
            }
            else
            {
                OpenGL.glPixelZoom((float)View.Width / frameWidth, -(float)View.Height / frameHeight);
            }

            int GL_UNSIGNED_INT_8_8_8_8 = 0x8035;
            OpenGL.glRasterPos2d(-1, 1);

            OpenGL.glDrawPixels(frameWidth, frameHeight, OpenGL.GL_RGBA, GL_UNSIGNED_INT_8_8_8_8, data);

            //OpenGL.glDepthMask(true); // Re-enable writing to depth buffer

            //OpenGL.glDisable(OpenGL.GL_BLEND);
        }

        private void DrawRectangle(UInt32 colorRGBA, float x, float y, float width, float height)
        {
            byte red = (Byte)(colorRGBA >> 24); 
            byte green = (Byte)((colorRGBA & 0xFF0000) >> 16);
            byte blue = (Byte)((colorRGBA & 0xFF00) >> 8);
            byte alpha = (Byte)(colorRGBA & 0xFF);

            float yFactor = y / height;
            float xFactor = x / width;

            float widthFactor = 1 -(width / 255);
            float heightFactor = 1 - (height / 239);

            OpenGL.glBegin(OpenGL.GL_LINE_LOOP);
            OpenGL.glColor4ub(red, green, blue, alpha);
            OpenGL.glVertex2f(-1 + xFactor, 0 - yFactor + heightFactor); // bottom left
            OpenGL.glVertex2f(-1 + xFactor, 1 - yFactor); // top left
            OpenGL.glVertex2f(0 + xFactor - widthFactor, 1 - yFactor); // top right
            OpenGL.glVertex2f(0 + xFactor - widthFactor, 0 - yFactor + heightFactor); // bottom right

            OpenGL.glEnd();
        }

        private void DrawLine(UInt32 colorRGBA, float x1, float y1, float x2, float y2)
        {
            byte red = (Byte)(colorRGBA >> 24);
            byte green = (Byte)((colorRGBA & 0xFF0000) >> 16);
            byte blue = (Byte)((colorRGBA & 0xFF00) >> 8);
            byte alpha = (Byte)(colorRGBA & 0xFF);


            x1 = 2 * x1 / 511 - 1;
            y1 = 2 * y1 / 479 - 1;

            x2 = 2 * x2 / 511 - 1;
            y2 = 2 * y2 / 479 - 1;

            OpenGL.glBegin(OpenGL.GL_LINES);
            OpenGL.glColor4ub(red, green, blue, alpha);
            OpenGL.glVertex2f(x1, y1);
            OpenGL.glVertex2f(x2, y2);

            OpenGL.glEnd();
        }
    }
}
