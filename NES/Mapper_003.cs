using System;
using System.Collections.Generic;
using System.Text;

namespace NES
{
    class Mapper_003: Mapper_000
    {
		public override void CpuMapWrite(int addr, byte data)
		{
			var bank = (data & 0x3) << 1;
			Bus.PatternTable[0] = CHRBanks[bank];
			Bus.PatternTable[1] = CHRBanks[bank | 0x1];
		}
	}
}
