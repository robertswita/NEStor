using System;

namespace NES
{
    class Mapper_011 : Mapper_000
    {
        public override void Poke(int addr, byte data)
        {
            if (addr <= 0x8000) return;
            var chrBank = (data >> 4) << 1;
            Bus.ChrMemory.Swap(0, chrBank);
            Bus.ChrMemory.Swap(1, chrBank | 1);
            var prgBank = (data & 0x3) << 1;
            Bus.PrgMemory.Swap(0, prgBank);
            Bus.PrgMemory.Swap(1, prgBank | 1);
        }
    }
}
