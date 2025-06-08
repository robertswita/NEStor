using NES;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NEStor.Core.Cartridge.Mappers
{
    class Mapper_015: Mapper
    {
        public override void Reset()
        {
            Bus.PrgMemory.BankSize = 0x2000;
        }
        public override void Poke(int addr, byte data)
        {
            Mirroring = (data & 0x40) != 0 ? MirrorType.Horizontal : MirrorType.Vertical;
            var bank = (data & 0x7F) << 1;
            var subBank = data >> 7;
            var mode = addr & 3;
            switch (mode)
            {
                case 0:
                    Bus.PrgMemory.Swap(0, bank ^ subBank);
                    Bus.PrgMemory.Swap(1, bank + 1 ^ subBank);
                    Bus.PrgMemory.Swap(2, bank + 2 ^ subBank);
                    Bus.PrgMemory.Swap(3, bank + 3 ^ subBank);
                    break;

                case 1:
                case 3:
                    bank |= subBank;
                    Bus.PrgMemory.Swap(0, bank);
                    Bus.PrgMemory.Swap(1, bank + 1);
                    if (mode == 1) bank |= 0xE;
                    Bus.PrgMemory.Swap(2, bank);
                    Bus.PrgMemory.Swap(3, bank + 1);
                    break;

                case 2:
                    bank |= subBank;
                    Bus.PrgMemory.Swap(0, bank);
                    Bus.PrgMemory.Swap(1, bank);
                    Bus.PrgMemory.Swap(2, bank);
                    Bus.PrgMemory.Swap(3, bank);
                    break;
            }
        }
    }
}
