﻿using NES;
using System;
using System.Collections.Generic;
using System.Text;

namespace NEStor.Audio
{
    abstract class Wave: Counter
    {
        public class LengthCounter : Counter
        {
            public static byte[] Periods = 
            {
                10, 254, 20, 2, 40, 4, 80, 6, 160, 8, 60, 10, 14, 12, 26, 14,
                12, 16, 24, 18, 48, 20, 96, 22, 192, 24, 72, 26, 16, 28, 32, 30
            };
            public bool Halted;
            public override void Step()
            {
                if (!Halted)
                    base.Step();
            }
        }
        public class EnvelopeCounter : Counter
        {
            public bool Loop;
            public int Volume;
            public bool ResetEnabled;
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
                if (ResetEnabled)
                    Reset();
                else
                    base.Step();
            }
            protected virtual void Reset()
            {
                Volume = 0xF;
                Reload();
                ResetEnabled = false;
            }

        }

        public APU Apu;
        //public static byte[] LengthPeriods = {
        //    10, 254, 20, 2, 40, 4, 80, 6, 160, 8, 60, 10, 14, 12, 26, 14,
        //    12, 16, 24, 18, 48, 20, 96, 22, 192, 24, 72, 26, 16, 28, 32, 30
        //};
        bool IsLatched;
        byte LatchData;
        public LengthCounter Length = new LengthCounter();
        public EnvelopeCounter Envelope;
        protected bool IsConstantVolume;
        public int SampleVolume;

        public Wave(APU apu)
        {
            Apu = apu;
            //Envelope.Period = 0xF;
            //Envelope.OnStop = EnvelopeRun;
            //Envelope.Enabled = true;
            //Envelope.IsLength = true;
        }
        bool enabled;
        public bool Enabled
        {
            get { return enabled; }
            set
            {
                enabled = value;
                Length.IsActive = value;
                //Envelope.IsActive = value;
                //if (!LengthCounter.Enabled)
                //{
                //    LengthCounter.Period = 0;
                //    LengthCounter.Reload();
                //}
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
            get { return !Enabled || !Length.IsActive; }
        }
        protected int volume;
        public int Volume
        {
            get
            {
                if (IsMuted)
                    return 0;
                return IsConstantVolume ? SampleVolume : Envelope.Volume;
            }
        }
        protected byte Duration
        {
            set
            {
                if (Enabled)
                {
                    var reload = true;
                    FrameHalted = false;
                    if (Apu.IsLengthCycle) // APU ($40xx & 3)/framecounter conflict
                    {
                        reload = !Length.IsActive;
                        if (reload)
                        {
                            FrameHalted = true;
                            Length.Halted = true;
                        }
                    }
                    if (reload)
                    {
                        Length.Period = value;
                        Length.Reload();
                    }
                }
            }
        }

        bool FrameHalted;
        public override void Step()
        {
            if (FrameHalted)
            {
                FrameHalted = false;
                Length.Halted = false;
            }
            if (IsLatched)
            {
                SetControl(LatchData);
                IsLatched = false;
            }
            base.Step();
        }

    }
}
