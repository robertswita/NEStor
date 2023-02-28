using System;
using System.Collections.Generic;
using System.Text;

namespace emulatorTest.NESGamePak
{
	class Mapper_007 : Mapper
	{
		int nCHRBankSelect;
		int nPRGBankSelect;
		MIRROR MirrorMode = MIRROR.ONESCREEN_HI;

		public Mapper_007(byte prgBanks, byte chrBanks) : base(prgBanks, chrBanks)
		{
		}

		public override bool CpuMapRead(int addr, ref int mapped_addr, ref byte data)
		{
			mapped_addr = 0;
			if (addr < 0x8000)
				return false;
			else if (addr <= 0xBFFF)
				mapped_addr = addr - 0x8000;
			else if (addr <= 0xFFFF)
				mapped_addr = addr - 0xC000;
			return true;
		}

		public override bool CpuMapWrite(int addr, ref int mapped_addr, byte data)
		{
			if (addr >= 0x8000 && addr <= 0xFFFF)
			{
				nPRGBankSelect = data;
				//this->prg_lo = &this->get_prg_bank(this->reg.bank_select.prg_bank * 2 + 0);
				//this->prg_hi = &this->get_prg_bank(this->reg.bank_select.prg_bank * 2 + 1);

				//this->chr_mem = &this->get_chr_bank(0);
			}
			return false;
		}

		public override MIRROR Mirror()
		{
			return MirrorMode;
		}

		public override bool PpuMapRead(int addr, ref int mapped_addr)
		{
			mapped_addr = 0;
			if (addr < 0x2000)
			{
				mapped_addr = nCHRBankSelect * 0x2000 + addr;
				return true;
			}
			return false;
		}

		public override bool PpuMapWrite(int addr, ref int mapped_addr)
		{
			return false;
		}

		public override void Reset()
		{
			nCHRBankSelect = 0;
		}

	}
}
