using System.Net;

namespace NES
{
    class Mapper_065 : Mapper_004
    {
        //public int TargetRegister;
        //public int PrgMode;
        //public int ChrMode;
        //public int IrqMode;
        //public int[] PrgBanks = new int[] { 0, 1, -2, -1 };
        //public int[] ChrBanks = new int[] { 0, 1, 2, 3, 4, 5, 6, 7 };
        //public bool IrqEnabled;
        //public int IrqCounter;
        //public int IrqCounterLatch;
        bool Reload;

        public override void Reset()
        {
            base.Reset();
            //cpuMemory.Switch16kPrgRom(0, 0);
            //cpuMemory.Switch16kPrgRom((cartridge.PrgPages - 1) * 4, 1);
            //if (cartridge.HasCharRam)
            //    cpuMemory.FillChr(16);
            //cpuMemory.Switch8kChrRom(0);

            Bus.ChrMemory.BankSize = 0x400;
            Bus.PrgMemory.BankSize = 0x2000;
            PrgBanks[2] += Bus.PrgMemory.SwapBanks.Count;
            PrgBanks[3] += Bus.PrgMemory.SwapBanks.Count;
            UpdatePrg();
        }

        void UpdatePrg()
        {
            for (int i = 0; i < PrgBanks.Length; i++)
                Bus.PrgMemory.Swap(i, PrgBanks[i]);
            if (PrgMode == 1)
            {
                Bus.PrgMemory.Swap(0, PrgBanks[2]);
                Bus.PrgMemory.Swap(2, PrgBanks[0]);
            }
        }

        void UpdateChr()
        {
            for (int i = 0; i < ChrBanks.Length; i++)
                Bus.ChrMemory.Swap((i + 4 * ChrMode) & 7, ChrBanks[i]);
        }

        public override void Poke(int addr, byte data)
        {
            //base.Poke(addr, data);
            switch (addr)
            {
                case 0x8000: // Bank Select
                    TargetRegister = data & 0xF;
                    Bus.PrgMemory.Swap(0, data);
                    //var mode = (data >> 6) & 1;
                    //if (mode != PrgMode)
                    //{
                    //    PrgMode = mode;
                    //    UpdatePrg();
                    //}
                    //mode = (data >> 7) & 1;
                    //if (mode != ChrMode)
                    //{
                    //    ChrMode = mode;
                    //    UpdateChr();
                    //}
                    break;
                case 0x8001:
                    if (TargetRegister == 6 || TargetRegister == 7 || TargetRegister == 0xF)
                    {
                        var bank = TargetRegister == 0xF ? 2 : TargetRegister - 6;
                        PrgBanks[bank] = data;
                        UpdatePrg();
                    }
                    else
                    {
                        if (TargetRegister <= 1)
                        {
                            var idx = TargetRegister << 1;
                            ChrBanks[idx] = data;
                            if (Id == 4) ChrBanks[idx] &= ~1;
                            ChrBanks[idx + 1] = ChrBanks[idx] + 1;
                        }
                        else if (TargetRegister <= 5)
                            ChrBanks[TargetRegister + 2] = data;
                        else if (TargetRegister == 8)
                            ChrBanks[1] = data;
                        else if (TargetRegister == 9)
                            ChrBanks[3] = data;
                        UpdateChr();
                    }
                    break;
                case 0xB000: Bus.ChrMemory.Swap(0, data); break;
                case 0xB001: Bus.ChrMemory.Swap(1, data); break;
                case 0xB002: Bus.ChrMemory.Swap(2, data); break;
                case 0xB003: Bus.ChrMemory.Swap(3, data); break;
                case 0xB004: Bus.ChrMemory.Swap(4, data); break;
                case 0xB005: Bus.ChrMemory.Swap(5, data); break;
                case 0xB006: Bus.ChrMemory.Swap(6, data); break;
                case 0xB007: Bus.ChrMemory.Swap(7, data); break;
                case 0xA000: Bus.PrgMemory.Swap(1, data); break;
                case 0xC000: Bus.PrgMemory.Swap(2, data); break;
                case 0x9001:
                    Mirroring = (data & 0x40) == 0 ? MirrorType.Horizontal : MirrorType.Vertical;
                    break;
                case 0xA001: // PRG RAM Protect
                             //PrgRAMenabled = (data & 0xC0) == 0x80;
                    break;
                case 0x9005:
                    IrqCounterLatch = (IrqCounterLatch & 0x00FF) | (data << 8);
                    break;
                case 0x9004:
                    IrqMode = data & 1;
                    IrqCounter = 0;
                    Reload = true;
                    break;
                case 0x9003:
                    IrqEnabled = false;
                    Bus.Cpu.MapperIrq.Acknowledge();
                    break;
                case 0x9006:
                    IrqCounterLatch = (IrqCounterLatch & 0xFF00) | data;
                    IrqEnabled = true;
                    break;
            }
        }

        public override void IrqScanline()
        {
            if (Bus.Ppu.Vram.A12Toggled)
            {
                var prev = IrqCounter;
                if (Reload || IrqCounter == 0)
                {
                    IrqCounter = IrqCounterLatch;
                    Reload = false;
                }
                else
                    IrqCounter--;
                if ((prev > 0 || Id == 4) && IrqCounter == 0 && IrqEnabled)
                    Bus.Cpu.MapperIrq.Start(3);
            }
        }

        public override void OnSpritelineStart()
        {
            Bus.Ppu.Vram.A12Toggled = true;
            //if (Bus.Ppu.Control.PatternSprite == 1 && Bus.Ppu.Control.PatternBg == 0)
            if (Bus.Ppu.Control.PatternBg == 0)
                IrqScanline();
            Bus.Ppu.Vram.A12Toggled = false;
        }

        public override void OnSpritelineEnd()
        {
            Bus.Ppu.Vram.A12Toggled = true;
            //if (Bus.Ppu.Control.PatternSprite == 0 && Bus.Ppu.Control.PatternBg == 1)
            if (Bus.Ppu.Control.PatternBg == 1)
                IrqScanline();
            Bus.Ppu.Vram.A12Toggled = false;
        }

    }
}
