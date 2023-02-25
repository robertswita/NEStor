using System;
using System.Collections.Generic;
using System.Text;

namespace NESemuCore.PPU2C02.Flags
{
    /// <summary>
    /// unused : 5
    /// sprite_overflow : 1
    /// sprite_zero_hit : 1
    /// vertical_blank : 1
    /// </summary>
    public class PPUStatusFlags
    {

        public Byte reg = 0x00;

        public Byte unused
        {
            get
            {
                return (Byte)((reg >> 0) & (Byte)0b11111);
            }
            set
            {
                reg = (Byte)(reg & ~((Byte)0b11111 << 0));
                reg = (Byte)(reg | ((value & (Byte)0b11111) << 0));
            }
        }

        public Byte sprite_overflow
        {
            get
            {
                return (Byte)((reg >> 5) & (Byte)1);
            }
            set
            {
                reg = (Byte)(reg & ~((Byte)0b1 << 5));
                reg = (Byte)(reg | ((value & (Byte)0b1) << 5));
            }
        }

        public Byte sprite_zero_hit
        {
            get
            {
                return (Byte)((reg >> 6) & (Byte)1);
            }
            set
            {
                reg = (Byte)(reg & ~((Byte)0b1 << 6));
                reg = (Byte)(reg | ((value & (Byte)0b1) << 6));
            }
        }

        public Byte vertical_blank
        {
            get
            {
                return (Byte)((reg >> 7) & (Byte)1);
            }
            set
            {
                //clear
                reg = (Byte)(reg & ~((Byte)0b1 << 7));
                //set
                reg = (Byte)(reg | ((value & (Byte)0b1) << 7));
            }
        }
    }
}
