using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NES;

namespace NEStor.Audio
{
    class EnvelopeCounter: Counter
    {
        public bool Loop;
        public int Volume;
        public bool IsResetting;
        public Action OnReset;

        public EnvelopeCounter()
        {
            Period = 0xF;
            OnReset = ResetVolume;
        }
        public override void Fire()
        {
            if (Volume > 0)
                Volume--;
            else if (Loop)
                Volume = 0xF;
            Reload();
        }

        public override void Step()
        {
            if (IsResetting)
                OnReset();
            else
                base.Step();
        }

        public void Reset() { IsResetting = true; }

        public void ResetVolume()
        {
            Volume = 0xF;
            Reload();
            IsResetting = false;
        }

    }
}
