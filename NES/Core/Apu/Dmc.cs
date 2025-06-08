using System;

namespace NEStor.Core.Apu
{
    class Dmc: Counter
    {
        public APU Apu;
        bool IsSilenced;
        bool IrqEnabled;
        bool Loop;
        int SampleAddress;
        int SampleBytes;
        int SampleBuffer;
        int ShiftRegister;
        int BitCount;
        int StartDmaDelay;
        int StopDmaDelay;
        public int Address;
        public int ByteCount;
        public int[] Periods;
        public int Volume;
        public static bool DuplicationGlitchEnabled = true;

        //public void Reset()
        //{
        //    Address = 0xC000;
        //    SampleBytes = 1;
        //    BufferEmpty = true;
        //    BitCount = 7;
        //    Period = Periods[0];
        //    Reload();
        //}

        //public Dmc(APU apu) : base(apu) { }

        void InitSample()
        {
            Address = SampleAddress;
            ByteCount = SampleBytes;
        }

        int Control
        //protected override void SetControl(byte value)
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
                    if (StopDmaDelay == 0)
                    {
                        StopDmaDelay = Apu.Bus.Cpu.IsGetCycle() ? 2 : 3;
                    }
                }
                else if (ByteCount == 0)
                {
                    InitSample();
                    if (ByteCount > 0)
                        StartDmaDelay = Apu.Bus.Cpu.IsGetCycle() ? 2 : 3;
                }
            }
        }

        bool BufferEmpty;
        public override void Execute()
        {
            Reload();
            if (!IsSilenced)
            {
                var targetVolume = Volume + ((ShiftRegister & 1) != 0 ? 2 : -2);
                if (targetVolume >= 0 && targetVolume < 128)
                    Volume = targetVolume;
                ShiftRegister >>= 1;
            }
            if (BitCount > 0)
                BitCount--;
            else
            {
                BitCount = 7;
                IsSilenced = BufferEmpty && SampleBuffer == 0;
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

        public void Dma()
        {
            var buffer = Apu.Bus.Cpu.Read(Address);
            //if (Apu.Bus.Cpu.DmaDmcEnabled)
            //{
            //    return;
            //}
            if (ByteCount > 0)
            {
                SampleBuffer = buffer;
                BufferEmpty = false;
                Address++;
                if (Address == 0x10000)
                    Address = 0x8000;
                ByteCount--;
                if (ByteCount == 0)
                {
                    if (Loop)
                        InitSample();
                    else if (IrqEnabled)
                        Apu.Bus.Cpu.DmcIrq.Start(1);
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
            Apu.Bus.Cpu.DmaDmcEnabled = false;
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
