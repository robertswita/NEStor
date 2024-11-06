using System;

namespace NES
{
    class Counter
    {
        int Count;
        public Action OnStop;
        public virtual bool Enabled { get; set; }
        public bool Halted;
        public int Period;
        public void Reload()
        {
            Count = Period;
        }
        public bool IsActive
        {
            get { return Count > 0; }
        }
        public Counter()
        {
            OnStop = Run;
        }
        public virtual void Run() { }
        public virtual void Step()
        {
            if (Enabled && !Halted)
            {
                if (Count > 0)
                    Count--;
                else
                    OnStop();
            }
        }
    }
}
