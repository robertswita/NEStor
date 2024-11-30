using System;

namespace NesCore.Audio.Generators
{
    class TriangleWave : Wave
    {
        public int DutyValue;

        protected override void SetControl(byte value)
        {
            LengthCounter.Enabled = (value & 0x20) == 0;
            Envelope.Enabled = true;
            Envelope.Period = value & 0xF;
            Envelope.Reload();
        }
        public override bool IsMuted
        {
            get { return base.IsMuted || !Envelope.IsActive; }
        }
        public override void EnvelopeRun() { }

        public override void Run()
        {
            if (LengthCounter.IsActive && Envelope.IsActive)
            {
                DutyValue++;
                DutyValue &= 0x1F;
                if (DutyValue < 16)
                    EnvelopeVolume = 15 - DutyValue;
                else
                    EnvelopeVolume = DutyValue - 16;
            }
            Reload();
        }

        public void Poke(int addr, byte data)
        {
            switch (addr)
            {
                case 0x4008: Control = data; break;
                case 0x400A:
                    Period &= 0xFF00;
                    Period |= data;
                    break;
                case 0x400B:
                    Length = LengthPeriods[data >> 3];
                    Period = (Period & 0x00FF) | ((data & 7) << 8);
                    Reload();
                    break;
            }
        }

    }
}
