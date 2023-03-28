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
		public bool IncrementMode;
		public int PatternSprite;
		public int PatternBg;
		public bool SpriteSize16;
		public bool SlaveMode;
		public bool EnableNMI;
		public int Register
        {
            get
            {
				var value = NameTableX | NameTableY << 1 | PatternSprite << 3 | PatternBg << 4;
				if (IncrementMode) value |= 0x04;
                if (SpriteSize16) value |= 0x20;
                if (SlaveMode) value |= 0x40;
				if (EnableNMI) value |= 0x80;
				return value;
			}
			set
            {
				NameTableX = value & 0x1;
				NameTableY = (value >> 1) & 0x1;
				PatternSprite = (value >> 3) & 0x1;
				PatternBg = (value >> 4) & 0x1;
				IncrementMode = (value & 0x04) != 0;
                SpriteSize16 = (value & 0x20) != 0;
                SlaveMode = (value & 0x40) != 0;
				EnableNMI = (value & 0x80) != 0;
			}
		}
	}
}
