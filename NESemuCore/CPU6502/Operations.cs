using System;

namespace NESemuCore.CPU6502
{
    partial class CPU
    {
        public Byte ADC()
        {
            Fetch();
            _temp = (UInt16)((UInt16)A + (UInt16)_fetched + (UInt16)GetFlag(FLAGS6502.C));
            SetFlag(FLAGS6502.C, _temp > 255);
            SetFlag(FLAGS6502.Z, (_temp & 0x00FF) == 0);
            SetFlag(FLAGS6502.V, ((~((UInt16)A ^ (UInt16)_fetched) & ((UInt16)A ^ (UInt16)_temp)) & 0x0080) != 0);
            SetFlag(FLAGS6502.N, (_temp & 0x80) != 0);
            A = (Byte)(_temp & 0x00FF);
            return 1;
        }

        public Byte SBC()
        {
            Fetch();
            UInt16 value = (UInt16)(((UInt16)_fetched) ^ 0x00FF);
            _temp = (UInt16)((UInt16)A + value + (UInt16)GetFlag(FLAGS6502.C));
            SetFlag(FLAGS6502.C, (_temp & 0xFF00) != 0);
            SetFlag(FLAGS6502.Z, ((_temp & 0x00FF) == 0));
            SetFlag(FLAGS6502.V, ((_temp ^ (UInt16)A) & (_temp ^ value) & 0x0080) != 0);
            SetFlag(FLAGS6502.N, (_temp & 0x0080) != 0);
            A = (Byte)(_temp & 0x00FF);
            return 1;
        }

        /// <summary>
        /// Bitwise Logic AND
        /// A = A & M
        /// N, Z
        /// </summary>
        /// <returns>Additional clock _cycles</returns>
        public Byte AND()
        {
            Fetch();
            A = (Byte)(A & _fetched);

            SetFlag(FLAGS6502.Z, A == 0x00);
            SetFlag(FLAGS6502.N, (A & 0x80) != 0);

            return 0x01;
        }

        public Byte ASL()
        {
            Fetch();
            _temp = (UInt16)((UInt16)_fetched << 1);

            SetFlag(FLAGS6502.C, (_temp & 0xFF00) > 0);
            SetFlag(FLAGS6502.Z, (_temp & 0x00FF) == 0x00);
            SetFlag(FLAGS6502.N, (_temp & 0x80) != 0);

            if (lookup[_opcode].addressingMode == IMP)
                A = (Byte)(_temp & 0x00FF);
            else
                Write(_addrAbs, (Byte)(_temp & 0x00FF));

            return 0;
        }

        public Byte BCC()
        {
            if (GetFlag(FLAGS6502.C) == 0)
            {
                _cycles++;
                _addrAbs = (UInt16)(PC + _addrRel);

                if ((_addrAbs & 0xFF00) != (PC & 0xFF00))
                    _cycles++;

                PC = _addrAbs;
            }

            return 0x00;
        }

        public Byte BCS()
        {
            if(GetFlag(FLAGS6502.C) == 1)
            {
                _cycles++;
                _addrAbs = (UInt16)(PC + _addrRel);

                if ((_addrAbs & 0xFF00) != (PC & 0xFF00))
                    _cycles++;

                PC = _addrAbs;
            }

            return 0x00;
        }

        public Byte BEQ()
        {
            if (GetFlag(FLAGS6502.Z) == 1)
            {
                _cycles++;
                _addrAbs = (UInt16)(PC + _addrRel);

                if ((_addrAbs & 0xFF00) != (PC & 0xFF00))
                    _cycles++;

                PC = _addrAbs;
            }

            return 0x00;
        }

        public Byte BIT()
        {
            Fetch();
            _temp = (UInt16)(A & _fetched);
            SetFlag(FLAGS6502.Z, (_temp & 0x00FF) == 0x00);
            SetFlag(FLAGS6502.N, (_fetched & (1 << 7)) != 0);
            SetFlag(FLAGS6502.V, (_fetched & (1 << 6)) != 0);
            return 0;
        }

        public Byte BMI()
        {
            if (GetFlag(FLAGS6502.N) == 1)
            {
                _cycles++;
                _addrAbs = (UInt16)(PC + _addrRel);

                if ((_addrAbs & 0xFF00) != (PC & 0xFF00))
                    _cycles++;

                PC = _addrAbs;
            }

            return 0x00;
        }

