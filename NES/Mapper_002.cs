﻿using System;
using System.Collections.Generic;
using System.Text;

namespace NES
{
	class Mapper_002: Mapper_000
    {
        public override void CpuMapWrite(int addr, byte data)
        {
            PrgMap[0] = data;
        }
    }
}
