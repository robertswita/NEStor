using System;

namespace NEStor.Core.Cartridge.Mappers
{
    internal class Mapper_091 : Mapper_004
    {
        public override void Reset()
        {
            base.Reset();
            Bus.ChrMemory.BankSize = 0x800;
            PrgRamEnabled = false;
        }

        public override void Poke(int addr, byte value)
        {
            switch (addr & 0x7003)
            {
                case 0x6000: Bus.ChrMemory.Swap(0, value); break;
                case 0x6001: Bus.ChrMemory.Swap(1, value); break;
                case 0x6002: Bus.ChrMemory.Swap(2, value); break;
                case 0x6003: Bus.ChrMemory.Swap(3, value); break;
                case 0x7000: Bus.PrgMemory.Swap(0, value & 0x0F); break;
                case 0x7001: Bus.PrgMemory.Swap(1, value & 0x0F); break;
                case 0x7002:
                    base.Poke(0xE000, value);
                    break;
                case 0x7003:
                    base.Poke(0xC000, 0x07);
                    base.Poke(0xC001, value);
                    base.Poke(0xE001, value);
                    break;
            }
        }

    }
}
