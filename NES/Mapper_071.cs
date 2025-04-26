using System;

namespace NES
{
    class Mapper_071 : Mapper_000
    {
        public override void Poke(int addr, byte data)
        {
            if (addr >= 0xC000)
                Bus.PrgMemory.Swap(0, data & 0xF);
            else if (SubId == 1 && addr >= 0x8000 && addr <= 0x9FFF)
                Mirroring = (data & 0x10) != 0 ? MirrorType.OneScreenHi : MirrorType.OneScreenLo;
        }
    }
}
