using System;
using System.Collections.Generic;
using System.Text;

namespace emulatorTest.NESGamePak
{
	class Mapper_004 : Mapper
	{
		private byte[] RAMStatic;
		int nCHRBankSelect;
		MIRROR MirrorMode = MIRROR.HORIZONTAL;
		int nTargetRegister = 0x00;
		bool bPRGBankMode = false;
		bool bCHRInversion = false;

		int[] pRegister = new int[8];
		int[] pCHRBank = new int[8];
		int[] pPRGBank = new int[4];

		bool bIRQActive = false;
		bool bIRQEnable = false;
		bool bIRQUpdate = false;
		int nIRQCounter = 0x0000;
		int nIRQReload = 0x0000;

		public Mapper_004(byte prgBanks, byte chrBanks) : base(prgBanks, chrBanks)
		{
			RAMStatic = new byte[32 * 1024];
		}

		public override bool CpuMapRead(int addr, ref int mapped_addr, ref byte data)
		{
			if (addr >= 0x6000 && addr <= 0x7FFF)
			{
				// Write is to static ram on cartridge
				mapped_addr = -1;

				// Write data to RAM
				data = RAMStatic[addr & 0x1FFF];

				// Signal mapper has handled request
				return true;
			}


			if (addr >= 0x8000 && addr <= 0x9FFF)
			{
				mapped_addr = pPRGBank[0] + (addr & 0x1FFF);
				return true;
			}

			if (addr >= 0xA000 && addr <= 0xBFFF)
			{
				mapped_addr = pPRGBank[1] + (addr & 0x1FFF);
				return true;
			}

			if (addr >= 0xC000 && addr <= 0xDFFF)
			{
				mapped_addr = pPRGBank[2] + (addr & 0x1FFF);
				return true;
			}

			if (addr >= 0xE000 && addr <= 0xFFFF)
			{
				mapped_addr = pPRGBank[3] + (addr & 0x1FFF);
				return true;
			}

			return false;
		}

		public override bool CpuMapWrite(int addr, ref int mapped_addr, byte data)
		{
			if (addr >= 0x6000 && addr <= 0x7FFF)
			{
				// Write is to static ram on cartridge
				mapped_addr = -1;

				// Write data to RAM
				RAMStatic[addr & 0x1FFF] = data;

				// Signal mapper has handled request
				return true;
			}

			if (addr >= 0x8000 && addr <= 0x9FFF)
			{
				// Bank Select
				if ((addr & 0x0001) == 0)
				{
					nTargetRegister = data & 0x07;
					bPRGBankMode = (data & 0x40) != 0;
					bCHRInversion = (data & 0x80) != 0;
				}
				else
				{
					// Update target register
					pRegister[nTargetRegister] = data;

					// Update Pointer Table
					if (bCHRInversion)
					{
						pCHRBank[0] = pRegister[2] * 0x0400;
						pCHRBank[1] = pRegister[3] * 0x0400;
						pCHRBank[2] = pRegister[4] * 0x0400;
						pCHRBank[3] = pRegister[5] * 0x0400;
						pCHRBank[4] = (pRegister[0] & 0xFE) * 0x0400;
						pCHRBank[5] = pRegister[0] * 0x0400 + 0x0400;
						pCHRBank[6] = (pRegister[1] & 0xFE) * 0x0400;
						pCHRBank[7] = pRegister[1] * 0x0400 + 0x0400;
					}
					else
					{
						pCHRBank[0] = (pRegister[0] & 0xFE) * 0x0400;
						pCHRBank[1] = pRegister[0] * 0x0400 + 0x0400;
						pCHRBank[2] = (pRegister[1] & 0xFE) * 0x0400;
						pCHRBank[3] = pRegister[1] * 0x0400 + 0x0400;
						pCHRBank[4] = pRegister[2] * 0x0400;
						pCHRBank[5] = pRegister[3] * 0x0400;
						pCHRBank[6] = pRegister[4] * 0x0400;
						pCHRBank[7] = pRegister[5] * 0x0400;
					}

					if (bPRGBankMode)
					{
						pPRGBank[2] = (pRegister[6] & 0x3F) * 0x2000;
						pPRGBank[0] = (nPRGBanks * 2 - 2) * 0x2000;
					}
					else
					{
						pPRGBank[0] = (pRegister[6] & 0x3F) * 0x2000;
						pPRGBank[2] = (nPRGBanks * 2 - 2) * 0x2000;
					}

					pPRGBank[1] = (pRegister[7] & 0x3F) * 0x2000;
					pPRGBank[3] = (nPRGBanks * 2 - 1) * 0x2000;

				}

				return false;
			}

			if (addr >= 0xA000 && addr <= 0xBFFF)
			{
				if ((addr & 0x0001) == 0)
				{
					// Mirroring
					if ((data & 0x01) != 0)
						MirrorMode = MIRROR.HORIZONTAL;
					else
						MirrorMode = MIRROR.VERTICAL;
				}
				else
				{
					// PRG Ram Protect
					// TODO:
				}
				return false;
			}

			if (addr >= 0xC000 && addr <= 0xDFFF)
			{
				if ((addr & 0x0001) == 0)
				{
					nIRQReload = data;
				}
				else
				{
					nIRQCounter = 0x0000;
				}
				return false;
			}

			if (addr >= 0xE000 && addr <= 0xFFFF)
			{
				if ((addr & 0x0001) == 0)
				{
					bIRQEnable = false;
					bIRQActive = false;
				}
				else
				{
					bIRQEnable = true;
				}
				return false;
			}



			return false;
		}

