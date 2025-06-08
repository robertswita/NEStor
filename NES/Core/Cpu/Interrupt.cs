using System;

namespace NEStor.Core.Cpu
{
    public interface ICycleProviding
    {
        int Cycle { get; set; }
    }
    public interface IInterrupt : ICycleProviding
    {
        bool IsActive { get; }
        bool IsReady { get; }
        bool Enabled { get; set; }
        int Delay { get; set; }
        int CyclesPassed { get; }
        void Start(int delay = 1);
        void Acknowledge();
    }
    internal class Interrupt: IInterrupt
    {
        ICycleProviding cycleProvider;
        public bool Enabled { get; set; } = false; 
        public int Delay { get; set; } = 1;
        public int Cycle { get; set; } = 0;

        //public int Eta { get { return Cpu.Cycle - Cycle - Delay; } }
        public int CyclesPassed { get { return cycleProvider.Cycle - Cycle; } }
        public bool IsActive { get { return Enabled && CyclesPassed >= Delay; } }
        public bool IsReady { get { return Enabled && CyclesPassed == Delay; } }
        public void Start(int delay = 1) 
        {
            if (!Enabled)
            {
                Enabled = true;
                Cycle = cycleProvider.Cycle;
                Delay = delay;
            }
        }
        public void Acknowledge()
        {
            Enabled = false;
            //Delay = 1;
        }
        public Interrupt(ICycleProviding cycleProvider)
        {
            this.cycleProvider = cycleProvider;
        }
    }
}
