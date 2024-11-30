using System;
using System.Collections.Generic;
using System.Text;

namespace NES
{
	class Mapper_066 : Mapper_000
	{
		public override void Poke(int addr, byte data)
		{
			var chrBank = (data & 0x03) << 1;
			Bus.ChrMemory.Swap(0, chrBank);
			Bus.ChrMemory.Swap(1, chrBank | 1);
			var prgBank = (data & 0x30) << 1;
			Bus.PrgMemory.Swap(0, prgBank);
			Bus.PrgMemory.Swap(1, prgBank | 1);
		}

	}
}
