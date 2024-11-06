using Microsoft.VisualBasic.Devices;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NesCore.Audio.Generators
{
    class Noise : Wave
    {
        static int[] noiseTable = {
            4, 8, 16, 32, 64, 96, 128, 160, 202, 254, 380, 508, 762, 1016, 2034, 4068,
        };
        public Noise()
        {
            ShiftRegister = 1;
        }

        protected override void SetControl(byte value)
        {
            //set
            {
                LengthCounter.Enabled = ((value >> 5) & 1) == 0;
                EnvelopeLoop = !LengthCounter.Enabled;
                Envelope.Enabled = ((value >> 4) & 1) == 0;
                Envelope.Period = ConstantVolume = (byte)(value & 15);
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

                if ((ShiftRegister & 1) == 1)
                    return 0;

                return Envelope.Enabled ? EnvelopeVolume : ConstantVolume;
            }
        }

        public bool Mode { get; private set; }

        public ushort ShiftRegister { get; private set; }

        public byte ConstantVolume { get; private set; }

        public override void Run()
        {
            if (LengthCounter.IsActive)
            {
                //TimerValue = TimerPeriod;
                Reload();// = true;
                byte shift = Mode ? (byte)6 : (byte)1;

                byte b1 = (byte)(ShiftRegister & 1);
                byte b2 = (byte)((ShiftRegister >> shift) & 1);

                ShiftRegister >>= 1;
                ShiftRegister |= (ushort)((b1 ^ b2) << 14);
            }
            //Reload = true;
        }

        public void Poke(int addr, byte data)
        {
            switch (addr)
            {
                case 0x400C: Control = data; break;
                case 0x400E:
                    Mode = (data & 0x80) != 0;
                    Period = noiseTable[data & 0x0F];
                    break;
                case 0x400F:
                    Length = lengthTable[data >> 3];
                    EnvelopeVolume = 0xF;
                    Envelope.Period = 0xF;
                    //Envelope.Reload = true;
                    break;
            }
        }

    }
}
