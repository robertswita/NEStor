using System;

namespace NES
{
    public interface IInterrupt
    {
        bool IsActive { get; }
        bool IsReady { get; }
        bool Enabled { get; set; }
        int Cycle { get; set; }
        void Start(int delay = 1);
        void Acknowledge();
        void Delay();
    }

    internal class Interrupt: IInterrupt
    {
        CPU Cpu;
        internal int delay = 1;
        public int Cycle { get; set; }
        public bool Enabled { get; set; }
        int Eta { get { return Cpu.Cycle - Cycle - delay; } }
        public bool IsActive { get { return Enabled && Eta >= 0; } }
        public bool IsReady { get { return Enabled && Eta == 0; } }

        public void Start(int delay = 1)
        {
            if (!Enabled)
            {
                Enabled = true;
                Cycle = Cpu.Cycle;
                this.delay = delay;
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
        public void Delay()
        {
            delay++;
        }
    }
}
