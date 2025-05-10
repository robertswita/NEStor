using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using Common;
using TGL;

namespace NES
{
    public class GLContext
    {
        public GLView View;
        IntPtr HDC; // Windows Device Context
        IntPtr HRC; // OpenGL Render Context
        int Texture;

        public IntPtr Handle
        {
            get
            {
                if (HRC == IntPtr.Zero)
                {
                    var pfd = new Win32.PIXELFORMATDESCRIPTOR();
                    pfd.dwFlags &= ~Win32.PFD_DOUBLEBUFFER;
                    HDC = View.CreateGraphics().GetHdc();
                    var index = Win32.ChoosePixelFormat(HDC, pfd);
                    Win32.SetPixelFormat(HDC, index, pfd);
                    HRC = Win32.wglCreateContext(HDC);
                    Win32.wglMakeCurrent(HDC, HRC);
                    var to = new int[1];
                    OpenGL.glGenTextures(1, to);
                    Texture = to[0];
                    var fbo = new int[1];
                    OpenGL.GenFramebuffers(1, fbo);
                    OpenGL.BindFramebuffer(OpenGL.GL_READ_FRAMEBUFFER, fbo[0]);
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
                OpenGL.glClear(OpenGL.GL_COLOR_BUFFER_BIT);
                DrawScene();
            }
        }

        public bool IsInited;
        internal void Init()
        {
            if (!IsInited)
            {
                //OpenGL.glEnable(OpenGL.GL_TEXTURE_2D);
                //OpenGL.glBindTexture(OpenGL.GL_TEXTURE_2D, TextureDisplayList);
                //var image = View.FrameBuffer;
                //if (image != null)
                //    OpenGL.glTexImage2D(OpenGL.GL_TEXTURE_2D, 0, 4, image.Width, image.Height, 0, OpenGL.GL_BGRA_EXT, OpenGL.GL_UNSIGNED_BYTE, IntPtr.Zero);
                //OpenGL.glTexParameteri(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MAG_FILTER, OpenGL.GL_NEAREST);
                //OpenGL.glTexParameteri(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MIN_FILTER, OpenGL.GL_NEAREST);
                //DisplayList = OpenGL.glGenLists(1);
                //OpenGL.glNewList(DisplayList, OpenGL.GL_COMPILE_AND_EXECUTE);
                //OpenGL.glBegin(OpenGL.GL_QUADS);
                //OpenGL.glTexCoord2f(0, 1); OpenGL.glVertex2f(-1, -1);
                //OpenGL.glTexCoord2f(1, 1); OpenGL.glVertex2f(+1, -1);
                //OpenGL.glTexCoord2f(1, 0); OpenGL.glVertex2f(+1, +1);
                //OpenGL.glTexCoord2f(0, 0); OpenGL.glVertex2f(-1, +1);
                //OpenGL.glEnd();
                //OpenGL.glEndList();

                IsInited = true;
            }
        }

        internal void DrawScene()
        {
            var image = View.FrameBuffer;
            if (image == null) return;
            OpenGL.glBindTexture(OpenGL.GL_TEXTURE_2D, Texture);
            OpenGL.glTexImage2D(OpenGL.GL_TEXTURE_2D, 0, 4, image.Width, image.Height, 0, OpenGL.GL_BGRA, OpenGL.GL_UNSIGNED_BYTE, image.Pixels);
            OpenGL.FramebufferTexture2D(OpenGL.GL_READ_FRAMEBUFFER, OpenGL.GL_COLOR_ATTACHMENT0, OpenGL.GL_TEXTURE_2D, Texture, 0);
            OpenGL.BlitFramebuffer(0, image.Height, image.Width, -1, 0, 0, View.Width, View.Height, OpenGL.GL_COLOR_BUFFER_BIT, OpenGL.GL_NEAREST);
        }

        //public void RenderImage()
        //{
        //    var image = View.FrameBuffer;
        //    if (image == null) return;
        //    OpenGL.glPixelZoom((float)View.Width / image.Width, -(float)View.Height / image.Height);
        //    OpenGL.glRasterPos2f(-1, 1);
        //    OpenGL.glDrawPixels(image.Width, image.Height, OpenGL.GL_BGRA_EXT, OpenGL.GL_UNSIGNED_BYTE, image.Pixels);
        //}
    }
}
