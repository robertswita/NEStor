using System;
using System.Collections.Generic;

namespace NESemuCore.CPU6502
{
    partial class CPU
    {
        public Byte A = 0x00; // Accumulator 8-bit
        public Byte X = 0x00; // X register 8-bit
        public Byte Y = 0x00; // Y register 8-bit
        public Byte StackPointer = 0x00; // Stack Pointer 8-bit
        public UInt16 PC = 0x0000; // Program Counter 16-bit
        public Byte Status = 0x00; // Processor Status 8-bit

        public List<INSTRUCTION> lookup = new List<INSTRUCTION>();

        private Byte _fetched = 0x00;

        private UInt16 _addrAbs = 0x0000;
        private UInt16 _addrRel = 0x00;
        private Byte _opcode = 0x00;
        private Byte _cycles = 0;

        private UInt16 _temp = 0x0000;

        private readonly Bus _bus;

        public CPU(Bus bus)
        {
            SetFlag(FLAGS6502.U, true);
            _bus = bus;
            PopulateLookupTable();
        }

        //bool test = true;
        //bool overwrite = true;

        public void Clock()
        {
            if (_cycles == 0)
            {
                //if (overwrite)
                //{
                //    PC = 0xFFE0;
                //    overwrite = false;
                //}

                _opcode = Read(PC);

                PC++;

                _cycles = lookup[_opcode].cycles;

                //if(PC == 0x801A)
                //{
                //    test = true;
                //}

                //if (test)
                //{
                //    //Console.WriteLine("[" + PC.ToString("X4") + "] " + lookup[_opcode].name);
                //    Console.WriteLine("PC:" + (PC - 1).ToString("X4") + " " + lookup[_opcode].name + "(" +_opcode.ToString("X2")+ ")" +
                //        " A:" + A.ToString("X2") + " X:" + X.ToString("X2") + " Y:" + Y.ToString("X2") +
                //        " "+ (GetFlag(FLAGS6502.N) != 0 ? "N" : ".") +
                //        (GetFlag(FLAGS6502.V) != 0 ? (string)"V" : (string)".") +
                //        (GetFlag(FLAGS6502.U) != 0 ? "U" : ".") +
                //        (GetFlag(FLAGS6502.B) != 0 ? "B" : ".") +
                //        (GetFlag(FLAGS6502.D) != 0 ? "D" : ".") +
                //        (GetFlag(FLAGS6502.I) != 0 ? "I" : ".") +
                //        (GetFlag(FLAGS6502.Z) != 0 ? "Z" : ".") +
                //        (GetFlag(FLAGS6502.C) != 0 ? "C" : ".")
                //        + " STKP:" + StackPointer.ToString("X2"));
                //    Console.ReadKey();
                //}

                Byte additional_cycle1 = lookup[_opcode].addressingMode();
                Byte additional_cycle2 = lookup[_opcode].operation();

                _cycles += (Byte)(additional_cycle1 & additional_cycle2);
            }
            _cycles--;
        }

        public void Reset()
        {
            _addrAbs = 0xFFFC;
            UInt16 lo = Read((UInt16)(_addrAbs + 0));
            UInt16 hi = Read((UInt16)(_addrAbs + 1));

            PC = (UInt16)((hi << 8) | lo);

            A = 0;
            X = 0;
            Y = 0;
            StackPointer = 0xFD;
            Status = 0x00 | (Byte)FLAGS6502.U;

            _addrRel = 0x0000;
            _addrAbs = 0x0000;
            _fetched = 0x00;

            _cycles = 8;
        }

        // Used for debugging
        public Boolean Complete()
        {
            return _cycles == 0; // true if all _cycles completed
        }

        public void SetFlag(FLAGS6502 f, Boolean v)
        {
            if (v)
                Status |= (Byte)f;
            else
                Status &= (Byte)~f;
        }

        public Byte GetFlag(FLAGS6502 f)
        {
            return ((Status & (Byte)f) > 0) ? (Byte)1 : (Byte)0;
        }

        private Byte Read(UInt16 addr)
        {
            return _bus.CpuRead(addr, false);
        }

        private void Write(UInt16 addr, Byte data)
        {
            _bus.CpuWrite(addr, data);
        }

        public Byte Fetch()
        {
            if (!(lookup[_opcode].addressingMode == IMP))
                _fetched = Read(_addrAbs);

            return _fetched;
        }

        public void IRQ()
        {
            if (GetFlag(FLAGS6502.I) == 0)
            {
                Write((UInt16)(0x0100 + StackPointer), (Byte)((PC >> 8) & 0x00FF));
                StackPointer--;
                Write((UInt16)(0x0100 + StackPointer), (Byte)(PC & 0x00FF));
                StackPointer--;

                SetFlag(FLAGS6502.B, false);
                SetFlag(FLAGS6502.U, true);
                SetFlag(FLAGS6502.I, true);
                Write((UInt16)(0x0100 + StackPointer), Status);
                StackPointer--;

                _addrAbs = 0xFFFE;
                UInt16 lo = Read((UInt16)(_addrAbs + 0));
                UInt16 hi = Read((UInt16)(_addrAbs + 1));
                PC = (UInt16)((hi << 8) | lo);

                _cycles = 7;
            }
        }

        public void NMI()
        {
            Write((UInt16)(0x0100 + StackPointer), (Byte)((PC >> 8) & 0x00FF));
            StackPointer--;
            Write((UInt16)(0x0100 + StackPointer), (Byte)(PC & 0x00FF));
            StackPointer--;

            SetFlag(FLAGS6502.B, false);
            SetFlag(FLAGS6502.U, true);
            SetFlag(FLAGS6502.I, true);
            Write((UInt16)(0x0100 + StackPointer), Status);
            StackPointer--;

            _addrAbs = 0xFFFA;
            UInt16 lo = Read((UInt16)(_addrAbs + 0));
            UInt16 hi = Read((UInt16)(_addrAbs + 1));
            PC = (UInt16)((hi << 8) | lo);

            _cycles = 8;
        }

