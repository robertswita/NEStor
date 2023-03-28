using System;
using System.Collections.Generic;

namespace NES
{
    public enum VRAMmirroring
    {
        Horizontal =    0x0011,
        Vertical =      0x0101,
        OneScreenLo =   0x0000,
        OneScreenHi =   0x1111,
        FourScreen =    0x0123
    };
    class Mapper
    {
        public Bus Bus;
        public byte[][] NameTableBanks = { new byte[0x400], new byte[0x400], null, null };
        public byte[][] WRAMBanks;
        public byte[] PrgROM;
        public byte[] ChrROM;
        public byte[] ChrRAM;
        public MemoryMap ChrMap;
        public MemoryMap PrgMap;
        public bool PrgRAMenabled;
        public bool ChrRAMenabled;

        public virtual void Reset()
        {
            ChrMap = new MemoryMap(Bus.ChrTable, ChrROM);
            PrgMap = new MemoryMap(Bus.PRGMemory, PrgROM);
        }

        public virtual void CpuMapWrite(int addr, byte data = 0)
        {
            if (PrgRAMenabled)
            {
                addr &= 0x7FFF;
                Bus.PRGMemory[addr >> 14][addr & 0x3FFF] = data;
            }
        }

        /*
         * HORIZONTAL:
         * Maps $2000 and $2400 of the ppu to the first physical name table.
         * Maps $2800 and $2C00 of the ppu to the second physical name table.
         *
         * VERTICAL:
         * Maps $2000 and $2800 of the ppu to the first physical name table.
         * Maps $2400 and $2C00 of the ppu to the second physical name table.
         *
         * ONE-SCREEN:
         * Maps all virtual name tables to the first physical name table.
         *
         * FOUR-SCREEN:
         * Maps each virtual name table to a physical name table using
         * 2KB of RAM in the game cartridge.
         */

        VRAMmirroring _Mirroring;
        public VRAMmirroring Mirroring
        {
            get { return _Mirroring; }
            set
            {
                if (value != _Mirroring)
                {
                    _Mirroring = value;
                    if (_Mirroring == VRAMmirroring.FourScreen)
                    {
                        NameTableBanks[2] = new byte[0x400];
                        NameTableBanks[3] = new byte[0x400];
                    }
                    var swapOrder = (int)_Mirroring;
                    Bus.NameTable[0] = NameTableBanks[swapOrder >> 12];
                    Bus.NameTable[1] = NameTableBanks[swapOrder >> 8 & 3];
                    Bus.NameTable[2] = NameTableBanks[swapOrder >> 4 & 3];
                    Bus.NameTable[3] = NameTableBanks[swapOrder & 3];
                }
            }
        }
        public virtual void PPUSync() {}

    }
}
