using System;
using System.IO;
using System.Windows.Forms;
using System.Collections.Generic;

namespace NES
{
    enum SystemType { NTSC, PAL, MultiRegion, Dendy }
    class Bus
    {
        //**** MEMORY *****     
        public byte[] InternalRAM = new byte[0x2000];   // CPU 0x0000-0x2000, 8kb / (1 + 3 mirrors) = 2 kB
        public byte[] WRAM = new byte[0x2000];          // CPU 0x6000-0x8000
        public List<byte[]> PRGMemory = new List<byte[]>();      // CPU 0x8000-0xFFFF, 32kb
        public List<byte[]> ChrTable = new List<byte[]>();   // PPU 0x0000-0x2000, 8kb
        public byte[][] NameTable = new byte[4][];      // PPU 0x2000-0x3F00, 4 x 2kB chunks
        string ImageFilename;

        //public class Pattern
        //{
        //    Bus Bus;
        //    public byte this[int idx]
        //    {
        //        get { return Bus.ChrTable[idx / Bus.ChrBankSize][idx % Bus.ChrBankSize]; }
        //        set { Bus.ChrTable[idx / Bus.ChrBankSize][idx % Bus.ChrBankSize] = value; }
        //    }
        //}
        //public Pattern Chr;
        byte Timing;
        SystemType systemType;
        public SystemType SystemType
        {
            get { return systemType; }
            set {
                systemType = value;
                if (systemType == SystemType.PAL)
                {
                    PPU.ClockRatio = 3.2f;
                    PPU.MaxY = 311;
                }
                else
                {
                    PPU.ClockRatio = 3;
                    PPU.MaxY = 260;
                }
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

        public CPU CPU;
        public PPU PPU;
        public Mapper Mapper;

        public bool CPUInSleep;
        public int Cycle = 0;

        public Bus()
        {
            CPU = new CPU(this);
            PPU = new PPU(this);
            ChrTable.Add(new byte[0x2000]);
            PRGMemory.Add(new byte[0x8000]);
            //ChrBankSize = 0x1000;
            //PrgBankSize = 0x4000;
        }
        int chrBankSize;
        public int ChrBankSize
        {
            get { return chrBankSize; }
            set
            {
                chrBankSize = value;
                //PatternTable = new byte[chrBankSize][];
                //for (int i = 0; i < 0x2000 / chrBankSize; i++)
                //    PatternTable[i] = new byte[chrBankSize];
                Mapper.ChrMap.BankSize = chrBankSize;
            }
        }
        int prgBankSize;
        public int PrgBankSize
        {
            get { return prgBankSize; }
            set
            {
                prgBankSize = value;
                //PRGMemory = new byte[prgBankSize][];
                //for (int i = 0; i < 0x8000 / prgBankSize; i++)
                //    PRGMemory[i] = new byte[prgBankSize];
                Mapper.PrgMap.BankSize = prgBankSize;
            }
        }

        private readonly byte[] controller_state = new byte[2];
        public byte[] controller = new byte[2];
        public bool ImageValid;
        public void LoadCartridge(string fileName)
        {
            //Reset();
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
                //throw new Exception("Format iNES 2.0 is not supported");
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
                    Mapper = new Mapper_002();
                    break;
                case 3:
                    Mapper = new Mapper_003();
                    break;
                case 4:
                    Mapper = new Mapper_004();
                    break;
                case 7:
                    Mapper = new Mapper_007();
                    break;
                //case 66:
                //    _pMapper = new Mapper_066(nPRGBanks, nCHRBanks);
                //    break;
                default:
                    Mapper = new Mapper_007();
                    //throw new Exception("Unknown mapper. Mapper ID = [" + mapperID + "]");
                    MessageBox.Show("Unknown mapper. Mapper ID = [" + mapperID + "]"); break;
            }
            Mapper.Bus = this;
            if ((mapperFlag1 & 0x8) != 0)
                Mapper.Mirroring = VRAMmirroring.FourScreen;
            else
                Mapper.Mirroring = (mapperFlag1 & 1) != 0 ? VRAMmirroring.Vertical : VRAMmirroring.Horizontal;
            var bankSize = 0x4000;
            if (prgBanksCount > 0xEFF)
            {
                var size = (1 << (prgBanksCount >> 2 & 0x3F)) * (2 * (prgBanksCount & 3) + 1);
                prgBanksCount = size / bankSize;
            }
            Mapper.PrgROM = reader.ReadBytes(prgBanksCount * bankSize);
            bankSize = 0x2000;
            if (chrBanksCount > 0xEFF)
            {
                var size = (1 << (chrBanksCount >> 2 & 0x3F)) * (2 * (chrBanksCount & 3) + 1);
                chrBanksCount = size / bankSize;
            }
            if (chrBanksCount == 0)
                Mapper.ChrROM = new byte[chRAMSize];
            else
                Mapper.ChrROM = reader.ReadBytes(chrBanksCount * bankSize);
            Mapper.ChrRAMenabled = chrBanksCount == 0;
            reader.Close();
            fs.Close();
            Mapper.Reset();
            ImageFilename = fileName;
            ImageValid = true;
        }

        public void CpuWrite(int addr, byte data)
        {
            if (addr < 0x2000) // 2K Internal RAM
                //if (Mapper.ChrRAMenabled)
                //    PatternTable[0][addr] = data;
                //else
                    InternalRAM[addr & 0x07FF] = data; // _mirror by masking first block range
            else if (addr <= 0x3FFF) // NES PPU registers
                PPU.CpuWrite(addr & 0x0007, data); // Mirrors of $2000-2007 (repeats every 8 bytes)
            else if (addr == 0x4014)
            {
                if (CPU.ActOp.Cycle % 2 == 1)
                    CPU.ActOp.Cycle++;
                CPU.ActOp.Cycle += 513;
                // DMA
                var address = data << 8;
                if (address < 0x2000)
                {
                    address &= 0x07FF;
                    Array.Copy(InternalRAM, address, PPU.OAM, 0, PPU.OAMaddr);
                    Array.Copy(InternalRAM, address, PPU.OAM, PPU.OAMaddr, PPU.OAM.Length - PPU.OAMaddr);
                    //PPU.VRAMaddress.Latch = PPU.OAM[0xff] != 0;
                }
                else
                    for (var i = 0; i < 256; i++)
                    {
                        PPU.OAM[PPU.OAMaddr] = CpuRead(address);
                        address++;
                        PPU.OAMaddr++;
                    }
            }
            else if (addr >= 0x4016 && addr <= 0x4017)
            {
                controller_state[addr & 0x0001] = controller[addr & 0x0001];
            }
            else if (addr >= 0x6000 && addr < 0x8000)
                WRAM[addr & 0x1FFF] = data;
            else if (addr >= 0x8000)
                Mapper.CpuMapWrite(addr, data);
        }

        public byte CpuRead(int addr)
        {
            int data = 0;
            if (addr <= 0x1FFF) // 2K Internal RAM
                data = InternalRAM[addr & 0x07FF]; // _mirror by masking first block range
            else if (addr <= 0x3FFF) // NES PPU registers          
                data = PPU.CpuRead(addr & 0x0007); // Mirrors of $2000-2007 (repeats every 8 bytes)
            else if (addr == 0x4014)
                data = 0x40;
            else if (addr >= 0x4016 && addr <= 0x4017)
            {
                data = (controller_state[addr & 0x1] & 0x80) > 0 ? 1 : 0;
                controller_state[addr & 0x1] <<= 1;
            }
            else if (addr >= 0x6000 && addr < 0x8000)
                data = WRAM[addr & 0x1FFF];
            else if (addr >= 0x8000)
            {
                addr &= 0x7FFF;
                data = PRGMemory[addr / PrgBankSize][addr % PrgBankSize];
            }
            return (byte)data;
        }

        public void FrameLoop()
        {
            PPU.FrameComplete = false;
            var ppuStep = 0;
            var cycle = 0;
            while (true)
            {
                var step = Math.Min(ppuStep, CPU.ActOp.Cycle) + 1;
                CPU.ActOp.Cycle -= step - 1;
                cycle += step;
                ppuStep -= step;
                while (ppuStep < 0)
                {
                    PPU.Tick();
                    if (PPU.FrameComplete) return;
                    ppuStep = PPU.NextCycle() - cycle;
                }                
                CPU.Tick();
            }
        }

    }
}
