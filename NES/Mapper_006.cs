using System;

namespace NES
{
    class Mapper_006: Mapper
    {
        public int[] PrgBanks = new int[] { 0, 1, -2, -1 };
        public int[] ChrBanks = new int[] { 0, 1, 2, 3, 4, 5, 6, 7 };
        bool FfeAltMode;
        int IrqCounter;
        public override void Reset()
        {
            if (ChrRamEnabled)
            {
                Bus.ChrMemory.SwapBanks.Clear();
                Bus.ChrMemory.SwapBanks.Add(new byte[0x8000]);
            }
            Bus.PrgMemory.BankSize = 0x2000;
            Bus.ChrMemory.BankSize = 0x400;
            switch (Id)
            {
                case 6:
                    PrgBanks[2] = 14;
                    PrgBanks[3] = 15;
                    break;
                case 8:
                    PrgBanks[2] = 2;
                    PrgBanks[3] = 3;
                    break;
                case 17:
                    for (int i = 0; i < 4; i++)
                        Bus.PrgMemory.Swap(i, i - 4);
                    break;
            }
            for (int i = 0; i < PrgBanks.Length; i++)
                Bus.PrgMemory.Swap(i, PrgBanks[i]);
        }
        public override void Poke(int addr, byte value)
        {
            switch (addr)
            {
                case 0x42FE:
                    FfeAltMode = (value & 0x80) == 0x00;
                    Mirroring = (value & 0x10) == 0 ? MirrorType.OneScreenLo: MirrorType.OneScreenHi;
                    break;
                case 0x42FF:
                    Mirroring = (value & 0x10) == 0 ? MirrorType.Vertical : MirrorType.Horizontal;
                    break;
                case 0x4501:
                    Bus.Cpu.MapperIrq.Acknowledge();
                    break;
                case 0x4502:
                    IrqCounter = (IrqCounter & 0xFF00) | value;
                    Bus.Cpu.MapperIrq.Acknowledge();
                    break;
                case 0x4503:
                    IrqCounter = (IrqCounter & 0x00FF) | (value << 8);
                    Bus.Cpu.MapperIrq.Acknowledge();
                    Bus.Cpu.MapperIrq.Start(0x10000 - IrqCounter);
                    break;
                default:
                    if (Id == 6 || Id == 8)
                    {
                        if (Id == 6)
                        {
                            if (ChrRamEnabled || FfeAltMode)
                            {
                                PrgBanks[0] = (value & 0xFC) >> 1;
                                value &= 0x03;
                            }
                            ChrBanks[0] = value << 3;
                        }
                        else
                        {
                            PrgBanks[0] = (value & 0xF8) >> 2;
                            ChrBanks[0] = (value & 7) << 3;
                        }
                        if (addr >= 0x8000)
                        {
                            Bus.PrgMemory.Swap(0, PrgBanks[0]);
                            Bus.PrgMemory.Swap(1, PrgBanks[0] | 1);
                            for (int i = 0; i < 8; i++)
                                Bus.ChrMemory.Swap(i, ChrBanks[0] + i);
                        }
                    }
                    else
                    {
                        if (addr >= 0x4504 && addr <= 0x4507)
                            Bus.PrgMemory.Swap(addr & 3, value);
                        else if (addr >= 0x4510 && addr <= 0x4517)
                            Bus.ChrMemory.Swap(addr & 7, value);
                    }
                    break;
            }
        }

    }
}