        public Byte BNE()
        {
            if (GetFlag(FLAGS6502.Z) == 0)
            {
                _cycles++;
                _addrAbs = (UInt16)(PC + _addrRel);

                if ((_addrAbs & 0xFF00) != (PC & 0xFF00))
                    _cycles++;

                PC = _addrAbs;
            }

            return 0x00;
        }

        public Byte BPL()
        {
            if (GetFlag(FLAGS6502.N) == 0)
            {
                _cycles++;
                _addrAbs = (UInt16)(PC + _addrRel);

                if ((_addrAbs & 0xFF00) != (PC & 0xFF00))
                    _cycles++;

                PC = _addrAbs;
            }

            return 0x00;
        }

        public Byte BRK()
        {
            PC++;

            SetFlag(FLAGS6502.I, true);
            Write((UInt16)(0x0100 + StackPointer), (Byte)((PC >> 8) & 0x00FF));
            StackPointer--;
            Write((UInt16)(0x0100 + StackPointer), (Byte)(PC & 0x00FF));
            StackPointer--;

            SetFlag(FLAGS6502.B, true);
            Write((UInt16)(0x0100 + StackPointer), Status);
            StackPointer--;
            SetFlag(FLAGS6502.B, false);

            PC = (UInt16)((UInt16)Read(0xFFFE) | ((UInt16)Read(0xFFFF) << 8));
            return 0;
        }

        public Byte BVC()
        {
            if (GetFlag(FLAGS6502.V) == 0)
            {
                _cycles++;
                _addrAbs = (UInt16)(PC + _addrRel);

                if ((_addrAbs & 0xFF00) != (PC & 0xFF00))
                    _cycles++;

                PC = _addrAbs;
            }

            return 0x00;
        }

        public Byte BVS()
        {
            if (GetFlag(FLAGS6502.V) == 1)
            {
                _cycles++;
                _addrAbs = (UInt16)(PC + _addrRel);

                if ((_addrAbs & 0xFF00) != (PC & 0xFF00))
                    _cycles++;

                PC = _addrAbs;
            }

            return 0x00;
        }

        public Byte CLC()
        {
            SetFlag(FLAGS6502.C, false);
            return 0;
        }

        public Byte CLD()
        {
            SetFlag(FLAGS6502.D, false);
            return 0;
        }

        public Byte CLI()
        {
            SetFlag(FLAGS6502.I, false);
            return 0;
        }

        public Byte CLV()
        {
            SetFlag(FLAGS6502.V, false);
            return 0;
        }

        public Byte CMP()
        {
            Fetch();
            _temp = (UInt16)((UInt16)A - (UInt16)_fetched);
            SetFlag(FLAGS6502.C, A >= _fetched);
            SetFlag(FLAGS6502.Z, (_temp & 0x00FF) == 0x0000);
            SetFlag(FLAGS6502.N, (_temp & 0x0080) != 0);
            return 1;
        }

        public Byte CPX()
        {
            Fetch();
            _temp = (UInt16)((UInt16)X - (UInt16)_fetched);
            SetFlag(FLAGS6502.C, X >= _fetched);
            SetFlag(FLAGS6502.Z, (_temp & 0x00FF) == 0x0000);
            SetFlag(FLAGS6502.N, (_temp & 0x0080) != 0);
            return 0;
        }

        public Byte CPY()
        {
            Fetch();
            _temp = (UInt16)((UInt16)Y - (UInt16)_fetched);
            SetFlag(FLAGS6502.C, Y >= _fetched);
            SetFlag(FLAGS6502.Z, (_temp & 0x00FF) == 0x0000);
            SetFlag(FLAGS6502.N, (_temp & 0x0080) != 0);
            return 0;
        }

        public Byte DEC()
        {
            Fetch();
            _temp = (UInt16)(_fetched - 1);
            Write(_addrAbs, (Byte)(_temp & 0x00FF));
            SetFlag(FLAGS6502.Z, (_temp & 0x00FF) == 0x0000);
            SetFlag(FLAGS6502.N, (_temp & 0x0080) != 0);
            return 0;
        }

        public Byte DEX()
        {
            X--;
            SetFlag(FLAGS6502.Z, X == 0x00);
            SetFlag(FLAGS6502.N, (X & 0x80) != 0);
            return 0;
        }

