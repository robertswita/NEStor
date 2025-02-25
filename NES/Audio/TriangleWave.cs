﻿using NES;
using System;

namespace NEStor.Audio
{
    class TriangleWave : Wave
    {
        public class LinearCounter: EnvelopeCounter
        {
            public bool ControlEnabled;
            public override void Fire() { }
            protected override void Reset()
            {
                Reload();
                if (!ControlEnabled)
                    ResetEnabled = false;
            }
        }
        public int DutyValue;

        public TriangleWave(APU apu) : base(apu) 
        {
            Envelope = new LinearCounter();
        }

        protected override void SetControl(byte value)
        {
            Length.Halted = (value & 0x80) != 0;
            (Envelope as LinearCounter).ControlEnabled = Length.Halted;
            Envelope.Period = value & 0x7F;
        }
        public override bool IsMuted
        {
            get { return base.IsMuted || !Envelope.IsActive; }
        }

        public override void Fire()
        {
            if (Length.IsActive && Envelope.IsActive)
            {
                DutyValue++;
                DutyValue &= 0x1F;
                Envelope.Volume = DutyValue < 16 ? 15 - DutyValue: DutyValue - 16;
            }
            Reload();
        }

        public void Poke(int addr, byte data)
        {
            switch (addr)
            {
                case 0x4008: Control = data; break;
                case 0x400A:
                    Period &= 0x700;
                    Period |= data;
                    break;
                case 0x400B:
                    Duration = LengthCounter.Periods[data >> 3];
                    Period &= 0xFF;
                    Period |= (data & 7) << 8;
                    Reload();
                    Envelope.ResetEnabled = true;
                    break;
            }
        }

    }
}
