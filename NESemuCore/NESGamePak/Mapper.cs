using System;

namespace emulatorTest.NESGamePak
{
    abstract class Mapper
    {
        protected byte nPRGBanks = 0;
        protected byte nCHRBanks = 0;

        public enum MIRROR
        {
            HARDWARE,
            HORIZONTAL,
            VERTICAL,
            ONESCREEN_LO,
            ONESCREEN_HI
        }

        public Mapper(byte prgBanks, byte chrBanks)
        {
            nPRGBanks = prgBanks;
            nCHRBanks = chrBanks;
        }

        public virtual void Reset()
        {

        }
        public virtual bool IsValidAddress(int addr)
        {
            return false;
        }
        // Transform CPU bus address into PRG ROM offset
        public abstract bool CpuMapRead(int addr, ref int mapped_addr, ref byte data);
	    public abstract bool CpuMapWrite(int addr, ref int mapped_addr, byte data = 0);

        // Transform PPU bus address into CHR ROM offset
        public abstract bool PpuMapRead(int addr, ref int mapped_addr);
	    public abstract bool PpuMapWrite(int addr, ref int mapped_addr);

        public virtual MIRROR Mirror()
        {
            return MIRROR.HARDWARE;
        }
    }
}
