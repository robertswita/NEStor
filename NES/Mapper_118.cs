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
            //switch (addr & 0xE001)
            //{
            //    case 0x8001:
            //        if (TargetRegister < 4)
            //            Bus.NameTables.Swap(TargetRegister, data >> 7 ^ 0x1);
            //        break;
            //}
            if ((addr & 0xE001) == 0x8001)
            {
                var nametable = data >> 7;

                if (ChrMode == 0)
                {
                    var target = TargetRegister + ChrMode << 1;
                    Bus.NameTables.Swap(target, nametable);
                    Bus.NameTables.Swap(target + 1, nametable);
                }
                else
                    Bus.NameTables.Swap(TargetRegister - 2, nametable);
            }

        }
    }
}
