using System;
using System.Runtime.InteropServices;

namespace TGL
{
    /// <summary>
    /// Useful functions imported from the Win32 SDK.
    /// </summary>
	public static class Win32
    {
        /// <summary>
        /// Initializes the <see cref="Win32"/> class.
        /// </summary>
        static Win32()
        {
            //  Load the openGL library - without this wgl calls will fail.
            IntPtr glLibrary = LoadLibrary(OpenGL.GL_DLL);
        }

        //  The names of the libraries we're importing.
        public const string KERNEL_DLL = "kernel32.dll";
        public const string GDI_DLL = "gdi32.dll";

        #region Kernel32 Functions

        [DllImport(KERNEL_DLL)]
        public static extern IntPtr LoadLibrary(string lpFileName);

        #endregion

        #region WGL Functions

        /// <summary>
        /// Gets the current render context.
        /// </summary>
        /// <returns>The current render context.</returns>
        [DllImport(OpenGL.GL_DLL)]
        public static extern IntPtr wglGetCurrentContext();

        /// <summary>
        /// Make the specified render context current.
        /// </summary>
        /// <param name="hdc">The handle to the device context.</param>
        /// <param name="hrc">The handle to the render context.</param>
        /// <returns></returns>
        [DllImport(OpenGL.GL_DLL)]
        public static extern bool wglMakeCurrent(IntPtr hdc, IntPtr hrc);

        /// <summary>
        /// Creates a render context from the device context.
        /// </summary>
        /// <param name="hdc">The handle to the device context.</param>
        /// <returns>The handle to the render context.</returns>
        [DllImport(OpenGL.GL_DLL)]
        public static extern IntPtr wglCreateContext(IntPtr hdc);

        /// <summary>
        /// Deletes the render context.
        /// </summary>
        /// <param name="hrc">The handle to the render context.</param>
        /// <returns></returns>
        [DllImport(OpenGL.GL_DLL)]
        public static extern int wglDeleteContext(IntPtr hrc);

        /// <summary>
        /// Gets a proc address.
        /// </summary>
        /// <param name="name">The name of the function.</param>
        /// <returns>The address of the function.</returns>
        [DllImport(OpenGL.GL_DLL)]
        public static extern IntPtr wglGetProcAddress(string name);

        /// <summary>
        /// The wglUseFontBitmaps function creates a set of bitmap display lists for use in the current OpenGL rendering context. The set of bitmap display lists is based on the glyphs in the currently selected font in the device context. You can then use bitmaps to draw characters in an OpenGL image.
        /// </summary>
        /// <param name="hDC">Specifies the device context whose currently selected font will be used to form the glyph bitmap display lists in the current OpenGL rendering context..</param>
        /// <param name="first">Specifies the first glyph in the run of glyphs that will be used to form glyph bitmap display lists.</param>
        /// <param name="count">Specifies the number of glyphs in the run of glyphs that will be used to form glyph bitmap display lists. The function creates count display lists, one for each glyph in the run.</param>
        /// <param name="listBase">Specifies a starting display list.</param>
        /// <returns>If the function succeeds, the return value is TRUE. If the function fails, the return value is FALSE. To get extended error information, call GetLastError.</returns>
        [DllImport(OpenGL.GL_DLL)]
        public static extern bool wglUseFontBitmaps(IntPtr hDC, uint first, uint count, uint listBase);

        /// <summary>
        /// The wglShareLists function enables multiple OpenGL rendering contexts to share a single display-list space
        /// </summary>
        /// <param name="hrc1">OpenGL rendering context with which to share display lists
        /// <param name="hrc2">OpenGL rendering context to share display lists
        /// <returns>If the function succeeds, the return value is TRUE. If the function fails, the return value is FALSE. To get extended error information, call GetLastError.</returns>
        [DllImport(OpenGL.GL_DLL)]
        public static extern bool wglShareLists(IntPtr hrc1, IntPtr hrc2);

        #endregion

        #region PixelFormatDescriptor structure and flags.

