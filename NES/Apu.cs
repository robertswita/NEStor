using NES;
using NesCore.Audio.Filtering;
using NesCore.Audio.Generators;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NesCore.Audio
{
    class APU
    {
        // wave generators
        public PulseWave Square0 { get; private set; }
        public PulseWave Square1 { get; private set; }
        public TriangleWave Triangle { get; private set; }
        public Noise Noise { get; private set; }
        public DmcGenerator Dmc { get; private set; }
        public delegate void WriteSampleHandler(float sampleValue);
        public Bus Bus;
        private float sampleRate;
        bool InhibitIRQ;
        private bool frameIrq;
        bool DmcInterruptRequestEnabled;
        private FilterChain filterChain;
        public float NextCycle;
        //static int[] FrameDelays = new int[] { 2, 1, 2, 3, -2 };
        public static int[] LengthCycles = new int[] { 7457, 14913, 22371, 29828, 29829, 29830, 37281 };
        int lengthCycle;
        public int Cycle;// = LengthCycles[4];
        public int LastCycleSample;// = 29800;
        private int framePeriod = 4;
        //int Jitter;
        public int Step;
        bool enabled;
        public bool Enabled
        {
            get { return enabled; }
            set
            {
                enabled = value;
                if (enabled)
                    Bus.waveOut.Play();
                else
                    Bus.waveOut.Stop();
            }
        }

        public APU(Bus bus)
        {
            Bus = bus;
            Square0 = new PulseWave(1);
            Square1 = new PulseWave(2);
            Triangle = new TriangleWave();
            Noise = new Noise();
            Dmc = new DmcGenerator();
            Square0.APU = this;
            Square1.APU = this;
            Triangle.APU = this;
            Noise.APU = this;
            Dmc.APU = this;

            filterChain = new FilterChain();
            FrameCounter = 0;
            //Step = (int)SampleRate - 2;
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
                FrameCounter = data;
        }

        /// <summary>
        /// Status register
        /// </summary>
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
                if (Dmc.CurrentLength > 0)
                    result |= 0x10;
                if (frameIrq)
                    result |= 0x40;
                //if (DmcInterruptRequestEnabled)
                if (Bus.cpu.dmcirq)
                    result |= 0x80;
                //if (framePeriod == 4)
                //if (Cycle < LengthCycles[3] || Cycle > LengthCycles[5])
                if (Cycle > 0)
                    frameIrq = false;
                //InhibitIRQ = true;
                //Bus.cpu.irq = false;
                //DmcInterruptRequestEnabled = Dmc.InterruptRequestEnabled;
                return result;
            }
        }

        /// <summary>
        /// Control register
        /// </summary>
        public byte Control
        {
            set
            {
                Bus.cpu.dmcirq = false;
                Square0.Enabled = (value & 1) != 0;
                Square1.Enabled = (value & 2) != 0;
                Triangle.Enabled = (value & 4) != 0;
                Noise.Enabled = (value & 8) != 0;
                Dmc.Enabled = (value & 0x10) != 0;
                if (Dmc.Enabled)
                {
                    if (Dmc.CurrentLength == 0)
                    {
                        Dmc.Restart();
                    }
                }
                else
                {
                    Dmc.CurrentLength = 0;
                }
            }
        }

        public byte FrameCounter
        {
            set
            {
                framePeriod = 4 + ((value >> 7) & 1);
                InhibitIRQ = ((value >> 6) & 1) != 0;
                if (InhibitIRQ)
                {
                    frameIrq = false;
                    //Bus.cpu.IRQList.Clear();
                    //Bus.cpu.irq = false;
                }

                // clock immediatly when $80 is written to frame counter port
                //if (value == 0x80)
                //{

                //}
                if (framePeriod == 5)
                {
                    StepSequence();
                    //if (Pulse1.LengthEnabled)
                    //    Pulse1.LengthValue = 0;

                    //if (Pulse2.LengthEnabled)
                    //    Pulse2.LengthValue = 0;

                    //if (Triangle.LengthEnabled)
                    //    Triangle.LengthValue = 0;

                    //if (Noise.LengthEnabled)
                    //    Noise.LengthValue = 0;

                    //Dmc.CurrentLength = 0;
                }

                //var ppuCycles = Bus.ppu.TotalCycles();
                //var odd = Cycle == 0;
                if (Cycle % 2 == 1)
                    Cycle = -3;
                else
                    Cycle = -2;
                //if (ppuCycles != (int)ppuCycles)
                //    Cycle--;
                //Jitter = 1;



                lengthCycle = 1;
                LastCycleSample = 0;
                NextCycle = (int)SampleRate;
            }
        }

        /// <summary>
        /// Sets the supported sample rate and configures the pass filters accordingly
        /// </summary>
        public float SampleRate
        {
            get { return sampleRate; }
            set
            {
                sampleRate = CpuFrequency / value;

                filterChain.Filters.Clear();
                filterChain.Filters.Add(FirstOrderFilter.CreateHighPassFilter(value, 90f));
                filterChain.Filters.Add(FirstOrderFilter.CreateHighPassFilter(value, 440f));
                filterChain.Filters.Add(FirstOrderFilter.CreateLowPassFilter(value, 14000f));
            }
        }

        public WriteSampleHandler WriteSample { get; set; }
        public Action TriggerInterruptRequest { get; set; }

        public float Output
        {
            get
            {
                byte pulseOutput1 = Square0.Output;
                byte pulseOutput2 = Square1.Output;
                float pulseOutput = pulseTable[pulseOutput1 + pulseOutput2];

                byte triangleOutput = Triangle.Output;
                byte noiseOutput = Noise.Output;
                byte dmcOutput = Dmc.Output;
                float tndOutput = tndTable[3 * triangleOutput + 2 * noiseOutput + dmcOutput];

                return pulseOutput + tndOutput;
            }
        }

        //public void Step()
        //{
        //    ulong lastCycle = cycle;
        //    ++cycle;
        //    ulong nextCycle = cycle;

        //    StepTimer();

        //    int lastCycleFrame = (int)((double)lastCycle / FrameCounterRate);
        //    int nextCycleFrame = (int)((double)nextCycle / FrameCounterRate);

        //    if (lastCycleFrame != nextCycleFrame)
        //        StepFrameCounter();

        //    int lastCycleSample = (int)((double)lastCycle / SampleRate);
        //    int nextCycleSample = (int)((double)nextCycle / SampleRate);

        //    if (lastCycleSample != nextCycleSample)
        //    {
        //        float filteredOutput = filterChain.Apply(Output);
        //        WriteSample?.Invoke(filteredOutput);
        //    }
        //}

        public void Tick()
        {
            //int lastCycle = Cycle;
            //int nextCycle = Cycle + 1;
            //StepTimer();

            //int lastCycleFrame = (int)(lastCycle / FrameCounterRate);
            //int nextCycleFrame = (int)(nextCycle / FrameCounterRate);

            //var frameCycleCount = 7457f;
            //if (lastCycleFrame != nextCycleFrame)
            //if (cycle >= FrameCounterRate)
            //if (Cycle % QuadFrameCycleCount == 0)
            //var frameCycleCount = (int)(CyclesCount / framePeriod);
            //var frameCycleCount = 7457.3875f;

            //if (Cycle == (int)((frameValue + 1) * frameCycleCount) + FrameDelays[frameValue])
            //{
            //    StepFrameCounter();
            //    if (frameValue == 0)
            //    {
            //        //Cycle = framePeriod == 4 ? 0 : 0;
            //        Cycle = 0;
            //        LastCycleSample = Cycle / (int)SampleRate * (int)SampleRate;
            //    }
            //}
            //NextCycle = (frameValue + 1) * frameCycleCount + FrameDelays[frameValue];

            if (Cycle == LengthCycles[lengthCycle])
                StepFrameCounter();
            //NextCycle = LengthCycles[lengthCycle];

            //if (Cycle >= framePeriod * QuadFrameCycleCount)
            //if (Cycle >= CyclesCount - 1)

            //NextCycle = (Cycle / QuadFrameCycleCount + 1) * QuadFrameCycleCount;


            //int lastCycleSample = (int)(lastCycle / SampleRate);
            //int nextCycleSample = (int)(nextCycle / SampleRate);
            //if (lastCycleSample != nextCycleSample)
            if (Cycle % (int)SampleRate == 0)
            {
                //float filteredOutput = filterChain.Apply(Output);
                WriteSample?.Invoke(Output);
                LastCycleSample = Cycle;
            }
            //NextCycle = Math.Min(NextCycle, LastCycleSample + (int)SampleRate);
            //Step -= step;
            //Cycle++;

        }

        public void StepTimer()
        {
            //if (IsLatched)
            //{
            //    Poke(LatchAddr, LatchData);
            //    IsLatched = false;
            //}
            if (Cycle % 2 == 0)
            {
                Square0.Step();
                Square1.Step();
                Noise.Step();
                Dmc.StepTimer();
            }
            Triangle.Step();
            if (Cycle == LengthCycles[lengthCycle])
                StepFrameCounter();
            if (Cycle % (int)SampleRate == 0)
            {
                float filteredOutput = filterChain.Apply(Output);
                WriteSample?.Invoke(filteredOutput);
                LastCycleSample = Cycle;
            }
            Cycle++;
        }

        void StepSequence()
        {
            StepEnvelope();
            StepLength();
        }

        private void StepFrameCounter()
        {
            if (lengthCycle == 0 || lengthCycle == 2)
                StepEnvelope();
            else if (lengthCycle == 1)
                StepSequence();
            else if (lengthCycle < 6 && framePeriod == 4)
            {
                if (!InhibitIRQ)
                {
                    frameIrq = true;
                    if (lengthCycle == 3)
                    {
                        Bus.cpu.IRQDelay = 2;
                        Bus.cpu.IRQ();
                    }
                }
                if (lengthCycle == 4)
                    StepSequence();
            }
            else if (lengthCycle == 6 && framePeriod == 5)
                StepSequence();
            lengthCycle++;
            lengthCycle %= 7;
            if (lengthCycle == 0)
            {
                Cycle = -1;
                if (framePeriod == 4)
                {
                    Cycle = LengthCycles[0] - 6;
                    //if (frameIrq)
                    //{
                    //    //Bus.cpu.IRQDelay = 1;
                    //    Bus.cpu.IRQ();
                    //    //frameIrq = false;
                    //}
                }
                LastCycleSample = Cycle / (int)SampleRate * (int)SampleRate;
            }

            //if (lengthCycle == 3)
            //    FrameIRQ();
            //else if (framePeriod == 4 && lengthCycle == 5 || lengthCycle == 6)
            //{
            //    FrameIRQ();
            //    Cycle = 0;
            //    LastCycleSample = 0;
            //}
            //else
            //{
            //    if (framePeriod != 5 || frameValue != 3)
            //        //if (Cycle == 7459 || Cycle == 22373)
            //        StepEnvelope();
            //    if (frameValue == 1 || frameValue == framePeriod - 1)
            //    //if (Cycle == 12642 || Cycle == 29829)
            //    {
            //        StepLength();
            //        StepSweep();
            //    }
            //    //if (framePeriod == 4 && frameValue == 3 && frameIrq)
            //    ////if (Cycle == 29829)
            //    //{
            //    //    //Bus.cpu.IRQDelay = 4;
            //    //    Bus.cpu.IRQ();
            //    //    //Cycle = 0;
            //    //    //frameIrq = true;
            //    //    //Dmc.TriggerInterruptRequest();
            //    //}
            //    frameValue++;
            //    frameValue %= framePeriod;
            //}

        }

        private void StepEnvelope()
        {
            Square0.Envelope.Step();
            Square1.Envelope.Step();
            Triangle.LinearCounter.Step();
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

        /// <summary>
        /// Builds the pulse and triangle/noise/dmc (tnd) tables
        /// </summary>
        static APU()
        {
            for (int i = 0; i < 31; i++)
                pulseTable[i] = 95.52f / (8128.0f / i + 100.0f);

            for (int i = 0; i < 203; i++)
                tndTable[i] = 163.67f / (24329.0f / i + 100.0f);
        }

        private static readonly float[] pulseTable = new float[31];
        private static readonly float[] tndTable = new float[203];

        private const uint CpuFrequency = 1789773;
        private const double FrameCounterRate = CpuFrequency / 240.0;
    }
}
