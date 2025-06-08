using System;

namespace NES
{
    struct Instruction
    {
        public byte Opcode;
        public delegate void opDelegate(int address);
        public opDelegate Operation;
        public delegate int addressDelegate();
        public addressDelegate AddressingMode;
        public int CycleCount;
        public int WriteCycles;
        public int Address;

        public Instruction(byte opcode, opDelegate operation, 
            addressDelegate addressingMode, byte cycles, int writeCycles)
        {
            Opcode = opcode;
            Operation = operation;
            AddressingMode = addressingMode;
            CycleCount = cycles;
            WriteCycles = writeCycles;
        }
        public void Run()
        {
            var address = AddressingMode();
            Address = address;
            Operation(address);
        }
    }
    partial class CPU
    {
        void CreateInstructionSet()
        {
            InstructionSet = new Instruction[] {
                new Instruction(0x00, BRK, IMP, 7, 0x1C),
                new Instruction(0x01, ORA, XIN, 6, 0x00),
                new Instruction(0x02, STP, IMP, 2, 0x00),
                new Instruction(0x03, SLO, XIN, 8, 0xC0),
                new Instruction(0x04, DOP, ZPG, 3, 0x00),
                new Instruction(0x05, ORA, ZPG, 3, 0x00),
                new Instruction(0x06, ASL, ZPG, 5, 0x18),
                new Instruction(0x07, SLO, ZPG, 5, 0x18),
                new Instruction(0x08, PHP, IMP, 3, 0x04),
                new Instruction(0x09, ORA, IMM, 2, 0x00),
                new Instruction(0x0A, ASL, IMP, 2, 0x00),
                new Instruction(0x0B, ANC, IMM, 2, 0x00),
                new Instruction(0x0C, TOP, ABS, 4, 0x00),
                new Instruction(0x0D, ORA, ABS, 4, 0x00),
                new Instruction(0x0E, ASL, ABS, 6, 0x30),
                new Instruction(0x0F, SLO, ABS, 6, 0x30),
                new Instruction(0x10, BPL, REL, 2, 0x00),
                new Instruction(0x11, ORA, INY, 5, 0x00),
                new Instruction(0x12, STP, IMP, 2, 0x00),
                new Instruction(0x13, SLO, INY, 8, 0xC0),
                new Instruction(0x14, DOP, ZPX, 4, 0x00),
                new Instruction(0x15, ORA, ZPX, 4, 0x00),
                new Instruction(0x16, ASL, ZPX, 6, 0x30),
                new Instruction(0x17, SLO, ZPX, 6, 0x30),
                new Instruction(0x18, CLC, IMP, 2, 0x00),
                new Instruction(0x19, ORA, ABY, 4, 0x00),
                new Instruction(0x1A, NOP, IMP, 2, 0x00),
                new Instruction(0x1B, SLO, ABY, 7, 0x60),
                new Instruction(0x1C, TOP, ABX, 4, 0x00),
                new Instruction(0x1D, ORA, ABX, 4, 0x00),
                new Instruction(0x1E, ASL, ABX, 7, 0x60),
                new Instruction(0x1F, SLO, ABX, 7, 0x60),
                new Instruction(0x20, JSR, ABS, 6, 0x1C), // ZPG not ABS!
                new Instruction(0x21, AND, XIN, 6, 0x00),
                new Instruction(0x22, STP, IMP, 2, 0x00),
                new Instruction(0x23, RLA, XIN, 8, 0xC0),
                new Instruction(0x24, BIT, ZPG, 3, 0x00),
                new Instruction(0x25, AND, ZPG, 3, 0x00),
                new Instruction(0x26, ROL, ZPG, 5, 0x18),
                new Instruction(0x27, RLA, ZPG, 5, 0x18),
                new Instruction(0x28, PLP, IMP, 4, 0x00),
                new Instruction(0x29, AND, IMM, 2, 0x00),
                new Instruction(0x2A, ROL, IMP, 2, 0x00),
                new Instruction(0x2B, ANC, IMM, 2, 0x00),
                new Instruction(0x2C, BIT, ABS, 4, 0x00),
                new Instruction(0x2D, AND, ABS, 4, 0x00),
                new Instruction(0x2E, ROL, ABS, 6, 0x30),
                new Instruction(0x2F, RLA, ABS, 6, 0x30),
                new Instruction(0x30, BMI, REL, 2, 0x00),
                new Instruction(0x31, AND, INY, 5, 0x00),
                new Instruction(0x32, STP, IMP, 2, 0x00),
                new Instruction(0x33, RLA, INY, 8, 0xC0),
                new Instruction(0x34, DOP, ZPX, 4, 0x00),
                new Instruction(0x35, AND, ZPX, 4, 0x00),
                new Instruction(0x36, ROL, ZPX, 6, 0x30),
                new Instruction(0x37, RLA, ZPX, 6, 0x30),
                new Instruction(0x38, SEC, IMP, 2, 0x00),
                new Instruction(0x39, AND, ABY, 4, 0x00),
                new Instruction(0x3A, NOP, IMP, 2, 0x00),
                new Instruction(0x3B, RLA, ABY, 7, 0x60),
                new Instruction(0x3C, TOP, ABX, 4, 0x00),
                new Instruction(0x3D, AND, ABX, 4, 0x00),
                new Instruction(0x3E, ROL, ABX, 7, 0x60),
                new Instruction(0x3F, RLA, ABX, 7, 0x60),
                new Instruction(0x40, RTI, IMP, 6, 0x00),
                new Instruction(0x41, EOR, XIN, 6, 0x00),
                new Instruction(0x42, STP, IMP, 2, 0x00),
                new Instruction(0x43, SRE, XIN, 8, 0xC0),
                new Instruction(0x44, DOP, ZPG, 3, 0x00),
                new Instruction(0x45, EOR, ZPG, 3, 0x00),
                new Instruction(0x46, LSR, ZPG, 5, 0x18),
                new Instruction(0x47, SRE, ZPG, 5, 0x18),
                new Instruction(0x48, PHA, IMP, 3, 0x04),
                new Instruction(0x49, EOR, IMM, 2, 0x00),
                new Instruction(0x4A, LSR, IMP, 2, 0x00),
                new Instruction(0x4B, ALR, IMM, 2, 0x00),
                new Instruction(0x4C, JMP, ABS, 3, 0x00),
                new Instruction(0x4D, EOR, ABS, 4, 0x00),
                new Instruction(0x4E, LSR, ABS, 6, 0x30),
                new Instruction(0x4F, SRE, ABS, 6, 0x30),
                new Instruction(0x50, BVC, REL, 2, 0x00),
                new Instruction(0x51, EOR, INY, 5, 0x00),
                new Instruction(0x52, STP, IMP, 2, 0x00),
                new Instruction(0x53, SRE, INY, 8, 0xC0),
                new Instruction(0x54, DOP, ZPX, 4, 0x00),
                new Instruction(0x55, EOR, ZPX, 4, 0x00),
                new Instruction(0x56, LSR, ZPX, 6, 0x30),
                new Instruction(0x57, SRE, ZPX, 6, 0x30),
                new Instruction(0x58, CLI, IMP, 2, 0x00),
                new Instruction(0x59, EOR, ABY, 4, 0x00),
                new Instruction(0x5A, NOP, IMP, 2, 0x00),
                new Instruction(0x5B, SRE, ABY, 7, 0x60),
                new Instruction(0x5C, TOP, ABX, 4, 0x00),
                new Instruction(0x5D, EOR, ABX, 4, 0x00),
                new Instruction(0x5E, LSR, ABX, 7, 0x60),
                new Instruction(0x5F, SRE, ABX, 7, 0x60),
                new Instruction(0x60, RTS, IMP, 6, 0x00),
                new Instruction(0x61, ADC, XIN, 6, 0x00),
                new Instruction(0x62, STP, IMP, 2, 0x00),
                new Instruction(0x63, RRA, XIN, 8, 0xC0),
                new Instruction(0x64, DOP, ZPG, 3, 0x00),
                new Instruction(0x65, ADC, ZPG, 3, 0x00),
                new Instruction(0x66, ROR, ZPG, 5, 0x18),
                new Instruction(0x67, RRA, ZPG, 5, 0x18),
                new Instruction(0x68, PLA, IMP, 4, 0x00),
                new Instruction(0x69, ADC, IMM, 2, 0x00),
                new Instruction(0x6A, ROR, IMP, 2, 0x00),
                new Instruction(0x6B, ARR, IMM, 2, 0x00),
                new Instruction(0x6C, JMP, IND, 5, 0x00),
                new Instruction(0x6D, ADC, ABS, 4, 0x00),
                new Instruction(0x6E, ROR, ABS, 6, 0x30),
                new Instruction(0x6F, RRA, ABS, 6, 0x30),
                new Instruction(0x70, BVS, REL, 2, 0x00),
                new Instruction(0x71, ADC, INY, 5, 0x00),
                new Instruction(0x72, STP, IMP, 2, 0x00),
                new Instruction(0x73, RRA, INY, 8, 0xC0),
                new Instruction(0x74, DOP, ZPX, 4, 0x00),
                new Instruction(0x75, ADC, ZPX, 4, 0x00),
                new Instruction(0x76, ROR, ZPX, 6, 0x30),
                new Instruction(0x77, RRA, ZPX, 6, 0x30),
                new Instruction(0x78, SEI, IMP, 2, 0x00),
                new Instruction(0x79, ADC, ABY, 4, 0x00),
                new Instruction(0x7A, NOP, IMP, 2, 0x00),
                new Instruction(0x7B, RRA, ABY, 7, 0x60),
                new Instruction(0x7C, TOP, ABX, 4, 0x00),
                new Instruction(0x7D, ADC, ABX, 4, 0x00),
                new Instruction(0x7E, ROR, ABX, 7, 0x60),
                new Instruction(0x7F, RRA, ABX, 7, 0x60),
                new Instruction(0x80, DOP, IMM, 2, 0x00),
                new Instruction(0x81, STA, XIN, 6, 0x20),
                new Instruction(0x82, DOP, IMM, 2, 0x00),
                new Instruction(0x83, SAX, XIN, 6, 0x20),
                new Instruction(0x84, STY, ZPG, 3, 0x04),
                new Instruction(0x85, STA, ZPG, 3, 0x04),
                new Instruction(0x86, STX, ZPG, 3, 0x04),
                new Instruction(0x87, SAX, ZPG, 3, 0x04),
                new Instruction(0x88, DEY, IMP, 2, 0x00),
                new Instruction(0x89, DOP, IMM, 2, 0x00),
                new Instruction(0x8A, TXA, IMP, 2, 0x00),
                new Instruction(0x8B, XAA, IMM, 2, 0x00),
                new Instruction(0x8C, STY, ABS, 4, 0x08),
                new Instruction(0x8D, STA, ABS, 4, 0x08),
                new Instruction(0x8E, STX, ABS, 4, 0x08),
                new Instruction(0x8F, SAX, ABS, 4, 0x08),
                new Instruction(0x90, BCC, REL, 2, 0x00),
                new Instruction(0x91, STA, INY, 6, 0x20),
                new Instruction(0x92, STP, IMP, 2, 0x00),
                new Instruction(0x93, AHX, INY, 6, 0x20),
                new Instruction(0x94, STY, ZPX, 4, 0x08),
                new Instruction(0x95, STA, ZPX, 4, 0x08),
                new Instruction(0x96, STX, ZPY, 4, 0x08),
                new Instruction(0x97, SAX, ZPY, 4, 0x08),
                new Instruction(0x98, TYA, IMP, 2, 0x00),
                new Instruction(0x99, STA, ABY, 5, 0x10),
                new Instruction(0x9A, TXS, IMP, 2, 0x00),
                new Instruction(0x9B, TAS, ABY, 5, 0x10),
                new Instruction(0x9C, SHY, ABX, 5, 0x10),
                new Instruction(0x9D, STA, ABX, 5, 0x10),
                new Instruction(0x9E, SHX, ABY, 5, 0x10),
                new Instruction(0x9F, AHX, ABY, 5, 0x10),
                new Instruction(0xA0, LDY, IMM, 2, 0x00),
                new Instruction(0xA1, LDA, XIN, 6, 0x00),
                new Instruction(0xA2, LDX, IMM, 2, 0x00),
                new Instruction(0xA3, LAX, XIN, 6, 0x00),
                new Instruction(0xA4, LDY, ZPG, 3, 0x00),
                new Instruction(0xA5, LDA, ZPG, 3, 0x00),
                new Instruction(0xA6, LDX, ZPG, 3, 0x00),
                new Instruction(0xA7, LAX, ZPG, 3, 0x00),
                new Instruction(0xA8, TAY, IMP, 2, 0x00),
                new Instruction(0xA9, LDA, IMM, 2, 0x00),
                new Instruction(0xAA, TAX, IMP, 2, 0x00),
                new Instruction(0xAB, LAX, IMM, 2, 0x00),
                new Instruction(0xAC, LDY, ABS, 4, 0x00),
                new Instruction(0xAD, LDA, ABS, 4, 0x00),
                new Instruction(0xAE, LDX, ABS, 4, 0x00),
                new Instruction(0xAF, LAX, ABS, 4, 0x00),
                new Instruction(0xB0, BCS, REL, 2, 0x00),
                new Instruction(0xB1, LDA, INY, 5, 0x00),
                new Instruction(0xB2, STP, IMP, 2, 0x00),
                new Instruction(0xB3, LAX, INY, 5, 0x00),
                new Instruction(0xB4, LDY, ZPX, 4, 0x00),
                new Instruction(0xB5, LDA, ZPX, 4, 0x00),
                new Instruction(0xB6, LDX, ZPY, 4, 0x00),
                new Instruction(0xB7, LAX, ZPY, 4, 0x00),
                new Instruction(0xB8, CLV, IMP, 2, 0x00),
                new Instruction(0xB9, LDA, ABY, 4, 0x00),
                new Instruction(0xBA, TSX, IMP, 2, 0x00),
                new Instruction(0xBB, LAS, ABY, 4, 0x00),
                new Instruction(0xBC, LDY, ABX, 4, 0x00),
                new Instruction(0xBD, LDA, ABX, 4, 0x00),
                new Instruction(0xBE, LDX, ABY, 4, 0x00),
                new Instruction(0xBF, LAX, ABY, 4, 0x00),
                new Instruction(0xC0, CPY, IMM, 2, 0x00),
                new Instruction(0xC1, CMP, XIN, 6, 0x00),
                new Instruction(0xC2, DOP, IMM, 2, 0x00),
                new Instruction(0xC3, DCP, XIN, 8, 0xC0),
                new Instruction(0xC4, CPY, ZPG, 3, 0x00),
                new Instruction(0xC5, CMP, ZPG, 3, 0x00),
                new Instruction(0xC6, DEC, ZPG, 5, 0x18),
                new Instruction(0xC7, DCP, ZPG, 5, 0x18),
                new Instruction(0xC8, INY, IMP, 2, 0x00),
                new Instruction(0xC9, CMP, IMM, 2, 0x00),
                new Instruction(0xCA, DEX, IMP, 2, 0x00),
                new Instruction(0xCB, AXS, IMM, 2, 0x00),
                new Instruction(0xCC, CPY, ABS, 4, 0x00),
                new Instruction(0xCD, CMP, ABS, 4, 0x00),
                new Instruction(0xCE, DEC, ABS, 6, 0x30),
                new Instruction(0xCF, DCP, ABS, 6, 0x30),
                new Instruction(0xD0, BNE, REL, 2, 0x00),
                new Instruction(0xD1, CMP, INY, 5, 0x00),
                new Instruction(0xD2, STP, IMP, 2, 0x00),
                new Instruction(0xD3, DCP, INY, 8, 0xC0),
                new Instruction(0xD4, DOP, ZPX, 4, 0x00),
                new Instruction(0xD5, CMP, ZPX, 4, 0x00),
                new Instruction(0xD6, DEC, ZPX, 6, 0x30),
                new Instruction(0xD7, DCP, ZPX, 6, 0x30),
                new Instruction(0xD8, CLD, IMP, 2, 0x00),
                new Instruction(0xD9, CMP, ABY, 4, 0x00),
                new Instruction(0xDA, NOP, IMP, 2, 0x00),
                new Instruction(0xDB, DCP, ABY, 7, 0x60),
                new Instruction(0xDC, TOP, ABX, 4, 0x00),
                new Instruction(0xDD, CMP, ABX, 4, 0x00),
                new Instruction(0xDE, DEC, ABX, 7, 0x60),
                new Instruction(0xDF, DCP, ABX, 7, 0x60),
                new Instruction(0xE0, CPX, IMM, 2, 0x00),
                new Instruction(0xE1, SBC, XIN, 6, 0x00),
                new Instruction(0xE2, DOP, IMM, 2, 0x00),
                new Instruction(0xE3, ISC, XIN, 8, 0xC0),
                new Instruction(0xE4, CPX, ZPG, 3, 0x00),
                new Instruction(0xE5, SBC, ZPG, 3, 0x00),
                new Instruction(0xE6, INC, ZPG, 5, 0x18),
                new Instruction(0xE7, ISC, ZPG, 5, 0x18),
                new Instruction(0xE8, INX, IMP, 2, 0x00),
                new Instruction(0xE9, SBC, IMM, 2, 0x00),
                new Instruction(0xEA, NOP, IMP, 2, 0x00),
                new Instruction(0xEB, SBC, IMM, 2, 0x00),
                new Instruction(0xEC, CPX, ABS, 4, 0x00),
                new Instruction(0xED, SBC, ABS, 4, 0x00),
                new Instruction(0xEE, INC, ABS, 6, 0x30),
                new Instruction(0xEF, ISC, ABS, 6, 0x30),
                new Instruction(0xF0, BEQ, REL, 2, 0x00),
                new Instruction(0xF1, SBC, INY, 5, 0x00),
                new Instruction(0xF2, STP, IMP, 2, 0x00),
                new Instruction(0xF3, ISC, INY, 8, 0xC0),
                new Instruction(0xF4, DOP, ZPX, 4, 0x00),
                new Instruction(0xF5, SBC, ZPX, 4, 0x00),
                new Instruction(0xF6, INC, ZPX, 6, 0x30),
                new Instruction(0xF7, ISC, ZPX, 6, 0x30),
                new Instruction(0xF8, SED, IMP, 2, 0x00),
                new Instruction(0xF9, SBC, ABY, 4, 0x00),
                new Instruction(0xFA, NOP, IMP, 2, 0x00),
                new Instruction(0xFB, ISC, ABY, 7, 0x60),
                new Instruction(0xFC, TOP, ABX, 4, 0x00),
                new Instruction(0xFD, SBC, ABX, 4, 0x00),
                new Instruction(0xFE, INC, ABX, 7, 0x60),
                new Instruction(0xFF, ISC, ABX, 7, 0x60)
            };
        }

