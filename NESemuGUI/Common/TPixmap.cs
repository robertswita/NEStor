using System;
using System.Collections.Generic;
using System.Text;

namespace Common
{
    public class TPixmap
    {
        public uint[] Pixels;
        public int Width;
        public int Height;
        public uint this[int x, int y]
        {
            get { return Pixels[y * Width + x]; }
            set { Pixels[y * Width + x] = value; }
        }
        public TPixmap(int width, int height)
        {
            Width = width;
            Height = height;
            Pixels = new uint[Width * Height];
        }

        public TPixmap HorzCat(TPixmap other)
        {
            var result = new TPixmap(Width + other.Width, Height);
            for (int row = 0; row < result.Height; row++)
            {
                Array.Copy(Pixels, row * Width, result.Pixels, row * result.Width, Width);
                Array.Copy(other.Pixels, row * other.Width, result.Pixels, row * result.Width + Width, other.Width);
            }
            return result;
        }

        public TPixmap VertCat(TPixmap other)
        {
            var result = new TPixmap(Width, Height + other.Height);
            Array.Copy(Pixels, result.Pixels, Pixels.Length);
            Array.Copy(other.Pixels, 0, result.Pixels, Pixels.Length, other.Pixels.Length);
            return result;
        }


        //protected void SetPixel(ref uint[] arr, int width, int x, int y, uint newValue, int height = 0)
        //{
        //    if (height != 0)
        //    {
        //        if (x >= 0 && x < width && y >= 0 && y < height)
        //        {
        //            arr[(y * width) + x] = newValue;
        //        }
        //    } else
        //    {
        //        arr[(y * width) + x] = newValue;
        //    }
        //}

        //protected UInt32 GetPixel(ref UInt32[] arr, int width, int x, int y)
        //{
        //    return arr[(y * width) + x];
        //}

        /// <summary>
        /// Function is specifically designed to concatenate 1D arrays with known dimensions. :)
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="concatenationDirection">true==vertically, false==horizontally</param>
        /// <returns></returns>
        //protected uint[] Join1DArraysWithKnownSize(uint[] a, uint[] b, int width, int height, bool concatenationDirection)
        //{
        //    var c = new uint[width * height * 2];
        //    if (concatenationDirection == false)
        //    {
        //        for (var i = 0; i < height; i++)
        //        {
        //            var offset = i * (width + width);
        //            var a_offset = i * width;
        //            var b_offset = offset + width;
        //            for (var j = 0; j < width; j++)
        //            {
        //                c[offset + j] = a[a_offset + j];
        //                c[b_offset + j] = b[a_offset + j];
        //            }
        //        }
        //    }
        //    else
        //    {
        //        for (var i = 0; i < a.Length; i++)
        //        {
        //            c[i] = a[i];
        //        }
        //        for (var i = 0; i < b.Length; i++)
        //        {
        //            c[i + a.Length] = b[i];
        //        }
        //    }

        //    return c;
        //}
    }
}
