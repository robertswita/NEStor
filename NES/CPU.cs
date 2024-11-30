using System;
using System.Collections.Generic;
using CPU;

namespace NES
{
    partial class CPU
    {
        public bool Enabled;
        byte a;
        public byte A // Accumulator 8-bit
        {
            get { return a; }
            set
            {
                a = value;
                SetFlagsNZ(a);
            }
        }
        byte x;
        public byte X // X register 8-bit
        {
            get { return x; }
            set
            {
                x = value;
                SetFlagsNZ(x);
            }
        }
        byte y;
        public byte Y // Y register 8-bit
        {
            get { return y; }
            set
            {
                y = value;
                SetFlagsNZ(y);
            }
        }
        public byte SP; // Stack Pointer 8-bit
        public int PC; // Program Counter 16-bit
        public StatusRegister Status = new StatusRegister(); // Processor Status 8-bit
        private readonly Bus Bus;
        public static Instruction[] InstructionSet;
        public Instruction ActOp;
        public bool DummyAccess;
        public int Cycle;
        bool Rst;
        public bool Nmi;
        public bool Irq;
        public bool DmcIrq;
        public int Frequency;
        public int IRQDelay;
        public int NMIDelay;

        public CPU(Bus bus)
        {
            Bus = bus;
            CreateInstructionSet();
        }

        public void SetFlagsNZ(int result)
        {
            Status.Negative = (result & 0x80) != 0;
            Status.Zero = (result & 0xFF) == 0;
        }

        void Branch(int address)
        {
            ActOp.Cycle++;
            DummyAccess = true;
            Read(PC);
            if ((address & 0xFF00) != (PC & 0xFF00))
            {
                DummyAccess = true;
                Read(address - 0x100);
                ActOp.Cycle++;
            }
            PC = address & 0xFFFF;
        }

        public void PageCross(int address, int pc)
        {
            if ((address & 0xFF00) != (pc & 0xFF00))
            {
                DummyAccess = true;
                Read(address - 0x100);
                if (ActOp.HasExtraCycle)
                    ActOp.Cycle++;
            }
            else if (!ActOp.HasExtraCycle)
            {
                DummyAccess = true;
                Read(address);
            }
        }

        public int Step(int step)
        {
            if (ActOp.Cycle == 0)
            {
                if (Rst || Nmi && NMIDelay == 0 || (Irq || DmcIrq) && !Status.IrqOff && IRQDelay == 0)
                {
                    Instruction.addressDelegate irqType = Rst ? RSTaddress : Nmi ? NMIaddress : IRQaddress;
                    Rst = false;
                    Nmi = false;
                    Irq = false;
                    DmcIrq = false;
                    ActOp = new Instruction(0, BRK, irqType, 7);
                }
                else
                {
                    ActOp = InstructionSet[Read(PC)];
                    PC++;
                }
                if (NMIDelay > 0) NMIDelay--;
                if (IRQDelay > 0) IRQDelay--;
                ActOp.Run();
            }
            step = Math.Min(step, ActOp.Cycle);
            Cycle += step;
            ActOp.Cycle -= step;
            return step;
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
            var result = addr < 0 ? A : Bus.Peek(addr);
            DummyAccess = false;
            return result;
        }
        private void Write(int addr, int data)
        {
            if (addr < 0)
                A = (byte)data;
            else
                Bus.Poke(addr, (byte)data);
            DummyAccess = false;
        }

        private int ReadWord(int addr)
        {
            var ptr_lo = Bus.Peek(addr); addr++;
            var ptr_hi = Bus.Peek(addr);
            return (ptr_hi << 8) | ptr_lo;
        }
        public void Reset()
        {
            Rst = true;
            ActOp = InstructionSet[0];
            ActOp.Cycle = 0;
            Step(0);
        }
        //public void NMI()
        //{
        //    //Status.IRQoff = false;
        //    Nmi = true;
        //    //irqType = NMIaddress;
        //    //IRQList.Add(NMIaddress);
        //}
        //public void IRQ()
        //{
        //    //IRQDelay = 1;
        //    Irq = true;
        //    //IRQList.Insert(0, IRQaddress);
        //}

        //public void DMCIRQ()
        //{
        //    //Status.IRQoff = false;
        //    //IRQDelay = 2;
        //    DmcIrq = true;
        //    //IRQList.Insert(0, IRQaddress);
        //}

    }

}
