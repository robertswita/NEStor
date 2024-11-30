using System;
using System.Collections.Generic;
using System.Text;

namespace NES
{
	class Mapper_007 : Mapper_000
	{
		public override void Poke(int addr, byte data)
		{
			var prgBankNo = (data & 0xF) << 1;
			Bus.PrgMemory.Swap(0, prgBankNo);
			Bus.PrgMemory.Swap(1, prgBankNo | 1);
            Mirroring = (data & 0x10) != 0 ? MirrorType.OneScreenHi: MirrorType.OneScreenLo;
        }

	}
}
