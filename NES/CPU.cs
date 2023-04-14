using System;
using System.Collections.Generic;
using CPU;

namespace NES
{
    partial class CPU
    {
        public byte A; // Accumulator 8-bit
        public byte X; // X register 8-bit
        public byte Y; // Y register 8-bit
        public byte SP; // Stack Pointer 8-bit
        public int PC; // Program Counter 16-bit
        public StatusRegister Status = new StatusRegister(); // Processor Status 8-bit
        private readonly Bus Bus;
        public List<Instruction.addressDelegate> IRQList = new List<Instruction.addressDelegate>();
        public int IRQDelay;
        public Instruction[] InstructionSet;
        public Instruction ActOp;

        public CPU(Bus bus)
        {
            Bus = bus;
            CreateInstructionSet();
            ActOp = InstructionSet[0];
            ActOp.Cycle = 0;
            Reset();
        }

        public void SetFlagsNZ(int result)
        {
            Status.Negative = (result & 0x80) != 0;
            Status.Zero = (result & 0xFF) == 0;
        }

        void Branch(int address)
        {
            PageCross(address, PC, true);
            PC = address & 0xFFFF;
        }

        public void Tick()
        {
            if (ActOp.Cycle == 0)
            {
                if (IRQDelay == 0 && IRQList.Count > 0)
                {
                    var irqType = IRQList[IRQList.Count - 1];
                    ActOp = new Instruction(0, BRK, irqType, 7);
                    IRQList.RemoveAt(IRQList.Count - 1);
                }
                else
                {
                    ActOp = InstructionSet[Read(PC)];
                    PC++;
                }
                ActOp.Run();
                if (IRQDelay > 0)
                    IRQDelay--;
            }
        }

        int PopStack()
        {
            SP++;
            return Read(0x100 | SP);
        }
        void PushStack(int value)
        {
            Write(0x100 | SP, value & 0xFF);
            SP--;
        }

        private byte Read(int addr)
        {
            if (addr < 0)
                return A;
            return Bus.CpuRead(addr);
        }

        private void Write(int addr, int data)
        {
            if (addr < 0)
                A = (byte)data;
            else
                Bus.CpuWrite(addr, (byte)data);
        }

        private int ReadWord(int addr)
        {
            var ptr_lo = Bus.CpuRead(addr); addr++;
            var ptr_hi = Bus.CpuRead(addr);
            return (ptr_hi << 8) | ptr_lo;
        }
        public void Reset()
        {
            IRQList.Add(RSTaddress);
        }
        public void NMI()
        {
            IRQList.Add(NMIaddress);
        }
        public void IRQ()
        {
            IRQList.Insert(0, IRQaddress);
        }

