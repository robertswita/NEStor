using System;

namespace NesCore.Audio.Generators
{
    class Noise : Wave
    {
        public int[] Periods;
        public bool Mode;
        public int ShiftRegister = 1;

        protected override void SetControl(byte value)
        {
            LengthCounter.Enabled = (value & 0x20) == 0;
            EnvelopeLoop = !LengthCounter.Enabled;
            Envelope.Enabled = (value & 0x10) == 0;
            Envelope.Period = ConstantVolume = value & 0xF;
        }

        public override bool IsMuted
        {
            get { return base.IsMuted || (ShiftRegister & 1) != 0; }
        }

        public override void Run()
        {
            if (LengthCounter.IsActive)
            {
                var shift = Mode ? 6 : 1;
                var b1 = ShiftRegister & 1;
                var b2 = (ShiftRegister >> shift) & 1;
                ShiftRegister >>= 1;
                ShiftRegister |= (b1 ^ b2) << 14;
                Reload();
            }
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
                    Length = LengthPeriods[data >> 3];
                    EnvelopeVolume = 0xF;
                    Envelope.Period = 0xF;
                    Envelope.Reload();
                    break;
            }
        }

    }
}
