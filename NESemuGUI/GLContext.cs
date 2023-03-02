using System;
using Common;

namespace NESemuGUI
{
    public class GLContext
    {
        public GLView View;
        IntPtr HDC; //Windows handler (Device Context)
        IntPtr HRC; //OpenGL handler (Render Context)

        public float defaultZoom = 0;

        public short xScroll = 0;
        public short yScroll = 0;

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

                //var bg = View.BackColor;
                //OpenGL.glClearColor(bg.R / 255f, bg.G / 255f, bg.B / 255f, 1);
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
                OpenGL.glEnable(OpenGL.GL_DEPTH_TEST);
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
            RenderImage();
        }

        public void RenderImage()
        {
            var image = View.FrameBuffer;
            if (image == null) return;
            if (defaultZoom > 0)
            {
                OpenGL.glPixelZoom(defaultZoom, -defaultZoom);
            }
            else
            {
                OpenGL.glPixelZoom((float)View.Width / image.Width, -(float)View.Height / image.Height);
            }

            //int GL_UNSIGNED_INT_8_8_8_8 = 0x8035;
            OpenGL.glRasterPos2d(-1, 1);

            float x = xScroll;
            float y = yScroll;

            float yFactor = y / 240;
            float xFactor = x / 256;

            //OpenGL.glBegin(OpenGL.GL_LINE_LOOP);
            //OpenGL.glColor3ub(0xFF, 0x00, 0x00);
            //OpenGL.glVertex2f(-0.5f + xFactor, 0 - yFactor); // bottom left
            //OpenGL.glVertex2f(-0.5f + xFactor, 0.5f - yFactor); // top left
            //OpenGL.glVertex2f(0 + xFactor, 0.5f - yFactor); // top right
            //OpenGL.glVertex2f(0 + xFactor, 0 - yFactor); // bottom right
            //OpenGL.glEnd();
            OpenGL.glDrawPixels(image.Width, image.Height, OpenGL.GL_BGRA_EXT, OpenGL.GL_UNSIGNED_BYTE, image.Pixels);
        }
    }
}
