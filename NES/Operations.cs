using System;

namespace NES
{
    partial class CPU
    {
        public void ADC(int address)
        {
            var mem = Read(address);
            var result = A + mem;
            if (Status.Carry) result++;
            SetFlagsNZ(result);
            Status.Carry = (result & 0xFF00) != 0;
            Status.Overflow = ((A ^ result) & ~(A ^ mem) & 0x80) != 0;
            A = (byte)result;
        }

        public void SBC(int address)
        {
            var invM = Read(address) ^ 0xFF;
            var result = A + invM;
            if (Status.Carry) result++;
            SetFlagsNZ(result);
            Status.Carry = (result & 0xFF00) != 0;
            Status.Overflow = ((A ^ result) & (invM ^ result) & 0x80) != 0;
            A = (byte)result;
        }

        public void AND(int address)
        {
            A &= Read(address);
            SetFlagsNZ(A);
        }

        public void ASL(int address)
        {
            var result = Read(address) << 1;
            SetFlagsNZ(result);
            Status.Carry = (result & 0xFF00) != 0;
            Write(address, result);
        }

        public void BCC(int address)
        {
            if (!Status.Carry)
                Branch(address);
        }

        public void BCS(int address)
        {
            if (Status.Carry)
                Branch(address);
        }

        public void BEQ(int address)
        {
            if (Status.Zero)
                Branch(address);
        }

        public void BIT(int address)
        {
            var mem = Read(address);
            SetFlagsNZ(mem);
            Status.Zero = (A & mem) == 0;
            Status.Overflow = (mem & 0x40) != 0;
        }

        public void BMI(int address)
        {
            if (Status.Negative)
                Branch(address);
        }

        public void BNE(int address)
        {
            if (!Status.Zero)
                Branch(address);
        }

        public void BPL(int address)
        {
            if (!Status.Negative)
                Branch(address);
        }
        public void BRA(int address)
        {
            Branch(address);
        }

        public void BRK(int address)
        {
            if (address == 0xFFFC) SP -= 3;
            else
            {
                PushStack(PC >> 8);
                PushStack(PC);
                PushStack(Status.reg);
            }
            if (Status.Break || !Status.IRQoff)
                PC = ReadWord(address);
            Status.IRQoff = true; // don't want interrupts being interrupted
        }

        public void BVC(int address)
        {
            if (!Status.Overflow)
                Branch(address);
        }

        public void BVS(int address)
        {
            if (Status.Overflow)
                Branch(address);
        }

        public void CLC(int address)
        {
            Status.Carry = false;
        }

        public void CLD(int address)
        {
            Status.Decimal = false;
        }

        public void CLI(int address)
        {
            Status.IRQoff = false;
        }

        public void CLV(int address)
        {
            Status.Overflow = false;
        }

        public void CMP(int address)
        {
            var mem = Read(address);
            Status.Carry = A >= mem;
            SetFlagsNZ(A - mem);
        }

        public void CPX(int address)
        {
            var mem = Read(address);
            Status.Carry = X >= mem;
            SetFlagsNZ(X - mem);
        }

        public void CPY(int address)
        {
            var mem = Read(address);
            Status.Carry = Y >= mem;
            SetFlagsNZ(Y - mem);
        }

        public void DEC(int address)
        {
            var mem  = Read(address);
            mem--;
            SetFlagsNZ(mem);
            Write(address, mem);
        }

        public void DEX(int address)
        {
            X--;
            SetFlagsNZ(X);
        }

        public void DEY(int address)
        {
            Y--;
            SetFlagsNZ(Y);
        }

        public void EOR(int address)
        {
            A ^= Read(address);
            SetFlagsNZ(A);
        }

        public void INC(int address)
        {
            var mem = Read(address);
            mem++;
            SetFlagsNZ(mem);
            Write(address, mem);
        }

        public void INX(int address)
        {
            X++;
            SetFlagsNZ(X);
        }

        public void INY(int address)
        {
            Y++;
            SetFlagsNZ(Y);
        }