        #region Control instructions
        public void BRK(int address)
        {
            //Status.Break = !IrqActive;
            if (Status.Break) PC++; else PC--;
            if (Rst)
            {
                Rst = false;
                PopStack(); SP -= 2;
                PopStack(); SP -= 2;
                PopStack(); SP -= 2;
                address = 0xFFFC;
            }
            else
            {
                PushStack(PC >> 8);
                PushStack(PC);
                PushStack(Status.Register);
                if (Nmi.IsActive)
                {
                    Nmi.Acknowledge();
                    address = 0xFFFA;
                }
                else
                    address = 0xFFFE;
            }
            Status.IrqDisable = true; // don't want interrupts being interrupted
            PC = ReadWord(address);
            if (Nmi.IsActive)
            {
                Nmi.Acknowledge();
                Nmi.Start();
            }
        }
        public void RTI(int address) // ReTurn from Interrupt
        {
            Read(0x100 | SP);
            Status.Register = PopStack();
            Status.Break = false;
            PC = PopStack();
            PC |= PopStack() << 8;
        }
        //public void JSR(int address) // Jump and Save Return addr - safe to unwrap because JSR is Absolute
        //{
        //    Read(0x100 | SP);
        //    PushStack(PC >> 8);
        //    PushStack(PC);
        //    PC = Read(PC) << 8 | address;
        //}
        public void JSR(int address) // Jump and Save Return addr - safe to unwrap because JSR is Absolute
        {
            Read(0x100 | SP);
            PC--;
            PushStack(PC >> 8);
            PushStack(PC);
            PC = address;
        }
        public void RTS(int address) // ReTurn from Subroutine
        {
            Read(0x100 | SP);
            PC = PopStack();
            PC |= PopStack() << 8;
            PC++;
            Read(PC);
        }
        public void BIT(int address) // Test BITs of M with A (Affects N, V and Z)
        {
            var mem = Read(address);
            Status.Zero = (A & mem) == 0;
            Status.Negative = (mem & 0x80) != 0;
            Status.Overflow = (mem & 0x40) != 0;
        }
        public void PHA(int address) { PushStack(A); }
        public void PLA(int address) { Read(0x100 | SP); A = (byte)PopStack(); }
        public void PHP(int address)
        {
            Status.Break = true;
            PushStack(Status.Register | 0x10);
        }
        public void PLP(int address)
        {
            Read(0x100 | SP);
            var irqDisable = Status.IrqDisable;
            Status.Register = PopStack();
            if (Status.IrqDisable != irqDisable)
            {
                Status.IrqDisable = irqDisable;
                Status.ToggleIrqDisable = true;
            }
        }
        public void JMP(int address) { PC = address; }
        public void BMI(int address) { if (Status.Negative) Branch(address); }
        public void BPL(int address) { if (!Status.Negative) Branch(address); }
        public void BEQ(int address) { if (Status.Zero) Branch(address); }
        public void BNE(int address) { if (!Status.Zero) Branch(address); }
        public void BCS(int address) { if (Status.Carry) Branch(address); }
        public void BCC(int address) { if (!Status.Carry) Branch(address); }
        public void BVS(int address) { if (Status.Overflow) Branch(address); }
        public void BVC(int address) { if (!Status.Overflow) Branch(address); }
        public void CLC(int address) { Status.Carry = false; }
        public void CLD(int address) { Status.Decimal = false; }
        public void CLV(int address) { Status.Overflow = false; }
        public void CLI(int address) { if (Status.IrqDisable) Status.ToggleIrqDisable = true; }
        public void SEC(int address) { Status.Carry = true; }
        public void SED(int address) { Status.Decimal = true; }
        public void SEI(int address) { if (!Status.IrqDisable) Status.ToggleIrqDisable = true; }
        #endregion

