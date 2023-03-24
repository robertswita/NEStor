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
        public byte[][] NameTableBanks = { new byte[0x400], new byte[0x400], null, null };
        public byte[][] WRAMBanks;
        public byte[][] PRGBanks;
        public byte[][] CHRBanks;

        //public bool IRQActive;
        public Bus Bus;

        public virtual void Reset()
        {
            Bus.PatternTable[0] = new byte[0x1000];
            Bus.PatternTable[1] = new byte[0x1000];
            Bus.PRGMemory[0] = new byte[0x4000];
            Bus.PRGMemory[1] = new byte[0x4000];
        }

        protected MemoryMap CreateCHRMap(int bankSize)
        {
            Bus.PatternTable[0] = new byte[0x1000];
            Bus.PatternTable[1] = new byte[0x1000];
            var chrMap = new MemoryMap(0x1000, 0x1000);
            chrMap.Sources.Add(Bus.PatternTable[0]);
            chrMap.Sources.Add(Bus.PatternTable[1]);
            for (int i = 0; i < CHRBanks.Length; i++)
                chrMap.Targets.Add(CHRBanks[i]);
            chrMap.BankSize = bankSize;
            return chrMap;
        }

        protected MemoryMap CreatePRGMap(int bankSize)
        {
            Bus.PRGMemory[0] = new byte[0x4000];
            Bus.PRGMemory[1] = new byte[0x4000];
            var prgMap = new MemoryMap(0x4000, 0x4000);
            prgMap.Sources.Add(Bus.PRGMemory[0]);
            prgMap.Sources.Add(Bus.PRGMemory[1]);
            for (int i = 0; i < PRGBanks.Length; i++)
                prgMap.Targets.Add(PRGBanks[i]);
            prgMap.BankSize = bankSize;
            return prgMap;
        }

        public virtual void CpuMapWrite(int addr, byte data = 0)
        {
            addr &= 0x7FFF;
            Bus.PRGMemory[addr >> 14][addr & 0x3FFF] = data;
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

        public virtual void PPUSync() {}

    }
}
