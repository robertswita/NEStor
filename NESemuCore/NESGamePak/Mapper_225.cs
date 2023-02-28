using System;
using System.Collections.Generic;
using System.Text;

namespace emulatorTest.NESGamePak
{
//========================
// =  Mapper 225          =
// ========================
 
// Example Games:
// --------------------------
// 52 Games
// 58-in-1
// 64-in-1
 
 
// Registers:
// ---------------------------
 
//   $5800-5803:  [.... RRRR]  RAM  (readable/writable)
//                 (16 bits of RAM -- 4 bits in each of the 4 regs)
//   $5804-5FFF:    mirrors $5800-5803
 
//   $8000-FFFF:  A~[.HMO PPPP PPCC CCCC]
//     H = High bit (acts as bit 7 for PRG and CHR regs)
//     M = Mirroring (0=Vert, 1=Horz)
//     O = PRG Mode
//     P = PRG Reg
//     C = CHR Reg
 
 
// CHR Setup:
// ---------------------------
 
//                $0000   $0400   $0800   $0C00   $1000   $1400   $1800   $1C00 
//              +---------------------------------------------------------------+
// CHR Mode 0:  |                             $8000                             |
//              +---------------------------------------------------------------+
 
 
// PRG Setup:
// ---------------------------
 
//                $8000   $A000   $C000   $E000  
//              +-------------------------------+
// PRG Mode 0:  |            <$8000>            |
//              +-------------------------------+
// PRG Mode 1:  |     $8000     |     $8000     |
//              +---------------+---------------+
    class Mapper_225: Mapper
    {
		int nCHRBankSelect;
		int _nPRGBankSelect16;
		int _nPRGBankSelect32;
		public Mapper_225(byte prgBanks, byte chrBanks) : base(prgBanks, chrBanks)
		{
		}

		public override bool CpuMapRead(int addr, ref int mapped_addr, ref byte data)
		{
			if (addr < 0x8000)
				return false;
			else if (addr <= 0xFFFF)
			{
			}
			return true;
		}

		public override bool CpuMapWrite(int addr, ref int mapped_addr, byte data)
		{
		//	if (addr >= 0x8000 && addr <= 0xFFFF)
		//	{
		//		nCHRBankSelect = data & 0x3F;
		//		mapped_addr = addr;
		//	}
		//	// Get Mapper Target Register, by examining
		//	// bit 14 of the address
		//	var nTargetRegister = addr >> 14;

		//	if (nTargetRegister == 0) // 0x8000 - 0x9FFF
		//	{
		//		// Set Control Register
		//		_nControlRegister = (byte)(_nLoadRegister & 0x1F);

		//		switch (_nControlRegister & 0x03)
		//		{
		//			case 0: _mirrorMode = MIRROR.ONESCREEN_LO; break;
		//			case 1: _mirrorMode = MIRROR.ONESCREEN_HI; break;
		//			case 2: _mirrorMode = MIRROR.VERTICAL; break;
		//			case 3: _mirrorMode = MIRROR.HORIZONTAL; break;
		//		}
		//	}
		//	else if (nTargetRegister == 1) // 0xA000 - 0xBFFF
		//	{
		//		// Set CHR Bank Lo
		//		if ((_nControlRegister & 0b10000) != 0)
		//		{
		//			// 4K CHR Bank at PPU 0x0000
		//			_nCHRBankSelect4Lo = (byte)(_nLoadRegister & 0x1F);
		//		}
		//		else
		//		{
		//			// 8K CHR Bank at PPU 0x0000
		//			_nCHRBankSelect8 = (byte)((_nLoadRegister & 0x1E) >> 1);
		//		}
		//	}
		//	else if (nTargetRegister == 2) // 0xC000 - 0xDFFF
		//	{
		//		// Set CHR Bank Hi
		//		if ((_nControlRegister & 0b10000) != 0)
		//		{
		//			// 4K CHR Bank at PPU 0x1000
		//			_nCHRBankSelect4Hi = (byte)(_nLoadRegister & 0x1F);
		//		}
		//	}
		//	else if (nTargetRegister == 3) // 0xE000 - 0xFFFF
		//	{
		//		// Configure PRG Banks
		//		byte nPRGMode = (byte)((_nControlRegister >> 2) & 0x03);

		//		if (nPRGMode == 0 || nPRGMode == 1)
		//		{
		//			// Set 32K PRG Bank at CPU 0x8000
		//			_nPRGBankSelect32 = (byte)((_nLoadRegister & 0x0E) >> 1);
		//		}
		//		else if (nPRGMode == 2)
		//		{
		//			// Fix 16KB PRG Bank at CPU 0x8000 to First Bank
		//			_nPRGBankSelect16Lo = 0;
		//			// Set 16KB PRG Bank at CPU 0xC000
		//			_nPRGBankSelect16Hi = (byte)(_nLoadRegister & 0x0F);
		//		}
		//		else if (nPRGMode == 3)
		//		{
		//			// Set 16KB PRG Bank at CPU 0x8000
		//			_nPRGBankSelect16Lo = (byte)(_nLoadRegister & 0x0F);
		//			// Fix 16KB PRG Bank at CPU 0xC000 to Last Bank
		//			_nPRGBankSelect16Hi = (byte)(nPRGBanks - 1);
		//		}
		//	}
		//}
		//	return true;

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