        void CreateInstructionSet()
        {
            InstructionSet = new Instruction[] {
                new Instruction(0x00, BRK, BRKaddress, 7),
                new Instruction(0x01, ORA, IZX, 6),
                new Instruction(0x02, UNK, IMP, 2),
                new Instruction(0x03, SLO, IZX, 8),
                new Instruction(0x04, TSB, ZP0, 5),
                new Instruction(0x05, ORA, ZP0, 3),
                new Instruction(0x06, ASL, ZP0, 5),
                new Instruction(0x07, SLO, ZP0, 5),
                new Instruction(0x08, PHP, IMP, 3),
                new Instruction(0x09, ORA, IMM, 2),
                new Instruction(0x0A, ASL, IMP, 2),
                new Instruction(0x0B, UNK, IMP, 2),
                new Instruction(0x0C, TSB, ABS, 6),
                new Instruction(0x0D, ORA, ABS, 4),
                new Instruction(0x0E, ASL, ABS, 6),
                new Instruction(0x0F, SLO, ABS, 6),
                new Instruction(0x10, BPL, REL, 2),
                new Instruction(0x11, ORA, IZY, 5, true),
                new Instruction(0x12, ORA, ZP0, 5),
                new Instruction(0x13, SLO, IZY, 8, true),
                new Instruction(0x14, TRB, ZP0, 5),
                new Instruction(0x15, ORA, ZPX, 4),
                new Instruction(0x16, ASL, ZPX, 6),
                new Instruction(0x17, SLO, ZPX, 6),
                new Instruction(0x18, CLC, IMP, 2),
                new Instruction(0x19, ORA, ABY, 4, true),
                new Instruction(0x1A, INC, IMP, 2),//NOP
                new Instruction(0x1B, SLO, ABY, 7, true),
                new Instruction(0x1C, TRB, ABS, 6),
                new Instruction(0x1D, ORA, ABX, 4, true),
                new Instruction(0x1E, ASL, ABX, 7),
                new Instruction(0x1F, SLO, ABX, 7, true),
                new Instruction(0x20, JSR, ABS, 6),
                new Instruction(0x21, AND, IZX, 6),
                new Instruction(0x22, UNK, IMP, 2),
                new Instruction(0x23, RLA, IZX, 8),
                new Instruction(0x24, BIT, ZP0, 3),
                new Instruction(0x25, AND, ZP0, 3),
                new Instruction(0x26, ROL, ZP0, 5),
                new Instruction(0x27, RLA, ZP0, 5),
                new Instruction(0x28, PLP, IMP, 4),
                new Instruction(0x29, AND, IMM, 2),
                new Instruction(0x2A, ROL, IMP, 2),
                new Instruction(0x2B, UNK, IMP, 2),
                new Instruction(0x2C, BIT, ABS, 4),
                new Instruction(0x2D, AND, ABS, 4),
                new Instruction(0x2E, ROL, ABS, 6),
                new Instruction(0x2F, RLA, ABS, 6),
                new Instruction(0x30, BMI, REL, 2),
                new Instruction(0x31, AND, IZY, 5, true),
                new Instruction(0x32, AND, ZP0, 5),
                new Instruction(0x33, RLA, IZY, 8, true),
                new Instruction(0x34, BIT, ZPX, 4),
                new Instruction(0x35, AND, ZPX, 4),
                new Instruction(0x36, ROL, ZPX, 6),
                new Instruction(0x37, RLA, ZPX, 6),
                new Instruction(0x38, SEC, IMP, 2),
                new Instruction(0x39, AND, ABY, 4, true),
                new Instruction(0x3A, DEC, IMP, 2),// NOP
                new Instruction(0x3B, RLA, ABY, 7, true),
                new Instruction(0x3C, BIT, ABX, 4),
                new Instruction(0x3D, AND, ABX, 4, true),
                new Instruction(0x3E, ROL, ABX, 7),
                new Instruction(0x3F, RLA, ABX, 7, true),
                new Instruction(0x40, RTI, IMP, 6),
                new Instruction(0x41, EOR, IZX, 6),
                new Instruction(0x42, UNK, IMP, 2),
                new Instruction(0x43, SRE, IZX, 8),
                new Instruction(0x44, NOP, IMP, 3),
                new Instruction(0x45, EOR, ZP0, 3),
                new Instruction(0x46, LSR, ZP0, 5),
                new Instruction(0x47, SRE, ZP0, 5),
                new Instruction(0x48, PHA, IMP, 3),
                new Instruction(0x49, EOR, IMM, 2),
                new Instruction(0x4A, LSR, IMP, 2),
                new Instruction(0x4B, UNK, IMP, 2),
                new Instruction(0x4C, JMP, ABS, 3),
                new Instruction(0x4D, EOR, ABS, 4),
                new Instruction(0x4E, LSR, ABS, 6),
                new Instruction(0x4F, SRE, ABS, 6),
                new Instruction(0x50, BVC, REL, 2),
                new Instruction(0x51, EOR, IZY, 5, true),
                new Instruction(0x52, EOR, ZP0, 5),
                new Instruction(0x53, SRE, IZY, 8, true),
                new Instruction(0x54, NOP, IMP, 4),
                new Instruction(0x55, EOR, ZPX, 4),
                new Instruction(0x56, LSR, ZPX, 6),
                new Instruction(0x57, SRE, ZPX, 6),
                new Instruction(0x58, CLI, IMP, 2),
                new Instruction(0x59, EOR, ABY, 4, true),
                new Instruction(0x5A, PHY, IMP, 2),// NOP
                new Instruction(0x5B, SRE, ABY, 7, true),
                new Instruction(0x5C, NOP, IMP, 4),
                new Instruction(0x5D, EOR, ABX, 4, true),
                new Instruction(0x5E, LSR, ABX, 7),
                new Instruction(0x5F, SRE, ABX, 7, true),
                new Instruction(0x60, RTS, IMP, 6),
                new Instruction(0x61, ADC, IZX, 6),
                new Instruction(0x62, UNK, IMP, 2),
                new Instruction(0x63, RRA, IZX, 8),
                new Instruction(0x64, STZ, ZP0, 3),
                new Instruction(0x65, ADC, ZP0, 3),
                new Instruction(0x66, ROR, ZP0, 5),
                new Instruction(0x67, RRA, ZP0, 5),
                new Instruction(0x68, PLA, IMP, 4),
                new Instruction(0x69, ADC, IMM, 2),
                new Instruction(0x6A, ROR, IMP, 2),
                new Instruction(0x6B, UNK, IMP, 2),
                new Instruction(0x6C, JMP, IND, 5),
                new Instruction(0x6D, ADC, ABS, 4),
                new Instruction(0x6E, ROR, ABS, 6),
                new Instruction(0x6F, RRA, ABS, 6),
                new Instruction(0x70, BVS, REL, 2),
                new Instruction(0x71, ADC, IZY, 5, true),
                new Instruction(0x72, ADC, ZP0, 5),
                new Instruction(0x73, RRA, IZY, 8, true),
                new Instruction(0x74, STZ, ZPX, 4),
                new Instruction(0x75, ADC, ZPX, 4),
                new Instruction(0x76, ROR, ZPX, 6),
                new Instruction(0x77, RRA, ZPX, 6),
                new Instruction(0x78, SEI, IMP, 2),
                new Instruction(0x79, ADC, ABY, 4, true),
                new Instruction(0x7A, PLY, IMP, 4),// NOP
                new Instruction(0x7B, RRA, ABY, 7, true),
                new Instruction(0x7C, JMP, ABX, 6),
                new Instruction(0x7D, ADC, ABX, 4, true),
                new Instruction(0x7E, ROR, ABX, 7),
                new Instruction(0x7F, RRA, ABX, 7, true),
                new Instruction(0x80, BRA, REL, 3),
                new Instruction(0x81, STA, IZX, 6),
                new Instruction(0x82, NOP, IMP, 2),
                new Instruction(0x83, UNK, IMP, 6),
                new Instruction(0x84, STY, ZP0, 3),
                new Instruction(0x85, STA, ZP0, 3),
                new Instruction(0x86, STX, ZP0, 3),
                new Instruction(0x87, UNK, IMP, 3),
                new Instruction(0x88, DEY, IMP, 2),
                new Instruction(0x89, BIT, IMM, 3),
                new Instruction(0x8A, TXA, IMP, 2),
                new Instruction(0x8B, UNK, IMP, 2),
                new Instruction(0x8C, STY, ABS, 4),
                new Instruction(0x8D, STA, ABS, 4),
                new Instruction(0x8E, STX, ABS, 4),
                new Instruction(0x8F, UNK, IMP, 4),
                new Instruction(0x90, BCC, REL, 2),
                new Instruction(0x91, STA, IZY, 6),
                new Instruction(0x92, STA, ZP0, 5),
                new Instruction(0x93, UNK, IMP, 6),
                new Instruction(0x94, STY, ZPX, 4),
                new Instruction(0x95, STA, ZPX, 4),
                new Instruction(0x96, STX, ZPY, 4),
                new Instruction(0x97, UNK, IMP, 4),
                new Instruction(0x98, TYA, IMP, 2),
                new Instruction(0x99, STA, ABY, 5),
                new Instruction(0x9A, TXS, IMP, 2),
                new Instruction(0x9B, UNK, IMP, 5),
                new Instruction(0x9C, STZ, ABS, 4),
                new Instruction(0x9D, STA, ABX, 5),
                new Instruction(0x9E, STZ, ABX, 5),
                new Instruction(0x9F, UNK, IMP, 5),
                new Instruction(0xA0, LDY, IMM, 2),
                new Instruction(0xA1, LDA, IZX, 6),
                new Instruction(0xA2, LDX, IMM, 2),
                new Instruction(0xA3, UNK, IMP, 6),
                new Instruction(0xA4, LDY, ZP0, 3),
                new Instruction(0xA5, LDA, ZP0, 3),
                new Instruction(0xA6, LDX, ZP0, 3),
                new Instruction(0xA7, UNK, IMP, 3),
                new Instruction(0xA8, TAY, IMP, 2),
                new Instruction(0xA9, LDA, IMM, 2),
                new Instruction(0xAA, TAX, IMP, 2),
                new Instruction(0xAB, UNK, IMP, 2),
                new Instruction(0xAC, LDY, ABS, 4),
                new Instruction(0xAD, LDA, ABS, 4),
                new Instruction(0xAE, LDX, ABS, 4),
                new Instruction(0xAF, UNK, IMP, 4),
                new Instruction(0xB0, BCS, REL, 2),
                new Instruction(0xB1, LDA, IZY, 5, true),
                new Instruction(0xB2, LDA, ZP0, 5),
                new Instruction(0xB3, UNK, IMP, 5),
                new Instruction(0xB4, LDY, ZPX, 4),
                new Instruction(0xB5, LDA, ZPX, 4),
                new Instruction(0xB6, LDX, ZPY, 4),
                new Instruction(0xB7, UNK, IMP, 4),
                new Instruction(0xB8, CLV, IMP, 2),
                new Instruction(0xB9, LDA, ABY, 4, true),
                new Instruction(0xBA, TSX, IMP, 2),
                new Instruction(0xBB, UNK, IMP, 4),
                new Instruction(0xBC, LDY, ABX, 4, true),
                new Instruction(0xBD, LDA, ABX, 4, true),
                new Instruction(0xBE, LDX, ABY, 4, true),
                new Instruction(0xBF, UNK, IMP, 4),
                new Instruction(0xC0, CPY, IMM, 2),
                new Instruction(0xC1, CMP, IZX, 6),
                new Instruction(0xC2, NOP, IMP, 2),
                new Instruction(0xC3, DCP, IZX, 8),
                new Instruction(0xC4, CPY, ZP0, 3),
                new Instruction(0xC5, CMP, ZP0, 3),
                new Instruction(0xC6, DEC, ZP0, 5),
                new Instruction(0xC7, DCP, ZP0, 5),
                new Instruction(0xC8, INY, IMP, 2),
                new Instruction(0xC9, CMP, IMM, 2),
                new Instruction(0xCA, DEX, IMP, 2),
                new Instruction(0xCB, WAI, IMP, 2),
                new Instruction(0xCC, CPY, ABS, 4),
                new Instruction(0xCD, CMP, ABS, 4),
                new Instruction(0xCE, DEC, ABS, 6),
                new Instruction(0xCF, DCP, ABS, 6),
                new Instruction(0xD0, BNE, REL, 2),
                new Instruction(0xD1, CMP, IZY, 5, true),
                new Instruction(0xD2, CMP, ZP0, 5),
                new Instruction(0xD3, DCP, IZY, 8, true),
                new Instruction(0xD4, NOP, IMP, 4),
                new Instruction(0xD5, CMP, ZPX, 4),
                new Instruction(0xD6, DEC, ZPX, 6),
                new Instruction(0xD7, DCP, ZPX, 6),
                new Instruction(0xD8, CLD, IMP, 2),
                new Instruction(0xD9, CMP, ABY, 4, true),
                new Instruction(0xDA, PHX, IMP, 3),// NOP
                new Instruction(0xDB, DCP, ABY, 7, true),
                new Instruction(0xDC, NOP, IMP, 4),
                new Instruction(0xDD, CMP, ABX, 4, true),
                new Instruction(0xDE, DEC, ABX, 7),
                new Instruction(0xDF, DCP, ABX, 7, true),
                new Instruction(0xE0, CPX, IMM, 2),
                new Instruction(0xE1, SBC, IZX, 6),
                new Instruction(0xE2, NOP, IMP, 2),
                new Instruction(0xE3, ISC, IZX, 8),
                new Instruction(0xE4, CPX, ZP0, 3),
                new Instruction(0xE5, SBC, ZP0, 3),
                new Instruction(0xE6, INC, ZP0, 5),
                new Instruction(0xE7, ISC, ZP0, 5),
                new Instruction(0xE8, INX, IMP, 2),
                new Instruction(0xE9, SBC, IMM, 2),
                new Instruction(0xEA, NOP, IMP, 2),
                new Instruction(0xEB, SBC, IMP, 2),
                new Instruction(0xEC, CPX, ABS, 4),
                new Instruction(0xED, SBC, ABS, 4),
                new Instruction(0xEE, INC, ABS, 6),
                new Instruction(0xEF, ISC, ABS, 6),
                new Instruction(0xF0, BEQ, REL, 2),
                new Instruction(0xF1, SBC, IZY, 5, true),
                new Instruction(0xF2, SBC, ZP0, 5),
                new Instruction(0xF3, ISC, IZY, 8, true),
                new Instruction(0xF4, NOP, IMP, 4),
                new Instruction(0xF5, SBC, ZPX, 4),
                new Instruction(0xF6, INC, ZPX, 6),
                new Instruction(0xF7, ISC, ZPX, 6),
                new Instruction(0xF8, SED, IMP, 2),
                new Instruction(0xF9, SBC, ABY, 4, true),
                new Instruction(0xFA, NOP, IMP, 2),
                new Instruction(0xFB, ISC, ABY, 7, true),
                new Instruction(0xFC, NOP, IMP, 4),
                new Instruction(0xFD, SBC, ABX, 4, true),
                new Instruction(0xFE, INC, ABX, 7),
                new Instruction(0xFF, ISC, ABX, 7, true)
            };
        }

