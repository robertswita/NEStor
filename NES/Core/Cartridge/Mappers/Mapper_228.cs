using NES;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NEStor.Core.Cartridge.Mappers
{
    class Mapper_228: Mapper
    {
        public override void Reset()
        {
            //Bus.PrgMemory.Swap(1, -1);
        }
        public override void Poke(int addr, byte data)
        {
            if (addr > 0x6000)
            {
                //var chrBank = (addr & 0xF) << 2 | data & 0x3;
                //Bus.ChrMemory.Swap(0, chrBank);
                //var prgBank = addr >> 6 & 0x7F;
                //if ((prgBank & 0x40) != 0)
                //    prgBank &= ~0x20;
                //Bus.PrgMemory.Swap(0, prgBank);
                //Bus.PrgMemory.Swap(1, prgBank | 1);
                //var prgMode = addr >> 5 & 1;
                //if (prgMode != 0)
                //    Bus.PrgMemory.Swap(1, prgBank);
                //Mirroring = (addr & 0x2000) != 0 ? MirrorType.Horizontal : MirrorType.Vertical;

                var chipSelect = addr >> 11 & 0x03;
                if (chipSelect == 3) chipSelect = 2;
                var prgPage = addr >> 6 & 0x1F | chipSelect << 5;
                if ((addr & 0x20) != 0)
                {
                    Bus.PrgMemory.Swap(0, prgPage);
                    Bus.PrgMemory.Swap(1, prgPage);
                }
                else
                {
                    prgPage &= 0xFE;
                    Bus.PrgMemory.Swap(0, prgPage);
                    Bus.PrgMemory.Swap(1, prgPage | 1);
                }
                Bus.ChrMemory.Swap(0, (addr & 0x0F) << 2 | data & 0x03);
                Mirroring = (addr & 0x2000) != 0 ? MirrorType.Horizontal : MirrorType.Vertical;
            }
        }

    }
}
