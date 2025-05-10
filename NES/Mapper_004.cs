using System.Xml;

namespace NES
{
    class Mapper_004 : Mapper
    {
        public int TargetRegister;
        public int PrgMode;
        public int ChrMode;
        public int IrqMode;
        public int[] PrgBanks = new int[] { 0, 1, -2, -1 };
        public int[] ChrBanks = new int[] { 0, 1, 2, 3, 4, 5, 6, 7 };
        public bool IrqEnabled;
        public int IrqCounter;
        public int IrqCounterLatch;
        protected bool Reload;

        public override void Reset()
        {
            base.Reset();
            Bus.ChrMemory.BankSize = 0x400;
            Bus.PrgMemory.BankSize = 0x2000;
            PrgBanks[2] += Bus.PrgMemory.SwapBanks.Count;
            PrgBanks[3] += Bus.PrgMemory.SwapBanks.Count;
            UpdatePrg();
        }

        protected void UpdatePrg()
        {
            for (int i = 0; i < PrgBanks.Length; i++)
                Bus.PrgMemory.Swap(i, PrgBanks[i]);
            if (PrgMode == 1)
            {
                Bus.PrgMemory.Swap(0, PrgBanks[2]);
                Bus.PrgMemory.Swap(2, PrgBanks[0]);
            }
        }

        protected void UpdateChr()
        {
            for (int i = 0; i < ChrBanks.Length; i++)
                Bus.ChrMemory.Swap((i + 4 * ChrMode) & 7, ChrBanks[i]);
        }

        public override void Poke(int addr, byte data)
        {
            base.Poke(addr, data);
            switch (addr & 0xE001)
            {
                case 0x8000: // Bank Select
                    TargetRegister = data & 0xF;
                    var mode = (data >> 6) & 1;
                    if (mode != PrgMode)
                    {
                        PrgMode = mode;
                        UpdatePrg();
                    }
                    mode = (data >> 7) & 1;
                    if (mode != ChrMode)
                    {
                        ChrMode = mode;
                        UpdateChr();
                    }
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
                        var nametable = data >> 7;
                        if (TargetRegister <= 1)
                        {
                            var idx = TargetRegister << 1;
                            ChrBanks[idx] = data;
                            if (Id == 4) ChrBanks[idx] &= ~1;
                            ChrBanks[idx + 1] = ChrBanks[idx] + 1;
                            if (Id == 118 && ChrMode == 0)
                            {
                                Bus.NameTables.Swap(idx, nametable);
                                Bus.NameTables.Swap(idx + 1, nametable);
                            }
                        }
                        else if (TargetRegister <= 5)
                        {
                            ChrBanks[TargetRegister + 2] = data;
                            if (Id == 118 && ChrMode != 0)
                                Bus.NameTables.Swap(TargetRegister - 2, nametable);
                        }
                        else if (TargetRegister == 8)
                            ChrBanks[1] = data;
                        else if (TargetRegister == 9)
                            ChrBanks[3] = data;
                        UpdateChr();
                    }
                    break;
                case 0xA000:
                    if (Id != 118)
                        Mirroring = (data & 1) != 0 ? MirrorType.Horizontal : MirrorType.Vertical;
                    break;
                case 0xA001: // PRG RAM Protect
                             //PrgRAMenabled = (data & 0xC0) == 0x80;
                    break;
                case 0xC000:
                    IrqCounterLatch = data;
                    break;
                case 0xC001:
                    IrqMode = data & 1;
                    IrqCounter = 0;
                    Reload = true;
                    break;
                case 0xE000:
                    IrqEnabled = false;
                    Bus.Cpu.MapperIrq.Acknowledge();
                    break;
                case 0xE001:
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
