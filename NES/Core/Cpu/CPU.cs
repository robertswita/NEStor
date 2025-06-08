using System;
using System.Collections.Generic;
using System.Linq;
using NEStor.Core;

namespace NEStor.Core.Cpu
{
    partial class CPU
    {
        Bus Bus;
        public int Frequency;
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
        bool PrevIrqDisable;
        bool ActIrqDisable;
        int Drift;

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
            Interrupts.ForEach(irq => { if (irq.IsReady) irq.Delay++; });
            ReadPageCross(address, PC);
            PC = address;
        }
        public void ReadPageCross(int address, int pc)
        {
            var pcAddress = pc & 0xFF00 | address & 0xFF;
            if (ActOp.WriteCycles != 0 || (pcAddress != address))
                Read(pcAddress);
        }
        public void Step()
        {
            var irqActive = Rst || Nmi.IsActive || !PrevIrqDisable && Interrupts.Any(irq => irq.IsActive);
            var opCode = Read(PC); PC++;
            if (irqActive) opCode = 0;
            Status.Break = !irqActive;
            ActOp = InstructionSet[opCode];
            ActOp.Run();
        }
        public void BeginCycle()
        {
            Cycle++;
            Bus.Apu.Step();
        }
        public void EndCycle()
        {
            //Cycle++;
            Bus.Ppu.Step();
            //Bus.Apu.Step();
            if (Status.ToggleIrqDisable)
            {
                Status.ToggleIrqDisable = false;
                Status.IrqDisable = !Status.IrqDisable;
            }
            PrevIrqDisable = ActIrqDisable;
            ActIrqDisable = Status.IrqDisable;
        }
        public void ResetCycles()
        {
            Drift += Cycle & 1;
            Interrupts.ForEach(x => x.Cycle -= Cycle);
            Cycle = 0;
        }
        public bool IsGetCycle()
        {
            return ((Cycle + Drift) & 1) == 0;
        }

        public bool DmaDmcEnabled;
        public bool DmaOamEnabled;
        public void DmaTransfer(int addr)
        {
            var prevDmaDmcEnabled = DmaDmcEnabled;
            var isGetCycle = IsGetCycle();
            if (!isGetCycle) Read(addr);
            if (prevDmaDmcEnabled && !DmaDmcEnabled)
                Read(addr);
            if (DmaOamEnabled)
            {
                for (int i = 0; i < 256; i++)
                {
                    if (prevDmaDmcEnabled)
                    {
                        Bus.Apu.Dmc.Dma();
                        Read(addr);
                    }
                    prevDmaDmcEnabled = DmaDmcEnabled;
                    Write(0x2004, Read(Bus.DmaOamAddr++));
                }
                isGetCycle = !prevDmaDmcEnabled;
                DmaOamEnabled = false;
            }
            if (DmaDmcEnabled)
            {
                if (isGetCycle)
                {
                    Read(addr);
                    Read(addr);
                    if (!DmaDmcEnabled)
                        ;
                }
                Bus.Apu.Dmc.Dma();
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
                BeginCycle();
                Bus.Peek(addr);
                EndCycle();
                //if (DmaOamEnabled || DmaDmcEnabled)
                {
                    Enabled = false;
                    DmaTransfer(addr);
                    Enabled = true;
                }
            }
            BeginCycle();
            var result = Bus.Peek(addr);
            EndCycle();
            return result;
        }
        void Write(int addr, int data)
        {
            if (addr == int.MaxValue)
                A = (byte)data;
            else
            {
                BeginCycle();
                Bus.Poke(addr, (byte)data);
                EndCycle();
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
            //Write(0x4017, 0);
            Cycle = 0;
            Step();
            Write(0x4015, 0);
        }

    }

}
