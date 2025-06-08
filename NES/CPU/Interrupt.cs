using System;

namespace NES
{
    internal class Interrupt
    {
        CPU Cpu;
        public bool Enabled;
        public int Delay = 1;
        public int Cycle;

        public int Eta { get { return Cpu.Cycle - Cycle - Delay; } }
        public bool IsActive { get { return Enabled && Eta >= 0; } }
        public bool IsReady { get { return Enabled && Eta == 0; } }
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
