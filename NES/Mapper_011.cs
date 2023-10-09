using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NES
{
    class Mapper_011: Mapper_000
    {
        public override void CpuMapWrite(int addr, byte data)
        {
            var chrBank = (data >> 4) << 1;
            ChrMap[0] = chrBank;
            ChrMap[1] = chrBank | 1;
            var prgBank = (data & 0x3) << 1;
            PrgMap[0] = prgBank;
            PrgMap[1] = prgBank | 1;
        }
    }
}
