﻿using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;

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
        public const string MM_DLL = "winmm.dll";

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

        [DllImport("user32")]
        public static extern bool ValidateRect(IntPtr hWnd, IntPtr rc);

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

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct WaveFormatEx
        {
            public short wFormatTag;
            public short nChannels;
            public int nSamplesPerSec;
            public int nAvgBytesPerSec;
            public short nBlockAlign;
            public short wBitsPerSample;
            public short cbSize;

            public WaveFormatEx(int SamplesPerSec, int BitsPerSample, int Channels)
            {
                const short WAVE_FORMAT_PCM = 1;
                wFormatTag = WAVE_FORMAT_PCM;
                nSamplesPerSec = SamplesPerSec;
                nChannels = (short)Channels;
                wBitsPerSample = (short)BitsPerSample;
                nBlockAlign = (short)(Channels * BitsPerSample / 8);
                nAvgBytesPerSec = SamplesPerSec * nBlockAlign;
                cbSize = 0;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct WaveHeader
        {
            public IntPtr lpData;
            public uint dwBufferLength;
            public uint dwBytesRecorded;
            public IntPtr dwUser;
            public uint dwFlags;
            public uint dwLoops;
            public IntPtr lpNext;
            public IntPtr reserved;
            public static readonly int Size = Marshal.SizeOf(typeof(WaveHeader));
            public static WaveHeader FromIntPtr(IntPtr p)
            {
                return (WaveHeader)Marshal.PtrToStructure(p, typeof(WaveHeader));
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct WaveOutCaps
        {
            public ushort wMid; // Manufacturer identifier
            public ushort wPid; // Product identifier 
            public uint vDriverVersion;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string szPname;
            public CapsFormat dwFormats;
            public ushort wChannels;
            public ushort wReserved1;
            public CapsSupport dwSupport;
            public static readonly int Size = Marshal.SizeOf(typeof(WaveOutCaps));
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct WaveInCaps
        {
            public ushort wMid; // Manufacturer identifier
            public ushort wPid; // Product identifier 
            public uint vDriverVersion;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string szPname;
            public CapsFormat dwFormats;
            public ushort wChannels;
            public ushort wReserved1;
            public static readonly int Size = Marshal.SizeOf(typeof(WaveInCaps));
        }

        [Flags]
        public enum CapsFormat : uint
        {
            WAVE_INVALIDFORMAT = 0x00000000,  // invalid format
            WAVE_FORMAT_1M08 = 0x00000001,    // 11.025 kHz, Mono,   8-bit
            WAVE_FORMAT_1S08 = 0x00000002,    // 11.025 kHz, Stereo, 8-bit
            WAVE_FORMAT_1M16 = 0x00000004,    // 11.025 kHz, Mono,   16-bit
            WAVE_FORMAT_1S16 = 0x00000008,    // 11.025 kHz, Stereo, 16-bit
            WAVE_FORMAT_2M08 = 0x00000010,    // 22.05  kHz, Mono,   8-bit
            WAVE_FORMAT_2S08 = 0x00000020,    // 22.05  kHz, Stereo, 8-bit
            WAVE_FORMAT_2M16 = 0x00000040,    // 22.05  kHz, Mono,   16-bit
            WAVE_FORMAT_2S16 = 0x00000080,    // 22.05  kHz, Stereo, 16-bit
            WAVE_FORMAT_4M08 = 0x00000100,    // 44.1   kHz, Mono,   8-bit
            WAVE_FORMAT_4S08 = 0x00000200,    // 44.1   kHz, Stereo, 8-bit
            WAVE_FORMAT_4M16 = 0x00000400,    // 44.1   kHz, Mono,   16-bit
            WAVE_FORMAT_4S16 = 0x00000800,    // 44.1   kHz, Stereo, 16-bit

            WAVE_FORMAT_44M08 = 0x00000100,   // 44.1   kHz, Mono,   8-bit
            WAVE_FORMAT_44S08 = 0x00000200,   // 44.1   kHz, Stereo, 8-bit
            WAVE_FORMAT_44M16 = 0x00000400,   // 44.1   kHz, Mono,   16-bit
            WAVE_FORMAT_44S16 = 0x00000800,   // 44.1   kHz, Stereo, 16-bit
            WAVE_FORMAT_48M08 = 0x00001000,   // 48     kHz, Mono,   8-bit
            WAVE_FORMAT_48S08 = 0x00002000,   // 48     kHz, Stereo, 8-bit
            WAVE_FORMAT_48M16 = 0x00004000,   // 48     kHz, Mono,   16-bit
            WAVE_FORMAT_48S16 = 0x00008000,   // 48     kHz, Stereo, 16-bit
            WAVE_FORMAT_96M08 = 0x00010000,   // 96     kHz, Mono,   8-bit
            WAVE_FORMAT_96S08 = 0x00020000,   // 96     kHz, Stereo, 8-bit
            WAVE_FORMAT_96M16 = 0x00040000,   // 96     kHz, Mono,   16-bit
            WAVE_FORMAT_96S16 = 0x00080000,   // 96     kHz, Stereo, 16-bit
        }

        [Flags]
        public enum CapsSupport : uint
        {
            WAVECAPS_PITCH = 0x0001, // supports pitch control
            WAVECAPS_PLAYBACKRATE = 0x0002, // supports playback rate control
            WAVECAPS_VOLUME = 0x0004, // supports volume control
            WAVECAPS_LRVOLUME = 0x0008, // separate left-right volume control
            WAVECAPS_SYNC = 0x0010,
            WAVECAPS_SAMPLEACCURATE = 0x0020,
        }

        public const int MMSYSERR_NOERROR = 0x00;
        public const int MMSYSERR_BADDEVICEID = 0x02;
        public const int MMSYSERR_ALLOCATED = 0x04;
        public const int MMSYSERR_INVALHANDLE = 0x05;
        public const int MMSYSERR_NODRIVER = 0x06;
        public const int MMSYSERR_NOMEM = 0x07;
        public const int MMSYSERR_HANDLEBUSY = 0x0C;
        public const int WAVERR_BADFORMAT = 0x20;
        public const int WAVERR_STILLPLAYING = 0x21;
        public const int WAVERR_UNPREPARED = 0x22;

        public const int CALLBACK_WINDOW = 0x00010000;
        public const int CALLBACK_THREAD = 0x00020000;
        public const int CALLBACK_FUNCTION = 0x00030000;

        [DllImport(MM_DLL)] public static extern int waveOutOpen(out IntPtr phwo, uint uDeviceID, ref WaveFormatEx pwfx, WaveOutProc/*IntPtr*/ dwCallback, IntPtr dwCallbackInstance, int fdwOpen);

        public delegate void WaveOutProc(IntPtr hdrvr, WaveOutMessage uMsg, IntPtr dwUser, IntPtr/*int*/ dwParam1, int dwParam2);
        public enum WaveOutMessage { Open = 0x3BB, Close = 0x3BC, Done = 0x3BD }

        [DllImport(MM_DLL)] public static extern int waveOutGetErrorText(int mmrError, StringBuilder pszText, int cchtext);
        [DllImport(MM_DLL)] public static extern int waveOutPrepareHeader(IntPtr hwo, IntPtr pwh, int cbwh);
        [DllImport(MM_DLL)] public static extern int waveOutWrite(IntPtr hwo, IntPtr pwh, int cbwh);
        [DllImport(MM_DLL)] public static extern int waveOutUnprepareHeader(IntPtr hwo, IntPtr pwh, int cbwh);
        [DllImport(MM_DLL)] public static extern int waveOutReset(IntPtr hwo);
        [DllImport(MM_DLL)] public static extern int waveOutClose(IntPtr hwo);
        [DllImport(MM_DLL)] public static extern uint waveOutGetNumDevs();
        [DllImport(MM_DLL)] public static extern int waveOutGetDevCaps(uint uDeviceID, out WaveOutCaps pwoc, int cbwoc);

        public delegate void WaveInProc(IntPtr hdrvr, WaveInMessage uMsg, IntPtr dwUser, IntPtr/*int*/ dwParam1, int dwParam2);
        public enum WaveInMessage { Open = 0x3BE, Close = 0x3BF, Data = 0x3C0 }
        [DllImport(MM_DLL)] public static extern int waveInGetErrorText(int mmrError, StringBuilder pszText, int cchtext);
        [DllImport(MM_DLL)] public static extern uint waveInGetNumDevs();
        [DllImport(MM_DLL)] public static extern int waveInGetDevCaps(uint uDeviceID, out WaveInCaps pwic, int cbwic);
        [DllImport(MM_DLL)] public static extern int waveInOpen(out IntPtr phwi, uint uDeviceID, ref WaveFormatEx pwfx, WaveInProc/*IntPtr*/ dwCallback, IntPtr dwCallbackInstance, int fdwOpen);
        [DllImport(MM_DLL)] public static extern int waveInPrepareHeader(IntPtr hwi, ref WaveHeader pwh, int cbwh);
        [DllImport(MM_DLL)] public static extern int waveInPrepareHeader(IntPtr hwi, IntPtr pwh, int cbwh);
        [DllImport(MM_DLL)] public static extern int waveInAddBuffer(IntPtr hwi, ref WaveHeader pwh, int cbwh);
        [DllImport(MM_DLL)] public static extern int waveInAddBuffer(IntPtr hwi, IntPtr pwh, int cbwh);
        [DllImport(MM_DLL)] public static extern int waveInUnprepareHeader(IntPtr hwi, ref WaveHeader pwh, int cbwh);
        [DllImport(MM_DLL)] public static extern int waveInUnprepareHeader(IntPtr hwi, IntPtr pwh, int cbwh);
        [DllImport(MM_DLL)] public static extern int waveInStart(IntPtr hwi);
        [DllImport(MM_DLL)] public static extern int waveInReset(IntPtr hwi);
        [DllImport(MM_DLL)] public static extern int waveInClose(IntPtr hwi);
        [DllImport(MM_DLL)] public static extern uint timeBeginPeriod(uint uMilliseconds);
        [DllImport(MM_DLL)] public static extern uint timeEndPeriod(uint uMilliseconds);

    }
}
