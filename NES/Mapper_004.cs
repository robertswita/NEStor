﻿using System;
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
			ChrMap.BankSize = 0x400;
			PrgMap.BankSize = 0x2000;
			UpdatePrg();
		}

		void UpdatePrg()
		{
			for (int i = 0; i < PrgBanks.Length; i++)
				PrgMap[i] = PrgBanks[i];
		}

		void UpdateChr()
		{ 
			for (int i = 0; i < ChrBanks.Length; i++)
				ChrMap[i] = ChrBanks[i];
		}

		public override void CpuMapWrite(int addr, byte data)
		{
			base.CpuMapWrite(addr, data);
			switch (addr & 0xE001)
			{
				case 0x8000: // Bank Select
					TargetRegister = data & 0xF;
					var mode = (data >> 6) & 1;
					if (mode != PRGmode)
                    {
						PRGmode = mode;
                        if (PRGmode == 0)
                            PrgMap.MapOrder = new int[] { 0, 1, 2, 3 };
                        else
                            PrgMap.MapOrder = new int[] { 2, 1, 0, 3 };
						UpdatePrg();
                    }
					mode = (data >> 7) & 1;
					if (mode != CHRmode)
                    {
						CHRmode = mode;
                        if (CHRmode == 0)
                            ChrMap.MapOrder = new int[] { 0, 1, 2, 3, 4, 5, 6, 7 };
                        else
                            ChrMap.MapOrder = new int[] { 4, 5, 6, 7, 0, 1, 2, 3 };
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
					Bus.CPU.IRQList.Clear();
					break;
				case 0xE001:
					IRQEnabled = true;
					break;
			}
		}
		bool Reload;
		public override void IRQScanline()
        {
            if (Bus.PPU.VRAM.A12Toggled)
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
                        Bus.CPU.IRQDelay = 3;
                        Bus.CPU.IRQ();
                }
            }
        }
        public override void OnSpritelineStart()
        {
			Bus.PPU.VRAM.A12Toggled = true;
			if (Bus.PPU.Control.PatternSprite == 1 && Bus.PPU.Control.PatternBg == 0)
				IRQScanline();
			Bus.PPU.VRAM.A12Toggled = false;
		}

		public override void OnSpritelineEnd()
        {
            Bus.PPU.VRAM.A12Toggled = true;
            if (Bus.PPU.Control.PatternSprite == 0 && Bus.PPU.Control.PatternBg == 1)
                IRQScanline();
            Bus.PPU.VRAM.A12Toggled = false;
        }

	}
}
