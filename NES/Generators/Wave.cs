using NES;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NesCore.Audio.Generators
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
        public int ConstantVolume;

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
                if (!value)
                {
                    LengthCounter.Period = 0;
                    LengthCounter.Reload();
                }
                LengthCounter.Enabled = value;
            }
        }
        protected abstract void SetControl(byte value);
        public byte Control 
        { 
            set
            {
                LatchData = value;
                IsLatched = true;
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
                return Envelope.Enabled ? EnvelopeVolume : ConstantVolume;
            }
        }
        protected int Length
        {
            set
            {
                if (Enabled)
                {
                    var reload = true;
                    LengthCounter.Halted = false;
                    if (Apu.IsLengthCycle) // APU $40xx/framecounter conflict
                    {
                        LengthCounter.Halted = !LengthCounter.IsActive;
                        reload = LengthCounter.Halted;
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
        }
        public override void Step()
        {
            LengthCounter.Halted = false;
            if (IsLatched)
            {
                SetControl(LatchData);
                IsLatched = false;
            }
            base.Step();
        }

    }
}
