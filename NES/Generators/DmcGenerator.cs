using NES;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NesCore.Audio.Generators
{
    class DmcGenerator: Counter
    {
        public APU Apu;
        byte SampleValue;
        int SampleAddress;
        int SampleLength;
        int Address;
        int ShiftRegister;
        int BitCount;
        public int BufSize;
        bool IsMuted;
        public bool InterruptRequestEnabled;
        bool Loop;
        public int[] Periods;

        //protected override void SetControl(byte value)
        public byte Control
        {
            set
            {
                InterruptRequestEnabled = (value & 0x80) == 0x80;
                Loop = (value & 0x40) == 0x40;
                Period = (Periods[value & 0x0F] >> 1) - 1;
                //Reload();
            }
        }

        public byte Output
        {
            get
            {
                if (IsMuted) return 0;
                return SampleValue;
            }
        }

        public override void Run()
        {
            //if (!IsMuted)
            //if (Enabled)
            if (BufSize > 0) 
            {
                if ((ShiftRegister & 1) != 0)
                {
                    if (SampleValue <= 0x7F - 2)
                        SampleValue += 2;
                }
                else
                {
                    if (SampleValue >= 0x00 + 2)
                        SampleValue -= 2;
                }
                ShiftRegister >>= 1;
            }
            if (BitCount == 0)
            {
                if (BufSize > 0)
                {
                    BitCount = 7;
                    //IsMuted = false;
                    //if (Apu.Bus.Cpu.irq || Apu.Bus.Cpu.DmcIrq)
                        Apu.Bus.Cpu.ActOp.Cycle += 4;
                    //else
                      //  Apu.Bus.Cpu.ActOp.Cycle += 3;
                    ShiftRegister = Apu.Bus.Peek(Address);
                    Address++;
                    if (Address == 0x10000)
                        Address = 0x8000;
                    BufSize--;
                }
                if (BufSize == 0)
                {
                    //IsMuted = true;
                    if (Loop)
                        Restart();
                    else if (InterruptRequestEnabled)
                        Apu.Bus.Cpu.DmcIrq = true;
                }
            }
            else
                BitCount--;
            Reload();
        }

        public void Restart()
        {
            Address = SampleAddress;
            BufSize = SampleLength;
            //IsMuted = false;
        }

        public void Poke(int addr, byte data)
        {
            switch (addr)
            {
                case 0x4010:
                        Control = data;
                        if ((data & 0x80) == 0)
                            Apu.Bus.Cpu.DmcIrq = false;
                    break;
                case 0x4011: SampleValue = (byte)(data & 0x7F); break;
                case 0x4012: SampleAddress = data << 6 | 0xC000; break;
                case 0x4013: SampleLength = data << 4 | 1; break;
            }
        }
    }
}
