using System;
using System.Collections.Generic;
using System.IO;
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
        public bool DummyAccess;
        public int Cycles;

        public CPU(Bus bus)
        {
            Bus = bus;
            CreateInstructionSet();
            ActOp = InstructionSet[0];
            ActOp.Cycle = 0;
            Cycles = 0;
            Reset();
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
        public StreamWriter Writer;
        public void Tick()
        {
            if (ActOp.Cycle == 0)
            {
                //if ((Status.Break || !Status.IRQoff) && IRQDelay == 0 && IRQList.Count > 0)
                if (!Status.IRQoff && IRQDelay == 0 && IRQList.Count > 0)
                //if (IRQDelay == 0 && IRQList.Count > 0)
                {
                    var irqType = IRQList[IRQList.Count - 1];
                    //if (irqType == NMIaddress || irqType == RSTaddress || !Status.IRQoff)
                    {
                        ActOp = new Instruction(0, BRK, irqType, 7);
                        IRQList.RemoveAt(IRQList.Count - 1);
                    }
                    //else
                    //{
                    //    ActOp = InstructionSet[Read(PC)];
                    //    PC++;
                    //}
                }
                else
                {
                    ActOp = InstructionSet[Read(PC)];
                    PC++;
                }
                ActOp.Run();
                //if (Status.IRQoff)
                //    IRQDelay = 1;
                //else 

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
            Status.IRQoff = false;
            IRQList.Add(RSTaddress);
        }
        public void NMI()
        {
            Status.IRQoff = false;
            IRQList.Add(NMIaddress);
        }
        public void IRQ()
        {
            IRQList.Insert(0, IRQaddress);
        }

    }

}
