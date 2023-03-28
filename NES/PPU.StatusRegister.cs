using System;
using System.Collections.Generic;
using System.Text;

namespace PPU
{
    // $2002 | R   | PPUSTATUS
    //       | 0-4 | Unknown (???)
    //       |   5 | Sprite Overflow Flag, more than 8 sprites on the line
    //       |   6 | Sprite0 Hit Flag, 1 = PPU rendering has hit sprite #0
    //       |     | This flag resets to 0 when VBlank starts, or CPU reads $2002
    //       |   7 | VBlank Flag, 1 = PPU is generating a Vertical Blanking Impulse
    //       |     | This flag resets to 0 when VBlank ends, or CPU reads $2002
    public class StatusRegister
    {
        public bool SpriteOverflow;
        public bool SpriteZeroHit;
        public bool VerticalBlank;
        public int Register
        {
            get
            {
                var result = 0;
                if (SpriteOverflow) result |= 0x20;
                if (SpriteZeroHit)  result |= 0x40;
                if (VerticalBlank)  result |= 0x80;
                return result;
            }
            set
            {
                SpriteOverflow  = (value & 0x20) != 0;
                SpriteZeroHit   = (value & 0x40) != 0;
                VerticalBlank   = (value & 0x80) != 0;
            }
        }
    }
}
