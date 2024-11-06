using System;
using System.Collections.Generic;
using System.Text;

namespace NES
{
	class Mapper_066 : Mapper_000
	{
		public override void CpuMapWrite(int addr, byte data)
		{
			var chrBank = (data & 0x03) << 1;
			ChrMap[0] = chrBank;
			ChrMap[1] = chrBank | 1;
			var prgBank = (data & 0x30) << 1;
			PrgMap[0] = prgBank;
			PrgMap[1] = prgBank | 1;
		}

	}
}