        public Byte DEY()
        {
            Y--;
            SetFlag(FLAGS6502.Z, Y == 0x00);
            SetFlag(FLAGS6502.N, (Y & 0x80) != 0);
            return 0;
        }

        public Byte EOR()
        {
            Fetch();
            A = (Byte)(A ^ _fetched);
            SetFlag(FLAGS6502.Z, A == 0x00);
            SetFlag(FLAGS6502.N, (A & 0x80) != 0);
            return 1;
        }

        public Byte INC()
        {
            Fetch();
            _temp = (UInt16)(_fetched + 1);
            Write(_addrAbs, (Byte)(_temp & 0x00FF));
            SetFlag(FLAGS6502.Z, (_temp & 0x00FF) == 0x0000);
            SetFlag(FLAGS6502.N, (_temp & 0x0080) != 0);
            return 0;
        }

        public Byte INX()
        {
            X++;
            SetFlag(FLAGS6502.Z, X == 0x00);
            SetFlag(FLAGS6502.N, (X & 0x80) != 0);
            return 0;
        }

        public Byte INY()
        {
            Y++;
            SetFlag(FLAGS6502.Z, Y == 0x00);
            SetFlag(FLAGS6502.N, (Y & 0x80) != 0);
            return 0;
        }

        public Byte JMP()
        {
            PC = _addrAbs;
            return 0;
        }

        public Byte JSR()
        {
            PC--;

            Write((UInt16)(0x0100 + StackPointer), (Byte)((PC >> 8) & 0x00FF));
            StackPointer--;
            Write((UInt16)(0x0100 + StackPointer), (Byte)(PC & 0x00FF));
            StackPointer--;

            PC = _addrAbs;
            return 0;
        }

        public Byte LDA()
        {
            Fetch();
            A = _fetched;
            SetFlag(FLAGS6502.Z, A == 0x00);
            SetFlag(FLAGS6502.N, (A & 0x80) != 0);
            return 1;
        }

        public Byte LDX()
        {
            Fetch();
            X = _fetched;
            SetFlag(FLAGS6502.Z, X == 0x00);
            SetFlag(FLAGS6502.N, (X & 0x80) != 0);
            return 1;
        }

        public Byte LDY()
        {
            Fetch();
            Y = _fetched;
            SetFlag(FLAGS6502.Z, Y == 0x00);
            SetFlag(FLAGS6502.N, (Y & 0x80) != 0);
            return 1;
        }

        public Byte LSR()
        {
            Fetch();
            SetFlag(FLAGS6502.C, (_fetched & 0x0001) != 0);
            _temp = (UInt16)(_fetched >> 1);
            SetFlag(FLAGS6502.Z, (_temp & 0x00FF) == 0x0000);
            SetFlag(FLAGS6502.N, (_temp & 0x0080) != 0);
            if (lookup[_opcode].addressingMode == IMP)
                A = (Byte)(_temp & 0x00FF);
            else
                Write(_addrAbs, (Byte)(_temp & 0x00FF));
            return 0;
        }

        public Byte NOP()
        {
            switch (_opcode)
            {
                case 0x1C:
                case 0x3C:
                case 0x5C:
                case 0x7C:
                case 0xDC:
                case 0xFC:
                    return 1;
            }
            return 0;
        }

        public Byte ORA()
        {
            Fetch();
            A = (Byte)(A | _fetched);
            SetFlag(FLAGS6502.Z, A == 0x00);
            SetFlag(FLAGS6502.N, (A & 0x80) != 0);
            return 1;
        }

        public Byte PHA()
        {
            Write((UInt16)(0x0100 + StackPointer), A);
            StackPointer--;
            return 0;
        }

        public Byte PHP()
        {
            Write((UInt16)(0x0100 + StackPointer), (Byte)(Status | (Byte)FLAGS6502.B | (Byte)FLAGS6502.U));
            SetFlag(FLAGS6502.B, false);
            SetFlag(FLAGS6502.U, false);
            StackPointer--;
            return 0;
        }

        public Byte PLA()
        {
            StackPointer++;
            A = Read((UInt16)(0x0100 + StackPointer));
            SetFlag(FLAGS6502.Z, A == 0x00);
            SetFlag(FLAGS6502.N, (A & 0x80) != 0);
            return 0;
        }

