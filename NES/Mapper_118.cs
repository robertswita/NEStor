using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NES
{
    class Mapper_118: Mapper_004
    {
        public override void Poke(int addr, byte data)
        {
            base.Poke(addr, data);
            switch (addr & 0xE001)
            {
                case 0x8001:
                    if (TargetRegister < 4)
                        Bus.NameTables.Swap(TargetRegister, data >> 7 ^ 0x1);
                    break;
            }
        }
    }
}
