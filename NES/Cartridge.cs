using System;
using System.IO;

namespace NES
{
    class Cartridge
    {
        public byte[] BinHeader = new byte[16];

        private MIRROR _mirror = MIRROR.HORIZONTAL;

        private bool _bImageValid = false;

        private byte nMapperID = 0;
        private byte nPRGBanks = 0;
        private byte nCHRBanks = 0;

        private byte[] _prgMemory;
        private byte[] _chrMemory;

        private Mapper _pMapper;

        public Cartridge(string sFileName)
        {
            // iNES Format Header
            //0-3 Constant $4E $45 $53 $1A(ASCII "NES" followed by MS - DOS end - of - file)
            byte[] name = new byte[4];
            //4	Size of PRG ROM in 16 KB units
            byte[] prg_rom_chunks = new byte[1];
            //5	Size of CHR ROM in 8 KB units (value 0 means the board uses CHR RAM)
            byte[] chr_rom_chunks = new byte[1];
            //6	Flags 6 – Mapper, mirroring, battery, trainer
            byte[] mapper1 = new byte[1];
            //7	Flags 7 – Mapper, VS/Playchoice, NES 2.0
            byte[] mapper2 = new byte[1];
            //8	Flags 8 – PRG-RAM size (rarely used extension)
            byte[] prg_ram_size = new byte[1];
            //9	Flags 9 – TV system (rarely used extension)
            byte[] tv_system1 = new byte[1];
            //10 Flags 10 – TV system, PRG-RAM presence (unofficial, rarely used extension)
            byte[] tv_system2 = new byte[1];
            //11-15	Unused padding (should be filled with zero, but some rippers put their name across bytes 7-15)
            byte[] unused = new byte[5];

            _bImageValid = false;

            FileStream fs = new FileStream(sFileName, FileMode.Open);

            fs.Read(BinHeader, 0, 16);
            fs.Position = 0;

            fs.Read(name, 0, 4);
            if (name[0] != 'N' && name[1] != 'E' && name[2] != 'S' && name[3] != 0x1A)
                throw new Exception("Nieznany format pliku.");

            fs.Read(prg_rom_chunks, 0, 1);
            fs.Read(chr_rom_chunks, 0, 1);
            fs.Read(mapper1, 0, 1);
            fs.Read(mapper2, 0, 1);
            if((mapper2[0] & 0x0C) == 0x08)
                throw new Exception("Nieobsługiwany format pliku. Wykryto format NES 2.0");

            fs.Read(prg_ram_size, 0, 1);
            fs.Read(tv_system1, 0, 1);
            fs.Read(tv_system2, 0, 1);
            fs.Read(unused, 0, 5);

            UInt16 skip = 0;

            if ((mapper1[0] & 0x04) == 0x04)
            {
                skip = 512;
            }

            nMapperID = (byte)(((mapper2[0] >> 4) << 4) | (mapper1[0] >> 4));
            _mirror = ((mapper1[0] & 0x01) == 0x01) ? MIRROR.VERTICAL : MIRROR.HORIZONTAL;

            byte nFileType = 1;

            if ((mapper2[0] & 0x0C) == 0x08) nFileType = 2;

            if (nFileType == 0)
            {

            }

            // iNES Format
            if (nFileType == 1)
            {
                nPRGBanks = prg_rom_chunks[0];
                _prgMemory = new byte[nPRGBanks * 16384];
                fs.Read(_prgMemory, skip, _prgMemory.Length);

                fs.Position--;
                byte[] temp = new byte[1];
                fs.Read(temp, 0, 1);
                Console.WriteLine(temp[0].ToString("X2"));
                fs.Read(temp, 0, 1);
                Console.WriteLine(temp[0].ToString("X2"));
                fs.Read(temp, 0, 1);
                Console.WriteLine(temp[0].ToString("X2"));
                fs.Position--; fs.Position--;

                nCHRBanks = chr_rom_chunks[0];
                if (nCHRBanks == 0)
                {
                    _chrMemory = new byte[8192];
                } else
                {
                    _chrMemory = new byte[nCHRBanks * 8192];
                }
                fs.Read(_chrMemory, 0, _chrMemory.Length);
            }

            // NES 2.0 format
            if (nFileType == 2)
            {

            }

            // Load mapper
            switch (nMapperID)
            {
                case 0:
                    _pMapper = new Mapper_000(nPRGBanks, nCHRBanks);
                    break;
                case 1:
                    _pMapper = new Mapper_001(nPRGBanks, nCHRBanks);
                    break;
                case 2:
                    _pMapper = new Mapper_002(nPRGBanks, nCHRBanks);
                    break;
                case 3:
                    _pMapper = new Mapper_003(nPRGBanks, nCHRBanks);
                    break;
                case 4:
                    _pMapper = new Mapper_004(nPRGBanks, nCHRBanks);
                    break;
                case 66:
                    _pMapper = new Mapper_066(nPRGBanks, nCHRBanks);
                    break;
                default:
                    throw new Exception("Wykryto nie obsługiwany rodzaj mapper'a. nMapperID=["+ nMapperID + "]");
            }


            _bImageValid = true;
            fs.Close();
        }

        public bool ImageValid()
        {
            return _bImageValid;
        }

        // CPU <---> CARTRIDGE (PRG)
        public bool CpuRead(int addr, ref byte data)
        {
            int mapped_addr = 0;
            if (_pMapper.CpuMapRead(addr, ref mapped_addr, ref data))
            {
                if (mapped_addr != -1) // Mapper has produced an offset into cartridge bank memory
                    data = _prgMemory[mapped_addr];
                return true; // Mapper has actually set the data value, for example cartridge based RAM
            }
            return false;
        }

        // CPU <---> CARTRIDGE (PRG)
        public bool CpuWrite(int addr, byte data)
        {
            int mapped_addr = 0;
            if (_pMapper.CpuMapWrite(addr, ref mapped_addr, data))
            {
                if (mapped_addr != -1) // Mapper has produced an offset into cartridge bank memory
                    _prgMemory[mapped_addr] = data;                  
                return true; // Mapper has actually set the data value, for example cartridge based RAM
            }
            return false;
        }
        
        // PPU <---> CARTRIDGE (CHR)
        public bool PpuRead(int addr, ref byte data)
        {
            int mapped_addr = 0;
            if (_pMapper.PpuMapRead(addr, ref mapped_addr))
            {
                data = _chrMemory[(Int32)mapped_addr];
                return true;
            }
            else
            {
                return false;
            }
        }

        // PPU <---> CARTRIDGE (CHR)
        public bool PpuWrite(int addr, byte data)
        {
            int mapped_addr = 0;
            if (_pMapper.PpuMapWrite(addr, ref mapped_addr))
            {
                _chrMemory[mapped_addr] = data;
                return true;
            }
            return false;
        }

        public void Reset()
        {
            
            if (_pMapper != null)
            {
                _pMapper.Reset();
            }
        }

        public MIRROR Mirror()
        {
            MIRROR m = _pMapper.Mirror();
            if (m == MIRROR.HARDWARE)
            {
                return _mirror;
            }
            else
            {
                return m;
            }
        }
    }
}
