using System;

namespace NEStor.Core.Apu
{
    class TriangleWave : Wave
    {
        public int DutyValue;
        public class LinearCounter: EnvelopeCounter
        {
            public bool ControlEnabled;
            public override void Execute() { }
            protected override void Reset()
            {
                Reload();
                if (!ControlEnabled)
                    ResetEnabled = false;
            }
        }

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
            get { return base.IsMuted || !Envelope.IsActive || Period < 2; }
        }

        public override void Execute()
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
