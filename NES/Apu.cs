using GitHub.secile.Audio;
using NesCore.Audio.Filtering;
using NEStor.Audio;
using System;
using System.Collections.Generic;
using System.Text;


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
        public float sampleRate;
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
        public float SampleRate;

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
            Square0 = new SquareWave(this);
            Square1 = new SquareWave(this);
            Triangle = new TriangleWave(this);
            Noise = new Noise(this);
            Dmc = new Dmc();
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
            var sample = (int)(sampleValue * ushort.MaxValue);
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
                if (Square0.Length.IsActive)
                    result |= 1;
                if (Square1.Length.IsActive)
                    result |= 2;
                if (Triangle.Length.IsActive)
                    result |= 4;
                if (Noise.Length.IsActive)
                    result |= 8;
                if (Dmc.ByteCount > 0)
                    result |= 0x10;
                if (Bus.Cpu.ApuIrq.Enabled)
                    result |= 0x40;
                if (Bus.Cpu.DmcIrq.Enabled)
                    result |= 0x80;
                Bus.Cpu.ApuIrq.Acknowledge();
                return result;
            }
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
            }
        }

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
        //        sampleRate = value;
        //        var freq = sampleRate * ResampleFrequency;
        //        FilterChain.Filters.Clear();
        //        FilterChain.Filters.Add(FirstOrderFilter.CreateHighPassFilter(freq, 90f));
        //        FilterChain.Filters.Add(FirstOrderFilter.CreateHighPassFilter(freq, 440f));
        //        FilterChain.Filters.Add(FirstOrderFilter.CreateLowPassFilter(freq, 14000f));
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
                //if (!Dmc.IsSilenced) tndOutput += Dmc.Volume / 226.38f;
                tndOutput = 1.5979f * tndOutput / (1 + tndOutput);
                return pulseOutput + 0 * tndOutput - 0.5f;
            }
        }

        float NextSampleCycle;
        public void Step()
        {
            if (Enabled)
            {
                if ((Cycle & 1) == 0)
                {
                    Square0.Step();
                    Square1.Step();
                    Noise.Step();
                    Dmc.Step();
                }
                Dmc.CheckDma();
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
                Bus.Cpu.ApuIrq.Start(1);
            FrameCycleIdx++;
            FrameCycleIdx %= FramePeriod == 4 ? 6 : 8;
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
            Square0.Sweep.Step();
            Square1.Sweep.Step();
            Square0.Length.Step();
            Square1.Length.Step();
            Triangle.Length.Step();
            Noise.Length.Step();
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
