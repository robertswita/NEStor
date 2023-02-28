using System;
using System.Collections.Generic;
using System.Text;

namespace emulatorTest.PPU2C02
{
    class Common
    {
        protected void SetPixel(ref uint[] arr, int width, int x, int y, uint newValue, int height = 0)
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
        protected uint[] Join1DArraysWithKnownSize(uint[] a, uint[] b, int width, int height, bool concatenationDirection)
        {
            var c = new uint[width * height * 2];
            if (concatenationDirection == false)
            {
                for (var i = 0; i < height; i++)
                {
                    var offset = i * (width + width);
                    var a_offset = i * width;
                    var b_offset = offset + width;
                    for (var j = 0; j < width; j++)
                    {
                        c[offset + j] = a[a_offset + j];
                        c[b_offset + j] = b[a_offset + j];
                    }
                }
            }
            else
            {
                for (var i = 0; i < a.Length; i++)
                {
                    c[i] = a[i];
                }
                for (var i = 0; i < b.Length; i++)
                {
                    c[i + a.Length] = b[i];
                }
            }

            return c;
        }
    }
}
