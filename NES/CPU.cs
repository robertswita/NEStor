using System;
using System.Collections.Generic;
//using System.IO;
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
        public static Instruction[] InstructionSet;
        public Instruction ActOp;
        public bool DummyAccess;
        public int Cycle;
        public bool rst;
        public bool nmi;
        public bool irq;
        public bool dmcirq;
        Instruction.addressDelegate irqType;

        public CPU(Bus bus)
        {
            Bus = bus;
            CreateInstructionSet();
            //ActOp = InstructionSet[0];
            //ActOp.Cycle = 0;
            //Reset();
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
            Read(PC); // dummy read
            if ((address & 0xFF00) != (PC & 0xFF00))
            {
                DummyAccess = true;
                Read(address - 0x100); // dummy read
                ActOp.Cycle++;
            }
            PC = address & 0xFFFF;
        }

        public void PageCross(int address, int pc)
        {
            if ((address & 0xFF00) != (pc & 0xFF00))
            {
                DummyAccess = true;
                Read(address - 0x100); // dummy read
                if (ActOp.ExtraCycle)
                    ActOp.Cycle++;
            }
            else if (!ActOp.ExtraCycle)
            {
                DummyAccess = true;
                Read(address); // dummy read
            }
        }

        //void handle_interrupts()
        //{
        //    // https://www.nesdev.org/wiki/CPU_interrupts
        //    //
        //    // The internal signal goes high during φ1 of the cycle that follows the one where
        //    // the edge is detected, and stays high until the NMI has been handled. NMI is handled only
        //    // when `prev_nmi` is true.
        //    prev_nmi = nmi;

        //    // This edge detector polls the status of the NMI line during φ2 of each CPU cycle (i.e.,
        //    // during the second half of each cycle, hence here in `end_cycle`) and raises an internal
        //    // signal if the input goes from being high during one cycle to being low during the
        //    // next.
        //    nmi_pending = Bus.nmi_pending();
        //    if (!prev_nmi_pending && nmi_pending {
        //        nmi = true;
        //        //log::trace!("NMI Edge Detected: {}", self.cycle);
        //    }
        //    prev_nmi_pending = nmi_pending;

        //    IRQList = Bus.irqs_pending();

        //    // The IRQ status at the end of the second-to-last cycle is what matters,
        //    // so keep the second-to-last status.
        //    prev_run_irq = run_irq;
        //    run_irq = IRQList.Count > 0 && !Status.IRQoff;
        //    //if (run_irq) {
        //    //    log::trace!("IRQ Level Detected: {}: {:?}", self.cycle, self.irq);
        //    //}

        //    //if self.bus.dmc_dma() {
        //    //    self.dmc_dma = true;
        //    //    self.halt = true;
        //    //    self.dummy_read = true;
        //    //}
        //}
        //public StreamWriter Writer;
        //bool IRQHandling;
        //bool IRQSecond;
        public int IRQDelay;
        //public int IRQOffDelay;
        public int NMIDelay;
        int SkipOp;
        //bool IRQEnabled;

        public int Tick(int step)
        {
            if (ActOp.Cycle == 0)
            {
                //if (rst || (nmi ||  irq && !Status.IRQoff) && SkipOp == 0)
                if (rst || (nmi && NMIDelay == 0 || (irq || dmcirq) && !Status.IRQoff && IRQDelay == 0) && SkipOp == 0)
                //if (!Status.IRQoff && IRQDelay == 0 && IRQList.Count > 0)
                {
                    //var irqType = IRQList[IRQList.Count - 1];
                    //IRQSecond = false;
                    //if (IRQHandling)
                    //    IRQSecond = true;
                    //if (irqType != RSTaddress)
                    //    IRQHandling = true;
                    //if (irqType == NMIaddress || irqType == RSTaddress || !Status.IRQoff)
                    {
                        //var lastOp = ActOp;
                        irqType = rst ? RSTaddress : nmi ? NMIaddress : IRQaddress;
                        //if (irqType == RSTaddress)
                            rst = false;
                        //else if (irqType == NMIaddress)
                            nmi = false;
                        //else
                        ////if (irqType == IRQaddress)
                            irq = false;
                        dmcirq = false;
                        ActOp = new Instruction(0, BRK, irqType, 7);
                        //IRQList.RemoveAt(IRQList.Count - 1);
                    }
                    //IRQEnabled = false;
                }
                else
                {
                    ActOp = InstructionSet[Read(PC)];
                    PC++;
                }
                if (NMIDelay > 0) NMIDelay--;
                if (IRQDelay > 0) IRQDelay--;
                //if (IRQOffDelay > 0)
                //{
                //    IRQOffDelay--;
                //    if (IRQOffDelay == 0)
                //        irq = false;
                //}
                if (SkipOp > 0) SkipOp--;
                ActOp.Run();


                //if (Writer != null)
                //{
                //    Writer.Write(Cycles);
                //    Writer.Write(" ");
                //    Writer.Write(ActOp.Operation.Method.Name);
                //    Writer.Write(" ");
                //    Writer.WriteLine(ActOp.Address);
                //}

                //Cycles += ActOp.Cycle;
                //if (Writer != null && Cycles > 0x2b625)
                //{
                //    Writer.Flush();
                //    Writer.Close();
                //    Writer = null;
                //}
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
            var result = addr < 0 ? A : Bus.CpuRead(addr);
            DummyAccess = false;
            return result;
        }
        private void Write(int addr, int data)
        {
            if (addr < 0)
                A = (byte)data;
            else
                Bus.CpuWrite(addr, (byte)data);
            DummyAccess = false;
        }

        private int ReadWord(int addr)
        {
            var ptr_lo = Bus.CpuRead(addr); addr++;
            var ptr_hi = Bus.CpuRead(addr);
            return (ptr_hi << 8) | ptr_lo;
        }
        public void Reset()
        {
            //Status.IRQoff = false;
            //IRQList.Clear();
            //IRQList.Add(RSTaddress);
            rst = true;
            //irqType = RSTaddress;
            //IRQList.Add(RSTaddress);
            ActOp = InstructionSet[0];
            ActOp.Cycle = 0;
            Tick(0);
        }
        public void NMI()
        {
            //Status.IRQoff = false;
            nmi = true;
            //irqType = NMIaddress;
            //IRQList.Add(NMIaddress);
        }
        public void IRQ()
        {
            //IRQDelay = 1;
            irq = true;
            //IRQList.Insert(0, IRQaddress);
        }

        public void DMCIRQ()
        {
            //Status.IRQoff = false;
            //IRQDelay = 2;
            dmcirq = true;
            //IRQList.Insert(0, IRQaddress);
        }

        //public void Interrupt(Instruction.addressDelegate irqType, int delay)
        //{
        //    if (!IRQList.Contains(irqType))
        //        if (irqType == RST || irqType == NMI)
        //        {
        //            IRQList.Add(irqType);
        //            NMIDelay = delay;
        //        }
        //        else
        //        {
        //            IRQList.Insert(0, irqType);
        //            IRQDelay = delay;
        //        }
        //}

    }

}
