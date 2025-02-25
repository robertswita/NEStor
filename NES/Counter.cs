using System;

namespace NES
{
    class Counter
    {
        public int Count;
        //public Action OnStop;
        //public virtual bool Enabled { get; set; }
        //public bool Halted;
        public int Period;
        bool Rst;
        public virtual void Reload()
        {
            Count = Period;
        }
        //public void Reset()
        //{
        //    Rst = true;
        //    Count = 0;
        //}
        public virtual bool IsActive
        {
            get { return Count > 0; }
            set { if (!value) Count = 0; }
        }
        //public Counter()
        //{
        //    OnStop = Fire;
        //}
        public virtual void Fire() { }
        //public virtual void OnReset() { }

        public virtual void Step()
        {
            //if (!Halted)
            {
                if (Count > 0)
                    Count--;
                else
                    Fire();
            }
        }
    }
}
