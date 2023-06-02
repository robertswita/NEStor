using System;
using System.Collections.Generic;
using System.Text;

namespace NES
{
    class Mapper_000 : Mapper
    {
        public override void Reset()
        {
            base.Reset();
            ChrMap.BankSize = 0x1000;
            PrgMap.BankSize = 0x4000;
            PrgMap[1] = PrgMap.Banks.Length - 1;
        }
    }
}
