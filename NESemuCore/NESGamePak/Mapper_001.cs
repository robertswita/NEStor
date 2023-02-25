using System;
using System.Collections.Generic;
using System.Text;

namespace NESemuCore.NESGamePak
{
    class Mapper_001 : Mapper
    {
		private Byte _nCHRBankSelect4Lo = 0x00;
		private Byte _nCHRBankSelect4Hi = 0x00;
		private Byte _nCHRBankSelect8 = 0x00;

		private Byte _nPRGBankSelect16Lo = 0x00;
		private Byte _nPRGBankSelect16Hi = 0x00;
		private Byte _nPRGBankSelect32 = 0x00;

		private Byte _nLoadRegister = 0x00;
		private Byte _nLoadRegisterCount = 0x00;
		private Byte _nControlRegister = 0x00;


		private Byte[] _ramStatic;
		private MIRROR _mirrorMode = MIRROR.HORIZONTAL;

		public override MIRROR Mirror()
        {
			return _mirrorMode;
		}

		public Mapper_001(Byte prgBanks, Byte chrBanks) : base(prgBanks, chrBanks)
        {
            _ramStatic = new Byte[32 * 1024];
        }

        public override void Reset()
        {
			_nControlRegister = 0x1C;
			_nLoadRegister = 0x00;
			_nLoadRegisterCount = 0x00;

			_nCHRBankSelect4Lo = 0;
			_nCHRBankSelect4Hi = 0;
			_nCHRBankSelect8 = 0;

			_nPRGBankSelect32 = 0;
			_nPRGBankSelect16Lo = 0;
			_nPRGBankSelect16Hi = (Byte)(nPRGBanks - 1);
		}

        public override bool CpuMapRead(UInt16 addr, ref UInt32 mapped_addr, ref Byte data)
        {
			if (addr >= 0x6000 && addr <= 0x7FFF)
			{
				// Read is from static ram on cartridge
				mapped_addr = 0xFFFFFFFF;
				// Read data from RAM
				data = _ramStatic[addr & 0x1FFF];
				// Signal mapper has handled request
				return true;
			}

			if (addr >= 0x8000)
			{
				if ((_nControlRegister & 0b01000) != 0)
				{
					// 16K Mode
					if (addr >= 0x8000 && addr <= 0xBFFF)
					{
						mapped_addr = (UInt32)(_nPRGBankSelect16Lo * 0x4000 + (addr & 0x3FFF));
						return true;
					}

					if (addr >= 0xC000 && addr <= 0xFFFF)
					{
						mapped_addr = (UInt32)(_nPRGBankSelect16Hi * 0x4000 + (addr & 0x3FFF));
						return true;
					}
				}
				else
				{
					// 32K Mode
					mapped_addr = (UInt32)(_nPRGBankSelect32 * 0x8000 + (addr & 0x7FFF));
					return true;
				}
			}
			return false;
		}

