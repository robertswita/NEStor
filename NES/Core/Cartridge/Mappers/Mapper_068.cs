using NES;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NEStor.Core.Cartridge.Mappers
{
    internal class Mapper_068 : Mapper
    {
        int[] _ntRegs = new int[2];
        bool _useChrForNametables;
        bool _prgRamEnabled;
        int _licensingTimer;
        bool _usingExternalRom;
        int _externalPage;
        MirrorType DefaultMirroring;

        void UpdateNametables()
        {
            if (_useChrForNametables)
            {
                //for (int i = 0; i < 4; i++)
                //{
                //    int reg = 0;
                //    switch (Mirroring)
                //    {
                //        case MirrorType.FourScreens: break; //4-screen mirroring is not supported by this mapper
                //        case MirrorType.Vertical: reg = i & 0x01; break;
                //        case MirrorType.Horizontal: reg = (i & 0x02) >> 1; break;
                //        case MirrorType.OneScreenLo: reg = 0; break;
                //        case MirrorType.OneScreenHi: reg = 1; break;
                //    }

                //    //SetPpuMemoryMapping(0x2000 + i * 0x400, 0x2000 + i * 0x400 + 0x3FF, ChrMemoryType.Default, _ntRegs[reg] * 0x400, _chrRamSize > 0 ? MemoryAccessType.ReadWrite : MemoryAccessType.Read);
                //    Bus.NameTables.Banks[i] = (byte[])Bus.ChrMemory.SwapBanks[_ntRegs[reg]].Clone();
                //}
                Bus.NameTables.SwapBanks[0] = (byte[])Bus.ChrMemory.SwapBanks[_ntRegs[0]].Clone();
                Bus.NameTables.SwapBanks[1] = (byte[])Bus.ChrMemory.SwapBanks[_ntRegs[1]].Clone();
            }
            Mirroring = DefaultMirroring;
        }

        public override void Reset()
        {
            Bus.ChrMemory.BankSize = 0x400;
            //ChrRamEnabled = true;
            //Bank 0's initial state is undefined, but some roms expect it to be the first page
            Bus.PrgMemory.Swap(0, 0);
            Bus.PrgMemory.Swap(1, 7);
            DefaultMirroring = Mirroring;
            UpdateState();
        }

        //void Serialize(Serializer& s) override
        //{
        //    BaseMapper.Serialize(s);

        //    SV(_ntRegs[0]);
        //    SV(_ntRegs[1]);
        //    SV(_useChrForNametables);
        //    SV(_prgRamEnabled);
        //    SV(_usingExternalRom);
        //    SV(_externalPage);
        //}

        void UpdateState()
        {
            //MemoryAccessType access = _prgRamEnabled ? MemoryAccessType.ReadWrite : MemoryAccessType.NoAccess;
            //SetCpuMemoryMapping(0x6000, 0x7FFF, 0, HasBattery() ? PrgMemoryType.SaveRam : PrgMemoryType.WorkRam, access);

            if (_usingExternalRom)
            {
                //if (_licensingTimer == 0)
                //{
                //    RemoveCpuMemoryMapping(0x8000, 0xBFFF);
                //}
                //else
                {
                    Bus.PrgMemory.Swap(0, _externalPage);
                }
            }
        }

        //void ProcessCpuClock() override
        //{
        //    BaseProcessCpuClock();

        //    if (_licensingTimer)
        //    {
        //        _licensingTimer--;
        //        if (_licensingTimer == 0)
        //        {
        //            UpdateState();
        //        }
        //    }
        //}

        //void WriteRam(int addr, byte value) override
        //{
        //    if (addr >= 0x6000 && addr <= 0x7FFF)
        //    {
        //        _licensingTimer = 1024 * 105;
        //        UpdateState();
        //    }
        //    BaseMapper.WriteRam(addr, value);
        //}

        public override void Poke(int addr, byte value)
        {
            switch (addr & 0xF000)
            {
                case 0x8000:
                case 0x9000:
                case 0xA000:
                case 0xB000:
                    var chrBank = addr - 0x8000 >> 11;
                    var swapBank = value << 1;
                    Bus.ChrMemory.Swap(chrBank, swapBank);
                    Bus.ChrMemory.Swap(chrBank | 1, swapBank | 1);
                    break;
                case 0xC000:
                    _ntRegs[0] = value | 0x80;
                    UpdateNametables();
                    break;
                case 0xD000:
                    _ntRegs[1] = value | 0x80;
                    UpdateNametables();
                    break;
                case 0xE000:
                    switch (value & 0x03)
                    {
                        case 0: DefaultMirroring = MirrorType.Vertical; break;
                        case 1: DefaultMirroring = MirrorType.Horizontal; break;
                        case 2: DefaultMirroring = MirrorType.OneScreenLo; break;
                        case 3: DefaultMirroring = MirrorType.OneScreenHi; break;
                    }
                    _useChrForNametables = (value & 0x10) != 0;
                    UpdateNametables();
                    break;
                case 0xF000:
                    _usingExternalRom = (value & 0x08) == 0 && Bus.PrgMemory.SwapBanks.Count > 8;
                    var prgBank = value & 7;
                    if (_usingExternalRom)
                    {
                        _externalPage = prgBank % (Bus.PrgMemory.SwapBanks.Count - 8) | 8;
                        Bus.PrgMemory.Swap(0, _externalPage);
                    }
                    else
                        Bus.PrgMemory.Swap(0, prgBank);
                    _prgRamEnabled = (value & 0x10) == 0x10;
                    UpdateState();
                    break;
            }
        }
    }
}
