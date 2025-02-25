using System;

namespace NES
{
    class Mapper_002 : Mapper_000
    {
        public override void Poke(int addr, byte data)
        {
            if (addr < 0x8000) return;
            Bus.PrgMemory.Swap(0, data & 0xF);
        }
    }
}
