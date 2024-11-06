using NES;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NesCore.Audio.Generators
{
    class DmcGenerator
    {
        public APU APU;
        public bool Enabled;
        byte SampleValue;
        int SampleAddress;
        int SampleLength;
        int currentAddress;
        int shiftRegister;
        int bitCount;
        int tickPeriod;
        int tickValue;
        bool loop;
        static byte[] dmcTable = {
            214, 190, 170, 160, 143, 127, 113, 107, 95, 80, 71, 64, 53, 42, 36, 27,
        };
        public delegate byte ReadMemorySampleHandler(int address);

        public byte Control
        {
            set
            {
                InterruptRequestEnabled = (value & 0x80) == 0x80;
                loop = (value & 0x40) == 0x40;
                tickPeriod = dmcTable[value & 0x0F];
            }
        }

        public byte Output
        {
            get
            {
                return SampleValue;
            }
        }


        public int CurrentLength { get; set; }

        public bool InterruptRequestEnabled { get; set; }

        public ReadMemorySampleHandler ReadMemorySample { get; set; }

        /// <summary>
        /// Handler for triggering interrupt requests
        /// </summary>
        public Action TriggerInterruptRequest { get; set; }

        public void StepTimer()
        {
            if (!Enabled)
                return;
            if (bitCount == 0)
            {
                if (CurrentLength > 0)
                {
                    shiftRegister = ReadMemorySample(currentAddress);
                    bitCount = 8;
                    currentAddress++;
                    if (currentAddress == 0)
                        currentAddress = 0x8000;
                    CurrentLength--;
                    if (CurrentLength <= 0)
                    {
                        //CurrentLength = 0;
                        if (loop)
                            Restart();
                        else
                        {
                            if (InterruptRequestEnabled)
                            {
                                TriggerInterruptRequest();
                            }
                        }

                    }

                }
            }
            else if (tickValue == 0)
            {
                tickValue = tickPeriod;
                tickValue--;
                //StepShifter();
                if ((shiftRegister & 1) == 1)
                {
                    if (SampleValue <= 0x7F - 2)
                        SampleValue += 2;
                }
                else
                {
                    if (SampleValue >= 0x00 + 2)
                        SampleValue -= 2;
                }
                bitCount--;
                shiftRegister >>= 1;
            }
            else
            {
                tickValue--;
            }
        }

        public void Restart()
        {
            currentAddress = SampleAddress;
            CurrentLength = SampleLength;
        }

        public void Poke(int addr, byte data)
        {
            switch (addr)
            {
                case 0x4010:
                        Control = data;
                        if ((data & 0x80) == 0)
                            APU.Bus.cpu.dmcirq = false;
                    break;
                case 0x4011: SampleValue = (byte)(data & 0x7F); break;
                case 0x4012: SampleAddress = data << 6 | 0xC000; break;
                case 0x4013: SampleLength = data << 4 | 1; break;
            }
        }
    }
}
