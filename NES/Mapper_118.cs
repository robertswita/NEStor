using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NES
{
    class Mapper_118: Mapper_004
    {
        public override void CpuMapWrite(int addr, byte data)
        {
            base.CpuMapWrite(addr, data);
            switch (addr & 0xE001)
            {
                case 0x8001:
                    if (TargetRegister < 4)
                        Bus.NameTable[TargetRegister] = NameTableBanks[data >> 7 ^ 0x1];
                    break;
            }
        }
    }
}