        private void PopulateLookupTable()
        {
            Byte i = 0x00;

            lookup.Insert(i, new INSTRUCTION() { name = "BRK", opcode = i, operation = BRK, addressingMode = IMM, cycles = 7 });
            lookup.Insert(++i, new INSTRUCTION() { name = "ORA", opcode = i, operation = ORA, addressingMode = IZX, cycles = 6 });
            lookup.Insert(++i, new INSTRUCTION() { name = "???", opcode = i, operation = UNK, addressingMode = IMP, cycles = 2 });
            lookup.Insert(++i, new INSTRUCTION() { name = "???", opcode = i, operation = UNK, addressingMode = IMP, cycles = 8 });
            lookup.Insert(++i, new INSTRUCTION() { name = "???", opcode = i, operation = NOP, addressingMode = IMP, cycles = 3 });
            lookup.Insert(++i, new INSTRUCTION() { name = "ORA", opcode = i, operation = ORA, addressingMode = ZP0, cycles = 3 });
            lookup.Insert(++i, new INSTRUCTION() { name = "ASL", opcode = i, operation = ASL, addressingMode = ZP0, cycles = 5 });
            lookup.Insert(++i, new INSTRUCTION() { name = "???", opcode = i, operation = UNK, addressingMode = IMP, cycles = 5 });
            lookup.Insert(++i, new INSTRUCTION() { name = "PHP", opcode = i, operation = PHP, addressingMode = IMP, cycles = 3 });
            lookup.Insert(++i, new INSTRUCTION() { name = "ORA", opcode = i, operation = ORA, addressingMode = IMM, cycles = 2 });
            lookup.Insert(++i, new INSTRUCTION() { name = "ASL", opcode = i, operation = ASL, addressingMode = IMP, cycles = 2 });
            lookup.Insert(++i, new INSTRUCTION() { name = "???", opcode = i, operation = UNK, addressingMode = IMP, cycles = 2 });
            lookup.Insert(++i, new INSTRUCTION() { name = "???", opcode = i, operation = NOP, addressingMode = IMP, cycles = 4 });
            lookup.Insert(++i, new INSTRUCTION() { name = "ORA", opcode = i, operation = ORA, addressingMode = ABS, cycles = 4 });
            lookup.Insert(++i, new INSTRUCTION() { name = "ASL", opcode = i, operation = ASL, addressingMode = ABS, cycles = 6 });
            lookup.Insert(++i, new INSTRUCTION() { name = "???", opcode = i, operation = UNK, addressingMode = IMP, cycles = 6 });
            lookup.Insert(++i, new INSTRUCTION() { name = "BPL", opcode = i, operation = BPL, addressingMode = REL, cycles = 2 });
            lookup.Insert(++i, new INSTRUCTION() { name = "ORA", opcode = i, operation = ORA, addressingMode = IZY, cycles = 5 });
            lookup.Insert(++i, new INSTRUCTION() { name = "???", opcode = i, operation = UNK, addressingMode = IMP, cycles = 2 });
            lookup.Insert(++i, new INSTRUCTION() { name = "???", opcode = i, operation = UNK, addressingMode = IMP, cycles = 8 });
            lookup.Insert(++i, new INSTRUCTION() { name = "???", opcode = i, operation = NOP, addressingMode = IMP, cycles = 4 });
            lookup.Insert(++i, new INSTRUCTION() { name = "ORA", opcode = i, operation = ORA, addressingMode = ZPX, cycles = 4 });
            lookup.Insert(++i, new INSTRUCTION() { name = "ASL", opcode = i, operation = ASL, addressingMode = ZPX, cycles = 6 });
            lookup.Insert(++i, new INSTRUCTION() { name = "???", opcode = i, operation = UNK, addressingMode = IMP, cycles = 6 });
            lookup.Insert(++i, new INSTRUCTION() { name = "CLC", opcode = i, operation = CLC, addressingMode = IMP, cycles = 2 });
            lookup.Insert(++i, new INSTRUCTION() { name = "ORA", opcode = i, operation = ORA, addressingMode = ABY, cycles = 4 });
            lookup.Insert(++i, new INSTRUCTION() { name = "???", opcode = i, operation = NOP, addressingMode = IMP, cycles = 2 });
            lookup.Insert(++i, new INSTRUCTION() { name = "???", opcode = i, operation = UNK, addressingMode = IMP, cycles = 7 });
            lookup.Insert(++i, new INSTRUCTION() { name = "???", opcode = i, operation = NOP, addressingMode = IMP, cycles = 4 });
            lookup.Insert(++i, new INSTRUCTION() { name = "ORA", opcode = i, operation = ORA, addressingMode = ABX, cycles = 4 });
            lookup.Insert(++i, new INSTRUCTION() { name = "ASL", opcode = i, operation = ASL, addressingMode = ABX, cycles = 7 });
            lookup.Insert(++i, new INSTRUCTION() { name = "???", opcode = i, operation = UNK, addressingMode = IMP, cycles = 7 });
            lookup.Insert(++i, new INSTRUCTION() { name = "JSR", opcode = i, operation = JSR, addressingMode = ABS, cycles = 6 });
            lookup.Insert(++i, new INSTRUCTION() { name = "AND", opcode = i, operation = AND, addressingMode = IZX, cycles = 6 });
            lookup.Insert(++i, new INSTRUCTION() { name = "???", opcode = i, operation = UNK, addressingMode = IMP, cycles = 2 });
            lookup.Insert(++i, new INSTRUCTION() { name = "???", opcode = i, operation = UNK, addressingMode = IMP, cycles = 8 });
            lookup.Insert(++i, new INSTRUCTION() { name = "BIT", opcode = i, operation = BIT, addressingMode = ZP0, cycles = 3 });
            lookup.Insert(++i, new INSTRUCTION() { name = "AND", opcode = i, operation = AND, addressingMode = ZP0, cycles = 3 });
            lookup.Insert(++i, new INSTRUCTION() { name = "ROL", opcode = i, operation = ROL, addressingMode = ZP0, cycles = 5 });
            lookup.Insert(++i, new INSTRUCTION() { name = "???", opcode = i, operation = UNK, addressingMode = IMP, cycles = 5 });
            lookup.Insert(++i, new INSTRUCTION() { name = "PLP", opcode = i, operation = PLP, addressingMode = IMP, cycles = 4 });
            lookup.Insert(++i, new INSTRUCTION() { name = "AND", opcode = i, operation = AND, addressingMode = IMM, cycles = 2 });
            lookup.Insert(++i, new INSTRUCTION() { name = "ROL", opcode = i, operation = ROL, addressingMode = IMP, cycles = 2 });
            lookup.Insert(++i, new INSTRUCTION() { name = "???", opcode = i, operation = UNK, addressingMode = IMP, cycles = 2 });
            lookup.Insert(++i, new INSTRUCTION() { name = "BIT", opcode = i, operation = BIT, addressingMode = ABS, cycles = 4 });
            lookup.Insert(++i, new INSTRUCTION() { name = "AND", opcode = i, operation = AND, addressingMode = ABS, cycles = 4 });
            lookup.Insert(++i, new INSTRUCTION() { name = "ROL", opcode = i, operation = ROL, addressingMode = ABS, cycles = 6 });
            lookup.Insert(++i, new INSTRUCTION() { name = "???", opcode = i, operation = UNK, addressingMode = IMP, cycles = 6 });
            lookup.Insert(++i, new INSTRUCTION() { name = "BMI", opcode = i, operation = BMI, addressingMode = REL, cycles = 2 });
            lookup.Insert(++i, new INSTRUCTION() { name = "AND", opcode = i, operation = AND, addressingMode = IZY, cycles = 5 });
            lookup.Insert(++i, new INSTRUCTION() { name = "???", opcode = i, operation = UNK, addressingMode = IMP, cycles = 2 });
            lookup.Insert(++i, new INSTRUCTION() { name = "???", opcode = i, operation = UNK, addressingMode = IMP, cycles = 8 });
            lookup.Insert(++i, new INSTRUCTION() { name = "???", opcode = i, operation = NOP, addressingMode = IMP, cycles = 4 });
            lookup.Insert(++i, new INSTRUCTION() { name = "AND", opcode = i, operation = AND, addressingMode = ZPX, cycles = 4 });
            lookup.Insert(++i, new INSTRUCTION() { name = "ROL", opcode = i, operation = ROL, addressingMode = ZPX, cycles = 6 });
            lookup.Insert(++i, new INSTRUCTION() { name = "???", opcode = i, operation = UNK, addressingMode = IMP, cycles = 6 });
            lookup.Insert(++i, new INSTRUCTION() { name = "SEC", opcode = i, operation = SEC, addressingMode = IMP, cycles = 2 });
            lookup.Insert(++i, new INSTRUCTION() { name = "AND", opcode = i, operation = AND, addressingMode = ABY, cycles = 4 });
            lookup.Insert(++i, new INSTRUCTION() { name = "???", opcode = i, operation = NOP, addressingMode = IMP, cycles = 2 });
            lookup.Insert(++i, new INSTRUCTION() { name = "???", opcode = i, operation = UNK, addressingMode = IMP, cycles = 7 });
            lookup.Insert(++i, new INSTRUCTION() { name = "???", opcode = i, operation = NOP, addressingMode = IMP, cycles = 4 });
            lookup.Insert(++i, new INSTRUCTION() { name = "AND", opcode = i, operation = AND, addressingMode = ABX, cycles = 4 });
            lookup.Insert(++i, new INSTRUCTION() { name = "ROL", opcode = i, operation = ROL, addressingMode = ABX, cycles = 7 });
            lookup.Insert(++i, new INSTRUCTION() { name = "???", opcode = i, operation = UNK, addressingMode = IMP, cycles = 7 });
            lookup.Insert(++i, new INSTRUCTION() { name = "RTI", opcode = i, operation = RTI, addressingMode = IMP, cycles = 6 });
            lookup.Insert(++i, new INSTRUCTION() { name = "EOR", opcode = i, operation = EOR, addressingMode = IZX, cycles = 6 });
            lookup.Insert(++i, new INSTRUCTION() { name = "???", opcode = i, operation = UNK, addressingMode = IMP, cycles = 2 });
            lookup.Insert(++i, new INSTRUCTION() { name = "???", opcode = i, operation = UNK, addressingMode = IMP, cycles = 8 });
            lookup.Insert(++i, new INSTRUCTION() { name = "???", opcode = i, operation = NOP, addressingMode = IMP, cycles = 3 });
            lookup.Insert(++i, new INSTRUCTION() { name = "EOR", opcode = i, operation = EOR, addressingMode = ZP0, cycles = 3 });
            lookup.Insert(++i, new INSTRUCTION() { name = "LSR", opcode = i, operation = LSR, addressingMode = ZP0, cycles = 5 });
            lookup.Insert(++i, new INSTRUCTION() { name = "???", opcode = i, operation = UNK, addressingMode = IMP, cycles = 5 });
            lookup.Insert(++i, new INSTRUCTION() { name = "PHA", opcode = i, operation = PHA, addressingMode = IMP, cycles = 3 });
            lookup.Insert(++i, new INSTRUCTION() { name = "EOR", opcode = i, operation = EOR, addressingMode = IMM, cycles = 2 });
            lookup.Insert(++i, new INSTRUCTION() { name = "LSR", opcode = i, operation = LSR, addressingMode = IMP, cycles = 2 });
            lookup.Insert(++i, new INSTRUCTION() { name = "???", opcode = i, operation = UNK, addressingMode = IMP, cycles = 2 });
            lookup.Insert(++i, new INSTRUCTION() { name = "JMP", opcode = i, operation = JMP, addressingMode = ABS, cycles = 3 });
            lookup.Insert(++i, new INSTRUCTION() { name = "EOR", opcode = i, operation = EOR, addressingMode = ABS, cycles = 4 });
            lookup.Insert(++i, new INSTRUCTION() { name = "LSR", opcode = i, operation = LSR, addressingMode = ABS, cycles = 6 });
            lookup.Insert(++i, new INSTRUCTION() { name = "???", opcode = i, operation = UNK, addressingMode = IMP, cycles = 6 });
            lookup.Insert(++i, new INSTRUCTION() { name = "BVC", opcode = i, operation = BVC, addressingMode = REL, cycles = 2 });
            lookup.Insert(++i, new INSTRUCTION() { name = "EOR", opcode = i, operation = EOR, addressingMode = IZY, cycles = 5 });
            lookup.Insert(++i, new INSTRUCTION() { name = "???", opcode = i, operation = UNK, addressingMode = IMP, cycles = 2 });
            lookup.Insert(++i, new INSTRUCTION() { name = "???", opcode = i, operation = UNK, addressingMode = IMP, cycles = 8 });
            lookup.Insert(++i, new INSTRUCTION() { name = "???", opcode = i, operation = NOP, addressingMode = IMP, cycles = 4 });
            lookup.Insert(++i, new INSTRUCTION() { name = "EOR", opcode = i, operation = EOR, addressingMode = ZPX, cycles = 4 });
            lookup.Insert(++i, new INSTRUCTION() { name = "LSR", opcode = i, operation = LSR, addressingMode = ZPX, cycles = 6 });
            lookup.Insert(++i, new INSTRUCTION() { name = "???", opcode = i, operation = UNK, addressingMode = IMP, cycles = 6 });
            lookup.Insert(++i, new INSTRUCTION() { name = "CLI", opcode = i, operation = CLI, addressingMode = IMP, cycles = 2 });
            lookup.Insert(++i, new INSTRUCTION() { name = "EOR", opcode = i, operation = EOR, addressingMode = ABY, cycles = 4 });
            lookup.Insert(++i, new INSTRUCTION() { name = "???", opcode = i, operation = NOP, addressingMode = IMP, cycles = 2 });
            lookup.Insert(++i, new INSTRUCTION() { name = "???", opcode = i, operation = UNK, addressingMode = IMP, cycles = 7 });
            lookup.Insert(++i, new INSTRUCTION() { name = "???", opcode = i, operation = NOP, addressingMode = IMP, cycles = 4 });
            lookup.Insert(++i, new INSTRUCTION() { name = "EOR", opcode = i, operation = EOR, addressingMode = ABX, cycles = 4 });
            lookup.Insert(++i, new INSTRUCTION() { name = "LSR", opcode = i, operation = LSR, addressingMode = ABX, cycles = 7 });
            lookup.Insert(++i, new INSTRUCTION() { name = "???", opcode = i, operation = UNK, addressingMode = IMP, cycles = 7 });
            lookup.Insert(++i, new INSTRUCTION() { name = "RTS", opcode = i, operation = RTS, addressingMode = IMP, cycles = 6 });
            lookup.Insert(++i, new INSTRUCTION() { name = "ADC", opcode = i, operation = ADC, addressingMode = IZX, cycles = 6 });
            lookup.Insert(++i, new INSTRUCTION() { name = "???", opcode = i, operation = UNK, addressingMode = IMP, cycles = 2 });
            lookup.Insert(++i, new INSTRUCTION() { name = "???", opcode = i, operation = UNK, addressingMode = IMP, cycles = 8 });
            lookup.Insert(++i, new INSTRUCTION() { name = "???", opcode = i, operation = NOP, addressingMode = IMP, cycles = 3 });
            lookup.Insert(++i, new INSTRUCTION() { name = "ADC", opcode = i, operation = ADC, addressingMode = ZP0, cycles = 3 });
            lookup.Insert(++i, new INSTRUCTION() { name = "ROR", opcode = i, operation = ROR, addressingMode = ZP0, cycles = 5 });
            lookup.Insert(++i, new INSTRUCTION() { name = "???", opcode = i, operation = UNK, addressingMode = IMP, cycles = 5 });
            lookup.Insert(++i, new INSTRUCTION() { name = "PLA", opcode = i, operation = PLA, addressingMode = IMP, cycles = 4 });
            lookup.Insert(++i, new INSTRUCTION() { name = "ADC", opcode = i, operation = ADC, addressingMode = IMM, cycles = 2 });
            lookup.Insert(++i, new INSTRUCTION() { name = "ROR", opcode = i, operation = ROR, addressingMode = IMP, cycles = 2 });
            lookup.Insert(++i, new INSTRUCTION() { name = "???", opcode = i, operation = UNK, addressingMode = IMP, cycles = 2 });
            lookup.Insert(++i, new INSTRUCTION() { name = "JMP", opcode = i, operation = JMP, addressingMode = IND, cycles = 5 });
            lookup.Insert(++i, new INSTRUCTION() { name = "ADC", opcode = i, operation = ADC, addressingMode = ABS, cycles = 4 });
            lookup.Insert(++i, new INSTRUCTION() { name = "ROR", opcode = i, operation = ROR, addressingMode = ABS, cycles = 6 });
            lookup.Insert(++i, new INSTRUCTION() { name = "???", opcode = i, operation = UNK, addressingMode = IMP, cycles = 6 });
            lookup.Insert(++i, new INSTRUCTION() { name = "BVS", opcode = i, operation = BVS, addressingMode = REL, cycles = 2 });
            lookup.Insert(++i, new INSTRUCTION() { name = "ADC", opcode = i, operation = ADC, addressingMode = IZY, cycles = 5 });
            lookup.Insert(++i, new INSTRUCTION() { name = "???", opcode = i, operation = UNK, addressingMode = IMP, cycles = 2 });
            lookup.Insert(++i, new INSTRUCTION() { name = "???", opcode = i, operation = UNK, addressingMode = IMP, cycles = 8 });
            lookup.Insert(++i, new INSTRUCTION() { name = "???", opcode = i, operation = NOP, addressingMode = IMP, cycles = 4 });
            lookup.Insert(++i, new INSTRUCTION() { name = "ADC", opcode = i, operation = ADC, addressingMode = ZPX, cycles = 4 });
            lookup.Insert(++i, new INSTRUCTION() { name = "ROR", opcode = i, operation = ROR, addressingMode = ZPX, cycles = 6 });
            lookup.Insert(++i, new INSTRUCTION() { name = "???", opcode = i, operation = UNK, addressingMode = IMP, cycles = 6 });
            lookup.Insert(++i, new INSTRUCTION() { name = "SEI", opcode = i, operation = SEI, addressingMode = IMP, cycles = 2 });
            lookup.Insert(++i, new INSTRUCTION() { name = "ADC", opcode = i, operation = ADC, addressingMode = ABY, cycles = 4 });
            lookup.Insert(++i, new INSTRUCTION() { name = "???", opcode = i, operation = NOP, addressingMode = IMP, cycles = 2 });
            lookup.Insert(++i, new INSTRUCTION() { name = "???", opcode = i, operation = UNK, addressingMode = IMP, cycles = 7 });
            lookup.Insert(++i, new INSTRUCTION() { name = "???", opcode = i, operation = NOP, addressingMode = IMP, cycles = 4 });
            lookup.Insert(++i, new INSTRUCTION() { name = "ADC", opcode = i, operation = ADC, addressingMode = ABX, cycles = 4 });
            lookup.Insert(++i, new INSTRUCTION() { name = "ROR", opcode = i, operation = ROR, addressingMode = ABX, cycles = 7 });
            lookup.Insert(++i, new INSTRUCTION() { name = "???", opcode = i, operation = UNK, addressingMode = IMP, cycles = 7 });
            lookup.Insert(++i, new INSTRUCTION() { name = "???", opcode = i, operation = NOP, addressingMode = IMP, cycles = 2 });
            lookup.Insert(++i, new INSTRUCTION() { name = "STA", opcode = i, operation = STA, addressingMode = IZX, cycles = 6 });
            lookup.Insert(++i, new INSTRUCTION() { name = "???", opcode = i, operation = NOP, addressingMode = IMP, cycles = 2 });
            lookup.Insert(++i, new INSTRUCTION() { name = "???", opcode = i, operation = UNK, addressingMode = IMP, cycles = 6 });
            lookup.Insert(++i, new INSTRUCTION() { name = "STY", opcode = i, operation = STY, addressingMode = ZP0, cycles = 3 });
            lookup.Insert(++i, new INSTRUCTION() { name = "STA", opcode = i, operation = STA, addressingMode = ZP0, cycles = 3 });
            lookup.Insert(++i, new INSTRUCTION() { name = "STX", opcode = i, operation = STX, addressingMode = ZP0, cycles = 3 });
            lookup.Insert(++i, new INSTRUCTION() { name = "???", opcode = i, operation = UNK, addressingMode = IMP, cycles = 3 });
            lookup.Insert(++i, new INSTRUCTION() { name = "DEY", opcode = i, operation = DEY, addressingMode = IMP, cycles = 2 });
            lookup.Insert(++i, new INSTRUCTION() { name = "???", opcode = i, operation = NOP, addressingMode = IMP, cycles = 2 });
            lookup.Insert(++i, new INSTRUCTION() { name = "TXA", opcode = i, operation = TXA, addressingMode = IMP, cycles = 2 });
            lookup.Insert(++i, new INSTRUCTION() { name = "???", opcode = i, operation = UNK, addressingMode = IMP, cycles = 2 });
            lookup.Insert(++i, new INSTRUCTION() { name = "STY", opcode = i, operation = STY, addressingMode = ABS, cycles = 4 });
            lookup.Insert(++i, new INSTRUCTION() { name = "STA", opcode = i, operation = STA, addressingMode = ABS, cycles = 4 });
            lookup.Insert(++i, new INSTRUCTION() { name = "STX", opcode = i, operation = STX, addressingMode = ABS, cycles = 4 });
            lookup.Insert(++i, new INSTRUCTION() { name = "???", opcode = i, operation = UNK, addressingMode = IMP, cycles = 4 });
            lookup.Insert(++i, new INSTRUCTION() { name = "BCC", opcode = i, operation = BCC, addressingMode = REL, cycles = 2 });
            lookup.Insert(++i, new INSTRUCTION() { name = "STA", opcode = i, operation = STA, addressingMode = IZY, cycles = 6 });
            lookup.Insert(++i, new INSTRUCTION() { name = "???", opcode = i, operation = UNK, addressingMode = IMP, cycles = 2 });
            lookup.Insert(++i, new INSTRUCTION() { name = "???", opcode = i, operation = UNK, addressingMode = IMP, cycles = 6 });
            lookup.Insert(++i, new INSTRUCTION() { name = "STY", opcode = i, operation = STY, addressingMode = ZPX, cycles = 4 });
            lookup.Insert(++i, new INSTRUCTION() { name = "STA", opcode = i, operation = STA, addressingMode = ZPX, cycles = 4 });
            lookup.Insert(++i, new INSTRUCTION() { name = "STX", opcode = i, operation = STX, addressingMode = ZPY, cycles = 4 });
            lookup.Insert(++i, new INSTRUCTION() { name = "???", opcode = i, operation = UNK, addressingMode = IMP, cycles = 4 });
            lookup.Insert(++i, new INSTRUCTION() { name = "TYA", opcode = i, operation = TYA, addressingMode = IMP, cycles = 2 });
            lookup.Insert(++i, new INSTRUCTION() { name = "STA", opcode = i, operation = STA, addressingMode = ABY, cycles = 5 });
            lookup.Insert(++i, new INSTRUCTION() { name = "TXS", opcode = i, operation = TXS, addressingMode = IMP, cycles = 2 });
            lookup.Insert(++i, new INSTRUCTION() { name = "???", opcode = i, operation = UNK, addressingMode = IMP, cycles = 5 });
            lookup.Insert(++i, new INSTRUCTION() { name = "???", opcode = i, operation = NOP, addressingMode = IMP, cycles = 5 });
            lookup.Insert(++i, new INSTRUCTION() { name = "STA", opcode = i, operation = STA, addressingMode = ABX, cycles = 5 });
            lookup.Insert(++i, new INSTRUCTION() { name = "???", opcode = i, operation = UNK, addressingMode = IMP, cycles = 5 });
            lookup.Insert(++i, new INSTRUCTION() { name = "???", opcode = i, operation = UNK, addressingMode = IMP, cycles = 5 });
            lookup.Insert(++i, new INSTRUCTION() { name = "LDY", opcode = i, operation = LDY, addressingMode = IMM, cycles = 2 });
            lookup.Insert(++i, new INSTRUCTION() { name = "LDA", opcode = i, operation = LDA, addressingMode = IZX, cycles = 6 });
            lookup.Insert(++i, new INSTRUCTION() { name = "LDX", opcode = i, operation = LDX, addressingMode = IMM, cycles = 2 });
            lookup.Insert(++i, new INSTRUCTION() { name = "???", opcode = i, operation = UNK, addressingMode = IMP, cycles = 6 });
            lookup.Insert(++i, new INSTRUCTION() { name = "LDY", opcode = i, operation = LDY, addressingMode = ZP0, cycles = 3 });
            lookup.Insert(++i, new INSTRUCTION() { name = "LDA", opcode = i, operation = LDA, addressingMode = ZP0, cycles = 3 });
            lookup.Insert(++i, new INSTRUCTION() { name = "LDX", opcode = i, operation = LDX, addressingMode = ZP0, cycles = 3 });
            lookup.Insert(++i, new INSTRUCTION() { name = "???", opcode = i, operation = UNK, addressingMode = IMP, cycles = 3 });
            lookup.Insert(++i, new INSTRUCTION() { name = "TAY", opcode = i, operation = TAY, addressingMode = IMP, cycles = 2 });
            lookup.Insert(++i, new INSTRUCTION() { name = "LDA", opcode = i, operation = LDA, addressingMode = IMM, cycles = 2 });
            lookup.Insert(++i, new INSTRUCTION() { name = "TAX", opcode = i, operation = TAX, addressingMode = IMP, cycles = 2 });
            lookup.Insert(++i, new INSTRUCTION() { name = "???", opcode = i, operation = UNK, addressingMode = IMP, cycles = 2 });
            lookup.Insert(++i, new INSTRUCTION() { name = "LDY", opcode = i, operation = LDY, addressingMode = ABS, cycles = 4 });
            lookup.Insert(++i, new INSTRUCTION() { name = "LDA", opcode = i, operation = LDA, addressingMode = ABS, cycles = 4 });
            lookup.Insert(++i, new INSTRUCTION() { name = "LDX", opcode = i, operation = LDX, addressingMode = ABS, cycles = 4 });
            lookup.Insert(++i, new INSTRUCTION() { name = "???", opcode = i, operation = UNK, addressingMode = IMP, cycles = 4 });
            lookup.Insert(++i, new INSTRUCTION() { name = "BCS", opcode = i, operation = BCS, addressingMode = REL, cycles = 2 });
            lookup.Insert(++i, new INSTRUCTION() { name = "LDA", opcode = i, operation = LDA, addressingMode = IZY, cycles = 5 });
            lookup.Insert(++i, new INSTRUCTION() { name = "???", opcode = i, operation = UNK, addressingMode = IMP, cycles = 2 });
            lookup.Insert(++i, new INSTRUCTION() { name = "???", opcode = i, operation = UNK, addressingMode = IMP, cycles = 5 });
            lookup.Insert(++i, new INSTRUCTION() { name = "LDY", opcode = i, operation = LDY, addressingMode = ZPX, cycles = 4 });
            lookup.Insert(++i, new INSTRUCTION() { name = "LDA", opcode = i, operation = LDA, addressingMode = ZPX, cycles = 4 });
            lookup.Insert(++i, new INSTRUCTION() { name = "LDX", opcode = i, operation = LDX, addressingMode = ZPY, cycles = 4 });
            lookup.Insert(++i, new INSTRUCTION() { name = "???", opcode = i, operation = UNK, addressingMode = IMP, cycles = 4 });
            lookup.Insert(++i, new INSTRUCTION() { name = "CLV", opcode = i, operation = CLV, addressingMode = IMP, cycles = 2 });
            lookup.Insert(++i, new INSTRUCTION() { name = "LDA", opcode = i, operation = LDA, addressingMode = ABY, cycles = 4 });
            lookup.Insert(++i, new INSTRUCTION() { name = "TSX", opcode = i, operation = TSX, addressingMode = IMP, cycles = 2 });
            lookup.Insert(++i, new INSTRUCTION() { name = "???", opcode = i, operation = UNK, addressingMode = IMP, cycles = 4 });
            lookup.Insert(++i, new INSTRUCTION() { name = "LDY", opcode = i, operation = LDY, addressingMode = ABX, cycles = 4 });
            lookup.Insert(++i, new INSTRUCTION() { name = "LDA", opcode = i, operation = LDA, addressingMode = ABX, cycles = 4 });
            lookup.Insert(++i, new INSTRUCTION() { name = "LDX", opcode = i, operation = LDX, addressingMode = ABY, cycles = 4 });
            lookup.Insert(++i, new INSTRUCTION() { name = "???", opcode = i, operation = UNK, addressingMode = IMP, cycles = 4 });
            lookup.Insert(++i, new INSTRUCTION() { name = "CPY", opcode = i, operation = CPY, addressingMode = IMM, cycles = 2 });
            lookup.Insert(++i, new INSTRUCTION() { name = "CMP", opcode = i, operation = CMP, addressingMode = IZX, cycles = 6 });
            lookup.Insert(++i, new INSTRUCTION() { name = "???", opcode = i, operation = NOP, addressingMode = IMP, cycles = 2 });
            lookup.Insert(++i, new INSTRUCTION() { name = "???", opcode = i, operation = UNK, addressingMode = IMP, cycles = 8 });
            lookup.Insert(++i, new INSTRUCTION() { name = "CPY", opcode = i, operation = CPY, addressingMode = ZP0, cycles = 3 });
            lookup.Insert(++i, new INSTRUCTION() { name = "CMP", opcode = i, operation = CMP, addressingMode = ZP0, cycles = 3 });
            lookup.Insert(++i, new INSTRUCTION() { name = "DEC", opcode = i, operation = DEC, addressingMode = ZP0, cycles = 5 });
            lookup.Insert(++i, new INSTRUCTION() { name = "???", opcode = i, operation = UNK, addressingMode = IMP, cycles = 5 });
            lookup.Insert(++i, new INSTRUCTION() { name = "INY", opcode = i, operation = INY, addressingMode = IMP, cycles = 2 });
            lookup.Insert(++i, new INSTRUCTION() { name = "CMP", opcode = i, operation = CMP, addressingMode = IMM, cycles = 2 });
            lookup.Insert(++i, new INSTRUCTION() { name = "DEX", opcode = i, operation = DEX, addressingMode = IMP, cycles = 2 });
            lookup.Insert(++i, new INSTRUCTION() { name = "???", opcode = i, operation = UNK, addressingMode = IMP, cycles = 2 });
            lookup.Insert(++i, new INSTRUCTION() { name = "CPY", opcode = i, operation = CPY, addressingMode = ABS, cycles = 4 });
            lookup.Insert(++i, new INSTRUCTION() { name = "CMP", opcode = i, operation = CMP, addressingMode = ABS, cycles = 4 });
            lookup.Insert(++i, new INSTRUCTION() { name = "DEC", opcode = i, operation = DEC, addressingMode = ABS, cycles = 6 });
            lookup.Insert(++i, new INSTRUCTION() { name = "???", opcode = i, operation = UNK, addressingMode = IMP, cycles = 6 });
            lookup.Insert(++i, new INSTRUCTION() { name = "BNE", opcode = i, operation = BNE, addressingMode = REL, cycles = 2 });
            lookup.Insert(++i, new INSTRUCTION() { name = "CMP", opcode = i, operation = CMP, addressingMode = IZY, cycles = 5 });
            lookup.Insert(++i, new INSTRUCTION() { name = "???", opcode = i, operation = UNK, addressingMode = IMP, cycles = 2 });
            lookup.Insert(++i, new INSTRUCTION() { name = "???", opcode = i, operation = UNK, addressingMode = IMP, cycles = 8 });
            lookup.Insert(++i, new INSTRUCTION() { name = "???", opcode = i, operation = NOP, addressingMode = IMP, cycles = 4 });
            lookup.Insert(++i, new INSTRUCTION() { name = "CMP", opcode = i, operation = CMP, addressingMode = ZPX, cycles = 4 });
            lookup.Insert(++i, new INSTRUCTION() { name = "DEC", opcode = i, operation = DEC, addressingMode = ZPX, cycles = 6 });
            lookup.Insert(++i, new INSTRUCTION() { name = "???", opcode = i, operation = UNK, addressingMode = IMP, cycles = 6 });
            lookup.Insert(++i, new INSTRUCTION() { name = "CLD", opcode = i, operation = CLD, addressingMode = IMP, cycles = 2 });
            lookup.Insert(++i, new INSTRUCTION() { name = "CMP", opcode = i, operation = CMP, addressingMode = ABY, cycles = 4 });
            lookup.Insert(++i, new INSTRUCTION() { name = "NOP", opcode = i, operation = NOP, addressingMode = IMP, cycles = 2 });
            lookup.Insert(++i, new INSTRUCTION() { name = "???", opcode = i, operation = UNK, addressingMode = IMP, cycles = 7 });
            lookup.Insert(++i, new INSTRUCTION() { name = "???", opcode = i, operation = NOP, addressingMode = IMP, cycles = 4 });
            lookup.Insert(++i, new INSTRUCTION() { name = "CMP", opcode = i, operation = CMP, addressingMode = ABX, cycles = 4 });
            lookup.Insert(++i, new INSTRUCTION() { name = "DEC", opcode = i, operation = DEC, addressingMode = ABX, cycles = 7 });
            lookup.Insert(++i, new INSTRUCTION() { name = "???", opcode = i, operation = UNK, addressingMode = IMP, cycles = 7 });
            lookup.Insert(++i, new INSTRUCTION() { name = "CPX", opcode = i, operation = CPX, addressingMode = IMM, cycles = 2 });
            lookup.Insert(++i, new INSTRUCTION() { name = "SBC", opcode = i, operation = SBC, addressingMode = IZX, cycles = 6 });
            lookup.Insert(++i, new INSTRUCTION() { name = "???", opcode = i, operation = NOP, addressingMode = IMP, cycles = 2 });
            lookup.Insert(++i, new INSTRUCTION() { name = "???", opcode = i, operation = UNK, addressingMode = IMP, cycles = 8 });
            lookup.Insert(++i, new INSTRUCTION() { name = "CPX", opcode = i, operation = CPX, addressingMode = ZP0, cycles = 3 });
            lookup.Insert(++i, new INSTRUCTION() { name = "SBC", opcode = i, operation = SBC, addressingMode = ZP0, cycles = 3 });
            lookup.Insert(++i, new INSTRUCTION() { name = "INC", opcode = i, operation = INC, addressingMode = ZP0, cycles = 5 });
            lookup.Insert(++i, new INSTRUCTION() { name = "???", opcode = i, operation = UNK, addressingMode = IMP, cycles = 5 });
            lookup.Insert(++i, new INSTRUCTION() { name = "INX", opcode = i, operation = INX, addressingMode = IMP, cycles = 2 });
            lookup.Insert(++i, new INSTRUCTION() { name = "SBC", opcode = i, operation = SBC, addressingMode = IMM, cycles = 2 });
            lookup.Insert(++i, new INSTRUCTION() { name = "NOP", opcode = i, operation = NOP, addressingMode = IMP, cycles = 2 });
            lookup.Insert(++i, new INSTRUCTION() { name = "???", opcode = i, operation = SBC, addressingMode = IMP, cycles = 2 });
            lookup.Insert(++i, new INSTRUCTION() { name = "CPX", opcode = i, operation = CPX, addressingMode = ABS, cycles = 4 });
            lookup.Insert(++i, new INSTRUCTION() { name = "SBC", opcode = i, operation = SBC, addressingMode = ABS, cycles = 4 });
            lookup.Insert(++i, new INSTRUCTION() { name = "INC", opcode = i, operation = INC, addressingMode = ABS, cycles = 6 });
            lookup.Insert(++i, new INSTRUCTION() { name = "???", opcode = i, operation = UNK, addressingMode = IMP, cycles = 6 });
            lookup.Insert(++i, new INSTRUCTION() { name = "BEQ", opcode = i, operation = BEQ, addressingMode = REL, cycles = 2 });
            lookup.Insert(++i, new INSTRUCTION() { name = "SBC", opcode = i, operation = SBC, addressingMode = IZY, cycles = 5 });
            lookup.Insert(++i, new INSTRUCTION() { name = "???", opcode = i, operation = UNK, addressingMode = IMP, cycles = 2 });
            lookup.Insert(++i, new INSTRUCTION() { name = "???", opcode = i, operation = UNK, addressingMode = IMP, cycles = 8 });
            lookup.Insert(++i, new INSTRUCTION() { name = "???", opcode = i, operation = NOP, addressingMode = IMP, cycles = 4 });
            lookup.Insert(++i, new INSTRUCTION() { name = "SBC", opcode = i, operation = SBC, addressingMode = ZPX, cycles = 4 });
            lookup.Insert(++i, new INSTRUCTION() { name = "INC", opcode = i, operation = INC, addressingMode = ZPX, cycles = 6 });
            lookup.Insert(++i, new INSTRUCTION() { name = "???", opcode = i, operation = UNK, addressingMode = IMP, cycles = 6 });
            lookup.Insert(++i, new INSTRUCTION() { name = "SED", opcode = i, operation = SED, addressingMode = IMP, cycles = 2 });
            lookup.Insert(++i, new INSTRUCTION() { name = "SBC", opcode = i, operation = SBC, addressingMode = ABY, cycles = 4 });
            lookup.Insert(++i, new INSTRUCTION() { name = "NOP", opcode = i, operation = NOP, addressingMode = IMP, cycles = 2 });
            lookup.Insert(++i, new INSTRUCTION() { name = "???", opcode = i, operation = UNK, addressingMode = IMP, cycles = 7 });
            lookup.Insert(++i, new INSTRUCTION() { name = "???", opcode = i, operation = NOP, addressingMode = IMP, cycles = 4 });
            lookup.Insert(++i, new INSTRUCTION() { name = "SBC", opcode = i, operation = SBC, addressingMode = ABX, cycles = 4 });
            lookup.Insert(++i, new INSTRUCTION() { name = "INC", opcode = i, operation = INC, addressingMode = ABX, cycles = 7 });
            lookup.Insert(++i, new INSTRUCTION() { name = "???", opcode = i, operation = UNK, addressingMode = IMP, cycles = 7 });
        }
    }

    struct INSTRUCTION
    {
        public string name;
        public Byte opcode;
        public delegate Byte operationDelegate();
        public operationDelegate operation;
        public delegate Byte addressingModeDelegate();
        public addressingModeDelegate addressingMode;
        public Byte cycles;
    }

    public enum FLAGS6502 : Byte
    {
        /// <summary>
        /// Carry Bit
        /// </summary>
        C = (1 << 0),   // Carry Bit
        Z = (1 << 1),   // Zero
        I = (1 << 2),   // Disable Interrupts
        D = (1 << 3),   // Decimal Mode (unused in this implementation)
        B = (1 << 4),   // Break
        U = (1 << 5),   // Unused
        V = (1 << 6),   // Overflow
        N = (1 << 7),   // Negative
    };
}
