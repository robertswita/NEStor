using System;
using System.Collections.Generic;
using System.Text;

namespace NES
{
//CPU $8000-$BFFF: 16 KB switchable PRG ROM bank
//CPU $C000-$FFFF: 16 KB PRG ROM bank, fixed to the last bank
//	7  bit  0
//---- ----
//xxxx pPPP
//     ||||
//     ++++- Select 16 KB PRG ROM bank for CPU $8000-$BFFF
//		  (UNROM uses bits 2-0; UOROM uses bits 3-0)
	class Mapper_002: Mapper
    {
		private int nPRGBankSelectLo = 0x00;
		private int nPRGBankSelectHi = 0x00;

		public Mapper_002(byte prgBanks, byte chrBanks) : base(prgBanks, chrBanks)
		{
			//_ramStatic = new byte[32 * 1024];
		}
		public override bool CpuMapRead(int addr, ref int mapped_addr, ref byte data)
		{
			if (addr >= 0x8000 && addr <= 0xBFFF)
			{
				mapped_addr = nPRGBankSelectLo * 0x4000 + (addr & 0x3FFF);
				return true;
			}

			if (addr >= 0xC000 && addr <= 0xFFFF)
			{
				mapped_addr = nPRGBankSelectHi * 0x4000 + (addr & 0x3FFF);
				return true;
			}

			return false;
		}

		public override bool CpuMapWrite(int addr, ref int mapped_addr, byte data)
		{
			if (addr >= 0x8000 && addr <= 0xFFFF)
			{
				nPRGBankSelectLo = (byte)(data & 0x0F);
			}

			// Mapper has handled write, but do not update ROMs
			return false;
		}

		public override bool PpuMapRead(int addr, ref int mapped_addr)
		{
			if (addr < 0x2000)
			{
				mapped_addr = addr;
				return true;
			}
			else
				return false;
		}

		public override bool PpuMapWrite(int addr, ref int mapped_addr)
		{
			if (addr < 0x2000)
			{
				if (nCHRBanks == 0) // Treating as RAM
				{
					mapped_addr = addr;
					return true;
				}
			}
			return false;
		}

		public override void Reset()
		{
			nPRGBankSelectLo = 0;
			nPRGBankSelectHi = nPRGBanks - 1;
		}
	}
}
