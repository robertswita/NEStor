using System;
using System.Collections.Generic;

namespace NES
{
     // HORIZONTAL:
     // Maps $2000 and $2400 of the ppu to the first physical name table.
     // Maps $2800 and $2C00 of the ppu to the second physical name table.
     // VERTICAL:
     // Maps $2000 and $2800 of the ppu to the first physical name table.
     // Maps $2400 and $2C00 of the ppu to the second physical name table.
     // ONE-SCREEN:
     // Maps all virtual name tables to the first physical name table.
     // FOUR-SCREEN:
     // Maps each virtual name table to a physical name table
     // using 2KB of RAM in the game cartridge.
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
        public int Id;
        public int SubId;
        public Bus Bus;
        public bool PrgRAMenabled = true;
        public bool ChrRAMenabled;

        public virtual void Reset() { }
        public virtual void Poke(int addr, byte data = 0) { }
        public virtual byte Peek(int addr) { return 0; }

        int mirroring;
        public MirrorType Mirroring
        {
            get { return (MirrorType)mirroring; }
            set
            {
                mirroring = (int)value;
                Bus.NameTables.Swap(3, mirroring >> 6);
                Bus.NameTables.Swap(2, mirroring >> 4 & 3);
                Bus.NameTables.Swap(1, mirroring >> 2 & 3);
                Bus.NameTables.Swap(0, mirroring & 3);
            }
        }
        public virtual void OnSpritelineStart() { }
        public virtual void OnSpritelineEnd() { }
        public virtual void IrqScanline() { }
        public virtual void OnFetchTile(TileRow tile) { }

    }
}
