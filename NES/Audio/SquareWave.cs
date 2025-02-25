using System;
using NES;

namespace NEStor.Audio
{
    class SquareWave: Wave
    {
        public class SweepCounter : EnvelopeCounter
        {
            public SquareWave Wave;
            public bool Enabled;
            public bool Negative;
            public int Shift;
            public override void Fire()
            {
                if (Wave.Period >= 0x8 && Enabled)
                {
                    int sweep = Wave.Period >> Shift;
                    if (Negative)
                    {
                        Wave.Period -= sweep;
                        if (Wave == Wave.Apu.Square0)
                            Wave.Period--;
                    }
                    else
                    {
                        var targetPeriod = Wave.Period + sweep;
                        if (targetPeriod < 0x800)
                            Wave.Period = targetPeriod;
                    }
                }
                Reload();
            }
        }
        static byte[,] DutyTable = {
            { 0, 1, 0, 0, 0, 0, 0, 0 },
            { 0, 1, 1, 0, 0, 0, 0, 0 },
            { 0, 1, 1, 1, 1, 0, 0, 0 },
            { 1, 0, 0, 1, 1, 1, 1, 1 },
        };
        public int DutyMode;
        public int DutyValue;
        public SweepCounter Sweep;

        public SquareWave(APU apu): base(apu)
        {
            Envelope = new EnvelopeCounter();
            Sweep = new SweepCounter();
            Sweep.Wave = this;
        }
        protected override void SetControl(byte value)
        {
            DutyMode = value >> 6 & 3;
            Length.Halted = (value & 0x20) != 0;
            Envelope.Loop = Length.Halted;
            IsConstantVolume = (value & 0x10) != 0;
            SampleVolume = value & 0xF;
            Envelope.Period = SampleVolume;
        }

        public override bool IsMuted
        {
            get { return base.IsMuted || DutyTable[DutyMode, DutyValue] == 0 || Period < 8 || (!Sweep.Negative && Period >= 0x800); }
        }

        public override void Fire()
        {
            DutyValue++;
            DutyValue &= 7;
            Reload();
        }

        public void Poke(int addr, byte data)
        {
            switch (addr)
            {
                case 0x4000: Control = data; break;
                case 0x4001:
                    Sweep.Enabled = (data & 0x80) != 0;
                    Sweep.Negative = (data & 0x08) != 0;
                    Sweep.Period = data >> 4 & 7;
                    Sweep.Shift = data & 7;
                    Sweep.ResetEnabled = true;
                    Sweep.Fire();
                    break;
                case 0x4002:
                    Period &= 0x700;
                    Period |= data;
                    break;
                case 0x4003:
                    Period &= 0xFF;
                    Period |= (data & 7) << 8;
                    Duration = LengthCounter.Periods[data >> 3];
                    Envelope.ResetEnabled = true;
                    DutyValue = 0;
                    break;
            }
        }

    }
}
