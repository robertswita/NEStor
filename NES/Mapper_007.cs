using System;
using System.Collections.Generic;
using System.Text;

namespace NES
{
	class Mapper_007 : Mapper_000
	{
		public override void CpuMapWrite(int addr, byte data)
		{
			var prgBankNo = (data & 0xF) << 1;
			PrgMap.TransferBank(prgBankNo, 0);
			PrgMap.TransferBank(prgBankNo + 1, 1);
			if ((data & 0x10) != 0)
                Mirroring = VRAMmirroring.OneScreenHi;
            else
                Mirroring = VRAMmirroring.OneScreenLo;
        }

		//public override void Reset()
		//{
		//	base.Reset();
		//	//Mirroring = VRAMmirroring.OneScreenHi;
		//}

	}
}
