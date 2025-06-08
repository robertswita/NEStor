using System;

namespace NES
{
    public interface ICycleProviding
    {
        int Cycle { get; set; }
    }
    public interface IInterrupt: ICycleProviding
    {
        bool IsActive { get; }
        bool IsReady { get; }
        bool Enabled { get; set; }
        void Start(int delay = 1);
        void Acknowledge();
        void Delay();
    }

    internal class Interrupt: IInterrupt
    {
        ICycleProviding CycleProvider;
        internal int delay = 1;
        public int Cycle { get; set; }
        public bool Enabled { get; set; }
        int Eta { get { return CycleProvider.Cycle - Cycle - delay; } }
        public bool IsActive { get { return Enabled && Eta >= 0; } }
        public bool IsReady { get { return Enabled && Eta == 0; } }

        public void Start(int delay = 1)
        {
            if (!Enabled)
            {
                Enabled = true;
                Cycle = CycleProvider.Cycle;
                this.delay = delay;
            }
        }
        public void Acknowledge()
        {
            Enabled = false;
            //Delay = 1;
        }
        public Interrupt(ICycleProviding cycleProvider)
        {
            CycleProvider = cycleProvider;
        }
        public void Delay()
        {
            delay++;
        }
    }
}
