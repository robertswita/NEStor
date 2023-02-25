using System;
using System.Collections.Generic;
using System.Text;

namespace emulatorTest.PPU2C02
{
    class Common
    {
        protected void SetPixel(ref UInt32[] arr, int width, int x, int y, UInt32 newValue, int height = 0)
        {
            if (height != 0)
            {
                if (x >= 0 && x < width && y >= 0 && y < height)
                {
                    arr[(y * width) + x] = newValue;
                }
            } else
            {
                arr[(y * width) + x] = newValue;
            }
        }

        protected UInt32 GetPixel(ref UInt32[] arr, int width, int x, int y)
        {
            return arr[(y * width) + x];
        }

        /// <summary>
        /// Function is specifically designed to concatenate 1D arrays with known dimensions. :)
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="concatenationDirection">true==vertically, false==horizontally</param>
        /// <returns></returns>
        protected UInt32[] Join1DArraysWithKnownSize(UInt32[] a, UInt32[] b, uint width, uint height, bool concatenationDirection)
        {
            UInt32[] c = new UInt32[width * height * 2];
            if (concatenationDirection == false)
            {
                for (UInt32 i = 0; i < height; i++)
                {
                    UInt32 offset = i * (width + width);
                    UInt32 a_offset = i * width;
                    UInt32 b_offset = offset + width;
                    for (UInt32 j = 0; j < width; j++)
                    {
                        c[offset + j] = a[a_offset + j];
                        c[b_offset + j] = b[a_offset + j];
                    }
                }
            }
            else
            {
                for (UInt32 i = 0; i < a.Length; i++)
                {
                    c[i] = a[i];
                }
                for (UInt32 i = 0; i < b.Length; i++)
                {
                    c[i + a.Length] = b[i];
                }
            }

            return c;
        }
    }
}
