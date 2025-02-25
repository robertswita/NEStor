using NES;
using System;

namespace NEStor.Audio
{
    class Noise : Wave
    {
        public int[] Periods;
        public bool Mode;
        public int ShiftRegister = 1;

        public Noise(APU apu) : base(apu) 
        {
            Envelope = new EnvelopeCounter();
        }

        protected override void SetControl(byte value)
        {
            Length.Halted = (value & 0x20) != 0;
            Envelope.Loop = Length.Halted;
            IsConstantVolume = (value & 0x10) != 0;
            Envelope.Period = SampleVolume = value & 0xF;
        }

        public override bool IsMuted
        {
            get { return base.IsMuted || (ShiftRegister & 1) != 0; }
        }

        public override void Fire()
        {
            if (Length.IsActive)
            {
                var shift = Mode ? 6 : 1;
                var b1 = ShiftRegister & 1;
                var b2 = (ShiftRegister >> shift) & 1;
                ShiftRegister >>= 1;
                ShiftRegister |= (b1 ^ b2) << 14;
            }
            Reload();
        }

        public void Poke(int addr, byte data)
        {
            switch (addr)
            {
                case 0x400C: Control = data; break;
                case 0x400E:
                    Mode = (data & 0x80) != 0;
                    Period = Periods[data & 0x0F];
                    break;
                case 0x400F:
                    Duration = LengthCounter.Periods[data >> 3];
                    Envelope.ResetEnabled = true;
                    break;
            }
        }

    }
}
