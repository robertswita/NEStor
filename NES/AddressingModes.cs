using System;

namespace NES
{
    partial class CPU
    {
        public int IMP() // Implied Accumulator
        {
            Read(PC); // dummy read
            return -1;
        }

        public int IMM() // Immediate
        {
            var address = PC;
            PC++;
            return address;
        }

        public int ZP0() // Zeropage
        {
            var address = Read(PC);
            PC++;
            return address;
        }

        public int ZPX() // Zeropage X
        {
            return (ZP0() + X) & 0xFF;
        }

        public int ZPY() // Zeropage Y
        {
            return (ZP0() + Y) & 0xFF;
        }

        public int ABS() // Absolute
        {
            var lo = ZP0();
            var hi = ZP0();
            return (hi << 8) | lo;
        }
        public int ABX() // Absolute X
        {
            var address = (ABS() + X) & 0xFFFF;
            PageCross(address, address - X);
            return address;
        }

        public int ABY() // Absolute Y
        {
            var address = (ABS() + Y) & 0xFFFF;
            PageCross(address, address - Y);
            return address;
        }

        // The supplied 16-bit address is read to get the actual 16-bit address. This
        // instruction is unusual in that it has a bug in the hardware! To emulate its
        // function accurately, we also need to emulate this bug. If the low byte of the
        // supplied address is 0xFF, then to read the high byte of the actual address
        // we need to cross a page boundary. This doesn't actually work on the chip as 
        // designed, instead it wraps back around in the same page, yielding actually an 
        // invalid address
        public int IND() // Indirect
        {
            var ptr = ABS();
            if ((ptr & 0xFF) == 0xFF)
                return Read(ptr & 0xFF00) << 8 | Read(ptr);
            else
                return ReadWord(ptr);
        }

        public int IZX() // Pre-indexed Indirect X
        {
            var t = ZPX();
            var lo = Read(t & 0xFF);
            var hi = Read((t + 1) & 0xFF);
            return (hi << 8) | lo;
        }

        public int IZY() // Post-indexed Indirect Y
        {
            var t = ZP0();
            var lo = Read(t & 0xFF);
            var hi = Read((t + 1) & 0xFF);
            var address = ((hi << 8) | lo) + Y;
            PageCross(address, address - Y);
            return address & 0xFFFF;
        }

        public int REL() // Relative
        {
            var offset = (sbyte)ZP0();
            return PC + offset;
        }

        public int BRKaddress()
        {
            Read(PC); // dummy read
            PC++;
            Status.Break = true;
            return 0xFFFE;
        }

        public int IRQaddress()
        {
            Status.Break = false;
            return 0xFFFE;
        }

        public int NMIaddress()
        {
            //Status.IRQoff = false;
            Status.Break = false;
            return 0xFFFA;
        }

        public int RSTaddress()
        {
            //Status.IRQoff = false;
            Status.Break = false;
            return 0xFFFC;
        }

    }
}
