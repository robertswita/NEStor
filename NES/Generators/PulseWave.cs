using NES;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace NesCore.Audio.Generators
{
    class PulseWave: Wave
    {
        static byte[,] DutyTable = {
            {0, 1, 0, 0, 0, 0, 0, 0},
            {0, 1, 1, 0, 0, 0, 0, 0},
            {0, 1, 1, 1, 1, 0, 0, 0},
            {1, 0, 0, 1, 1, 1, 1, 1},
        };
        public int Channel;
        public int DutyMode;
        public int DutyValue;
        public Counter SweepCounter = new Counter();
        bool SweepNegative;
        int SweepShift;

        public byte ConstantVolume { get; private set; }

        public PulseWave(byte channel)
        {
            Channel = channel;
            SweepCounter.OnStop = SweepRun;
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
                    Length = lengthTable[data >> 3];
                    Period &= 0x00FF;
                    Period |= (data & 7) << 8;
                    DutyValue = 0;
                    break;
            }
        }

        protected override void SetControl(byte value)
        {
            DutyMode = (byte)((value >> 6) & 3);
            LengthCounter.Enabled = ((value >> 5) & 1) == 0;
            EnvelopeLoop = ((value >> 5) & 1) == 1;
            Envelope.Enabled = ((value >> 4) & 1) == 0;
            Envelope.Period = ConstantVolume = (byte)(value & 0xF);
            Envelope.Reload();
            EnvelopeVolume = 0xF;
        }

        public override byte Output
        {
            get
            {
                if (!Enabled)
                    return 0;

                if (!LengthCounter.IsActive)
                    return 0;

                if (DutyTable[DutyMode, DutyValue] == 0)
                    return 0;

                if (Period < 8 || Period >= 0x800)
                    return 0;

                //if (!SweepNegate && TimerPeriod + (TimerPeriod >> SweepShift) > 0x7FF)
                //    return 0;

                return Envelope.Enabled ? EnvelopeVolume : ConstantVolume;
            }
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

        public override void Run()
        {
            DutyValue++;
            DutyValue &= 7;
            Reload();
        }

    }
}
