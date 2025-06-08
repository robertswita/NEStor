using NES;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NEStor.Core.Cartridge.Mappers
{
    class Mapper_118: Mapper_004
    {
        public override void Poke(int addr, byte data)
        {
            base.Poke(addr, data);
            if ((addr & 0xE001) == 0x8001)
            {
                var nametable = data >> 7;
                if (ChrMode == 0)
                {
                    var target = TargetRegister << 1;
                    Bus.NameTables.Swap(target, nametable);
                    Bus.NameTables.Swap(target + 1, nametable);
                }
                else
                    Bus.NameTables.Swap(TargetRegister - 2, nametable);
            }

        }
    }
}
