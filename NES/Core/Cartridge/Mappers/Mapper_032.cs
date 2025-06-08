using NES;
using System;

namespace NEStor.Core.Cartridge.Mappers
{
    class Mapper_032 : Mapper
    {
        int[] PrgRegs = new int[2];
        int PrgMode;

        public override void Reset()
        {
            Bus.PrgMemory.BankSize = 0x2000;
            Bus.ChrMemory.BankSize = 0x400;
            Bus.PrgMemory.Swap(2, -2);
            Bus.PrgMemory.Swap(3, -1);
            if (SubId == 1)
            {
                //032: 1 Major League
                //CIRAM A10 is tied high (fixed one-screen mirroring) and PRG banking style is fixed as 8+8+16F 
                Mirroring = MirrorType.OneScreenLo;
            }
        }

        public override void Poke(int addr, byte value)
        {
            switch (addr & 0xF000)
            {
                case 0x8000:
                    PrgRegs[0] = value & 0x1F;
                    Bus.PrgMemory.Swap(PrgMode == 0 ? 0 : 2, PrgRegs[0]);
                    break;
                case 0x9000:
                    PrgMode = (value & 0x02) >> 1;
                    if (SubId == 1) PrgMode = 0;
                    Bus.PrgMemory.Swap(1, PrgRegs[1]);
                    var target = new int[2];
                    target[PrgMode] = 2;
                    Bus.PrgMemory.Swap(target[0], -2);
                    Bus.PrgMemory.Swap(target[1], PrgRegs[0]);
                    Mirroring = (value & 0x01) != 0 ? MirrorType.Horizontal : MirrorType.Vertical;
                    break;
                case 0xA000:
                    PrgRegs[1] = value & 0x1F;
                    Bus.PrgMemory.Swap(1, PrgRegs[1]);
                    break;
                case 0xB000:
                    Bus.ChrMemory.Swap(addr & 0x07, value);
                    break;
            }
        }
    }
}
