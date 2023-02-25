using System;

namespace emulatorTest.NESGamePak
{
    abstract class Mapper
    {
        protected Byte nPRGBanks = 0;
        protected Byte nCHRBanks = 0;

        public enum MIRROR
        {
            HARDWARE,
            HORIZONTAL,
            VERTICAL,
            ONESCREEN_LO,
            ONESCREEN_HI
        }

        public Mapper(Byte prgBanks, Byte chrBanks)
        {
            nPRGBanks = prgBanks;
            nCHRBanks = chrBanks;
        }

        public virtual void Reset()
        {

        }

        // Transform CPU bus address into PRG ROM offset
        public abstract bool CpuMapRead(UInt16 addr, ref UInt32 mapped_addr, ref Byte data);
	    public abstract bool CpuMapWrite(UInt16 addr, ref UInt32 mapped_addr, Byte data = 0);

        // Transform PPU bus address into CHR ROM offset
        public abstract bool PpuMapRead(UInt16 addr, ref UInt32 mapped_addr);
	    public abstract bool PpuMapWrite(UInt16 addr, ref UInt32 mapped_addr);

        public virtual MIRROR Mirror()
        {
            return MIRROR.HARDWARE;
        }
    }
}
