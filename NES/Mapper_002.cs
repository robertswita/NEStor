using System;
using System.Collections.Generic;
using System.Text;

namespace NES
{
//CPU $8000-$BFFF: 16 KB switchable PRG ROM bank
//CPU $C000-$FFFF: 16 KB PRG ROM bank, fixed to the last bank
//	7  bit  0
//---- ----
//xxxx pPPP
//     ||||
//     ++++- Select 16 KB PRG ROM bank for CPU $8000-$BFFF
//		  (UNROM uses bits 2-0; UOROM uses bits 3-0)
	class Mapper_002: Mapper_000
    {
        public override void CpuMapWrite(int addr, byte data)
        {
            Bus.PRGMemory[0] = PRGBanks[data & 0xF];
        }
    }
}
