using NES;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NesCore.Audio.Generators
{
    class TriangleWave : Wave
    {
        static byte[] triangleTable = {
            15, 14, 13, 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1, 0,
            0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15
        };

        public int DutyValue { get; private set; }
        public Counter LinearCounter = new Counter();

        //public int CounterPeriod { get; private set; }
        //public int CounterValue { get; private set; }
        //public bool CounterReload { get; private set; }

        protected override void SetControl(byte value)
        {
            //set
            {
                LengthCounter.Enabled = ((value >> 5) & 1) == 0;
                LinearCounter.Period = value & 0xF;
            }
        }

        public override byte Output
        {
            get
            {
                if (!Enabled)
                    return 0;

                if (!LengthCounter.IsActive)
                    return 0;

                if (!LinearCounter.IsActive)
                    return 0;

                return triangleTable[DutyValue];
            }
        }

        public override void Run()
        {
            if (LengthCounter.IsActive && LinearCounter.IsActive)
            {
                DutyValue++;
                DutyValue &= 0x1F;
            }
            Reload();// = true;
        }

        //public void StepCounter()
        //{
        //    if (CounterReload)
        //        CounterValue = CounterPeriod;
        //    else if (CounterValue > 0)
        //        --CounterValue;

        //    if (LengthCounter.Enabled)
        //        CounterReload = false;
        //}

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
                    Length = lengthTable[data >> 3];
                    Period = (Period & 0x00FF) | ((data & 7) << 8);
                    //TimerValue = Period;
                    //Reload = true;
                    //CounterReload = true;
                    break;
            }
        }

    }
}
