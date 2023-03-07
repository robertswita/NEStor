using System;
using System.Collections.Generic;
using System.Text;

namespace NES
{
    /// <summary>
    /// coarse_x : 5
    /// coarse_y : 5
    /// nametable_x : 1
    /// nametable_y : 1
    /// fine_y : 3
    /// unused : 1
    /// </summary>
    public class PPUInternalRegister
    {
        public int reg;

        int Get(int shift, int mask) { return (reg >> shift) & mask; }
        void Set(int value, int shift, int mask) 
        {
            reg &= ~(mask << shift);
            reg |= (value & mask) << shift;
        }

        //DAFB
        //1101 1010 1111 1011
        //       10 111
        public int coarse_x
        {
            get { return Get(0, 0x1F); }
            set { Set(value, 0, 0x1F); }
        }

        public int coarse_y
        {
            get { return Get(5, 0x1F); }
            set { Set(value, 5, 0x1F); }
        }

        public int nametable_x
        {
            get { return Get(10, 0x1); }
            set { Set(value, 10, 0x1); }
        }

        public int nametable_y
        {
            get { return Get(11, 0x1); }
            set { Set(value, 11, 0x1); }
        }

        public int fine_y
        {
            get { return Get(12, 0x7); }
            set { Set(value, 12, 0x7); }
        }

        public int unused
        {
            get { return Get(15, 0x1); }
            set { Set(value, 15, 0x1); }
        }
    }
}
