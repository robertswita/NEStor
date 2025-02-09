using System;
using NES;

namespace NesCore.Audio
{
    class SquareWave: Wave
    {
        static byte[,] DutyTable = {
            { 0, 1, 0, 0, 0, 0, 0, 0 },
            { 0, 1, 1, 0, 0, 0, 0, 0 },
            { 0, 1, 1, 1, 1, 0, 0, 0 },
            { 1, 0, 0, 1, 1, 1, 1, 1 },
        };
        public int Channel;
        public int DutyMode;
        public int DutyValue;
        public Counter SweepCounter = new Counter();
        bool SweepNegative;
        int SweepShift;

        public SquareWave(byte channel)
        {
            Channel = channel;
            SweepCounter.OnStop = SweepRun;
        }
        protected override void SetControl(byte value)
        {
            DutyMode = value >> 6 & 3;
            LengthCounter.Enabled = (value & 0x20) == 0;
            EnvelopeLoop = !LengthCounter.Enabled;
            Envelope.Enabled = (value & 0x10) == 0;
            Envelope.Period = SampleVolume = value & 0xF;
            Envelope.Reload();
            EnvelopeVolume = 0xF;
        }
        public byte Sweep
        {
            set
            {
                SweepCounter.Enabled = (value & 0x80) != 0;
                SweepNegative = (value & 0x08) != 0;
                SweepCounter.Period = value >> 4 & 7;
                SweepShift = value & 7;
                SweepCounter.Reload();
            }
        }
        public override bool IsMuted
        {
            get { return base.IsMuted || DutyTable[DutyMode, DutyValue] == 0 || Period < 8 || Period >= 0x800; }
        }
        private void SweepRun()
        {
            if (Period >= 0x8)
            {
                int sweep = Period >> SweepShift;
                if (SweepNegative)
                {
                    Period -= sweep;
                    if (Channel == 1)
                        Period--;
                }
                else
                {
                    var targetPeriod = Period + sweep;
                    if (targetPeriod < 0x800)
                        Period = targetPeriod;
                }
            }
            Reload();
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
                case 0x4001: Sweep = data; break;
                case 0x4002:
                    Period &= 0xFF00;
                    Period |= data;
                    break;
                case 0x4003:
                    Length = LengthPeriods[data >> 3];
                    Period &= 0x00FF;
                    Period |= (data & 7) << 8;
                    DutyValue = 0;
                    break;
            }
        }

    }
}
