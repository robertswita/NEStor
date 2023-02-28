using System;

namespace emulatorTest.PPU2C02.Flags
{
    public class PPUMaskFlags
    {
        public int reg = 0;
		int Get(int shift, int mask = 1) { return (reg >> shift) & mask; }
		void Set(int value, int shift, int mask = 1)
		{
			reg &= ~(mask << shift);
			reg |= (value & mask) << shift;
		}

		public int greyscale
		{
			get { return Get(0); }
			set { Set(value, 0); }
		}

		public int render_background_left
		{
			get { return Get(1); }
			set { Set(value, 1); }
		}

		public int render_sprites_left
		{
			get { return Get(2); }
			set { Set(value, 2); }
		}

		public int render_background
		{
			get { return Get(3); }
			set { Set(value, 3); }
		}

		public int render_sprites
		{
			get { return Get(4); }
			set { Set(value, 4); }
		}

		public int enhance_red
		{
			get { return Get(5); }
			set { Set(value, 5); }
		}

		public int enhance_green
		{
			get { return Get(6); }
			set { Set(value, 6); }
		}

		public int enhance_blue
		{
			get { return Get(7); }
			set { Set(value, 7); }
		}
	}
}
