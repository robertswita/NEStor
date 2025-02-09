using System;
using System.Collections.Generic;
using System.Text;

namespace NES
{
    class Mapper_000 : Mapper
    {
        public override void Reset()
        {
            Bus.ChrMemory.BankSize = 0x1000;
            Bus.PrgMemory.Swap(1, -1);
        }
    }
}
