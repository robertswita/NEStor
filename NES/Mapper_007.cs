using System;
using System.Collections.Generic;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace NES
{
	class Mapper_007 : Mapper
	{
		public override void Poke(int addr, byte data)
        {
            if (addr < 0x8000) return;
            var prgBankNo = (data & 0xF) << 1;
            Bus.PrgMemory.Swap(0, prgBankNo);
            Bus.PrgMemory.Swap(1, prgBankNo + 1);
            Mirroring = (data & 0x10) != 0 ? MirrorType.OneScreenHi : MirrorType.OneScreenLo;
        }

    }
}
