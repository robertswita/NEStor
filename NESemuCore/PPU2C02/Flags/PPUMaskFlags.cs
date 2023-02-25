using System;

namespace NESemuCore.PPU2C02.Flags
{
    public class PPUMaskFlags
    {

        public Byte reg = 0x00;

		public Byte greyscale
		{
			get
			{
				return (Byte)((reg >> 0) & (Byte)1);
			}
			set
			{
				reg = (Byte)(reg & ~((Byte)0b1 << 0));
				reg = (Byte)(reg | ((value & (Byte)0b1) << 0));
			}
		}

		public Byte render_background_left
		{
			get
			{
				return (Byte)((reg >> 1) & (Byte)1);
			}
			set
			{
				reg = (Byte)(reg & ~((Byte)0b1 << 1));
				reg = (Byte)(reg | ((value & (Byte)0b1) << 1));
			}
		}

		public Byte render_sprites_left
		{
			get
			{
				return (Byte)((reg >> 2) & (Byte)1);
			}
			set
			{
				reg = (Byte)(reg & ~((Byte)0b1 << 2));
				reg = (Byte)(reg | ((value & (Byte)0b1) << 2));
			}
		}

		public Byte render_background
		{
			get
			{
				return (Byte)((reg >> 3) & (Byte)1);
			}
			set
			{
				reg = (Byte)(reg & ~((Byte)0b1 << 3));
				reg = (Byte)(reg | ((value & (Byte)0b1) << 3));
			}
		}

		public Byte render_sprites
		{
			get
			{
				return (Byte)((reg >> 4) & (Byte)1);
			}
			set
			{
				reg = (Byte)(reg & ~((Byte)0b1 << 4));
				reg = (Byte)(reg | ((value & (Byte)0b1) << 4));
			}
		}

		public Byte enhance_red
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

		public Byte enhance_green
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

		public Byte enhance_blue
		{
			get
			{
				return (Byte)((reg >> 7) & (Byte)0b1);
			}
			set
			{
				reg = (Byte)(reg & ~((Byte)0b1 << 7));
				reg = (Byte)(reg | ((value & (Byte)0b1) << 7));
			}
		}
	}
}
