using System;
using System.Collections.Generic;
using System.Text;

namespace emulatorTest.NESGamePak
{
//CPU $6000-$7FFF: 8 KB PRG RAM bank, (optional)
//CPU $8000-$BFFF: 16 KB PRG ROM bank, either switchable or fixed to the first bank
//CPU $C000 -$FFFF: 16 KB PRG ROM bank, either fixed to the last bank or switchable
//PPU $0000 -$0FFF: 4 KB switchable CHR bank
//PPU $1000 -$1FFF: 4 KB switchable CHR bank

	class Mapper_001 : Mapper
    {
		private byte _nCHRBankSelect4Lo = 0x00;
		private byte _nCHRBankSelect4Hi = 0x00;
		private byte _nCHRBankSelect8 = 0x00;

		private byte _nPRGBankSelect16Lo = 0x00;
		private byte _nPRGBankSelect16Hi = 0x00;
		private byte _nPRGBankSelect32 = 0x00;

		private byte _nLoadRegister = 0x00;
		private byte _nLoadRegisterCount = 0x00;
		private byte _nControlRegister = 0x00;


		private byte[] _ramStatic;
		private MIRROR _mirrorMode = MIRROR.HORIZONTAL;

		public override MIRROR Mirror()
        {
			return _mirrorMode;
		}

		public Mapper_001(byte prgBanks, byte chrBanks) : base(prgBanks, chrBanks)
        {
            _ramStatic = new byte[32 * 1024];
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
			_nPRGBankSelect16Hi = (byte)(nPRGBanks - 1);
		}

        public override bool CpuMapRead(int addr, ref int mapped_addr, ref byte data)
        {
			if (addr >= 0x6000 && addr <= 0x7FFF)
			{
				// Read is from static ram on cartridge
				mapped_addr = -1;
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
						mapped_addr = _nPRGBankSelect16Lo * 0x4000 + (addr & 0x3FFF);
						return true;
					}

					if (addr >= 0xC000 && addr <= 0xFFFF)
					{
						mapped_addr = _nPRGBankSelect16Hi * 0x4000 + (addr & 0x3FFF);
						return true;
					}
				}
				else
				{
					// 32K Mode
					mapped_addr = _nPRGBankSelect32 * 0x8000 + (addr & 0x7FFF);
					return true;
				}
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
					_nControlRegister = (byte)(_nControlRegister | 0x0C);
				}
				else
				{
					// Load data in serially into load register
					// It arrives LSB first, so implant this at
					// bit 5. After 5 writes, the register is ready
					_nLoadRegister >>= 1;
					_nLoadRegister |= (byte)((data & 0x01) << 4);
					_nLoadRegisterCount++;

					if (_nLoadRegisterCount == 5)
					{
						// Get Mapper Target Register, by examining
						// bits 13 & 14 of the address
						byte nTargetRegister = (byte)(((addr >> 13) & 0x03));

						if (nTargetRegister == 0) // 0x8000 - 0x9FFF
						{
							// Set Control Register
							_nControlRegister = (byte)(_nLoadRegister & 0x1F);

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
								_nCHRBankSelect4Lo = (byte)(_nLoadRegister & 0x1F);
							}
							else
							{
								// 8K CHR Bank at PPU 0x0000
								_nCHRBankSelect8 = (byte)((_nLoadRegister & 0x1E) >> 1);
							}
						}
						else if (nTargetRegister == 2) // 0xC000 - 0xDFFF
						{
							// Set CHR Bank Hi
							if ((_nControlRegister & 0b10000) != 0)
							{
								// 4K CHR Bank at PPU 0x1000
								_nCHRBankSelect4Hi = (byte)(_nLoadRegister & 0x1F);
							}
						}
						else if (nTargetRegister == 3) // 0xE000 - 0xFFFF
						{
							// Configure PRG Banks
							byte nPRGMode = (byte)((_nControlRegister >> 2) & 0x03);

							if (nPRGMode == 0 || nPRGMode == 1)
							{
								// Set 32K PRG Bank at CPU 0x8000
								_nPRGBankSelect32 = (byte)((_nLoadRegister & 0x0E) >> 1);
							}
							else if (nPRGMode == 2)
							{
								// Fix 16KB PRG Bank at CPU 0x8000 to First Bank
								_nPRGBankSelect16Lo = 0;
								// Set 16KB PRG Bank at CPU 0xC000
								_nPRGBankSelect16Hi = (byte)(_nLoadRegister & 0x0F);
							}
							else if (nPRGMode == 3)
							{
								// Set 16KB PRG Bank at CPU 0x8000
								_nPRGBankSelect16Lo = (byte)(_nLoadRegister & 0x0F);
								// Fix 16KB PRG Bank at CPU 0xC000 to Last Bank
								_nPRGBankSelect16Hi = (byte)(nPRGBanks - 1);
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

        public override bool PpuMapRead(int addr, ref int mapped_addr)
        {
			if (addr < 0x2000)
			{
				if (nCHRBanks == 0)
				{
					mapped_addr = addr;
				}
				else
				{
					if ((_nControlRegister & 0x10) != 0)
					{
						// 4K CHR Bank Mode
						if (addr < 0x1000)
							mapped_addr = _nCHRBankSelect4Lo * 0x1000 + (addr & 0x0FFF);
						else
							mapped_addr = _nCHRBankSelect4Hi * 0x1000 + (addr & 0x0FFF);
					}
					else
					{
						// 8K CHR Bank Mode
						mapped_addr = _nCHRBankSelect8 * 0x2000 + (addr & 0x1FFF);
					}
				}
				return true;
			}
			return false;
		}

        public override bool PpuMapWrite(int addr, ref int mapped_addr)
        {
			if (addr < 0x2000)
			{
				if (nCHRBanks == 0)
				{
					mapped_addr = addr;
				}

				return true;
			}
			return false;
		}
    }
}
