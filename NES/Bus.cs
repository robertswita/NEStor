using System;
using System.IO;
using System.Windows.Forms;
using System.Collections.Generic;
using NesCore.Audio;
using NAudio.Wave;
using System.Collections;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace NES
{
    enum SystemType { NTSC, PAL, MultiRegion, Dendy }
    class Bus
    {
        public CPU cpu;
        public PPU ppu;
        public APU apu;
        //ApuAudioProvider apuAudioProvider;
        BufferedWaveProvider apuAudioProvider;
        //public IWavePlayer waveOut;
        public WaveOut waveOut;
        public Mapper mapper;
        private readonly byte[] controllerState = new byte[2];
        public byte[] controller = new byte[2];
        public bool imageValid;
        string imageFilename;
        bool strobing;
        //**** MEMORY *****     
        public byte[] internalRAM = new byte[0x2000];       // CPU 0x0000-0x2000, 8kb / (1 + 3 mirrors) = 2 kB
        public byte[] wram = new byte[0x2000];              // CPU 0x6000-0x8000
        public List<byte[]> prgBanks = new List<byte[]>();  // CPU 0x8000-0xFFFF, 32kb
        public List<byte[]> chrBanks = new List<byte[]>();  // PPU 0x0000-0x2000, 8kb
        public byte[][] nameTable = new byte[4][];          // PPU 0x2000-0x3F00, 4 x 2kB chunks

        public byte GetPattern(int addr)
        {
            return chrBanks[addr / mapper.ChrMap.BankSize][addr % mapper.ChrMap.BankSize];
        }
        public void SetPattern(int addr, byte value)
        {
            chrBanks[addr / mapper.ChrMap.BankSize][addr % mapper.ChrMap.BankSize] = value;
        }

        byte Timing;
        SystemType systemType;
        public SystemType SystemType
        {
            get { return systemType; }
            set {
                systemType = value;
                if (systemType == SystemType.PAL)
                {
                    ppu.ClockRatio = 3.2f;
                    ppu.MaxY = 310;
                }
                else
                {
                    ppu.ClockRatio = 3;
                    ppu.MaxY = 260;
                }
                if (imageValid)
                {
                    var fs = new FileStream(imageFilename, FileMode.Open);
                    Timing &= 0xFC;
                    Timing |= (byte)systemType;
                    fs.Position = 12;
                    fs.WriteByte(Timing);
                    fs.Close();
                }
            }
        }

        public Bus()
        {
            cpu = new CPU(this);
            ppu = new PPU(this);
            apu = new APU(this);
            apu.Dmc.TriggerInterruptRequest = cpu.DMCIRQ;
            ConfigureAudio();
            // connect APU DMC to memory
            apu.Dmc.ReadMemorySample = (address) =>
            {
                cpu.ActOp.Cycle += 4;
                return CpuRead(address);
            };
            chrBanks.Add(new byte[0x2000]);
            prgBanks.Add(new byte[0x8000]);
        }

        private void ConfigureAudio()
        {
            apu.SampleRate = 44100;

            var outputBuffer = new short[1024];
            int writeIndex = 0;

            //apuAudioProvider = new ApuAudioProvider();
            apuAudioProvider = new BufferedWaveProvider(new WaveFormat(44100, 16, 1));
            apuAudioProvider.BufferLength = 44100;
            apuAudioProvider.DiscardOnBufferOverflow = true;
            //waveOut = new DirectSoundOut();
            //waveOut.Init(apuAudioProvider);
            //directOut.Play();

            apu.WriteSample = (sampleValue) =>
            {
                // fill buffer;
                outputBuffer[writeIndex++] = (short)(sampleValue * short.MaxValue);
                //writeIndex %= outputBuffer.Length;

                // when buffer full, send to wave provider
                if (writeIndex == outputBuffer.Length || 
                    apuAudioProvider.BufferedBytes < 0.1 * apuAudioProvider.BufferLength)
                {
                    if (writeIndex != outputBuffer.Length)
                        ;
                    byte[] buffer = new byte[writeIndex * sizeof(short)];
                    Buffer.BlockCopy(outputBuffer, 0, buffer, 0, buffer.Length);
                    //apuAudioProvider.Queue(outputBuffer);
                    apuAudioProvider.AddSamples(buffer, 0, buffer.Length);
                    writeIndex = 0;
                }

            };

            waveOut = new WaveOut();
            waveOut.DesiredLatency = 100;

            waveOut.Init(apuAudioProvider);

        }

        public void LoadCartridge(string fileName)
        {
            imageValid = false;
            if (!File.Exists(fileName)) return;
            var fs = new FileStream(fileName, FileMode.Open);
            var reader = new BinaryReader(fs);
            var name = new string(reader.ReadChars(4));
            if (name != "NES\u001A")
                throw new Exception("Unknown file format!");
            int prgBanksCount = reader.ReadByte();
            int chrBanksCount = reader.ReadByte();
            var mapperFlag1 = reader.ReadByte();
            var mapperFlag2 = reader.ReadByte();
            var mapperID = mapperFlag2 & 0xF0 | mapperFlag1 >> 4;
            var wramSize = 0;
            var chRAMSize = 0x2000;
            if ((mapperFlag2 >> 2 & 3) == 2) //Format iNES 2.0
            {
                var submapper = reader.ReadByte();
                mapperID |= submapper << 8 & 0xF00;
                submapper >>= 4;
                var MSBsizes = reader.ReadByte();
                prgBanksCount |= MSBsizes << 8 & 0xF00;
                chrBanksCount |= MSBsizes << 4 & 0xF00;
                var shiftPRG_RAMsize = reader.ReadByte();
                if (shiftPRG_RAMsize > 0)
                    wramSize = 0x40 << (shiftPRG_RAMsize & 0xF) + 0x40 << (shiftPRG_RAMsize >> 4);
                var shiftCHR_RAMsize = reader.ReadByte();
                if (shiftCHR_RAMsize > 7)
                    chRAMSize = 0x40 << (shiftCHR_RAMsize & 0xF) + 0x40 << (shiftCHR_RAMsize >> 4);
            }
            else
            {
                wramSize = 0x2000 * reader.ReadByte();
                var tv_system1 = reader.ReadByte();
                var tv_system2 = reader.ReadByte();
                //SystemType = (tv_system1 & 1) != 0 || (tv_system2 & 2) != 0 ? SystemType.PAL : SystemType.NTSC;
                //var ext = Path.GetFileNameWithoutExtension(fileName);
                //ext = Path.GetExtension(ext);
                //if (ext == ".pal") SystemType = SystemType.PAL;
                reader.ReadByte();
            }
            Timing = reader.ReadByte();
            SystemType = (SystemType)(Timing & 0x3);
            var systemType = reader.ReadByte();
            var extraROM = reader.ReadByte();
            var defaultExpansion = reader.ReadByte();
            if ((mapperFlag1 & 0x4) != 0)
                reader.ReadBytes(512);
            switch (mapperID)
            {
                case 0:
                    mapper = new Mapper_000();
                    break;
                case 1:
                    mapper = new Mapper_001();
                    break;
                case 2:
                case 71:
                    mapper = new Mapper_002();
                    break;
                case 3:
                    mapper = new Mapper_003();
                    break;
                case 4:
                case 64:
                    mapper = new Mapper_004();
                    break;
                case 5:
                    mapper = new Mapper_005();
                    break;
                case 7:
                    mapper = new Mapper_007();
                    break;
                case 11:
                    mapper = new Mapper_011();
                    break;
                case 66:
                    mapper = new Mapper_066();
                    break;
                case 118:
                    mapper = new Mapper_118();
                    break;
                case 228:
                    mapper = new Mapper_228();
                    break;
                default:
                    mapper = new Mapper_004();
                    //throw new Exception("Unknown mapper. Mapper ID = [" + mapperID + "]");
                    MessageBox.Show("Unknown mapper. Mapper ID = [" + mapperID + "]"); break;
            }
            mapper.ID = mapperID;
            mapper.Bus = this;
            if ((mapperFlag1 & 0x8) != 0)
                mapper.Mirroring = MirrorType.FourScreen;
            else
                mapper.Mirroring = (mapperFlag1 & 1) != 0 ? MirrorType.Vertical : MirrorType.Horizontal;
            var bankSize = 0x4000;
            if (prgBanksCount > 0xEFF)
            {
                var size = (1 << (prgBanksCount >> 2 & 0x3F)) * (2 * (prgBanksCount & 3) + 1);
                prgBanksCount = size / bankSize;
            }
            mapper.PrgROM = reader.ReadBytes(prgBanksCount * bankSize);
            bankSize = 0x2000;
            if (chrBanksCount > 0xEFF)
            {
                var size = (1 << (chrBanksCount >> 2 & 0x3F)) * (2 * (chrBanksCount & 3) + 1);
                chrBanksCount = size / bankSize;
            }
            if (chrBanksCount == 0)
                mapper.ChrROM = new byte[chRAMSize];
            else
                mapper.ChrROM = reader.ReadBytes(chrBanksCount * bankSize);
            mapper.ChrRAMenabled = chrBanksCount == 0;
            reader.Close();
            fs.Close();
            mapper.Reset();
            imageFilename = fileName;
            imageValid = true;
            cpu.Reset();
            //var cycles = cpu.ActOp.Cycle;
            //cpu.ActOp.Cycle = 0;
            //cpu.Tick(0);
            //cpu.Cycle = 0;
            //waveOut.Play();
        }
        void DMACopy(byte data)
        {
            cpu.ActOp.Cycle += 513;
            if (cpu.Cycle % 2 == 1)
                cpu.ActOp.Cycle++;
            // DMA
            //ppu.OAMDMAaddr = data; 

            var address = data << 8;
            if (address < 0x2000)
            {
                address &= 0x07FF;
                Array.Copy(internalRAM, address, ppu.OAM, ppu.OAMaddr, ppu.OAM.Length - ppu.OAMaddr);
                Array.Copy(internalRAM, address + ppu.OAM.Length - ppu.OAMaddr, ppu.OAM, 0, ppu.OAMaddr);
            }
            else
                for (var i = 0; i < 256; i++)
                {
                    ppu.OAM[ppu.OAMaddr] = CpuRead(address);
                    address++;
                    ppu.OAMaddr++;
                }
        }
        public void CpuWrite(int addr, byte data)
        {
            if (addr < 0x2000) // 2K Internal RAM
                internalRAM[addr & 0x7FF] = data; // mirror by masking first block range
            else if (addr < 0x4000) // PPU registers
                ppu.Poke(addr & 0x7, data); // mirrors of $2000-2007 (repeats every 8 bytes)
            else if (addr == 0x4014)
                DMACopy(data);
            else if (addr == 0x4016)
            {
                strobing = (data == 1);
                controllerState[addr & 0x1] = controller[addr & 0x1];
            }
            else if (addr <= 0x4020)
                apu.Poke(addr, data);
            else if (addr >= 0x6000 && addr < 0x8000 && mapper.PrgRAMenabled)
                wram[addr & 0x1FFF] = data;
            else
                mapper.CpuMapWrite(addr, data);
        }
        public byte CpuRead(int addr)
        {
            int data = 0;
            if (addr < 0x2000) // 2K Internal RAM
                data = internalRAM[addr & 0x07FF]; // _mirror by masking first block range
            else if (addr < 0x4000) // NES PPU registers          
                data = ppu.Peek(addr & 0x7); // Mirrors of $2000-2007 (repeats every 8 bytes)
            else if (addr == 0x4014)
                data = ppu.OpenBus;
            else if (addr == 0x4015)
                data = apu.Status;
            else if (addr == 0x4016 && addr <= 0x4017)
            {
                data = (controllerState[addr & 0x1] & 0x80) > 0 ? 1 : 0;
                if (!strobing)
                    controllerState[addr & 0x1] <<= 1;
            }
            else if (addr >= 0x6000 && addr < 0x8000 && mapper.PrgRAMenabled)
                data = wram[addr & 0x1FFF];
            else if (addr >= 0x8000)
            {
                addr &= 0x7FFF;
                data = prgBanks[addr / mapper.PrgMap.BankSize][addr % mapper.PrgMap.BankSize];
            }
            else
                data = mapper.CpuMapRead(addr);
            return (byte)data;
        }
        public void FrameLoop()
        {
            //return;
            int ppuStep = 0;
            //int apuStep = (int)apu.NextCycle - apu.Cycle;
            cpu.Cycle = 0;
            //var cpuCycle = 0;
            ppu.FrameComplete = false;
            bool evenSample = true;
            while (!ppu.FrameComplete)
            {
                while (ppuStep <= 0)
                {
                    var nextPPUcycle = ppu.Tick();
                    ppuStep = (int)nextPPUcycle - cpu.Cycle;
                    //ppuStep = (int)nextPPUcycle - cpuCycle;
                    //if (ppu.FrameComplete && nextPPUcycle != cpu.Cycle + 1)
                    if (ppu.FrameComplete && ppuStep == 0)
                        return;
                }

                //apuStep = ppuStep;
                //var step = Math.Min(ppuStep, apuStep);
                var step = ppuStep;
                //if (apuStep > 0)
                    //step = Math.Min(ppuStep, apu.Step);
                step = cpu.Tick(step);
                //cpuCycle += step;
                ppuStep -= step;

                //var apuStep = (int)apu.NextCycle - apu.Cycle;
                //var shift = 0;
                //while (apuStep < step)
                //{
                //    shift += apuStep;
                //    apu.Cycle = (int)apu.NextCycle;
                //    apu.Tick();
                //    apuStep += (int)apu.NextCycle - apu.Cycle;
                //    //apu.Cycle += apuStep;
                //}

                if (apu.Enabled)
                {
                    for (int index = 0; index < step; index++)
                    {
                        if (evenSample)
                        //if (apu.Cycle % 2 == 0)
                        {
                            apu.Square0.Step();
                            apu.Square1.Step();
                            apu.Noise.Step();
                            apu.Dmc.StepTimer();
                        }
                        apu.Triangle.Step();
                        //apu.StepTimer();
                        apu.Tick();
                        apu.Cycle++;
                        //if (apu.Pulse1.Changed)
                        //{
                        //    apu.Pulse1.lengthEnabled = apu.Pulse1.NoHalt;
                        //    apu.Pulse1.Changed = false;
                        //}
                        evenSample = !evenSample;
                    }
                    //apu.Cycle += step - shift;
                }
            }
        }

    }
}
