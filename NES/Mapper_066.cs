using System;
using System.Collections.Generic;
using System.Text;

namespace NES
{
	class Mapper_066 : Mapper
	{
		int nCHRBankSelect;
		int nPRGBankSelect;

		public Mapper_066(byte prgBanks, byte chrBanks) : base(prgBanks, chrBanks)
		{
		}

		public override bool CpuMapRead(int addr, ref int mapped_addr, ref byte data)
		{
			if (addr >= 0x8000 && addr <= 0xFFFF)
			{
				mapped_addr = nPRGBankSelect * 0x8000 + (addr & 0x7FFF);
				return true;
			}
			else
				return false;
		}

		public override bool CpuMapWrite(int addr, ref int mapped_addr, byte data)
		{
			if (addr >= 0x8000 && addr <= 0xFFFF)
			{
				nCHRBankSelect = data & 0x03;
				nPRGBankSelect = (data & 0x30) >> 4;
			}

			// Mapper has handled write, but do not update ROMs
			return false;
		}

		public override bool PpuMapRead(int addr, ref int mapped_addr)
		{
			if (addr < 0x2000)
			{
				mapped_addr = nCHRBankSelect * 0x2000 + addr;
				return true;
			}
			else
				return false;
		}

		public override bool PpuMapWrite(int addr, ref int mapped_addr)
		{
			return false;
		}

		public override void Reset()
		{
			nCHRBankSelect = 0;
			nPRGBankSelect = 0;
		}

	}
}