        [StructLayout(LayoutKind.Sequential)]
        public class PIXELFORMATDESCRIPTOR
        {
            public ushort nSize;
            public ushort nVersion;
            public uint dwFlags;
            public byte iPixelType;
            public byte cColorBits;
            public byte cRedBits;
            public byte cRedShift;
            public byte cGreenBits;
            public byte cGreenShift;
            public byte cBlueBits;
            public byte cBlueShift;
            public byte cAlphaBits;
            public byte cAlphaShift;
            public byte cAccumBits;
            public byte cAccumRedBits;
            public byte cAccumGreenBits;
            public byte cAccumBlueBits;
            public byte cAccumAlphaBits;
            public byte cDepthBits;
            public byte cStencilBits;
            public byte cAuxBuffers;
            public sbyte iLayerType;
            public byte bReserved;
            public uint dwLayerMask;
            public uint dwVisibleMask;
            public uint dwDamageMask;
            public PIXELFORMATDESCRIPTOR()
            {
                nSize = (ushort)Marshal.SizeOf(this);
                cAlphaBits = 32;
                dwFlags = PFD_SUPPORT_OPENGL | PFD_DOUBLEBUFFER | PFD_STEREO;
            }
        }

        public const byte PFD_TYPE_RGBA = 0;
        public const byte PFD_TYPE_COLORINDEX = 1;

        public const uint PFD_DOUBLEBUFFER = 1;
        public const uint PFD_STEREO = 2;
        public const uint PFD_DRAW_TO_WINDOW = 4;
        public const uint PFD_DRAW_TO_BITMAP = 8;
        public const uint PFD_SUPPORT_GDI = 16;
        public const uint PFD_SUPPORT_OPENGL = 32;
        public const uint PFD_GENERIC_FORMAT = 64;
        public const uint PFD_NEED_PALETTE = 128;
        public const uint PFD_NEED_SYSTEM_PALETTE = 256;
        public const uint PFD_SWAP_EXCHANGE = 512;
        public const uint PFD_SWAP_COPY = 1024;
        public const uint PFD_SWAP_LAYER_BUFFERS = 2048;
        public const uint PFD_GENERIC_ACCELERATED = 4096;
        public const uint PFD_SUPPORT_DIRECTDRAW = 8192;

        public const sbyte PFD_MAIN_PLANE = 0;
        public const sbyte PFD_OVERLAY_PLANE = 1;
        public const sbyte PFD_UNDERLAY_PLANE = -1;

        #endregion

        #region Gdi32 Functions

        //	Unmanaged functions from the Win32 graphics library.
        [DllImport(GDI_DLL, SetLastError = true)]
        public static extern int ChoosePixelFormat(IntPtr hDC,
            [In, MarshalAs(UnmanagedType.LPStruct)] PIXELFORMATDESCRIPTOR ppfd);

        [DllImport(GDI_DLL, SetLastError = true)]
        public static extern int SetPixelFormat(IntPtr hDC, int iPixelFormat,
            [In, MarshalAs(UnmanagedType.LPStruct)] PIXELFORMATDESCRIPTOR ppfd);

        [DllImport(GDI_DLL, SetLastError = true)]
        public static extern int DescribePixelFormat(IntPtr hDC, int iPixelFormat, int pfdSize,
            [In, MarshalAs(UnmanagedType.LPStruct)] PIXELFORMATDESCRIPTOR ppfd);

        [DllImport(GDI_DLL)]
        public static extern int SwapBuffers(IntPtr hDC);

        #endregion

        public const uint CS_VREDRAW = 0x0001;
        public const uint CS_HREDRAW = 0x0002;
        public const uint CS_DBLCLKS = 0x0008;
        public const uint CS_OWNDC = 0x0020;
        public const uint CS_CLASSDC = 0x0040;
        public const uint CS_PARENTDC = 0x0080;
        public const uint CS_NOCLOSE = 0x0200;
        public const uint CS_SAVEBITS = 0x0800;
        public const uint CS_BYTEALIGNCLIENT = 0x1000;
        public const uint CS_BYTEALIGNWINDOW = 0x2000;
        public const uint CS_GLOBALCLASS = 0x4000;
    }
}
