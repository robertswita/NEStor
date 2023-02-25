using System;
using System.Collections.Generic;
using System.Text;

namespace NESemuCore.PPU2C02
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
        public UInt16 reg = 0x0000;

        //DAFB
        //1101 1010 1111 1011
        //       10 111
        public UInt16 coarse_x
        {
            get
            {
                //zwroc 5 bitow, zaczynajacych sie od 5. najmlodszego bita w dwoch kilbajtach
                return (UInt16)((reg >> 0) & (UInt16)0b11111);
            }
            set
            {
                // podmien 5 bitow, zaczynajacych sie od 5. najmlodszego bita w dwoch kilbajtach
                //clear
                reg = (UInt16)(reg & ~((UInt16)0b11111 << 0));
                //set
                reg = (UInt16)(reg | ((value & (Byte)0b11111) << 0));
            }
        }

        public UInt16 coarse_y
        {
            get
            {
                return (UInt16)((reg >> 5) & (UInt16)0b11111);
            }
            set
            {
                //clear
                reg = (UInt16)(reg & ~((UInt16)0b11111 << 5));
                //set
                reg = (UInt16)(reg | ((value & (Byte)0b11111) << 5));
            }
        }

        public UInt16 nametable_x
        {
            get
            {
                return (UInt16)((reg >> 10) & (UInt16)0b1);
            }
            set
            {
                //clear
                reg = (UInt16)(reg & ~((UInt16)0b1 << 10));
                //set
                reg = (UInt16)(reg | ((value & (Byte)0b1) << 10));
            }
        }

        public UInt16 nametable_y
        {
            get
            {
                return (UInt16)((reg >> 11) & (UInt16)0b1);
            }
            set
            {
                //clear
                reg = (UInt16)(reg & ~((UInt16)0b1 << 11));
                //set
                reg = (UInt16)(reg | ((value & (Byte)0b1) << 11));
            }
        }

        public UInt16 fine_y
        {
            get
            {
                return (UInt16)((reg >> 12) & (UInt16)0b111);
            }
            set
            {
                //clear
                reg = (UInt16)(reg & ~((UInt16)0b111 << 12));
                //set
                reg = (UInt16)(reg | ((value & (Byte)0b111) << 12));
            }
        }

        public UInt16 unused
        {
            get
            {
                return (UInt16)((reg >> 15) & (UInt16)0b1);
            }
            set
            {
                //clear
                reg = (UInt16)(reg & ~((UInt16)0b1 << 15));
                //set
                reg = (UInt16)(reg | ((value & (Byte)0b1) << 15));
            }
        }
    }
}