        public override bool CpuMapWrite(UInt16 addr, ref UInt32 mapped_addr, Byte data)
        {
			if (addr >= 0x6000 && addr <= 0x7FFF)
			{
				// Write is to static ram on cartridge
				mapped_addr = 0xFFFFFFFF;

				// Write data to RAM
				_ramStatic[addr & 0x1FFF] = data;

				// Signal mapper has handled request
				return true;
			}

			if (addr >= 0x8000)
			{
				if ((data & 0x80) != 0)
				{
					// MSB is set, so Reset serial loading
					_nLoadRegister = 0x00;
					_nLoadRegisterCount = 0;
					_nControlRegister = (Byte)(_nControlRegister | 0x0C);
				}
				else
				{
					// Load data in serially into load register
					// It arrives LSB first, so implant this at
					// bit 5. After 5 writes, the register is ready
					_nLoadRegister >>= 1;
					_nLoadRegister |= (Byte)((data & 0x01) << 4);
					_nLoadRegisterCount++;

					if (_nLoadRegisterCount == 5)
					{
						// Get Mapper Target Register, by examining
						// bits 13 & 14 of the address
						Byte nTargetRegister = (Byte)(((addr >> 13) & 0x03));

						if (nTargetRegister == 0) // 0x8000 - 0x9FFF
						{
							// Set Control Register
							_nControlRegister = (Byte)(_nLoadRegister & 0x1F);

							switch (_nControlRegister & 0x03)
							{
								case 0: _mirrorMode = MIRROR.ONESCREEN_LO; break;
								case 1: _mirrorMode = MIRROR.ONESCREEN_HI; break;
								case 2: _mirrorMode = MIRROR.VERTICAL; break;
								case 3: _mirrorMode = MIRROR.HORIZONTAL; break;
							}
						}
						else if (nTargetRegister == 1) // 0xA000 - 0xBFFF
						{
							// Set CHR Bank Lo
							if ((_nControlRegister & 0b10000) != 0)
							{
								// 4K CHR Bank at PPU 0x0000
								_nCHRBankSelect4Lo = (Byte)(_nLoadRegister & 0x1F);
							}
							else
							{
								// 8K CHR Bank at PPU 0x0000
								_nCHRBankSelect8 = (Byte)((_nLoadRegister & 0x1E) >> 1);
							}
						}
						else if (nTargetRegister == 2) // 0xC000 - 0xDFFF
						{
							// Set CHR Bank Hi
							if ((_nControlRegister & 0b10000) != 0)
							{
								// 4K CHR Bank at PPU 0x1000
								_nCHRBankSelect4Hi = (Byte)(_nLoadRegister & 0x1F);
							}
						}
						else if (nTargetRegister == 3) // 0xE000 - 0xFFFF
						{
							// Configure PRG Banks
							Byte nPRGMode = (Byte)((_nControlRegister >> 2) & 0x03);

							if (nPRGMode == 0 || nPRGMode == 1)
							{
								// Set 32K PRG Bank at CPU 0x8000
								_nPRGBankSelect32 = (Byte)((_nLoadRegister & 0x0E) >> 1);
							}
							else if (nPRGMode == 2)
							{
								// Fix 16KB PRG Bank at CPU 0x8000 to First Bank
								_nPRGBankSelect16Lo = 0;
								// Set 16KB PRG Bank at CPU 0xC000
								_nPRGBankSelect16Hi = (Byte)(_nLoadRegister & 0x0F);
							}
							else if (nPRGMode == 3)
							{
								// Set 16KB PRG Bank at CPU 0x8000
								_nPRGBankSelect16Lo = (Byte)(_nLoadRegister & 0x0F);
								// Fix 16KB PRG Bank at CPU 0xC000 to Last Bank
								_nPRGBankSelect16Hi = (Byte)(nPRGBanks - 1);
							}
						}

						// 5 bits were written, and decoded, so
						// Reset load register
						_nLoadRegister = 0x00;
						_nLoadRegisterCount = 0;
					}

				}

			}

			// Mapper has handled write, but do not update ROMs
			return false;
		}

        public override bool PpuMapRead(UInt16 addr, ref UInt32 mapped_addr)
        {
			if (addr < 0x2000)
			{
				if (nCHRBanks == 0)
				{
					mapped_addr = addr;
					return true;
				}
				else
				{
					if ((_nControlRegister & 0b10000) != 0)
					{
						// 4K CHR Bank Mode
						if (addr >= 0x0000 && addr <= 0x0FFF)
						{
							mapped_addr = (UInt32)(_nCHRBankSelect4Lo * 0x1000 + (addr & 0x0FFF));
							return true;
						}

						if (addr >= 0x1000 && addr <= 0x1FFF)
						{
							mapped_addr = (UInt32)(_nCHRBankSelect4Hi * 0x1000 + (addr & 0x0FFF));
							return true;
						}
					}
					else
					{
						// 8K CHR Bank Mode
						mapped_addr = (UInt32)(_nCHRBankSelect8 * 0x2000 + (addr & 0x1FFF));
						return true;
					}
				}
			}

			return false;
		}

        public override bool PpuMapWrite(UInt16 addr, ref UInt32 mapped_addr)
        {
			if (addr < 0x2000)
			{
				if (nCHRBanks == 0)
				{
					mapped_addr = addr;
					return true;
				}

				return true;
			}
			else
				return false;
		}
    }
}
