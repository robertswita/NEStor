using System;
using System.Collections.Generic;

namespace NES
{
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
    public enum MirrorType
    {
        Horizontal =    0b01010000,
        Vertical =      0b01000100,
        OneScreenLo =   0b00000000,
        OneScreenHi =   0b01010101,
        FourScreen =    0b11100100
    };
    class Mapper
    {
        public int ID;
        public Bus Bus;
        public byte[][] NameTableBanks = { new byte[0x400], new byte[0x400], new byte[0x400], new byte[0x400] };
        //public byte[][] WRAMBanks;
        public byte[] PrgROM;
        public byte[] ChrROM;
        public MemoryMap ChrMap;
        public MemoryMap PrgMap;
        public bool PrgRAMenabled = true;
        public bool ChrRAMenabled;

        public virtual void Reset()
        {
            ChrMap = new MemoryMap(Bus.chrBanks, ChrROM);
            PrgMap = new MemoryMap(Bus.prgBanks, PrgROM);
        }

        public virtual void CpuMapWrite(int addr, byte data = 0) {}
        public virtual byte CpuMapRead(int addr) { return 0; }

        int _Mirroring;
        public MirrorType Mirroring
        {
            get { return (MirrorType)_Mirroring; }
            set
            {
                _Mirroring = (int)value;
                Bus.nameTable[3] = NameTableBanks[_Mirroring >> 6];
                Bus.nameTable[2] = NameTableBanks[_Mirroring >> 4 & 3];
                Bus.nameTable[1] = NameTableBanks[_Mirroring >> 2 & 3];
                Bus.nameTable[0] = NameTableBanks[_Mirroring & 3];
            }
        }
        public virtual void OnSpritelineStart() { }
        public virtual void OnSpritelineEnd() { }
        public virtual void IRQScanline() { }
        public virtual void OnFetchTile(TileRow tile) { }

    }
}
