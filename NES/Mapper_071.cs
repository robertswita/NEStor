using System;

namespace NES
{
    class Mapper_071 : Mapper_000
    {
        bool Bf9097Mode;
        public override void Poke(int addr, byte data)
        {
            if (addr == 0x9000) //Firehawk uses $9000 to change mirroring
                Bf9097Mode = true;
            if (addr < 0xC000 && Bf9097Mode)
                Mirroring = (data & 0x10) != 0 ? MirrorType.OneScreenHi : MirrorType.OneScreenLo;
            else
                Bus.PrgMemory.Swap(0, data & 0xF);
        }
    }
}
