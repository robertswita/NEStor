using System;

namespace NEStor.Core.Cartridge.Mappers
{
    internal class Mapper_227 : Mapper
    {
        public override void Poke(int addr, byte value)
        {
            int prgBank = addr >> 2 & 0x1F | (addr & 0x100) >> 3;
            bool sFlag = (addr & 0x01) == 0x01;
            bool lFlag = (addr >> 9 & 0x01) == 0x01;
            bool prgMode = (addr >> 7 & 0x01) == 0x01;

            if (prgMode)
            {
                if (sFlag)
                {
                    Bus.PrgMemory.Swap(0, prgBank & 0xFE);
                    Bus.PrgMemory.Swap(1, prgBank & 0xFE | 1);
                }
                else
                {
                    Bus.PrgMemory.Swap(0, prgBank);
                    Bus.PrgMemory.Swap(1, prgBank);
                }
            }
            else
            {
                if (sFlag)
                {
                    if (lFlag)
                    {
                        Bus.PrgMemory.Swap(0, prgBank & 0x3E);
                        Bus.PrgMemory.Swap(1, prgBank | 0x07);
                    }
                    else
                    {
                        Bus.PrgMemory.Swap(0, prgBank & 0x3E);
                        Bus.PrgMemory.Swap(1, prgBank & 0x38);
                    }
                }
                else
                {
                    if (lFlag)
                    {
                        Bus.PrgMemory.Swap(0, prgBank);
                        Bus.PrgMemory.Swap(1, prgBank | 0x07);
                    }
                    else
                    {
                        Bus.PrgMemory.Swap(0, prgBank);
                        Bus.PrgMemory.Swap(1, prgBank & 0x38);
                    }
                }
            }
            Mirroring = (addr & 0x02) == 0x02 ? MirrorType.Horizontal : MirrorType.Vertical;
        }

    }
}
