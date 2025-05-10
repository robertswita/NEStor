using System;
using System.Collections.Generic;

namespace NES
{
    class Mapper_119: Mapper_004
    {
        public override void Reset()
        {
            base.Reset();
            for (int i = Bus.ChrMemory.SwapBanks.Count; i < 128; i++)
                Bus.ChrMemory.SwapBanks.Add(new byte[0x400]);
            ChrRamEnabled = true;
        }
    }
}
