using NES;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NesCore.Audio
{
    abstract class Wave: Counter
    {
        public APU Apu;
        public static byte[] LengthPeriods = {
            10, 254, 20, 2, 40, 4, 80, 6, 160, 8, 60, 10, 14, 12, 26, 14,
            12, 16, 24, 18, 48, 20, 96, 22, 192, 24, 72, 26, 16, 28, 32, 30
        };
        bool IsLatched;
        byte LatchData;
        public Counter LengthCounter = new Counter(); 
        public Counter Envelope = new Counter();
        protected bool EnvelopeLoop;
        protected int EnvelopeVolume;
        public int SampleVolume;

        public Wave()
        {
            Envelope.Period = 0xF;
            Envelope.OnStop = EnvelopeRun;
        }
        public override bool Enabled
        {
            set
            {
                base.Enabled = value;
                LengthCounter.Enabled = value;
                if (!LengthCounter.Enabled)
                {
                    LengthCounter.Period = 0;
                    LengthCounter.Reload();
                }
            }
        }
        protected abstract void SetControl(byte value);
        public byte Control 
        { 
            set
            {
                LatchData = value;
                IsLatched = true;
                Init = 1;
            }
        }
        public virtual bool IsMuted
        {
            get { return !Enabled || !LengthCounter.IsActive; }
        }
        protected int volume;
        public int Volume
        {
            get
            {
                if (IsMuted)
                    return 0;
                return Envelope.Enabled ? EnvelopeVolume : SampleVolume;
            }
        }
        protected int Length
        {
            set
            {
                if (Enabled)
                {
                    var reload = true;
                    Halted = false;
                    if (Apu.IsLengthCycle) // APU $40xx/framecounter conflict
                    {
                        reload = !LengthCounter.IsActive;
                        if (reload)
                        {
                            Halted = true;
                            LengthCounter.Enabled = false;
                        }
                    }
                    if (reload)
                    {
                        LengthCounter.Period = value;
                        LengthCounter.Reload();
                    }
                }
            }
        }
        public virtual void EnvelopeRun()
        {
            if (EnvelopeVolume > 0)
                EnvelopeVolume--;
            else if (EnvelopeLoop)
                EnvelopeVolume = 0xF;
            //Reload();
        }
        int Init;
        bool Halted;
        public override void Step()
        {
            if (Halted)
            {
                Halted = false;
                LengthCounter.Enabled = true;
            }
            if (Init > 0)
            //if (IsLatched)
            {
                Init--;
                if (Init == 0)
                    SetControl(LatchData);
                IsLatched = false;
            }
            base.Step();
        }

    }
}
