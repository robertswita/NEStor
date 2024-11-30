﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Collections;
using System.Windows.Forms;

namespace NES
{
    enum SystemType { NTSC, PAL, MultiRegion, Dendy }
    class Bus
    {
        public struct ModelParams
        {
            public float PpuClockRatio;
            public static int PpuMaxX = 339;
            public int PpuMaxY;
            //public float PPUcycleCount
            //{
            //    get { return (PPUmaxY + 2) * (PPUmaxX + 2) / PPUclockRatio; }
            //}
            public int CpuFrequency;
            public int[] ApuFrameCycles;
            public int[] DmcPeriods;
            public int[] NoisePeriods;
        }
        public CPU Cpu;
        public PPU Ppu;
        public APU Apu;
        public Mapper Mapper;
        byte[] ControllerState = new byte[2];
        public byte[] Controller = new byte[2];
        public bool ImageValid;
        string ImageFilename;
        bool Strobing;
        byte OpenBus;
        //**** MEMORY *****     
        public byte[] InternalRam = new byte[0x2000];    // CPU 0x0000-0x2000, 8kb / (1 + 3 mirrors) = 2 kB
        public byte[] WRam = new byte[0x2000];           // CPU 0x6000-0x8000
        public BankMemory PrgMemory = new BankMemory();  // CPU 0x8000-0xFFFF, 32kb
        public BankMemory ChrMemory = new BankMemory();  // PPU 0x0000-0x2000, 8kb
        public BankMemory NameTables = new BankMemory(); // PPU 0x2000-0x3F00, 4 x 1kB chunks

        byte Timing;
        static ModelParams NTSC;
        static ModelParams PAL;