        public void PageCross(int address, int pc, bool isBranch = false)
        {
            //var isBranch = ActOp.AddressingMode.Method.Name == "REL";
            if (isBranch)
            {
                ActOp.Cycle++;
                Read(PC); // dummy read
            }
            if ((address & 0xFF00) != (pc & 0xFF00))
            {
                Read(address - 0x100); // dummy read
                if (ActOp.ExtraCycle || isBranch)
                    ActOp.Cycle++;
            }
            else if (!ActOp.ExtraCycle)
                Read(address); // dummy read
        }

    }

    class Instruction
    {
        public byte Opcode;
        public delegate void opDelegate(int address);
        public opDelegate Operation;
        public delegate int addressDelegate();
        public addressDelegate AddressingMode;
        public byte CycleCount;
        public bool ExtraCycle; // Extra cycle on page cross?
        public int Cycle;
        public Instruction(byte _opcode, opDelegate _operation, addressDelegate _addressingMode, byte _cycles, bool _extraCycle = false)
        {
            Opcode = _opcode;
            Operation = _operation;
            AddressingMode = _addressingMode;
            CycleCount = _cycles;
            ExtraCycle = _extraCycle;
            Cycle = CycleCount;
        }
        public void Run()
        {
            Cycle = CycleCount;
            Operation(AddressingMode());
        }
    }

}
