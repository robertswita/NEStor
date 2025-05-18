using System;

namespace NES
{
    class Counter
    {
        protected int Count;
        public int Period;
        public virtual void Reload()
        {
            Count = Period;
        }
        public virtual bool IsActive
        {
            get { return Count > 0; }
            set { if (!value) Count = 0; }
        }
        public virtual void Execute() { }
        public virtual void Step()
        {
            if (Count > 0)
                Count--;
            else
                Execute();
        }
    }
}