        #region ALU instructions
        public void ORA(int address) { A |= Read(address); }
        public void AND(int address) { A &= Read(address); }
        public void EOR(int address) { A ^= Read(address); }
        void ADC(byte mem, byte invMask)
        {
            mem ^= invMask;
            var result = A + mem;
            if (Status.Carry) result++;
            Status.Carry = (result & 0xFF00) != 0;
            Status.Overflow = ((A ^ result) & (mem ^ result) & 0x80) != 0;
            A = (byte)result;
        }
        public void SBC(int address) { var mem = Read(address); ADC(mem, 0xFF); }
        public void ADC(int address) { var mem = Read(address); ADC(mem, 0x00); }
        public void LDA(int address) { A = Read(address); }
        public void LDX(int address) { X = Read(address); }
        public void LDY(int address) { Y = Read(address); }
        public void STA(int address) { Write(address, A); }
        public void STX(int address) { Write(address, X); }
        public void STY(int address) { Write(address, Y); }
        public void TXA(int address) { A = X; }
        public void TAX(int address) { X = A; }
        public void TYA(int address) { A = Y; }
        public void TAY(int address) { Y = A; }
        public void TSX(int address) { X = SP; }
        public void TXS(int address) { SP = X; }
        void ASL(int address, bool carry)
        {
            var mem = Read(address);
            Write(address, mem);
            Status.Carry = (mem & 0x80) != 0;
            mem <<= 1;
            if (carry) mem |= 0x1;
            Write(address, mem);
            Result = mem;
        }
        public void ASL(int address) { ASL(address, false); }
        public void ROL(int address) { ASL(address, Status.Carry); }
        void LSR(int address, bool carry)
        {
            var mem = Read(address);
            Write(address, mem);
            Status.Carry = (mem & 1) != 0;
            mem >>= 1;
            if (carry) mem |= 0x80;
            Write(address, mem);
            Result = mem;
        }
        public void LSR(int address) { LSR(address, false); }
        public void ROR(int address) { LSR(address, Status.Carry); }
        public void DEC(int address)
        {
            var mem = Read(address);
            Write(address, mem);
            mem--;
            Write(address, mem);
            Result = mem;
        }
        public void INC(int address)
        {
            var mem = Read(address);
            Write(address, mem);
            mem++;
            Write(address, mem);
            Result = mem;
        }
        public void DEX(int address) { X--; }
        public void INX(int address) { X++; }
        public void DEY(int address) { Y--; }
        public void INY(int address) { Y++; }
        void CMP(byte reg, byte mem)
        {
            Status.Carry = reg >= mem;
            Result = (byte)(reg - mem);
        }
        public void CMP(int address) { CMP(A, Read(address)); }
        public void CPX(int address) { CMP(X, Read(address)); }
        public void CPY(int address) { CMP(Y, Read(address)); }
        #endregion

