using System;
using System.Collections.Generic;
using System.Text;

namespace NES
{
	class Mapper_004 : Mapper
	{
		int TargetRegister;
		int PRGmode;
		int CHRmode;
		int[] Register = new int[8];
		bool IRQEnable;
		int IRQCounter;
		int IRQReload;

		void UpdatePrg()
        {
			var banks = new int[] {
						Register[6] & 0x3F,
						Register[7] & 0x3F,
						PrgMap.Banks.Length - 2,
						PrgMap.Banks.Length - 1 };
			for (int i = 0; i < banks.Length; i++)
				PrgMap.TransferBank(banks[i], i);
		}
		void UpdateChr()
        {
			var banks = new int[] {
								Register[0] & ~1,
								Register[0] | 1,
								Register[1] & ~1,
								Register[1] | 1,
								Register[2],
								Register[3],
								Register[4],
								Register[5] };
			for (int i = 0; i < banks.Length; i++)
				ChrMap.TransferBank(banks[i], i);
		}

		public override void CpuMapWrite(int addr, byte data)
		{
			switch (addr & 0xE001)
			{
				case 0x8000: // Bank Select
					TargetRegister = data & 0x07;
					var mode = (data >> 6) & 1;
					if (mode != PRGmode)
                    {
						PRGmode = mode;
						if (PRGmode == 0)
							PrgMap.MapOrder = new int[] { 0, 1, 2, 3 };
						else
							PrgMap.MapOrder = new int[] { 2, 1, 0, 3 };
                        //UpdatePrg();
                        PrgMap.TransferBank(Register[6] & 0x3F, 0);
                        PrgMap.TransferBank(PrgMap.Banks.Length - 2, 2);
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
					Register[TargetRegister] = data;
					if (TargetRegister < 2)
					{
						ChrMap.TransferBank(data & ~1, 2 * TargetRegister);
						ChrMap.TransferBank(data | 1, 2 * TargetRegister + 1);
                        //UpdateChr();
                    }
					else if (TargetRegister < 6)
                    {
                        ChrMap.TransferBank(data, TargetRegister + 2);
						//UpdateChr();
					}
					else if (TargetRegister < 7)
					{
						PrgMap.TransferBank(data & 0x3F, 0);
						PrgMap.TransferBank(PrgMap.Banks.Length - 2, 2);
                        //UpdatePrg();
                    }
					else
						PrgMap.TransferBank(data & 0x3F, 1);
						//UpdatePrg();
					break;
				case 0xA000: // Mirroring
					if ((data & 0x01) != 0)
						Mirroring = VRAMmirroring.Horizontal;
					else
						Mirroring = VRAMmirroring.Vertical;
					break;
				case 0xA001: // PRG RAM Protect
					//PrgRAMenabled = (data & 0xC0) == 0x80;
					break;
				case 0xC000:
					IRQReload = data;
					break;
				case 0xC001:
					IRQCounter = 0;
					break;
				case 0xE000:
					IRQEnable = false;
					Bus.CPU.IRQList.Clear();
					break;
				case 0xE001:
					IRQEnable = true;
					break;
			}
		}

		public override void Reset()
		{
			base.Reset();
			//IRQEnable = false;
			//IRQCounter = 0;
			//IRQReload = 0;
			//TargetRegister = 0;
			Bus.ChrBankSize = 0x400;
			Bus.PrgBankSize = 0x2000;
			Register = new int[8] { 0, 2, 4, 5, 6, 7, 0, 1 };
			UpdateChr();
            UpdatePrg();
			PrgRAMenabled = true;
		}

		public override void PPUSync()
		{
			if (IRQCounter == 0)
				IRQCounter = IRQReload;
			else
				IRQCounter--;
			if (IRQCounter == 0 && IRQEnable)
				Bus.CPU.IRQ();
		}

	}
}
