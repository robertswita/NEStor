using System;
using System.Collections.Generic;
using System.Text;

namespace NES
{
	class Mapper_002: Mapper_000
    {
        public override void Poke(int addr, byte data)
        {
            Bus.PrgMemory.Swap(0, data);
        }
    }
}
