using System;

namespace NESemuCore.CPU6502
{
    partial class CPU
    {
        public Byte IMP()
        {
            _fetched = A;
            return 0;
        }

        public Byte IMM()
        {
            _addrAbs = PC++;
            return 0;
        }

        public Byte ZP0()
        {
            _addrAbs = Read(PC);
            PC++;
            _addrAbs &= 0x00FF;
            return 0;
        }

        public Byte ZPX()
        {
            _addrAbs = (UInt16)(Read(PC) + X);
            PC++;
            _addrAbs &= 0x00FF;
            return 0;
        }

        public Byte ZPY()
        {
            _addrAbs = (UInt16)(Read(PC) + Y);
            PC++;
            _addrAbs &= 0x00FF;
            return 0;
        }

        public Byte ABS()
        {
            UInt16 lo = Read(PC);
            PC++;
            UInt16 hi = Read(PC);
            PC++;

            _addrAbs = (UInt16)((hi << 8) | lo);

            return 0;
        }
        public Byte ABX()
        {
            UInt16 lo = Read(PC);
            PC++;
            UInt16 hi = Read(PC);
            PC++;

            _addrAbs = (UInt16)((hi << 8) | lo);
            _addrAbs += X;

            if ((_addrAbs & 0xFF00) != (hi << 8))
                return 1;
            else
                return 0;
        }

        public Byte ABY()
        {
            UInt16 lo = Read(PC);
            PC++;
            UInt16 hi = Read(PC);
            PC++;

            _addrAbs = (UInt16)((hi << 8) | lo);
            _addrAbs += Y;

            if ((_addrAbs & 0xFF00) != (hi << 8))
                return 1;
            else
                return 0;
        }

        public Byte IND()
        {
            UInt16 ptr_lo = Read(PC);
            PC++;
            UInt16 ptr_hi = Read(PC);
            PC++;

            //UInt16 ptr = (UInt16)(ptr_hi << 8);

            UInt16 ptr = (UInt16)((ptr_hi << 8) | ptr_lo);

            //_addrAbs = (UInt16)((read((UInt16)(ptr + 1)) << 8) | read((UInt16)(ptr + 0)));
            if(ptr_lo == 0x00FF)
            {
                _addrAbs = (UInt16)((Read((UInt16)(ptr & 0xFF00)) << 8) | Read((UInt16)(ptr + 0)));
            } 
            else
            {
                _addrAbs = (UInt16)((Read((UInt16)(ptr + 1)) << 8) | Read((UInt16)(ptr + 0)));
            }

            return 0;
        }

        public Byte IZX()
        {
            UInt16 t = Read(PC);
            PC++;

            UInt16 lo = Read((UInt16)((t + (UInt16)X) & 0x00FF));
            UInt16 hi = Read((UInt16)((t + (UInt16)X + 1) & 0x00FF));

            _addrAbs = (UInt16)((hi << 8) | lo);

            return 0;
        }

        public Byte IZY()
        {
            UInt16 t = Read(PC);
            PC++;

            UInt16 lo = Read((UInt16)(t & 0x00FF));
            UInt16 hi = Read((UInt16)((t + 1) & 0x00FF));

            _addrAbs = (UInt16)((hi << 8) | lo);
            _addrAbs += Y;

            if ((_addrAbs & 0xFF00) != (hi << 8))
                return 1;
            else
                return 0;
        }

        public Byte REL()
        {
            _addrRel = Read(PC);
            PC++;
            if ((_addrRel & 0x80) != 0) // check if bit 7 is set to 1
                _addrRel |= 0xFF00;

            return 0;
        }
    }
}
