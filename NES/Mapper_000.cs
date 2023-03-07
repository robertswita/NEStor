using System;
using System.Collections.Generic;
using System.Text;

namespace NES
{
    class Mapper_000 : Mapper
    {
        public Mapper_000(byte prgBanks, byte chrBanks) : base(prgBanks, chrBanks)
        {

        }

        public override void Reset()
        {

        }

        public override bool CpuMapRead(int addr, ref int mapped_addr, ref byte data)
        {
            // if PRGROM is 16KB
            //     CPU Address Bus          PRG ROM
            //     0x8000 -> 0xBFFF: Map    0x0000 -> 0x3FFF
            //     0xC000 -> 0xFFFF: Mirror 0x0000 -> 0x3FFF
            // if PRGROM is 32KB
            //     CPU Address Bus          PRG ROM
            //     0x8000 -> 0xFFFF: Map    0x0000 -> 0x7FFF
            if (addr >= 0x8000 && addr <= 0xFFFF)
            {
                mapped_addr = addr & (nPRGBanks > 1 ? 0x7FFF : 0x3FFF);
                return true;
            }
            return false;
        }
	    public override bool CpuMapWrite(int addr, ref int mapped_addr, byte data)
        {
            if (addr >= 0x8000 && addr <= 0xFFFF)
            {
                mapped_addr = addr & (nPRGBanks > 1 ? 0x7FFF : 0x3FFF);
                return true;
            }

            return false;
        }
	    public override bool PpuMapRead(int addr, ref int mapped_addr)
        {
            // There is no mapping required for PPU
            // PPU Address Bus          CHR ROM
            // 0x0000 -> 0x1FFF: Map    0x0000 -> 0x1FFF
            if (addr >= 0x0000 && addr <= 0x1FFF)
            {
                mapped_addr = addr;
                return true;
            }
            return false;
        }
	    public override bool PpuMapWrite(int addr, ref int mapped_addr)
        {
            if (addr >= 0x0000 && addr <= 0x1FFF)
            {
                if (nCHRBanks == 0)
                {
                    // Treat as RAM
                    mapped_addr = addr;

                    return true;
                }
            }

            return false;
        }
    }
}
