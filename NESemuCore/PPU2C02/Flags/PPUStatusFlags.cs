using System;
using System.Collections.Generic;
using System.Text;

namespace emulatorTest.PPU2C02.Flags
{
    /// <summary>
    /// unused : 5
    /// sprite_overflow : 1
    /// sprite_zero_hit : 1
    /// vertical_blank : 1
    /// </summary>
    public class PPUStatusFlags
    {
        public int reg;

        int Get(int shift, int mask) { return (reg >> shift) & mask; }
        void Set(int value, int shift, int mask)
        {
            reg &= ~(mask << shift);
            reg |= (value & mask) << shift;
        }

        public int unused
        {
            get { return Get(0, 0x1F); }
            set { Set(value, 0, 0x1F); }
        }

        public int sprite_overflow
        {
            get { return Get(5, 0x1); }
            set { Set(value, 5, 0x1); }
        }

        public int sprite_zero_hit
        {
            get { return Get(6, 0x1); }
            set { Set(value, 6, 0x1); }
        }

        public int vertical_blank
        {
            get { return Get(7, 0x1); }
            set { Set(value, 7, 0x1); }
        }
    }
}
