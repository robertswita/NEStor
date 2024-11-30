using System;
using System.Collections.Generic;
using System.Text;

namespace NES
{
    class Mapper_003: Mapper_000
    {
		public override void Poke(int addr, byte data)
		{
            var bank = data << 1;
            Bus.ChrMemory.Swap(0, bank);
            Bus.ChrMemory.Swap(1, bank | 1);
        }
	}
}
