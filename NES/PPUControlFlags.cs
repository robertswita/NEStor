using System;
using System.Collections.Generic;
using System.Text;

namespace NES
{
    public class PPUControlFlags
    {
		public int reg = 0;

		int Get(int shift, int mask = 1) { return (reg >> shift) & mask; }
		void Set(int value, int shift, int mask = 1)
		{
			reg &= ~(mask << shift);
			reg |= (value & mask) << shift;
		}

		public int nametable_x
		{
			get { return Get(0); }
			set { Set(value, 0); }
		}

		public int nametable_y
		{
			get { return Get(1); }
			set { Set(value, 1); }
		}

		public int increment_mode
		{
			get { return Get(2); }
			set { Set(value, 2); }
		}

		public int pattern_sprite
		{
			get { return Get(3); }
			set { Set(value, 3); }
		}

		public int pattern_bg
		{
			get { return Get(4); }
			set { Set(value, 4); }
		}

		public int sprite_size
		{
			get { return Get(5); }
			set { Set(value, 5); }
		}

		public int slave_mode
		{
			get { return Get(6); }
			set { Set(value, 6); }
		}

		public int enable_nmi
		{
			get { return Get(7); }
			set { Set(value, 7); }
		}
	}
}
