using System;
using System.Collections.Generic;
using System.Linq;
using CPU;

namespace NES
{
    partial class CPU
    {
        Bus Bus;
        public bool Enabled = true;
        byte a;
        public byte A // Accumulator 8-bit
        {
            get { return a; }
            set { a = value; SetZeroNegativeFlags(a); }
        }
        byte x;
        public byte X // X register 8-bit
        {
            get { return x; }
            set { x = value; SetZeroNegativeFlags(x); }
        }
        byte y;
        public byte Y // Y register 8-bit
        {
            get { return y; }
            set { y = value; SetZeroNegativeFlags(y); }
        }
        byte result;
        public byte Result
        {
            get { return result; }
            set { result = value; SetZeroNegativeFlags(result); }
        }
        public byte SP; // Stack Pointer 8-bit
        public int PC; // Program Counter 16-bit
        public StatusRegister Status = new StatusRegister(); // Processor Status 8-bit
        public static Instruction[] InstructionSet;
        public Instruction ActOp;
        public int Cycle;
        public Interrupt Nmi;
        public Interrupt ApuIrq;
        public Interrupt DmcIrq;
        public Interrupt MapperIrq;
        public List<Interrupt> Interrupts = new List<Interrupt>();
        public bool Rst;
        bool IrqDisable;
        bool LastIrqDisable;
        public int Frequency;

        public CPU(Bus bus)
        {
            Bus = bus;
            CreateInstructionSet();
            Nmi = new Interrupt(this);
            ApuIrq = new Interrupt(this);
            DmcIrq = new Interrupt(this);
            MapperIrq = new Interrupt(this);
        }

        public void SetZeroNegativeFlags(byte result)
        {
            Status.Negative = (result & 0x80) != 0;
            Status.Zero = result == 0;
        }

        void Branch(int address)
        {
            // If an interrupt occurs during the final cycle of a non-pagecrossing branch
            // then it will be ignored until the next instruction completes
            Read(PC);
            if (Interrupts.Any(x => x.IsActivating))
                IrqDisable = true;
            ReadPageCross(address, PC);
            PC = address;
        }

        public void ReadPageCross(int address, int pc)
        {
            if (ActOp.WriteCycles != 0 || (address & 0xFF00) != (pc & 0xFF00))
                Read(pc & 0xFF00 | address & 0xFF);
        }

        public void Step()
        {
            var irqActive = Rst || Nmi.IsActive || !IrqDisable && Interrupts.Any(x => x.IsActive);
            var opCode = Read(PC); PC++;
            if (irqActive) opCode = 0;
            Status.Break = !irqActive;
            ActOp = InstructionSet[opCode];
            ActOp.Run();
        }

        public void CycleStep()
        {
            Cycle++;
            Bus.Ppu.Step();
            Bus.Apu.Step();
            if (Status.ToggleIrqDisable)
            {
                Status.ToggleIrqDisable = false;
                Status.IrqDisable = !Status.IrqDisable;
            }
            IrqDisable = LastIrqDisable;
            LastIrqDisable = Status.IrqDisable;
        }

        public void ResetCycles()
        {
            Cycle--;
            Drift += Cycle & 1;
            Interrupts.ForEach(x => x.Cycle -= Cycle);
            Cycle = 1;
        }

        public bool IsGetCycle()
        {
            return ((Cycle + Drift) & 1) == 0;
        }

        int Drift;
        public bool DmaDmcEnabled;
        public bool DmaOamEnabled;
        public int DmaOamInit;
        public void DmaTransfer(int addr)
        {
            var oamValue = 0;
            var oamCount = 0;
            var dmcLatched = false;
            var halt = false;
            var dummyRead = false;
            if (DmaDmcEnabled)
            {
                dmcLatched = true;
                dummyRead = true;
            }
            var isGetCycle = IsGetCycle();// ((Cycle + Drift) & 1) == 0;
            while (DmaDmcEnabled || DmaOamEnabled)
            {
                if (isGetCycle) 
                {
                    if (DmaDmcEnabled && !halt && !dummyRead) 
                    {
                        //var buffer = Read(Bus.Apu.Dmc.Address);
                        Bus.Apu.Dmc.LoadBuffer();
                        dmcLatched = false;
                        DmaDmcEnabled = false;
                    }
                    else if (DmaOamEnabled) 
                    {
                        oamValue = Read(Bus.DmaOamAddr++);
                        oamCount++;
                    }
                    else
                        Read(addr);
                }
                else if (DmaOamEnabled && (oamCount & 1) != 0) 
                {
                    Write(0x2004, oamValue);
                    oamCount++;
                    if (oamCount == 0x200)
                        DmaOamEnabled = false;
                }
                else
                    Read(addr);
                if (DmaDmcEnabled)
                {
                    if (dmcLatched)
                    {
                        if (halt) halt = false;
                        else dummyRead = false;
                    }
                    else
                    {
                        dmcLatched = true;
                        halt = true;
                        dummyRead = true;
                    }
                }
                isGetCycle = !isGetCycle;
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

        public byte Read(int addr)
        {
            if (addr == int.MaxValue) return A;
            if (Enabled && (DmaOamEnabled || DmaDmcEnabled))
            {
                Bus.Peek(addr);
                CycleStep();
                Enabled = false;
                DmaTransfer(addr);
                Enabled = true;
            }
            var result = Bus.Peek(addr);
            CycleStep();
            return result;
        }

        void Write(int addr, int data)
        {
            if (addr == int.MaxValue)
                A = (byte)data;
            else
            {
                Bus.Poke(addr, (byte)data);
                CycleStep();
            }
        }

        int ReadWord(int addr)
        {
            var lo = Read(addr); addr++;
            var hi = Read(addr);
            return hi << 8 | lo;
        }
        int ReadWord(byte addr)
        {
            var lo = Read(addr); addr++;
            var hi = Read(addr);
            return hi << 8 | lo;
        }
        public void Reset()
        {
            Rst = true;
            Write(0x4017, 0);
            Cycle = 0;
            Write(0x4015, 0);
            Step();
        }

    }

}
