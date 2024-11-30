using GitHub.secile.Audio;
using NesCore.Audio.Filtering;
using NesCore.Audio.Generators;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace NES
{
    class APU
    {
        public Pulse Square0;
        public Pulse Square1;
        public TriangleWave Triangle;
        public Noise Noise;
        public DmcGenerator Dmc;
        public Bus Bus;
        public float SampleRate;
        bool InhibitIRQ;
        bool FrameIrqEnabled;
        bool DmcIrqEnabled;
        FilterChain FilterChain;
        public int[] FrameCycles;
        int FrameCycleIdx;
        public int Cycle;
        int FramePeriod = 4;
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
            Square0 = new Pulse(1);
            Square1 = new Pulse(2);
            Triangle = new TriangleWave();
            Noise = new Noise();
            Dmc = new DmcGenerator();
            Square0.Apu = this;
            Square1.Apu = this;
            Triangle.Apu = this;
            Noise.Apu = this;
            Dmc.Apu = this;
            FilterChain = new FilterChain();
            FrameCounter = 0;
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
            var sample = (short)(sampleValue * short.MaxValue);
            OutputBuffer[OutputLength++] = (byte)(sample & 0xFF);
            OutputBuffer[OutputLength++] = (byte)(sample >> 8 & 0xFF);
            if (OutputLength == OutputBuffer.Length)
            {
                //Flush();
                WaveOut.Write(OutputBuffer);
                OutputLength = 0;
            }
        }

        //public void Flush()
        //{
        //    byte[] buffer = new byte[OutputLength];
        //    Buffer.BlockCopy(OutputBuffer, 0, buffer, 0, buffer.Length);
        //    OutputLength = 0;
        //    waveOut.Write(buffer);
        //}

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
                FrameCounter = data;
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
                if (Dmc.BufSize > 0)
                    result |= 0x10;
                if (FrameIrqEnabled)
                    result |= 0x40;
                if (Bus.Cpu.DmcIrq)
                    result |= 0x80;
                if (Cycle > 0)
                    FrameIrqEnabled = false;
                return result;
            }
        }

        public byte Control
        {
            set
            {
                Bus.Cpu.DmcIrq = false;
                Square0.Enabled = (value & 1) != 0;
                Square1.Enabled = (value & 2) != 0;
                Triangle.Enabled = (value & 4) != 0;
                Noise.Enabled = (value & 8) != 0;
                Dmc.Enabled = (value & 0x10) != 0;
                if (!Dmc.Enabled)
                    Dmc.BufSize = 0;
                else if (Dmc.BufSize == 0)
                    Dmc.Restart();
            }
        }

        public byte FrameCounter
        {
            set
            {
                FramePeriod = 4 + ((value >> 7) & 1);
                InhibitIRQ = ((value >> 6) & 1) != 0;
                if (InhibitIRQ)
                    FrameIrqEnabled = false;
                if (FramePeriod == 5)
                {
                    StepEnvelope();
                    StepLength();
                }
                if (Cycle % 2 == 1)
                    Cycle = -3;
                else
                    Cycle = -2;
                FrameCycleIdx = 1;
            }
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

        public float Output
        {
            get
            {
                float pulseOutput = (Square0.Volume + Square1.Volume) / 8128f;
                pulseOutput = 95.88f * pulseOutput / (1 + pulseOutput * 100);
                //float tndOutput = tndTable[3 * triangleOutput + 2 * noiseOutput + dmcOutput];
                float tndOutput = Triangle.Volume / 8227f + Noise.Volume / 12241f + Dmc.Output / 22638f;
                tndOutput = 159.79f * tndOutput / (1 + tndOutput * 100);
                return pulseOutput + tndOutput;
            }
        }

        public void Step()
        {
            if (Cycle % 2 == 0)
            {
                Square0.Step();
                Square1.Step();
                Noise.Step();
                Dmc.Step();
            }
            Triangle.Step();
            if (Cycle == FrameCycles[FrameCycleIdx])
                StepFrameCounter();
            if (Cycle % SampleRate == 0)
            {
                float filteredOutput = FilterChain.Apply(Output);
                WriteSample(filteredOutput);
            }
            Cycle++;
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

        private void StepFrameCounter()
        {
            if (FrameCycleIdx == 0 || FrameCycleIdx == 2)
                StepEnvelope();
            else if (IsLengthCycle)
            {
                StepEnvelope();
                StepLength();
            }
            if (FrameCycleIdx >= 3 && FrameCycleIdx <= 5 && FramePeriod == 4 && !InhibitIRQ)
            {
                FrameIrqEnabled = true;
                if (FrameCycleIdx == 3)
                {
                    Bus.Cpu.IRQDelay = 2;
                    Bus.Cpu.Irq = true;
                }
            }
            FrameCycleIdx++;
            FrameCycleIdx %= FramePeriod == 4 ?  6 : 8;
            if (FrameCycleIdx == 0)
                Cycle = 0;
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
            Square0.LengthCounter.Step();
            Square0.SweepCounter.Step();
            Square1.LengthCounter.Step();
            Square1.SweepCounter.Step();
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
