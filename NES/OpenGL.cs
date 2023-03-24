using System;
using System.Runtime.InteropServices;

namespace Common
{
	/// <summary>
	/// The OpenGL class wraps Suns OpenGL 3D library.
	/// </summary>
	public static class OpenGL
	{
		#region The OpenGL constant definitions.

		//   OpenGL Version Identifier
		public const int GL_VERSION_1_1 = 1;

		//   DataType
		public const int GL_BYTE = 0x1400;
		public const int GL_UNSIGNED_BYTE = 0x1401;
		public const int GL_SHORT = 0x1402;
		public const int GL_UNSIGNED_SHORT = 0x1403;
		public const int GL_INT = 0x1404;
		public const int GL_UNSIGNED_INT = 0x1405;
		public const int GL_FLOAT = 0x1406;
		public const int GL_2_BYTES = 0x1407;
		public const int GL_3_BYTES = 0x1408;
		public const int GL_4_BYTES = 0x1409;
		public const int GL_DOUBLE = 0x140A;

		//   PixelFormat
		public const int GL_COLOR_INDEX = 0x1900;
		public const int GL_STENCIL_INDEX = 0x1901;
		public const int GL_DEPTH_COMPONENT = 0x1902;
		public const int GL_RED = 0x1903;
		public const int GL_GREEN = 0x1904;
		public const int GL_BLUE = 0x1905;
		public const int GL_ALPHA = 0x1906;
		public const int GL_RGB = 0x1907;
		public const int GL_RGBA = 0x1908;
		public const int GL_LUMINANCE = 0x1909;
		public const int GL_LUMINANCE_ALPHA = 0x190A;

		/*EXT_bgra*/
		public const int GL_BGR = 0x80E0;
		public const int GL_BGRA = 0x80E1;

		#endregion

		#region The OpenGL DLL Functions (Exactly the same naming).

		public const string GL_DLL = "opengl32.dll";

		[DllImport(GL_DLL)] public static extern void glDrawPixels(int width, int height, int format, int type, int[] pixels);
		[DllImport(GL_DLL)] public static extern void glPixelZoom(float xfactor, float yfactor);
		[DllImport(GL_DLL)] public static extern void glRasterPos2f(float x, float y);
		[DllImport(GL_DLL)] public static extern void glViewport(int x, int y, int width, int height);

		#endregion

	}
}
