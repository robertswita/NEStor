using System;
using System.Collections.Generic;
using System.Text;

namespace NES
{
    class Mapper_003: Mapper_000
    {
		public override void CpuMapWrite(int addr, byte data)
		{
            var bank = data << 1;
            ChrMap.TransferBank(bank, 0);
            ChrMap.TransferBank(bank | 1, 1);
        }
	}
}
