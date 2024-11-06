using NES;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NesCore.Audio.Generators
{
    abstract class Wave: Counter
    {
        public APU APU;
        protected static byte[] lengthTable = {
            10, 254, 20, 2, 40, 4, 80, 6, 160, 8, 60, 10, 14, 12, 26, 14,
            12, 16, 24, 18, 48, 20, 96, 22, 192, 24, 72, 26, 16, 28, 32, 30
        };
        bool IsLatched;
        byte LatchData;
        //public bool FrameReloadCycle;
        public Counter LengthCounter = new Counter(); 
        public Counter Envelope = new Counter();
        protected bool EnvelopeLoop;
        protected byte EnvelopeVolume;

        public Wave()
        {
            Envelope.Period = 0xF;
            Envelope.OnStop = EnvelopeRun;
            //LengthCounter.OnStop = LengthRun;
        }
        //bool enabled;
        public override bool Enabled
        {
            //get { return enabled; }
            set
            {
                base.Enabled = value;
                //LengthCounter.Period = 0;
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
        public abstract byte Output { get; }
        //public bool LengthEnabled;
        //int lengthValue;
        //public int LengthValue {
        //    get { return lengthValue; }
        //    set
        //    {
        //        if (Enabled)
        //        {
        //            //EnvelopeStart = true;
        //            var reload = true;
        //            FrameReloadCycle = false;
        //            if (APU.Cycle == APU.LengthCycles[1]) // APU $40xx/framecounter conflict
        //            {
        //                FrameReloadCycle = lengthValue == 0;
        //                reload = FrameReloadCycle;
        //            }
        //            if (reload)
        //                lengthValue = value;
        //        }
        //    }
        //}

        protected int Length
        {
            set
            {
                if (Enabled)
                {
                    //EnvelopeStart = true;
                    var reload = true;
                    LengthCounter.Halted = false;
                    if (APU.Cycle == APU.LengthCycles[1]) // APU $40xx/framecounter conflict
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

        //public void StepLength()
        //{
        //    if (!FrameReloadCycle && LengthEnabled && lengthValue > 0)
        //        lengthValue--;
        //}

        //public void StepEnvelope()
        //{
        //    if (EnvelopeStart)
        //    {
        //        EnvelopeStart = false;
        //        EnvelopeValue = EnvelopePeriod;
        //        EnvelopeVolume = 0xF;
        //    }
        //    else if (EnvelopeValue > 0)
        //        EnvelopeValue--;
        //    else
        //    {
        //        EnvelopeValue = EnvelopePeriod;
        //        if (EnvelopeVolume > 0)
        //            EnvelopeVolume--;
        //        else if (EnvelopeLoop)
        //            EnvelopeVolume = 0xF;
        //    }
        //}
        public void EnvelopeRun()
        {
            if (EnvelopeVolume > 0)
                EnvelopeVolume--;
            else if (EnvelopeLoop)
                EnvelopeVolume = 0xF;
        }

        //public abstract void OnTimerTick();
        public override void Step()
        {
            //FrameReloadCycle = false;
            LengthCounter.Halted = false;
            if (IsLatched)
            {
                SetControl(LatchData);
                IsLatched = false;
            }
            base.Step();
            //if (TimerValue > 0)
            //    TimerValue--;
            //else
            //{
            //    TimerValue = TimerPeriod;
            //    OnTimerTick();
            //}
        }

    }
}
