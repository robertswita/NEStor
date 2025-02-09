using GitHub.secile.Audio;
using NesCore.Audio.Filtering;
using NesCore.Audio;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace NES
{
    class APU
    {
        public Bus Bus;
        public SquareWave Square0;
        public SquareWave Square1;
        public TriangleWave Triangle;
        public Noise Noise;
        public Dmc Dmc;
        public float SampleRate;
        bool IrqDisable;
        FilterChain FilterChain;
        public int[] FrameCycles;
        int FrameCycleIdx;
        int FramePeriod = 4;
        public int Cycle;
        public static int Frequency = 44100;
        public static int ResampleFrequency = 41000;
        public AudioOutput WaveOut = new AudioOutput(Frequency, 16, 1);
        byte[] OutputBuffer;
        int OutputLength;

        bool enabled;
        public bool Enabled
        {
            get { return enabled; }
            set
            {
                enabled = value;
                Reset();
            }
        }

        public APU(Bus bus)
        {
            Bus = bus;
            Square0 = new SquareWave(1);
            Square1 = new SquareWave(2);
            Triangle = new TriangleWave();
            Noise = new Noise();
            Dmc = new Dmc();
            Square0.Apu = this;
            Square1.Apu = this;
            Triangle.Apu = this;
            Noise.Apu = this;
            Dmc.Apu = this;
            FilterChain = new FilterChain();
            FrameCounterPeriod = 0;
            Reset();
        }

        public void Reset()
        {
            SampleRate = Bus.Cpu.Frequency / ResampleFrequency;
            OutputBuffer = new byte[1024 * 4];
            OutputLength = 0;
            WaveOut.Close();
            WaveOut = new AudioOutput(Frequency, 16, 1);
        }

        void WriteSample(float sampleValue)
        {
            var sample = (int)(sampleValue * short.MaxValue);
            OutputBuffer[OutputLength++] = (byte)(sample & 0xFF);
            OutputBuffer[OutputLength++] = (byte)(sample >> 8 & 0xFF);
            if (OutputLength == OutputBuffer.Length)
            {
                WaveOut.Write(OutputBuffer);
                OutputLength = 0;
            }
        }

        public void Poke(int addr, byte data)
        {
            if (addr < 0x4004)
                Square0.Poke(addr, data);
            else if (addr < 0x4008)
                Square1.Poke(addr & ~4, data);
            else if (addr < 0x400C)
                Triangle.Poke(addr, data);
            else if (addr < 0x4010)
                Noise.Poke(addr, data);
            else if (addr < 0x4014)
                Dmc.Poke(addr, data);
            else if (addr == 0x4015)
                Control = data;
            else if (addr == 0x4017)
                FrameCounterPeriod = data;
        }

        public byte Status
        {
            get
            {
                byte result = 0;
                if (Square0.LengthCounter.IsActive)
                    result |= 1;
                if (Square1.LengthCounter.IsActive)
                    result |= 2;
                if (Triangle.LengthCounter.IsActive)
                    result |= 4;
                if (Noise.LengthCounter.IsActive)
                    result |= 8;
                if (Dmc.LengthCounter.IsActive)
                    result |= 0x10;
                if (Bus.Cpu.ApuIrq.Enabled)
                    result |= 0x40;
                if (Bus.Cpu.DmcIrq.Enabled)
                    result |= 0x80;
                Bus.Cpu.ApuIrq.Acknowledge();
                return result;
            }
        }
        int ControlLatch;
        bool Latched;
        public void SetControl()
        {
            var value = ControlLatch;
            Square0.Enabled = (value & 1) != 0;
            Square1.Enabled = (value & 2) != 0;
            Triangle.Enabled = (value & 4) != 0;
            Noise.Enabled = (value & 8) != 0;
            Dmc.Enabled = (value & 0x10) != 0;
        }
        public byte Control
        {
            set
            {
                Square0.Enabled = (value & 1) != 0;
                Square1.Enabled = (value & 2) != 0;
                Triangle.Enabled = (value & 4) != 0;
                Noise.Enabled = (value & 8) != 0;
                Dmc.Enabled = (value & 0x10) != 0;

                //ControlLatch = value;
                //Latched = true;
            }
        }

        //void SetFrameCounter()
        //{
        //    var value = FrameCounterLatch;
        //    FramePeriod = 4 + ((value >> 7) & 1);
        //    Cycle = Cycle % 2 == 0 ? -2 : -3;
        //    //Cycle = 0;
        //    FrameCycleIdx = 1;
        //    if (FramePeriod == 5)
        //        StepLength();
        //}

        //int FrameCounterDelay;
        //int FrameCounterLatch;
        //public byte FrameCounter
        //{
        //    set
        //    {
        //        FrameCounterLatch = value;
        //        // Writes occurring on odd clocks are delayed
        //        //FrameCounterDelay = Cycle % 2 == 0 ? 3: 4;
        //        SetFrameCounter();
        //        InhibitIRQ = ((value >> 6) & 1) != 0;
        //        if (InhibitIRQ)
        //        {
        //            FrameIrqEnabled = false;
        //            //Bus.Cpu.ApuIrq.Acknowledge();
        //            Bus.Cpu.ApuIrqLatched = false;
        //        }
        //    }
        //}
        public byte FrameCounterPeriod
        {
            set
            {
                FramePeriod = 4 + ((value >> 7) & 1);
                IrqDisable = ((value >> 6) & 1) != 0;
                if (IrqDisable)
                    Bus.Cpu.ApuIrq.Acknowledge();
                FrameCycleIdx = 0; // or 1?
                ResetCycles();
                Cycle = Bus.Cpu.IsGetCycle() ? -2 : -3;
                if (FramePeriod == 5)
                    StepLength();
            }
        }

        void ResetCycles()
        {
            NextSampleCycle -= Cycle;
            Cycle = 0;
        }

        //public float SampleRate
        //{
        //    get { return sampleRate; }
        //    set
        //    {
        //        sampleRate = Bus.Cpu.Frequency / value;
        //        FilterChain.Filters.Clear();
        //        FilterChain.Filters.Add(FirstOrderFilter.CreateHighPassFilter(value, 90f));
        //        FilterChain.Filters.Add(FirstOrderFilter.CreateHighPassFilter(value, 440f));
        //        FilterChain.Filters.Add(FirstOrderFilter.CreateLowPassFilter(value, 14000f));
        //    }
        //}

        private const float NLN_SQR_0 = 95.52f;
        private const float NLN_SQR_1 = 8128.00f;

        private const float NLN_TND_0 = 163.67f;
        private const float NLN_TND_1 = 24329.00f;
        //public float Output
        //{
        //    get
        //    {
        //        float pulseOutput = (Square0.Volume + Square1.Volume) / 8128f;
        //        pulseOutput = 95.88f * pulseOutput / (1 + pulseOutput * 100);
        //        //float tndOutput = tndTable[3 * triangleOutput + 2 * noiseOutput + dmcOutput];
        //        float tndOutput = Triangle.Volume / 8227f + Noise.Volume / 12241f + Dmc.Volume / 22638f;
        //        tndOutput = 159.79f * tndOutput / (1 + tndOutput * 100);
        //        var output = pulseOutput + tndOutput;
        //        //if (output > 1) 
        //        //    output = 1;
        //        //if (output < 0) 
        //        //    output = 0;

        //        //var pulseSample = Square0.Volume + Square1.Volume;
        //        //var tndSample = 3 * Triangle.Volume + 2 * Noise.Volume + Dmc.Volume;
        //        //var output = (NLN_SQR_0 / (NLN_SQR_1 / pulseSample + 100)) +
        //        //                (NLN_TND_0 / (NLN_TND_1 / tndSample + 100));
        //        return output;
        //    }
        //}

        public float Output
        {
            get
            {
                float pulseOutput = (Square0.Volume + Square1.Volume) / 81.28f;
                pulseOutput = 0.9588f * pulseOutput / (1 + pulseOutput);
                //float tndOutput = tndTable[3 * triangleOutput + 2 * noiseOutput + dmcOutput];
                float tndOutput = Triangle.Volume / 82.27f + Noise.Volume / 122.41f + Dmc.Volume / 226.38f;
                tndOutput = 1.5979f * tndOutput / (1 + tndOutput);
                return pulseOutput + tndOutput;
            }
        }

        float NextSampleCycle;
        public void Step()
        {
            //if (Enabled)
            {
                //if (Latched)
                //{
                //    SetControl();
                //    Latched = false;
                //}
                //if (FrameCounterDelay > 0)
                //{
                //    FrameCounterDelay--;
                //    if (FrameCounterDelay == 0)
                //        SetFrameCounter();
                //}
                if (Enabled)
                {
                    Dmc.check_pending_dma();
                    if (Cycle % 2 == 0)
                    {
                        Square0.Step();
                        Square1.Step();
                        Noise.Step();
                        Dmc.Step();
                    }
                    Triangle.Step();
                    if (Cycle == FrameCycles[FrameCycleIdx])
                        FrameCounterStep();
                    if (Cycle >= NextSampleCycle)
                    {
                        //float filteredOutput = FilterChain.Apply(Output);
                        WriteSample(Output);
                        NextSampleCycle += SampleRate;
                    }
                    Cycle++;
                }
            }
        }
        public bool IsLengthCycle
        {
            get
            {
                return Cycle == FrameCycles[1] ||
                    Cycle == FrameCycles[4] && FramePeriod == 4 ||
                    Cycle == FrameCycles[6] && FramePeriod == 5;
            }
        }

        private void FrameCounterStep()
        {
            if (FrameCycleIdx == 0 || FrameCycleIdx == 2)
                StepEnvelope();
            else if (IsLengthCycle)
                StepLength();
            if (FrameCycleIdx >= 3 && FrameCycleIdx <= 5 && FramePeriod == 4 && !IrqDisable)
            {
                //if (FrameCycleIdx == 4)
                //FrameIrqEnabled = true;
                //{
                //Bus.Cpu.ApuIrq.Delay = 3;
                Bus.Cpu.ApuIrq.Start(2);
                //Bus.Cpu.ApuIrqLatched = true;

                //Bus.Cpu.ApuIrqLatched = true;
                //}
            }
            FrameCycleIdx++;
            FrameCycleIdx %= FramePeriod == 4 ?  6 : 8;
            if (FrameCycleIdx == 0)
                ResetCycles();
        }

        private void StepEnvelope()
        {
            Square0.Envelope.Step();
            Square1.Envelope.Step();
            Triangle.Envelope.Step();
            Noise.Envelope.Step();
        }

        private void StepLength()
        {
            StepEnvelope();
            Square0.SweepCounter.Step();
            Square1.SweepCounter.Step();
            Square0.LengthCounter.Step();
            Square1.LengthCounter.Step();
            Triangle.LengthCounter.Step();
            Noise.LengthCounter.Step();
        }

        //static APU()
        //{
        //    for (int i = 0; i < 31; i++)
        //        pulseTable[i] = 95.52f / (8128.0f / i + 100.0f);

        //    for (int i = 0; i < 203; i++)
        //        tndTable[i] = 163.67f / (24329.0f / i + 100.0f);
        //}

        //private static readonly float[] pulseTable = new float[31];
        //private static readonly float[] tndTable = new float[203];

    }
}
