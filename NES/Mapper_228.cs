using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NES
{
    class Mapper_228: Mapper_000
    {
        int PrgMode;
        public override void Poke(int addr, byte data)
        {
            if (addr > 0x6000)
            {
                var chrBank = ((addr & 0xF) << 2 | data & 0x3) << 1;
                Bus.ChrMemory.Swap(0, chrBank);
                Bus.ChrMemory.Swap(1, chrBank | 1);
                PrgMode = (addr >> 5) & 1;
                var prgBank = (addr >> 6) & 0x7F;
                if ((prgBank & 0x40) != 0)
                    prgBank &= ~0x20;
                Bus.PrgMemory.Swap(0, prgBank);
                Bus.PrgMemory.Swap(1, prgBank | 1);
                if (PrgMode != 0)
                    Bus.PrgMemory.Swap(1, prgBank);
                Mirroring = (addr & 0x2000) != 0 ? MirrorType.Horizontal : MirrorType.Vertical;
            }
        }

    }
}
