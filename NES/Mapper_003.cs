using System;

namespace NES
{
    class Mapper_003 : Mapper
    {
        public override void Poke(int addr, byte data)
        {
            Bus.ChrMemory.Swap(0, data);
        }
    }
}
