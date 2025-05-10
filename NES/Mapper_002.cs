using System;

namespace NES
{
    class Mapper_002 : Mapper
    {
        public override void Reset()
        {
            if (Id == 2)
                Bus.PrgMemory.Swap(1, -1);
            else if (Id == 13)
            {
                Bus.ChrMemory.SwapBanks.Clear();
                Bus.ChrMemory.SwapBanks.Add(new byte[0x4000]);
            }
            Bus.ChrMemory.BankSize = 0x1000;
        }
        public override void Poke(int addr, byte data)
        {
            if (addr < 0x8000) return;
            switch (Id)
            {
                case 2: Bus.PrgMemory.Swap(0, data & 0xF); break;
                case 3:
                    var chrBank = (data & 0xF) << 1;
                    Bus.ChrMemory.Swap(0, chrBank);
                    Bus.ChrMemory.Swap(1, chrBank | 1);
                    break;
                case 7:
                    var prgBank = (data & 0xF) << 1;
                    Bus.PrgMemory.Swap(0, prgBank);
                    Bus.PrgMemory.Swap(1, prgBank | 1);
                    Mirroring = (data & 0x10) != 0 ? MirrorType.OneScreenHi : MirrorType.OneScreenLo;
                    break;
                case 11:
                    prgBank = (data & 0x3) << 1;
                    Bus.PrgMemory.Swap(0, prgBank);
                    Bus.PrgMemory.Swap(1, prgBank | 1);
                    chrBank = (data >> 4) << 1;
                    Bus.ChrMemory.Swap(0, chrBank);
                    Bus.ChrMemory.Swap(1, chrBank | 1);
                    break;
                case 13: Bus.ChrMemory.Swap(1, data & 3); break;
                case 66:
                    prgBank = (data & 0x30) >> 3;
                    Bus.PrgMemory.Swap(0, prgBank);
                    Bus.PrgMemory.Swap(1, prgBank | 1);
                    chrBank = (data & 0x03) << 1;
                    Bus.ChrMemory.Swap(0, chrBank);
                    Bus.ChrMemory.Swap(1, chrBank | 1);
                    break;
            }
        }
    }
}
