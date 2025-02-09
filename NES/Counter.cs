using System;

namespace NES
{
    class Counter
    {
        public int Count;
        public Action OnStop;
        public bool IsLength;
        public virtual bool Enabled { get; set; }
        //public bool Halted;
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
            OnStop = Fire;
        }
        public virtual void Fire() { }
        public virtual void Step()
        {
            if (Enabled)
            {
                if (IsLength)
                {
                    if (Count > 0)
                    {
                        Count--;
                        if (Count == 0)
                            OnStop();
                    }
                }
                else
                {
                    if (Count > 0)
                        Count--;
                    else
                        OnStop();
                }
            }
        }
    }
}
