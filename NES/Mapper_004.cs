using System;
using System.Collections.Generic;
using System.Text;

namespace NES
{
	class Mapper_004 : Mapper
	{
		public int TargetRegister;
		public int PRGmode;
		public int CHRmode;
		public int IRQmode;
		public int[] PrgBanks = new int[] { 0, 1, -2, -1 };
		public int[] ChrBanks = new int[] { 0, 1, 2, 3, 4, 5, 6, 7 };
		public bool IRQEnabled;
		public int IRQCounter;
		public int IRQLatch;

		public override void Reset()
		{
			base.Reset();
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
			if (PRGmode == 1)
			{
                Bus.PrgMemory.Swap(0, PrgBanks[2]);
                Bus.PrgMemory.Swap(2, PrgBanks[0]);
            }
        }

		void UpdateChr()
		{
            for (int i = 0; i < ChrBanks.Length; i++)
                Bus.ChrMemory.Swap((i + 4 * CHRmode) & 7, ChrBanks[i]);
        }

		public override void Poke(int addr, byte data)
		{
			base.Poke(addr, data);
			switch (addr & 0xE001)
			{
				case 0x8000: // Bank Select
					TargetRegister = data & 0xF;
					var mode = (data >> 6) & 1;
					if (mode != PRGmode)
                    {
						PRGmode = mode;
                        UpdatePrg();
                    }
					mode = (data >> 7) & 1;
					if (mode != CHRmode)
                    {
						CHRmode = mode;
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
						if (TargetRegister <= 1)
						{
							var idx = TargetRegister << 1;
							ChrBanks[idx] = data;
							if (ID == 4) ChrBanks[idx] &= ~1;
							ChrBanks[idx + 1] = ChrBanks[idx] + 1;
						}
						else if (TargetRegister <= 5)
							ChrBanks[TargetRegister + 2] = data;
						else
							ChrBanks[2 * (TargetRegister - 8) + 1] = data;
						UpdateChr();
					}
					break;
				case 0xA000:
					Mirroring = (data & 1) != 0 ? MirrorType.Horizontal: MirrorType.Vertical;
					break;
				case 0xA001: // PRG RAM Protect
					//PrgRAMenabled = (data & 0xC0) == 0x80;
					break;
				case 0xC000:
					IRQLatch = data;
                    break;
				case 0xC001:
					IRQmode = data & 1;
					Reload = true;
					break;
				case 0xE000:
					IRQEnabled = false;
					Bus.Cpu.Irq = false;
					break;
				case 0xE001:
					IRQEnabled = true;
					break;
			}
		}
		bool Reload;
		public override void IRQScanline()
        {
            if (Bus.Ppu.VRAM.A12Toggled)
            {
                var prev = IRQCounter;
				if (Reload || IRQCounter == 0)
				{
					IRQCounter = IRQLatch;
					Reload = false;
				}
				else
					IRQCounter--;
                if ((prev > 0 || ID == 4) && IRQCounter == 0 && IRQEnabled)
                {
                    Bus.Cpu.IRQDelay = 3;
                    Bus.Cpu.Irq = true;
                }
            }
        }
        public override void OnSpritelineStart()
        {
			Bus.Ppu.VRAM.A12Toggled = true;
			if (Bus.Ppu.Control.PatternSprite == 1 && Bus.Ppu.Control.PatternBg == 0)
				IRQScanline();
			Bus.Ppu.VRAM.A12Toggled = false;
		}

		public override void OnSpritelineEnd()
        {
            Bus.Ppu.VRAM.A12Toggled = true;
            if (Bus.Ppu.Control.PatternSprite == 0 && Bus.Ppu.Control.PatternBg == 1)
                IRQScanline();
            Bus.Ppu.VRAM.A12Toggled = false;
        }

	}
}
