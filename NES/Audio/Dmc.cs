using NES;
using System;

namespace NEStor.Audio
{
    class Dmc: Counter
    {
        public APU Apu;
        int SampleAddress;
        int SampleBytes;
        public int Address;
        int ShiftRegister;
        int BitCount;
        public int ByteCount;
        public bool IrqEnabled;
        bool Loop;
        public int[] Periods;
        int StartDmaDelay;
        int StopDmaDelay;
        public bool IsSilenced;
        public int SampleBuffer;
        public int Volume;
        public static bool DuplicationGlitchEnabled = true;

        public Dmc()
        {
            //Envelope.Enabled = false;
            //IsConstantVolume = true;
        }

        public void Reset()
        {
            Address = 0xC000;
            SampleBytes = 1;
            BufferEmpty = true;
            BitCount = 7;
            Period = Periods[0];
            Reload();
        }

        void InitSample()
        {
            Address = SampleAddress;
            ByteCount = SampleBytes;
        }

        int Control
        {
            set
            {
                IrqEnabled = (value & 0x80) != 0;
                Loop = (value & 0x40) != 0;
                Period = (Periods[value & 0x0F] >> 1) - 1;
                if (!IrqEnabled)
                    Apu.Bus.Cpu.DmcIrq.Acknowledge();
            }
        }

        public bool Enabled
        {
            set
            {
                Apu.Bus.Cpu.DmcIrq.Acknowledge();
                if (!value)
                {
                    StopDmaDelay = Apu.Bus.Cpu.IsGetCycle() ? 2 : 3;
                }
                else if (ByteCount == 0)
                {
                    InitSample();
                    if (ByteCount > 0)
                        StartDmaDelay = Apu.Bus.Cpu.IsGetCycle() ? 2 : 3;
                }
            }
        }

        //public override bool IsMuted
        //{
        //    get { return false/* IsSilenced*/; }// || !LengthCounter.IsActive; }
        //}

        bool BufferEmpty;
        public override void Fire()
        {
            Reload();
            //if (!IsSilenced)
            {
                if ((ShiftRegister & 1) != 0)
                {
                    if (Volume <= 125)
                        Volume += 2;
                }
                else
                {
                    if (Volume >= 2)
                        Volume -= 2;
                }
                ShiftRegister >>= 1;
            }
            if (BitCount > 0)
                BitCount--;
            else
            {
                BitCount = 7;
                //IsSilenced = BufferEmpty;
                ShiftRegister = SampleBuffer;
                if (!BufferEmpty)
                {
                    BufferEmpty = true;
                    if (ByteCount > 0)
                        Apu.Bus.Cpu.DmaDmcEnabled = true;
                }
            }
        }

        public void LoadBuffer()
        {
            //if (ByteCount > 0)
            {
                SampleBuffer = Apu.Bus.Cpu.Read(Address);
                //Fetching = false;
                BufferEmpty = false;
                //if (Apu.Bus.Mapper.Id == 71)
                    //BitCount = 6;
                Address++;
                if (Address == 0x10000)
                    Address = 0x8000;
                //LengthCounter.Step();
                ByteCount--;
                //if (!LengthCounter.IsActive)
                if (ByteCount == 0)
                {
                    if (Loop)
                    {
                        InitSample();
                    }
                    else
                    {
                        if (IrqEnabled)
                            Apu.Bus.Cpu.DmcIrq.Start(1);
                    }
                }
            }
            if (SampleBytes == 1 && !Loop)
            {
                //When DMA ends around the time the bit counter resets, a CPU glitch sometimes causes another DMA to be requested immediately.
                if (DuplicationGlitchEnabled && BitCount == 7 && Count == Period)
                {
                    //When the DMA ends on the same cycle as the bit counter resets
                    //This glitch exists on all H CPUs and some G CPUs (those from around 1990 and later)
                    //In this case, a full DMA is performed on the same address, and the same sample byte 
                    //is played twice in a row by the DMC
                    ShiftRegister = SampleBuffer;
                    IsSilenced = false;
                    BufferEmpty = true;
                    InitSample();
                    StartDmaDelay = 2;
                }
                if (BitCount == 0 && Count < 2)
                {
                    //When the DMA ends on the APU cycle before the bit counter resets
                    //If it this happens right before the bit counter resets,
                    //a DMA is triggered and aborted 1 cycle later (causing one halted CPU cycle)
                    ShiftRegister = SampleBuffer;
                    BufferEmpty = false;
                    InitSample();
                    StopDmaDelay = 3;
                }
            }

        }

        public void CheckDma()
        {
            if (StartDmaDelay > 0)
            {
                StartDmaDelay--;
                if (StartDmaDelay == 0 && BufferEmpty && ByteCount > 0)
                    Apu.Bus.Cpu.DmaDmcEnabled = true;
            }
            if (StopDmaDelay > 0)
            {
                StopDmaDelay--;
                if (StopDmaDelay == 0) //Abort any on-going transfer that hasn't fully started
                {
                    ByteCount = 0;
                    Apu.Bus.Cpu.DmaDmcEnabled = false;
                }
            }
        }
        public void Poke(int addr, byte data)
        {
            switch (addr)
            {
                case 0x4010: Control = data; break;
                case 0x4011: Volume = data & 0x7F; break;
                case 0x4012: SampleAddress = data << 6 | 0xC000; break;
                case 0x4013: SampleBytes = data << 4 | 1; break;
            }
        }
    }
}
