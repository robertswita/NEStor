using System;
using System.Collections.Generic;
using System.Text;

namespace NES
{
    class Mapper_000 : Mapper
    {
        public override void Reset()
        {
            Bus.PatternTable[0] = CHRBanks[0];
            Bus.PatternTable[1] = CHRBanks[1];
            Bus.PRGMemory[0] = PRGBanks[0];
            Bus.PRGMemory[1] = PRGBanks[PRGBanks.Length - 1];
        }
    }
}
