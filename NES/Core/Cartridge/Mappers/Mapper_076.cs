using System;
using System.Collections.Generic;

namespace NEStor.Core.Cartridge.Mappers
{
    class Mapper_076: Mapper
    {
        byte chrBank = 0;
        public override void Reset()
        {
            Bus.PrgMemory.BankSize = 0x2000;
            Bus.ChrMemory.BankSize = 0x800;
            Bus.PrgMemory.Swap(2, -2);
            Bus.PrgMemory.Swap(3, -1);
        }
        public override void Poke(int addr, byte data)
        {
            switch (addr)
            {
                case 0x8000:
                    chrBank = data;
                    break;
                case 0x8001:
                    switch (chrBank & 0x07)
                    {
                        case 2: Bus.ChrMemory.Swap(0, data); break;
                        case 3: Bus.ChrMemory.Swap(1, data); break;
                        case 4: Bus.ChrMemory.Swap(2, data); break;
                        case 5: Bus.ChrMemory.Swap(3, data); break;
                        case 6: Bus.PrgMemory.Swap(0, data); break;
                        case 7: Bus.PrgMemory.Swap(1, data); break;
                    }
                    break;
            }
        }

    }
}
