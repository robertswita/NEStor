using System;

namespace NEStor.Core.Ppu
{
	// $2001 | RW  | PPUMASK
	//       |   0 | Greyscale
	//       |   1 | BG Mask, 0 = don't show background in left 8 columns
	//       |   2 | Sprite Mask, 0 = don't show sprites in left 8 columns
	//       |   3 | BG Switch, 1 = show background, 0 = hide background
	//       |   4 | Sprites Switch, 1 = show sprites, 0 = hide sprites
	//       | 5-7 | Enhance RGB
	public class MaskRegister
    {
		public bool Greyscale;
		public bool RenderBackgroundLeft;
		public bool RenderSpritesLeft;
		public bool RenderBackground;
		public bool RenderSprites;
		public bool EnhanceRed;
		public bool EnhanceGreen;
		public bool EnhanceBlue;
		public int Register
        {
			get
            {
				var value = 0;
				if (Greyscale) value |= 0x01;
				if (RenderBackgroundLeft) value |= 0x02;
				if (RenderSpritesLeft) value |= 0x04;
				if (RenderBackground) value |= 0x08;
				if (RenderSprites) value |= 0x10;
				if (EnhanceRed) value |= 0x20;
				if (EnhanceGreen) value |= 0x40;
				if (EnhanceBlue) value |= 0x80;
				return value;
			}
			set
            {
				Greyscale = (value & 0x01) != 0;
				RenderBackgroundLeft = (value & 0x02) != 0;
				RenderSpritesLeft = (value & 0x04) != 0;
				RenderBackground = (value & 0x08) != 0;
				RenderSprites = (value & 0x10) != 0;
				EnhanceRed = (value & 0x20) != 0;
				EnhanceGreen = (value & 0x40) != 0;
				EnhanceBlue = (value & 0x80) != 0;
			}
		}
	}
}
