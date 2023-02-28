using System;
using System.Collections.Generic;
using System.Text;

namespace emulatorTest.NESGamePak
{
    class Mapper_003: Mapper
    {
		int nCHRBankSelect;

		public Mapper_003(byte prgBanks, byte chrBanks) : base(prgBanks, chrBanks)
        {
        }

		public override bool CpuMapRead(int addr, ref int mapped_addr, ref byte data)
		{
			if (addr >= 0x8000 && addr <= 0xFFFF)
			{
				if (nPRGBanks == 1) // 16K ROM 
					mapped_addr = addr & 0x3FFF;
				if (nPRGBanks == 2) // 32K ROM
					mapped_addr = addr & 0x7FFF;
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
				mapped_addr = addr;
			}

			// Mapper has handled write, but did not update ROMs
			return false;
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
