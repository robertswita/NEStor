using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NES
{
    class Mapper_064: Mapper_004
    {
        public override void CpuMapWrite(int addr, byte data)
        {
            base.CpuMapWrite(addr, data);
            switch (addr & 0xE001)
            {
                case 0x8001:
					//if (TargetRegister < 4)
					//    Bus.NameTable[TargetRegister] = NameTableBanks[data >> 7 ^ 0x1];
					//if (TargetRegister < 0x6)
					//{
					//	if (regs.chr[TargetRegister] != data)
					//	{
					//		regs.chr[TargetRegister] = data;
					//		UpdateChr();
					//	}
					//}
					//else switch (TargetRegister)
					//	{
					//		case 0x6:
					//		case 0x7:

					//			if (regs.prg[TargetRegister - 0x6] != data)
					//			{
					//				regs.prg[TargetRegister - 0x6] = data;
					//				UpdatePrg();
					//			}
					//			break;

					//		case 0x8:
					//		case 0x9:

					//			if (regs.chr[TargetRegister - 0x2] != data)
					//			{
					//				regs.chr[TargetRegister - 0x2] = data;
					//				UpdateChr();
					//			}
					//			break;

					//		case 0xF:

					//			if (regs.prg[2] != data)
					//			{
					//				regs.prg[2] = data;
					//				UpdatePrg();
					//			}
					//			break;
					//	}

					break;
            }


			//void UpdatePrg()
			//{
			//	if (PRGmode)
			//	prg.SwapBanks < SIZE_8K,0x0000 >
			//	  (
			//		  regs.prg[(regs.ctrl & 0x40U) ? 2 : 0],
			//		  regs.prg[(regs.ctrl & 0x40U) ? 0 : 1],
			//		  regs.prg[(regs.ctrl & 0x40U) ? 1 : 2],
			//		  0xFF
			//	  );
			//}

			//void UpdateChr()
			//	{
			//	int offset = (regs.ctrl & 0x80U) << 5;

			//	if (regs.ctrl & 0x20U)
			//		chr.SwapBanks<SIZE_1K>(offset, regs.chr[0], regs.chr[6], regs.chr[1], regs.chr[7]);
			//	else
			//		chr.SwapBanks<SIZE_2K>(offset, regs.chr[0] >> 1, regs.chr[1] >> 1);

			//	chr.SwapBanks<SIZE_1K>(offset ^ 0x1000, regs.chr[2], regs.chr[3], regs.chr[4], regs.chr[5]);
			//}
		}
    }
}
