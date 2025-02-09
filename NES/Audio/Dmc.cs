using NES;
using System;

namespace NesCore.Audio
{
    class Dmc: Wave
    {
        int SampleAddress;
        int SampleLength;
        public int Address;
        int ShiftRegister;
        int BitCount;
        //bool IsMuted;
        public bool IrqEnabled;
        bool Loop;
        public int[] Periods;
        int StartDmaDelay;
        int StopDmaDelay;
        bool IsSilenced;
        public int SampleBuffer;
        //int PeriodLatch;
        bool IrqLatched;

        public void Reset()
        {
            SampleAddress = 0xC000;
            SampleLength = 1;
            BufferEmpty = true;
            //PeriodLatch = Periods[0];
            Period = Periods[0];
            Reload();
        }
        bool IrqFlag;
        protected override void SetControl(byte value)
        //public byte SetControl
        {
            //set
            {
                IrqEnabled = (value & 0x80) == 0x80;
                Loop = (value & 0x40) == 0x40;
                Period = (Periods[value & 0x0F] >> 1) - 1;
                if (!IrqEnabled)
                {
                    Apu.Bus.Cpu.DmcIrq.Acknowledge();
                    //Apu.Bus.Cpu.DmcIrqLatched = false;
                    IrqFlag = false;
                }
                //else if (IrqFlag)
                //{
                //    Apu.Bus.Cpu.DmcIrqLatched = true;
                //}
            }
        }

        public override bool Enabled
        {
            set
            {
                IrqLatched = false;
                Apu.Bus.Cpu.DmcIrq.Acknowledge();
                //Apu.Bus.Cpu.DmcIrqLatched = false;
                base.Enabled = value;
                if (!Enabled)
                {
                    StopDmaDelay = Apu.Bus.Cpu.IsGetCycle() ? 2: 3;
                }
                else if (!LengthCounter.IsActive)
                {
                    Address = SampleAddress;
                    Length = SampleLength;
                    StartDmaDelay = Apu.Bus.Cpu.IsGetCycle() ? 2 : 3;
                }
            }
        }

        public override bool IsMuted
        {
            get { return false; }// || !LengthCounter.IsActive; }
        }

        bool BufferEmpty;
        bool ReloadBuffer;
        int DmaAddress;
        bool Fetching;
        public override void Fire()
        {
            Reload();
            if (!IsSilenced)
            //if (!BufferEmpty)
            {
                if ((ShiftRegister & 1) != 0)
                {
                    if (SampleVolume <= 125)
                        SampleVolume += 2;
                }
                else
                {
                    if (SampleVolume >= 2)
                        SampleVolume -= 2;
                }
                ShiftRegister >>= 1;

            }
            if (BitCount > 0)
            {
                BitCount--;
            }
            else
            {
                BitCount = 7;
                IsSilenced = BufferEmpty;
                if (!IsSilenced)
                {
                    BufferEmpty = true;
                    ShiftRegister = SampleBuffer;
                    //StartDmaDelay = 1;
                    if (LengthCounter.IsActive)
                    {
                        Fetching = true;
                        Apu.Bus.Cpu.DmaDmcEnabled = true;
                        LengthCounter.Step();
                    }
                }
            }
        }

        public void LoadBuffer()
        {
            //if (LengthCounter.IsActive)
            {
                SampleBuffer = Apu.Bus.Cpu.Read(Address);
                Fetching = false;
                BufferEmpty = false;
                //if (Apu.Bus.Mapper.Id == 71)
                    //BitCount = 6;
                Address++;
                if (Address == 0x10000)
                    Address = 0x8000;
                if (!LengthCounter.IsActive)
                {
                    if (Loop)
                    {
                        Address = SampleAddress;
                        Length = SampleLength;
                    }
                    else
                    {
                        IrqFlag = IrqEnabled;
                        if (IrqEnabled)
                            Apu.Bus.Cpu.DmcIrq.Start();
                    }
                }
            }
        }

        public void check_pending_dma()
        {
            if (StartDmaDelay > 0)
                StartDmaDelay--;
            if ((Apu.Cycle & 1) != 0)
            {
                if (StartDmaDelay == 0 && BufferEmpty && !Fetching && LengthCounter.IsActive)
                {
                    Fetching = true;
                    Apu.Bus.Cpu.DmaDmcEnabled = true;
                    LengthCounter.Step();
                }
                if (StopDmaDelay > 0)
                {
                    StopDmaDelay--;
                    if (StopDmaDelay == 0) //Abort any on-going transfer that hasn't fully started
                    {
                        LengthCounter.Period = 0;
                        LengthCounter.Reload();                        
                        Apu.Bus.Cpu.DmaDmcEnabled = false;
                    }
                }
            }
        }

        public void Poke(int addr, byte data)
        {
            switch (addr)
            {
                case 0x4010:
                    //Control = data;
                    SetControl(data);
                    break;
                case 0x4011: SampleVolume = data & 0x7F; break;
                case 0x4012: SampleAddress = data << 6 | 0xC000; break;
                case 0x4013: SampleLength = (data << 4) | 1; break;
            }
        }
    }
}