        static Bus()
        {
            NTSC = new ModelParams();
            NTSC.PpuClockRatio = 3;
            NTSC.PpuMaxY = 260;
            NTSC.CpuFrequency = 1789773;
            var qfc = NTSC.CpuFrequency / 60 / 4;
            var cycles = new int[] { qfc, qfc - 1, qfc + 1, qfc, 1, 1, qfc - 6, 1 };
            for (int i = 1; i < cycles.Length; i++)
                cycles[i] += cycles[i - 1];
            NTSC.ApuFrameCycles = cycles;
            NTSC.DmcPeriods = new int[] { 428, 380, 340, 320, 286, 254, 226, 214, 190, 160, 142, 128, 106, 84, 72, 54 };
            NTSC.NoisePeriods = new int[] { 4, 8, 16, 32, 64, 96, 128, 160, 202, 254, 380, 508, 762, 1016, 2034, 4068 };

            PAL = new ModelParams();
            PAL.PpuClockRatio = 3.2f;
            PAL.PpuMaxY = 310;
            PAL.CpuFrequency = 1662607;
            qfc = PAL.CpuFrequency / 50 / 4;
            cycles = new int[] { qfc, qfc + 1, qfc - 1, qfc, 1, 1, qfc - 2, 1 };
            for (int i = 1; i < cycles.Length; i++)
                cycles[i] += cycles[i - 1];
            PAL.ApuFrameCycles = cycles;
            PAL.DmcPeriods = new int[] { 398, 354, 316, 298, 276, 236, 210, 198, 176, 148, 132, 118, 98, 78, 66, 50 };
            PAL.NoisePeriods = new int[] { 4, 7, 14, 30, 60, 88, 118, 148, 188, 236, 354, 472, 708, 944, 1890, 3778 };
        }
        void LoadModel(ModelParams modelParams)
        {
            Ppu.ClockRatio = modelParams.PpuClockRatio;
            Ppu.MaxY = modelParams.PpuMaxY;
            Cpu.Frequency = modelParams.CpuFrequency;
            Apu.FrameCycles = modelParams.ApuFrameCycles;
            Apu.Dmc.Periods = modelParams.DmcPeriods;
            Apu.Noise.Periods = modelParams.NoisePeriods;
        }
        SystemType systemType;
        public SystemType SystemType
        {
            get { return systemType; }
            set {
                systemType = value;
                if (systemType == SystemType.PAL)
                    LoadModel(PAL);
                else
                    LoadModel(NTSC);
                if (ImageValid)
                {
                    var fs = new FileStream(ImageFilename, FileMode.Open);
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
            Cpu = new CPU(this);
            Ppu = new PPU(this);
            Apu = new APU(this);
            NameTables.Banks.Add(new byte[0x1000]);
            NameTables.SwapBanks.Add(new byte[0x1000]);
            NameTables.BankSize = 0x400;
            ChrMemory.Banks.Add(new byte[0x2000]);
            PrgMemory.Banks.Add(new byte[0x8000]);
        }   

        public void LoadCartridge(string fileName)
        {
            ImageValid = false;
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
                    Mapper = new Mapper_000();
                    break;
                case 1:
                    Mapper = new Mapper_001();
                    break;
                case 2:
                case 71:
                    Mapper = new Mapper_002();
                    break;
                case 3:
                    Mapper = new Mapper_003();
                    break;
                case 4:
                case 64:
                    Mapper = new Mapper_004();
                    break;
                case 5:
                    Mapper = new Mapper_005();
                    break;
                case 7:
                    Mapper = new Mapper_007();
                    break;
                case 11:
                    Mapper = new Mapper_011();
                    break;
                case 66:
                    Mapper = new Mapper_066();
                    break;
                case 118:
                    Mapper = new Mapper_118();
                    break;
                case 228:
                    Mapper = new Mapper_228();
                    break;
                default:
                    Mapper = new Mapper_004();
                    //throw new Exception("Unknown mapper. Mapper ID = [" + mapperID + "]");
                    MessageBox.Show("Unknown mapper. Mapper ID = [" + mapperID + "]"); break;
            }
            Mapper.ID = mapperID;
            Mapper.Bus = this;
            if ((mapperFlag1 & 0x8) != 0)
                Mapper.Mirroring = MirrorType.FourScreen;
            else
                Mapper.Mirroring = (mapperFlag1 & 1) != 0 ? MirrorType.Vertical : MirrorType.Horizontal;
            
            var bankSize = 0x4000;
            if (prgBanksCount >= 0xF00)
            {
                var size = (1 << (prgBanksCount >> 2 & 0x3F)) * (2 * (prgBanksCount & 3) + 1);
                prgBanksCount = size / bankSize;
            }
            var buffer = reader.ReadBytes(prgBanksCount * bankSize);
            PrgMemory.SwapBanks.Add(buffer);
            PrgMemory.BankSize = bankSize;

            bankSize = 0x2000;
            if (chrBanksCount >= 0xF00)
            {
                var size = (1 << (chrBanksCount >> 2 & 0x3F)) * (2 * (chrBanksCount & 3) + 1);
                chrBanksCount = size / bankSize;
            }
            Mapper.ChrRAMenabled = chrBanksCount == 0;
            buffer = Mapper.ChrRAMenabled ? new byte[chRAMSize] : reader.ReadBytes(chrBanksCount * bankSize);
            ChrMemory.SwapBanks.Add(buffer);
            ChrMemory.BankSize = bankSize;
            reader.Close();
            fs.Close();
            Mapper.Reset();
            ImageFilename = fileName;
            ImageValid = true;
            Cpu.Reset();
        }
        void DMACopy(byte data)
        {
            Cpu.ActOp.Cycle += 513;
            if (Cpu.Cycle % 2 == 1)
                Cpu.ActOp.Cycle++;
            //ppu.OAMDMAaddr = data; 

            var address = data << 8;
            if (address < 0x2000)
            {
                address &= 0x07FF;
                Array.Copy(InternalRam, address, Ppu.OAM, Ppu.OAMaddr, Ppu.OAM.Length - Ppu.OAMaddr);
                Array.Copy(InternalRam, address + Ppu.OAM.Length - Ppu.OAMaddr, Ppu.OAM, 0, Ppu.OAMaddr);
            }
            else
                for (var i = 0; i < 256; i++)
                {
                    Ppu.OAM[Ppu.OAMaddr] = Peek(address);
                    address++;
                    Ppu.OAMaddr++;
                }
        }

        public void Poke(int addr, byte data)
        {
            OpenBus = data;
            if (addr < 0x2000) // 2K Internal RAM
                InternalRam[addr & 0x7FF] = data; // mirror by masking first block range
            else if (addr < 0x4000)
                Ppu.Poke(addr & 0x7, data); // mirrors of $2000-2007 PPU registers (repeats every 8 bytes)
            else if (addr == 0x4014)
                DMACopy(data);
            else if (addr == 0x4016)
            {
                Strobing = data == 1;
                ControllerState[addr & 0x1] = Controller[addr & 0x1];
            }
            else if (addr <= 0x4020)
                Apu.Poke(addr, data);
            else if (addr >= 0x6000 && addr < 0x8000 && Mapper.PrgRAMenabled)
                WRam[addr & 0x1FFF] = data;
            else
                Mapper.Poke(addr, data);
        }
        public byte Peek(int addr)
        {
            int data = OpenBus;
            if (addr < 0x2000) // 2K Internal RAM
                data = InternalRam[addr & 0x07FF]; // _mirror by masking first block range
            else if (addr < 0x4000) // NES PPU registers          
                data = Ppu.Peek(addr & 0x7); // Mirrors of $2000-2007 (repeats every 8 bytes)
            else if (addr == 0x4015)
                data = Apu.Status;
            else if (addr == 0x4016 || addr == 0x4017)
            {
                data = (ControllerState[addr & 0x1] & 0x80) >> 7;
                data |= OpenBus & ~0x1F;
                if (!Strobing)
                    ControllerState[addr & 0x1] <<= 1;
            }
            else if (addr <= 0x40FF)
                data = OpenBus;
            else if (addr >= 0x6000 && addr < 0x8000 && Mapper.PrgRAMenabled)
                data = WRam[addr & 0x1FFF];
            else if (addr >= 0x8000)
                data = PrgMemory[addr & 0x7FFF];
            else
                data = Mapper.Peek(addr);
            OpenBus = (byte)data;
            return (byte)data;
        }
        public void FrameLoop()
        {
            int ppuStep = 0;
            Cpu.Cycle = 0;
            Ppu.FrameComplete = false;
            while (!Ppu.FrameComplete)
            {
                while (ppuStep <= 0)
                {
                    var nextPPUcycle = Ppu.Step();
                    ppuStep = (int)nextPPUcycle - Cpu.Cycle;
                    if (Ppu.FrameComplete && ppuStep == 0)
                        return;
                }
                var step = Cpu.Step(ppuStep);
                ppuStep -= step;
                if (Apu.Enabled)
                    for (int index = 0; index < step; index++)
                        Apu.Step();
            }
        }

    }
}