        #region Unofficial instructions
        public void SLO(int address) { ASL(address, false); A |= Result; }
        public void RLA(int address) { ASL(address, Status.Carry); A &= Result; }
        public void SRE(int address) { LSR(address, false); A ^= Result; }
        public void RRA(int address) { LSR(address, Status.Carry); ADC(Result, 0x00); }
        public void ISC(int address) { INC(address); ADC(Result, 0xFF); }
        public void DCP(int address) { DEC(address); CMP(A, Result); }
        public void SAX(int address) { Write(address, A & X); }
        public void LAX(int address) { A = X = Read(address); }
        public void LAS(int address)
        {
            SP &= Read(address);
            A = X = SP;
        }
        public void XAA(int address)
        {
            A = (byte)((A | 0xEE) & X & Read(address));
        }
        public void AHX(int address)
        {
            Write(address, A & X & (address >> 8) + 1);
        }
        public void TAS(int address)
        {
            SP = (byte)(A & X);
            Write(address, SP & (address >> 8) + 1);
        }
        public void SHX(int address)
        {
            var addressH = (byte)(address >> 8);
            addressH++;
            addressH &= X;
            address = addressH << 8 | address & 0xFF;
            Write(address, addressH);
        }
        public void SHY(int address)
        {
            var addressH = (byte)(address >> 8);
            addressH++;
            addressH &= Y;
            address = addressH << 8 | address & 0xFF;
            Write(address, addressH);
        }
        public void ANC(int address)
        {
            A &= Read(address);
            Status.Carry = Status.Negative;
        }
        public void AXS(int address)
        {
            var mem = (A & X) - Read(address);
            Status.Carry = (~mem >> 8) != 0;
            X = (byte)(mem);
        }
        public void ALR(int address)
        {
            A &= Read(address);
            Status.Carry = (A & 1) != 0;
            A >>= 1;
        }
        public void ARR(int address)
        {
            var mem = (A & Read(address)) >> 1;
            if (Status.Carry) mem |= 0x80;
            A = (byte)mem;
            Status.Carry = (A & 0x40) != 0;
            Status.Overflow = ((A << 1 ^ A) & 0x40) != 0;
        }
        public void STP(int address) { }//Enabled = false; }
        public void DOP(int address) { Read(address); }
        public void TOP(int address) { Read(address); }
        public void NOP(int address) { }
        #endregion
    }
}
