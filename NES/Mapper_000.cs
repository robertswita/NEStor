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
            Bus.ChrBankSize = 0x1000;
            Bus.PrgBankSize = 0x4000;
            PrgMap.TransferBank(PrgMap.Banks.Length - 1, 1);
        }
    }
}
