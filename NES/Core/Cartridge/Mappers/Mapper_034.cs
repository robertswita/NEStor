using NES;
using System;

namespace NEStor.Core.Cartridge.Mappers
{
    class Mapper_034 : Mapper
    {
        public override void Reset()
        {
            Bus.PrgMemory.BankSize = 0x8000;
            Bus.ChrMemory.BankSize = 0x1000;
            PrgRamEnabled = false;
        }

        public override void Poke(int addr, byte value)
        {
            if (Id == 34)
            {
                if (SubId == 0 && ChrRamEnabled || SubId == 2)
                    Bus.PrgMemory.Swap(0, value);
                else switch (addr)
                    {
                        case 0x7FFD: Bus.PrgMemory.Swap(0, value & 0x01); break;
                        case 0x7FFE: Bus.ChrMemory.Swap(0, value & 0x0F); break;
                        case 0x7FFF: Bus.ChrMemory.Swap(1, value & 0x0F); break;
                    }
            }
            else if ((addr & 0xE100) == 0x4100)
            {
                var prgBank = value >> 3;
                var chrBank = value & 7;
                if (Id == 113)
                {
                    chrBank |= prgBank & 8;
                    prgBank &= 7;
                    Mirroring = (value & 0x80) != 0 ? MirrorType.Vertical : MirrorType.Horizontal;
                }
                else
                    prgBank &= 1;
                chrBank <<= 1;
                Bus.PrgMemory.Swap(0, prgBank);
                Bus.ChrMemory.Swap(0, chrBank);
                Bus.ChrMemory.Swap(1, chrBank | 1);
            }
            if (addr >= 0x6000 && addr < 0x8000)
                Bus.WRam[addr & 0x1FFF] = value;
        }
    }
}
