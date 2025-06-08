using System;

namespace NEStor.Core.Cpu
{
    internal class Interrupt
    {
        CPU Cpu;
        public bool Enabled;
        public int Delay = 1;
        public int Cycle;

        //public int Eta { get { return Cpu.Cycle - Cycle - Delay; } }
        public int CyclesPassed { get { return Cpu.Cycle - Cycle; } }
        public bool IsActive { get { return Enabled && CyclesPassed >= Delay; } }
        public bool IsReady { get { return Enabled && CyclesPassed == Delay; } }
        public void Start(int delay = 1) 
        {
            if (!Enabled)
            {
                Enabled = true;
                Cycle = Cpu.Cycle;
                Delay = delay;
            }
        }
        public void Acknowledge()
        {
            Enabled = false;
            //Delay = 1;
        }
        public Interrupt(CPU cpu)
        {
            Cpu = cpu;
            Cpu.Interrupts.Add(this);
        }
    }
}
