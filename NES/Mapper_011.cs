using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NES
{
    class Mapper_011: Mapper_000
    {
        public override void Poke(int addr, byte data)
        {
            var chrBank = (data >> 4) << 1;
            Bus.ChrMemory.Swap(0, chrBank);
            Bus.ChrMemory.Swap(1, chrBank | 1);
            var prgBank = (data & 0x3) << 1;
            Bus.PrgMemory.Swap(0, prgBank);
            Bus.PrgMemory.Swap(1, prgBank | 1);
        }
    }
}