        public Byte PLP()
        {
            StackPointer++;
            Status = Read((UInt16)(0x0100 + StackPointer));
            SetFlag(FLAGS6502.U, true);
            return 0;
        }

        public Byte ROL()
        {
            Fetch();
            _temp = (UInt16)((UInt16)(_fetched << 1) | GetFlag(FLAGS6502.C));
            SetFlag(FLAGS6502.C, (_temp & 0xFF00) != 0);
            SetFlag(FLAGS6502.Z, (_temp & 0x00FF) == 0x0000);
            SetFlag(FLAGS6502.N, (_temp & 0x0080) != 0);
            if (lookup[_opcode].addressingMode == IMP)
                A = (Byte)(_temp & 0x00FF);
            else
                Write(_addrAbs, (Byte)(_temp & 0x00FF));
            return 0;
        }

        public Byte ROR()
        {
            Fetch();
            _temp = (UInt16)((UInt16)(GetFlag(FLAGS6502.C) << 7) | (_fetched >> 1));
            SetFlag(FLAGS6502.C, (_fetched & 0x01) != 0);
            SetFlag(FLAGS6502.Z, (_temp & 0x00FF) == 0x0000);
            SetFlag(FLAGS6502.N, (_temp & 0x0080) != 0);
            if (lookup[_opcode].addressingMode == IMP)
                A = (Byte)(_temp & 0x00FF);
            else
                Write(_addrAbs, (Byte)(_temp & 0x00FF));
            return 0;
        }

        public Byte RTI()
        {
            StackPointer++;
            Status = Read((UInt16)(0x0100 + StackPointer));
            //Status &= (Byte)((~(Byte)FLAGS6502.B) & Status);
            //Status = (Byte)((~(Byte)FLAGS6502.U) & Status);
            Status &= (Byte)(~FLAGS6502.B);
            Status &= (Byte)(~FLAGS6502.U);

            StackPointer++;
            PC = (UInt16)Read((UInt16)(0x0100 + StackPointer));
            StackPointer++;
            PC |= (UInt16)((UInt16)Read((UInt16)(0x0100 + StackPointer)) << 8);
            return 0;
        }

        public Byte RTS()
        {
            StackPointer++;
            PC = (UInt16)Read((UInt16)(0x0100 + StackPointer));
            StackPointer++;
            PC |= (UInt16)((UInt16)Read((UInt16)(0x0100 + StackPointer)) << 8);

            PC++;
            return 0;
        }

        public Byte SEC()
        {
            SetFlag(FLAGS6502.C, true);
            return 0;
        }

        public Byte SED()
        {
            SetFlag(FLAGS6502.D, true);
            return 0;
        }

        public Byte SEI()
        {
            SetFlag(FLAGS6502.I, true);
            return 0;
        }

        public Byte STA()
        {
            Write(_addrAbs, A);
            return 0;
        }

        public Byte STX()
        {
            Write(_addrAbs, X);
            return 0;
        }

        public Byte STY()
        {
            Write(_addrAbs, Y);
            return 0;
        }

        public Byte TAX()
        {
            X = A;
            SetFlag(FLAGS6502.Z, X == 0x00);
            SetFlag(FLAGS6502.N, (X & 0x80) != 0);
            return 0;
        }

        public Byte TAY()
        {
            Y = A;
            SetFlag(FLAGS6502.Z, Y == 0x00);
            SetFlag(FLAGS6502.N, (Y & 0x80) != 0);
            return 0;
        }

        public Byte TSX()
        {
            X = StackPointer;
            SetFlag(FLAGS6502.Z, X == 0x00);
            SetFlag(FLAGS6502.N, (X & 0x80) != 0);
            return 0;
        }

        public Byte TXA()
        {
            A = X;
            SetFlag(FLAGS6502.Z, A == 0x00);
            SetFlag(FLAGS6502.N, (A & 0x80) != 0);
            return 0;
        }

        public Byte TXS()
        {
            StackPointer = X;
            return 0;
        }

        public Byte TYA()
        {
            A = Y;
            SetFlag(FLAGS6502.Z, A == 0x00);
            SetFlag(FLAGS6502.N, (A & 0x80) != 0);
            return 0;
        }

        public Byte UNK()
        {
            return 0;
        }
    }
}
