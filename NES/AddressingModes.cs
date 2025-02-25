using System;
using System.Net;

namespace NES
{
    partial class CPU
    {
        public int IMP() // Implied Accumulator
        {
            Read(PC);
            return int.MaxValue;
        }

        public int IMM() // Immediate
        {
            var address = PC; PC++;
            return address;
        }

        public int ZPG() // Zeropage
        {
            var address = Read(PC); PC++;
            return address;
        }

        public int ZPX() // Zeropage X-indexed
        {
            var addr = ZPG();
            Read(addr);
            return (addr + X) & 0xFF;
        }

        public int ZPY() // Zeropage Y-indexed
        {
            var addr = ZPG();
            Read(addr);
            return (addr + Y) & 0xFF;
        }

        public int ABS() // Absolute
        {
            var addr = ReadWord(PC); PC += 2;
            return addr;
        }
        public int ABX() // Absolute X-indexed
        {
            var address = (ABS() + X) & 0xFFFF;
            ReadPageCross(address, address - X);
            return address;
        }

        public int ABY() // Absolute Y-indexed
        {
            var address = (ABS() + Y) & 0xFFFF;
            ReadPageCross(address, address - Y);
            return address;
        }

        // The supplied 16-bit address is read to get the actual 16-bit address.
        // This instruction is unusual in that it has a bug in the hardware!
        // To emulate its function accurately, we also need to emulate this bug.
        // If the low byte of the supplied address is 0xFF, then to read
        // the high byte of the actual address we need to cross a page boundary.
        // This doesn't actually work on the chip as designed, instead it
        // wraps back around in the same page, yielding an invalid address.
        public int IND() // Indirect
        {
            var addr = ABS();
            var lo = Read(addr); addr++;
            if ((addr & 0xFF) == 0x00)
                addr -= 0x100;
            var hi = Read(addr);
            return hi << 8 | lo;
        }

        public int XIN() // X-indexed Indirect
        {
            return ReadWord((byte)ZPX());
        }

        public int INY() // Indirect Y-indexed
        {
            var address = ReadWord((byte)ZPG()) + Y;
            ReadPageCross(address, address - Y);
            return address;
        }

        public int REL() // Relative
        {
            return (sbyte)ZPG() + PC;
        }

    }
}
