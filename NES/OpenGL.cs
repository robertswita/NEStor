using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace TGL
{
    public static class OpenGL
    {
        public const string GL_DLL = "opengl32.dll";
        [DllImport(GL_DLL)] public static extern void glEnable(int cap);
        [DllImport(GL_DLL)] public static extern void glPolygonMode(int face, int mode);
        [DllImport(GL_DLL)] public static extern void glViewport(int x, int y, int width, int height);
        [DllImport(GL_DLL)] public static extern void glClearColor(float red, float green, float blue, float alpha);
        [DllImport(GL_DLL)] public static extern void glClear(int mask);
        [DllImport(GL_DLL)] public static extern void glBlendFunc(int sfactor, int dfactor);
        [DllImport(GL_DLL)] public static extern void glGenTextures(int n, int[] textures);
        [DllImport(GL_DLL)] public static extern void glBindTexture(int target, int texture);
        [DllImport(GL_DLL)] public static extern void glTexImage2D(int target, int level, int internalformat, int width, int height, int border, int format, int type, IntPtr pixels);
        [DllImport(GL_DLL)] public static extern void glTexImage2D(int target, int level, int internalformat, int width, int height, int border, int format, int type, int[] pixels);
        [DllImport(GL_DLL)] public static extern void glTexSubImage2D(int target, int level, int xoffset, int yoffset, int width, int height, int format, int type, int[] pixels);

        [DllImport(GL_DLL)] public static extern void glDeleteTextures(int n, int[] textures);
        [DllImport(GL_DLL)] public static extern void glDrawArrays(int mode, int first, int count);
        [DllImport(GL_DLL)] public static extern int glGetError();
        [DllImport(GL_DLL)] public static extern void glDrawPixels(int width, int height, int format, int type, int[] pixels);
        [DllImport(GL_DLL)] public static extern void glPixelZoom(float xfactor, float yfactor);
        [DllImport(GL_DLL)] public static extern void glRasterPos2f(float x, float y);


        static Dictionary<string, Delegate> ExtFunctions = new Dictionary<string, Delegate>();
        public static T GetProc<T>() where T : Delegate
        {
            Type delegateType = typeof(T);
            string name = delegateType.Name;
            if (ExtFunctions.TryGetValue(name, out Delegate proc) == false)
            {
                IntPtr procPtr = Win32.wglGetProcAddress(name);
                if (procPtr == IntPtr.Zero)
                    throw new Exception("Extension function " + name + " not supported");
                proc = Marshal.GetDelegateForFunctionPointer(procPtr, delegateType);
                ExtFunctions.Add(name, proc);
            }
            return proc as T;
        }

        //  Delegates
        public delegate bool wglChoosePixelFormatARB(IntPtr HDC, int[] piAttribIList, float[] pfAttribFList, int nMaxFormats, int[] piFormats, int[] nNumFormats);
        public delegate void glActiveTexture(int texture);
        public delegate void glGenBuffers(int n, int[] buffers);
        public delegate void glBindBuffer(int target, int buffer);
        public delegate void glBindBufferBase(int target, int index, int buffer);
        public delegate void glBufferData(int target, int size, IntPtr data, int usage);
        public delegate void glGenVertexArrays(int n, int[] arrays);
        public delegate void glBindVertexArray(int array);
        public delegate void glVertexAttribPointer(int index, int size, int type, bool normalized, int stride, IntPtr pointer);
        public delegate void glEnableVertexAttribArray(int index);
        public delegate int glCreateShader(int type);
        public delegate void glShaderSource(int shader, int count, string[] source, int[] length);
        public delegate void glCompileShader(int shader);
        public delegate void glGetShaderiv(int shader, int pname, int[] parameters);
        public delegate void glGetShaderInfoLog(int shader, int bufSize, IntPtr length, StringBuilder infoLog);
        public delegate int glCreateProgram();
        public delegate void glAttachShader(int program, int shader);
        public delegate void glLinkProgram(int program);
        public delegate void glUseProgram(int program);
        public delegate void glUniform1f(int location, float v0);
        public delegate void glUniform1i(int location, int v0);
        public delegate void glUniformMatrix4fv(int location, int count, bool transpose, float[] value);
        public delegate void glGenerateMipmap(int target);
        public delegate void glGenRenderbuffers(int n, int[] renderbuffers);
        public delegate void glBindRenderbuffer(int target, int renderbuffer);
        public delegate void glRenderbufferStorageMultisample(int target, int samples, int internalformat, int width, int height);
        public delegate void glFramebufferRenderbuffer(int target, int attachment, int renderbuffertarget, int renderbuffer);
        public delegate void glFramebufferTexture2D(int target, int attachment, int texturetarget, int texture, int level);
        public delegate void glGenFramebuffers(int n, int[] framebuffers);
        public delegate void glBindFramebuffer(int target, int framebuffer);
        public delegate void glBlitFramebuffer(int srcX0, int srcY0, int srcX1, int srcY1, int dstX0, int dstY0, int dstX1, int dstY1, int mask, int filter);

        //  Methods
        public static bool ChoosePixelFormat(IntPtr HDC, int[] pixelFormat)
        {
            var numFormats = new int[1];
            int[] attribs = new int[] {
                        WGL_SUPPORT_OPENGL, 1,
                        WGL_DRAW_TO_WINDOW, 1,
                        WGL_ACCELERATION, WGL_FULL_ACCELERATION,
                        WGL_COLOR_BITS, 24,
                        WGL_DEPTH_BITS, 16,
                        WGL_DOUBLE_BUFFER, 1,
                        WGL_SAMPLE_BUFFERS, 1, // MSAA
                        WGL_SAMPLES, 4,
                        WGL_PIXEL_TYPE, WGL_TYPE_RGBA,
                        0
                    };
            return GetProc<wglChoosePixelFormatARB>()(HDC, attribs, null, 1, pixelFormat, numFormats);
        }

        public static glActiveTexture ActiveTexture { get { return GetProc<glActiveTexture>(); } }
        public static glBindBuffer BindBuffer { get { return GetProc<glBindBuffer>(); } }
        public static glGenBuffers GenBuffers { get { return GetProc<glGenBuffers>(); } }
        public static glBufferData BufferData { get { return GetProc<glBufferData>(); } }
        public static void BufferDatafv(int target, float[] data, int usage)
        {
            IntPtr dataPtr = Marshal.AllocHGlobal(data.Length * sizeof(float));
            Marshal.Copy(data, 0, dataPtr, data.Length);
            GetProc<glBufferData>()(target, data.Length * sizeof(float), dataPtr, usage);
            Marshal.FreeHGlobal(dataPtr);
        }
        public static glAttachShader AttachShader { get { return GetProc<glAttachShader>(); } }
        public static glCompileShader CompileShader { get { return GetProc<glCompileShader>(); } }
        public static glCreateProgram CreateProgram { get { return GetProc<glCreateProgram>(); } }
        public static glCreateShader CreateShader { get { return GetProc<glCreateShader>(); } }
        public static glEnableVertexAttribArray EnableVertexAttribArray { get { return GetProc<glEnableVertexAttribArray>(); } }       
        public static glGetShaderiv GetShader { get { return GetProc<glGetShaderiv>(); } }        
        public static glGetShaderInfoLog GetShaderInfoLog { get { return GetProc<glGetShaderInfoLog>(); } }        
        public static glLinkProgram LinkProgram { get { return GetProc<glLinkProgram>(); } }
        public static void ShaderSource(int shader, string source)
        {
            //  Remember, the function takes an array of strings but concatenates them, so we should NOT split into lines!
            GetProc<glShaderSource>()(shader, 1, new[] { source }, new[] { source.Length });
        }
        public static glUseProgram UseProgram { get { return GetProc<glUseProgram>(); } }
        public static glUniform1f Uniform1f { get { return GetProc<glUniform1f>(); } }
        public static glUniform1i Uniform1i { get { return GetProc<glUniform1i>(); } }
        public static glUniformMatrix4fv UniformMatrix4fv { get { return GetProc<glUniformMatrix4fv>(); } }
        public static glVertexAttribPointer VertexAttribPointer { get { return GetProc<glVertexAttribPointer>(); } }
        public static glBindBufferBase BindBufferBase { get { return GetProc<glBindBufferBase>(); } }
        public static glGenerateMipmap GenerateMipmap { get { return GetProc<glGenerateMipmap>(); } }
        public static glBindVertexArray BindVertexArray { get { return GetProc<glBindVertexArray>(); } }
        public static glGenVertexArrays GenVertexArrays { get { return GetProc<glGenVertexArrays>(); } }
        public static glGenRenderbuffers GenRenderbuffers { get { return GetProc<glGenRenderbuffers>(); } }
        public static glBindRenderbuffer BindRenderbuffer { get { return GetProc<glBindRenderbuffer>(); } }
        public static glRenderbufferStorageMultisample RenderbufferStorageMultisample { get { return GetProc<glRenderbufferStorageMultisample>(); } }
        public static glFramebufferRenderbuffer FramebufferRenderbuffer { get { return GetProc<glFramebufferRenderbuffer>(); } }
        public static glFramebufferTexture2D FramebufferTexture2D { get { return GetProc<glFramebufferTexture2D>(); } }
        public static glGenFramebuffers GenFramebuffers { get { return GetProc<glGenFramebuffers>(); } }
        public static glBindFramebuffer BindFramebuffer { get { return GetProc<glBindFramebuffer>(); } }
        public static glBlitFramebuffer BlitFramebuffer { get { return GetProc<glBlitFramebuffer>(); } }

        //  Constants
        public const int GL_TRIANGLES = 0x0004;
        public const int GL_BLEND = 0x0BE2;
        public const int GL_DEPTH_BUFFER_BIT = 0x00000100;
        public const int GL_ACCUM_BUFFER_BIT = 0x00000200;
        public const int GL_STENCIL_BUFFER_BIT = 0x00000400;
        public const int GL_COLOR_BUFFER_BIT = 0x00004000;
        public const int GL_DEPTH_TEST = 0x0B71;
        public const int GL_TEXTURE_1D = 0x0DE0;
        public const int GL_TEXTURE_2D = 0x0DE1;
        public const int GL_TEXTURE0 = 0x84C0;
        public const int GL_BGR = 0x80E0;
        public const int GL_BGRA = 0x80E1;
        public const int GL_RGB8 = 0x8051;
        public const int GL_STATIC_DRAW = 0x88E4;
        public const int GL_DYNAMIC_DRAW = 0x88E8;
        public const int GL_FRAGMENT_SHADER = 0x8B30;
        public const int GL_VERTEX_SHADER = 0x8B31;
        public const int GL_COMPILE_STATUS = 0x8B81;
        public const int GL_INFO_LOG_LENGTH = 0x8B84;
        public const int GL_ARRAY_BUFFER = 0x8892;
        public const int GL_UNIFORM_BUFFER = 0x8A11;
        public const int GL_SHADER_STORAGE_BUFFER = 0x90D2;
        public const int GL_MULTISAMPLE = 0x809D;
        public const int GL_FRAMEBUFFER = 0x8D40;
        public const int GL_RENDERBUFFER = 0x8D41;
        public const int GL_READ_FRAMEBUFFER = 0x8CA8;
        public const int GL_DRAW_FRAMEBUFFER = 0x8CA9;
        public const int GL_COLOR_ATTACHMENT0 = 0x8CE0;
        public const int GL_DEPTH_STENCIL_ATTACHMENT = 0x821A;
        public const int GL_DEPTH24_STENCIL8 = 0x88F0;
        //   DataType
        public const int GL_BYTE = 0x1400;
        public const int GL_UNSIGNED_BYTE = 0x1401;
        public const int GL_SHORT = 0x1402;
        public const int GL_UNSIGNED_SHORT = 0x1403;
        public const int GL_INT = 0x1404;
        public const int GL_FLOAT = 0x1406;
        //   PolygonMode
        public const int GL_POINT = 0x1B00;
        public const int GL_LINE = 0x1B01;
        public const int GL_FILL = 0x1B02;
        //   DrawBufferMode
        public const int GL_NONE = 0;
        public const int GL_FRONT_LEFT = 0x0400;
        public const int GL_FRONT_RIGHT = 0x0401;
        public const int GL_BACK_LEFT = 0x0402;
        public const int GL_BACK_RIGHT = 0x0403;
        public const int GL_FRONT = 0x0404;
        public const int GL_BACK = 0x0405;
        public const int GL_LEFT = 0x0406;
        public const int GL_RIGHT = 0x0407;
        public const int GL_FRONT_AND_BACK = 0x0408;
        public const int GL_AUX0 = 0x0409;
        public const int GL_AUX1 = 0x040A;
        public const int GL_AUX2 = 0x040B;
        public const int GL_AUX3 = 0x040C;
        //  BlendingFactorDest
        public const int GL_ZERO = 0;
        public const int GL_ONE = 1;
        public const int GL_SRC_COLOR = 0x0300;
        public const int GL_ONE_MINUS_SRC_COLOR = 0x0301;
        public const int GL_SRC_ALPHA = 0x0302;
        public const int GL_ONE_MINUS_SRC_ALPHA = 0x0303;
        public const int GL_DST_ALPHA = 0x0304;
        public const int GL_ONE_MINUS_DST_ALPHA = 0x0305;
        public const int GL_POLYGON_SMOOTH = 0x0B41;
        //  Mag filters
        public const int GL_NEAREST = 0x2600;
        public const int GL_LINEAR = 0x2601;
        //  WGL Context
        public const int WGL_DRAW_TO_WINDOW = 0x2001;
        public const int WGL_DRAW_TO_BITMAP = 0x2002;
        public const int WGL_ACCELERATION = 0x2003;
        public const int WGL_SUPPORT_OPENGL = 0x2010;
        public const int WGL_DOUBLE_BUFFER = 0x2011;
        public const int WGL_STEREO = 0x2012;
        public const int WGL_PIXEL_TYPE = 0x2013;
        public const int WGL_COLOR_BITS = 0x2014;
        public const int WGL_DEPTH_BITS = 0x2022;
        public const int WGL_STENCIL_BITS = 0x2023;
        public const int WGL_FULL_ACCELERATION = 0x2027;
        public const int WGL_TYPE_RGBA = 0x202B;
        public const int WGL_SAMPLE_BUFFERS = 0x2041;
        public const int WGL_SAMPLES = 0x2042;

    }
}
