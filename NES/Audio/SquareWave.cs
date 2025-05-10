using NES;

namespace NEStor.Audio
{
    class SquareWave: Wave
    {
        public class SweepCounter : EnvelopeCounter
        {
            public SquareWave Wave;
            public bool Negative;
            public int Shift;
            public override void Execute()
            {
                if (Enabled && Shift > 0)
                {
                    int sweep = Wave.Period >> Shift;
                    var targetPeriod = Wave.Period + (Negative ? -sweep : sweep);
                    if (Negative && Wave == Wave.Apu.Square0) targetPeriod--;
                    if (targetPeriod >= 8 && targetPeriod < 0x800)
                        Wave.Period = targetPeriod;
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
            Sweep = new SweepCounter();
            Sweep.Wave = this;
        }
        protected override void SetControl(byte value)
        {
            DutyMode = value >> 6 & 3;
            Length.Halted = (value & 0x20) != 0;
            Envelope.Loop = Length.Halted;
            Envelope.Enabled = (value & 0x10) == 0;
            Envelope.Period = value & 0xF;
        }

        public override bool IsMuted
        {
            get { return base.IsMuted || DutyTable[DutyMode, DutyValue] == 0; }
        }

        public override void Execute()
        {
            DutyValue--;
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
                    Sweep.Shift = data & 7;
                    Sweep.Period = data >> 4 & 7;
                    Sweep.ResetEnabled = true;
                    Sweep.Execute();
                    break;
                case 0x4002:
                    Period &= 0x700;
                    Period |= data;
                    break;
                case 0x4003:
                    Period &= 0xFF;
                    Period |= data << 8 & 0x700;
                    Duration = LengthCounter.Periods[data >> 3];
                    Envelope.ResetEnabled = true;
                    DutyValue = 0;
                    break;
            }
        }

    }
}
