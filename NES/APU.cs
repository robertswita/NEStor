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
    public class APU
    {
        public delegate void WriteSampleHandler(float sampleValue);

        public APU()
        {
            Pulse1 = new PulseGenerator(1);
            Pulse2 = new PulseGenerator(2);
            Triangle = new TriangleGenerator();
            Noise = new NoiseGenerator();
            Dmc = new DmcGenerator();

            filterChain = new FilterChain();
        }

        /// <summary>
        /// Status register
        /// </summary>
        public byte Status
        {
            get
            {
                byte result = 0;
                if (Pulse1.LengthValue > 0)
                    result |= 1;
                if (Pulse2.LengthValue > 0)
                    result |= 2;
                if (Triangle.LengthValue > 0)
                    result |= 4;
                if (Noise.LengthValue > 0)
                    result |= 8;
                if (Dmc.CurrentLength > 0)
                    result |= 16;
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
                Pulse1.Enabled = (value & 1) == 1;
                Pulse2.Enabled = (value & 2) == 2;
                Triangle.Enabled = (value & 4) == 4;
                Noise.Enabled = (value & 8) == 8;
                Dmc.Enabled = (value & 16) == 16;
                
                if (!Pulse1.Enabled) 
                    Pulse1.LengthValue = 0;

                if (!Pulse2.Enabled)
                    Pulse2.LengthValue = 0;

                if (!Triangle.Enabled)
                    Triangle.LengthValue = 0;

                if (!Noise.Enabled)
                    Noise.LengthValue = 0;

                if (!Dmc.Enabled)
                {
                    Dmc.CurrentLength = 0;
                }
                else // DMC enabled
                {
                    if (Dmc.CurrentLength == 0)
                        Dmc.Restart();
                }
            }
        }

        public byte FrameCounter
        {
            set
            {
                framePeriod = 4 + ((value >> 7) & 1);
                frameIrq = ((value >> 6) & 1) == 0;

                // clock immediatly when $80 is written to frame counter port
                if (value == 0x80)
                {
                    if (Pulse1.LengthEnabled)
                        Pulse1.LengthValue = 0;

                    if (Pulse2.LengthEnabled)
                        Pulse2.LengthValue = 0;

                    if (Triangle.LengthEnabled)
                        Triangle.LengthValue = 0;

                    if (Noise.LengthEnabled)
                        Noise.LengthValue = 0;

                    Dmc.CurrentLength = 0;
                }
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
                //filterChain.Filters.Add(FirstOrderFilter.CreateHighPassFilter(value, 90f));
                //filterChain.Filters.Add(FirstOrderFilter.CreateHighPassFilter(value, 440f));
                //filterChain.Filters.Add(FirstOrderFilter.CreateLowPassFilter(value, 14000f));
            }
        }

        public WriteSampleHandler WriteSample { get; set; }

        //public Action TriggerInterruptRequest { get; set; }

        public float Output
        {
            get
            {
                var pulseIdx = Pulse1.Output + Pulse2.Output;
                var tndIdx = 3 * Triangle.Output + 2 * Noise.Output + Dmc.Output;
                return pulseTable[pulseIdx] + tndTable[tndIdx];
            }
        }

        // wave generators

        public PulseGenerator Pulse1 { get; private set; }
        public PulseGenerator Pulse2 { get; private set; }
        public TriangleGenerator Triangle { get; private set; }
        public NoiseGenerator Noise { get; private set; }
        public DmcGenerator Dmc { get; private set; }

        public void Step()
        {
            ulong lastCycle = cycle;
            ++cycle;
            ulong nextCycle = cycle;

            StepTimer();

            int lastCycleFrame = (int)(lastCycle / FrameCounterRate);
            int nextCycleFrame = (int)(nextCycle / FrameCounterRate);

            if (lastCycleFrame != nextCycleFrame)
            //if (cycle >= FrameCounterRate)
            {
                StepFrameCounter();
                //cycle = 0;
            }

            int lastCycleSample = (int)(lastCycle / SampleRate);
            int nextCycleSample = (int)(nextCycle / SampleRate);

            if (lastCycleSample != nextCycleSample)
            {
                float filteredOutput = filterChain.Apply(Output);
                WriteSample?.Invoke(filteredOutput);
            }
        }

        private void StepTimer()
        {
            if (cycle % 2 == 0)
            {
                Pulse1.StepTimer();
                Pulse2.StepTimer();
                Noise.StepTimer();
                Dmc.StepTimer();
                Triangle.StepTimer();

            }
        }

        private void StepFrameCounter()
        {
            frameValue++;
            frameValue %= framePeriod;
            if (framePeriod != 5 || frameValue != 3)
                StepEnvelope();
            if (frameValue == 1 || frameValue == framePeriod - 1)
            {
                StepSweep();
                StepLength();
            }
            if (framePeriod == 4 && frameValue == 3 && frameIrq)
                Dmc.TriggerInterruptRequest();

            //    if (framePeriod == 4)
            //{
            //    ++frameValue;
            //    frameValue %= 4;

            //    StepEnvelope();

            //    if (frameValue == 1)
            //    {
            //        StepSweep();
            //        StepLength();
            //    }
            //    else if (frameValue == 3)
            //    {
            //        StepSweep();
            //        StepLength();
            //        if (frameIrq)
            //            TriggerInterruptRequest?.Invoke();
            //    }
            //} else if (framePeriod == 5)
            //{
            //    ++frameValue;
            //    frameValue %= 5;

            //    if (frameValue != 4)
            //        StepEnvelope();

            //    //if (frameValue == 0 || frameValue == 2)
            //    if (frameValue == 1 || frameValue == 4)
            //    {
            //        StepSweep();
            //        StepLength();
            //    }
            //}
        }

        private void StepEnvelope()
        {
            Pulse1.StepEnvelope();
            Pulse2.StepEnvelope();
            Triangle.StepCounter();
            Noise.StepEnvelope();
        }

        private void StepSweep()
        {
            Pulse1.StepSweep();
            Pulse2.StepSweep();
        }

        private void StepLength()
        {
            Pulse1.StepLength();
            Pulse2.StepLength();
            Triangle.StepLength();
            Noise.StepLength();
        }

        private float sampleRate;
        private ulong cycle;
        private int framePeriod = 4;
        private int frameValue;
        private bool frameIrq;
        private FilterChain filterChain;

        /// <summary>
        /// Builds the pulse and triangle/noise/dmc (tnd) tables
        /// </summary>
        static APU()
        {
            for (int i = 0; i < 31; i++)
                pulseTable[i] = 95.52f / (8128.0f / i + 100);

            for (int i = 0; i < 203; i++)
                tndTable[i] = 163.67f / (24329.0f / i + 100);
        }

        private static readonly float[] pulseTable = new float[31];
        private static readonly float[] tndTable = new float[203];

        //private const uint CpuFrequency = 1789773;
        private const float CpuFrequency = 1789772.7272f;
        private const float FrameCounterRate = CpuFrequency / 240;

        public void SaveState(BinaryWriter binaryWriter)
        {
            binaryWriter.Write(cycle);
            binaryWriter.Write(framePeriod);
            binaryWriter.Write(frameValue);
            binaryWriter.Write(frameIrq);

            Pulse1.SaveState(binaryWriter);
            Pulse2.SaveState(binaryWriter);
            Triangle.SaveState(binaryWriter);
            Noise.SaveState(binaryWriter);
            Dmc.SaveState(binaryWriter);
        }

        public void LoadState(BinaryReader binaryReader)
        {
            cycle = binaryReader.ReadUInt64();
            framePeriod = binaryReader.ReadByte();
            frameValue = binaryReader.ReadByte();
            frameIrq = binaryReader.ReadBoolean();

            Pulse1.LoadState(binaryReader);
            Pulse2.LoadState(binaryReader);
            Triangle.LoadState(binaryReader);
            Noise.LoadState(binaryReader);
            Dmc.LoadState(binaryReader);
        }
    }
}