        public void JMP(int address)
        {
            PC = address;
        }

        public void JSR(int address)
        {
            PC--;
            PushStack(PC >> 8);
            PushStack(PC);
            PC = address;
        }

        public void LDA(int address)
        {
            A = Read(address);
            SetFlagsNZ(A);
        }

        public void LDX(int address)
        {
            X = Read(address);
            SetFlagsNZ(X);
        }

        public void LDY(int address)
        {
            Y = Read(address);
            SetFlagsNZ(Y);
        }

        public void LSR(int address)
        {
            var mem = Read(address);
            Status.Carry = (mem & 1) != 0;
            mem >>= 1;
            SetFlagsNZ(mem);
            Write(address, mem);
        }

        public void NOP(int address)
        {
        }

        public void ORA(int address)
        {
            A |= Read(address);
            SetFlagsNZ(A);
        }
        public void PHA(int address)
        {
            PushStack(A);
        }
        public void PHX(int address)
        {
            PushStack(X);
        }
        public void PHY(int address)
        {
            PushStack(Y);
        }
        public void PHP(int address)
        {
            Status.Break = true;
            PushStack(Status.reg);
        }

        public void PLA(int address)
        {
            A = (byte)PopStack();
            SetFlagsNZ(A);
        }
        public void PLX(int address)
        {
            X = (byte)PopStack();
            SetFlagsNZ(X);
        }
        public void PLY(int address)
        {
            Y = (byte)PopStack();
            SetFlagsNZ(Y);
        }

        public void PLP(int address)
        {
            Status.reg = PopStack();
        }

        public void ROL(int address)
        {
            byte mem = Read(address);
            bool carry = (mem & 0x80) != 0;
            mem <<= 1;
            if (Status.Carry) mem |= 1;
            Status.Carry = carry;
            SetFlagsNZ(mem);
            Write(address, mem);
        }

        public void ROR(int address)
        {
            byte mem = Read(address);
            bool carry = (mem & 1) != 0;
            mem >>= 1;
            if (Status.Carry) mem |= 0x80;
            Status.Carry = carry;
            SetFlagsNZ(mem);
            Write(address, mem);
        }

        public void RTI(int address)
        {
            Status.reg = PopStack();
            Status.Break = false;
            PC = PopStack();
            PC |= PopStack() << 8;
        }

        public void RTS(int address)
        {
            PC = PopStack();
            PC |= PopStack() << 8;
            PC++;
        }

        public void SEC(int address)
        {
            Status.Carry = true;
        }

        public void SED(int address)
        {
            Status.Decimal = true;
        }

        public void SEI(int address)
        {
            Status.IRQoff = true;
        }

        public void STA(int address)
        {
            Write(address, A);
        }

        public void STX(int address)
        {
            Write(address, X);
        }

        public void STY(int address)
        {
            Write(address, Y);
        }
        public void STZ(int address)
        {
            Write(address, 0);
        }
        public void STP(int address)
        {
            _bus.CPUInSleep = true;
        }

        public void TAX(int address)
        {
            X = A;
            SetFlagsNZ(X);
        }

        public void TAY(int address)
        {
            Y = A;
            SetFlagsNZ(Y);
        }

        public void TRB(int address)
        {
            int mem = Read(address);
            Status.Zero = (A & mem) == 0;
            mem &= A ^ 0xFF;
            Write(address, mem);
        }

        public void TSB(int address)
        {
            int mem = Read(address);
            Status.Zero = (A & mem) == 0;
            mem |= A;
            Write(address, mem);
        }

        public void TSX(int address)
        {
            X = SP;
            SetFlagsNZ(X);
        }

        public void TXA(int address)
        {
            A = X;
            SetFlagsNZ(A);
        }

        public void TXS(int address)
        {
            SP = X;
        }

        public void TYA(int address)
        {
            A = Y;
            SetFlagsNZ(A);
        }

        public void WAI(int address)
        {
            _bus.CPUInSleep = true;
        }

        public void UNK(int address)
        {
        }
    }
}