		public override bool PpuMapRead(int addr, ref int mapped_addr)
		{
			if (addr >= 0x0000 && addr <= 0x03FF)
			{
				mapped_addr = pCHRBank[0] + (addr & 0x03FF);
				return true;
			}

			if (addr >= 0x0400 && addr <= 0x07FF)
			{
				mapped_addr = pCHRBank[1] + (addr & 0x03FF);
				return true;
			}

			if (addr >= 0x0800 && addr <= 0x0BFF)
			{
				mapped_addr = pCHRBank[2] + (addr & 0x03FF);
				return true;
			}

			if (addr >= 0x0C00 && addr <= 0x0FFF)
			{
				mapped_addr = pCHRBank[3] + (addr & 0x03FF);
				return true;
			}

			if (addr >= 0x1000 && addr <= 0x13FF)
			{
				mapped_addr = pCHRBank[4] + (addr & 0x03FF);
				return true;
			}

			if (addr >= 0x1400 && addr <= 0x17FF)
			{
				mapped_addr = pCHRBank[5] + (addr & 0x03FF);
				return true;
			}

			if (addr >= 0x1800 && addr <= 0x1BFF)
			{
				mapped_addr = pCHRBank[6] + (addr & 0x03FF);
				return true;
			}

			if (addr >= 0x1C00 && addr <= 0x1FFF)
			{
				mapped_addr = pCHRBank[7] + (addr & 0x03FF);
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
			nTargetRegister = 0x00;
			bPRGBankMode = false;
			bCHRInversion = false;
			MirrorMode = MIRROR.HORIZONTAL;

			bIRQActive = false;
			bIRQEnable = false;
			bIRQUpdate = false;
			nIRQCounter = 0x0000;
			nIRQReload = 0x0000;

			for (int i = 0; i < 4; i++) pPRGBank[i] = 0;
			for (int i = 0; i < 8; i++) { pCHRBank[i] = 0; pRegister[i] = 0; }

			pPRGBank[0] = 0 * 0x2000;
			pPRGBank[1] = 1 * 0x2000;
			pPRGBank[2] = (nPRGBanks * 2 - 2) * 0x2000;
			pPRGBank[3] = (nPRGBanks * 2 - 1) * 0x2000;
		}

		bool IRQState()
		{
			return bIRQActive;
		}

		void IRQClear()
		{
			bIRQActive = false;
		}

		void Scanline()
		{
			if (nIRQCounter == 0)
			{
				nIRQCounter = nIRQReload;
			}
			else
				nIRQCounter--;

			if (nIRQCounter == 0 && bIRQEnable)
			{
				bIRQActive = true;
			}

		}

		public override MIRROR Mirror()
		{
			return MirrorMode;
		}

	}
}
