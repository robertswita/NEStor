using System;
using System.Collections.Generic;
using System.Text;

namespace PPU
{
    // $2000 | RW  | PPUCTRL
    //       | 0-1 | Name Table to show:
    //       |     |
    //       |     |           +-----------+-----------+
    //       |     |           | 2 ($2800) | 3 ($2C00) |
    //       |     |           +-----------+-----------+
    //       |     |           | 0 ($2000) | 1 ($2400) |
    //       |     |           +-----------+-----------+
    //       |     |
    //       |     | Remember, though, that because of the mirroring, there are
    //       |     | only 2 real Name Tables, not 4.
    //       |   2 | Vertical Write, 1 = PPU memory address increments by 32:
    //       |     |
    //       |     |    Name Table, VW=0          Name Table, VW=1
    //       |     |   +----------------+        +----------------+
    //       |     |   |----> write     |        | | write        |
    //       |     |   |                |        | V              |
    //       |     |
    //       |   3 | Sprite Pattern Table address, 1 = $1000, 0 = $0000
    //       |   4 | Screen Pattern Table address, 1 = $1000, 0 = $0000
    //       |   5 | Sprite Size, 1 = 8x16, 0 = 8x8
    //       |   6 | Hit Switch, 1 = generate interrupts on Hit (incorrect ???)
    //       |   7 | VBlank Switch, 1 = generate interrupts on VBlank 
    public class ControlRegister
    {
		public int NameTableX;
		public int NameTableY;
		public int Increment;
		public int PatternSprite;
		public int PatternBg;
		public int SpriteSize;
		public bool SlaveMode;
		public bool EnableNMI;
        public byte DataLatch;
		public int Register
        {
            get
            {
				var value = NameTableX | NameTableY << 1 | PatternSprite << 3 | PatternBg << 4;
				if (Increment > 1) value |= 0x04;
                if (SpriteSize > 8) value |= 0x20;
                if (SlaveMode) value |= 0x40;
				if (EnableNMI) value |= 0x80;
				return value;
			}
			set
            {
                NameTableX = value & 0x1;
                NameTableY = (value & 0x2) >> 1;
                Increment = (value & 0x4) != 0 ? 32 : 1;
                PatternSprite = (value & 0x8) >> 3;
                PatternBg = (value & 0x10) >> 4;
                SpriteSize = (value & 0x20) != 0 ? 16 : 8;
                SlaveMode = (value & 0x40) != 0;
                EnableNMI = (value & 0x80) != 0;
			}
		}
	}
}
