using NES;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NEStor.Core.Cartridge.Mappers
{
    internal class Mapper_082 : Mapper
    {
        int _chrMode;
        int[] _chrRegs = new int[6];
        int[] _ramPermission = new int[3];

        void UpdateRamAccess()
        {
            //SetCpuMemoryMapping(0x6000, 0x63FF, 0, PrgMemoryType.SaveRam, _ramPermission[0] == 0xCA ? MemoryAccessType.ReadWrite : MemoryAccessType.NoAccess);
            //SetCpuMemoryMapping(0x6400, 0x67FF, 1, PrgMemoryType.SaveRam, _ramPermission[0] == 0xCA ? MemoryAccessType.ReadWrite : MemoryAccessType.NoAccess);

            //SetCpuMemoryMapping(0x6800, 0x6BFF, 2, PrgMemoryType.SaveRam, _ramPermission[1] == 0x69 ? MemoryAccessType.ReadWrite : MemoryAccessType.NoAccess);
            //SetCpuMemoryMapping(0x6C00, 0x6FFF, 3, PrgMemoryType.SaveRam, _ramPermission[1] == 0x69 ? MemoryAccessType.ReadWrite : MemoryAccessType.NoAccess);

            //SetCpuMemoryMapping(0x7000, 0x73FF, 4, PrgMemoryType.SaveRam, _ramPermission[2] == 0x84 ? MemoryAccessType.ReadWrite : MemoryAccessType.NoAccess);
        }

        //	int GetPrgPageSize() override { return 0x2000; }
        //	int GetChrPageSize() override { return 0x0400; }
        //int RegisterStartAddress() override
        //{ return 0x7EF0; }
        //int RegisterEndAddress() override
        //{ return 0x7EFF; }

        //uint32_t GetSaveRamSize() override
        //{ return 0x1400; }
        //uint32_t GetSaveRamPageSize() override
        //{ return 0x400; }

        public override void Reset()
        {
            PrgRamEnabled = false;
            Bus.PrgMemory.BankSize = 0x2000;
            Bus.ChrMemory.BankSize = 0x400;
            Bus.PrgMemory.Swap(3, -1);
            UpdateRamAccess();
        }

        void UpdateChrBanking()
        {
            if (_chrMode == 0)
            {
                //Regs 0 & 1 ignore the LSB
                Bus.ChrMemory.Swap(0, _chrRegs[0] & 0xFE);
                Bus.ChrMemory.Swap(1, _chrRegs[0] & 0xFE | 1);
                Bus.ChrMemory.Swap(2, _chrRegs[1] & 0xFE);
                Bus.ChrMemory.Swap(3, _chrRegs[1] & 0xFE | 1);
                Bus.ChrMemory.Swap(4, _chrRegs[2]);
                Bus.ChrMemory.Swap(5, _chrRegs[3]);
                Bus.ChrMemory.Swap(6, _chrRegs[4]);
                Bus.ChrMemory.Swap(7, _chrRegs[5]);
            }
            else
            {
                Bus.ChrMemory.Swap(0, _chrRegs[2]);
                Bus.ChrMemory.Swap(1, _chrRegs[3]);
                Bus.ChrMemory.Swap(2, _chrRegs[4]);
                Bus.ChrMemory.Swap(3, _chrRegs[5]);

                //Regs 0 & 1 ignore the LSB
                Bus.ChrMemory.Swap(4, _chrRegs[0] & 0xFE);
                Bus.ChrMemory.Swap(5, _chrRegs[0] & 0xFE | 1);
                Bus.ChrMemory.Swap(6, _chrRegs[1] & 0xFE);
                Bus.ChrMemory.Swap(7, _chrRegs[1] & 0xFE | 1);
            }
        }

        public override void Poke(int addr, byte value)
        {
            switch (addr)
            {
                case 0x7EF0:
                case 0x7EF1:
                case 0x7EF2:
                case 0x7EF3:
                case 0x7EF4:
                case 0x7EF5:
                    _chrRegs[addr & 0xF] = value;
                    UpdateChrBanking();
                    break;
                case 0x7EF6:
                    Mirroring = (value & 0x01) != 0 ? MirrorType.Vertical : MirrorType.Horizontal;
                    _chrMode = (value & 0x02) >> 1;
                    UpdateChrBanking();
                    break;
                case 0x7EF7:
                case 0x7EF8:
                case 0x7EF9:
                    _ramPermission[(addr & 0xF) - 7] = value;
                    UpdateRamAccess();
                    break;
                case 0x7EFA:
                    Bus.PrgMemory.Swap(0, value >> 2);
                    break;
                case 0x7EFB:
                    Bus.PrgMemory.Swap(1, value >> 2);
                    break;
                case 0x7EFC:
                    Bus.PrgMemory.Swap(2, value >> 2);
                    break;
            }
        }

        //void Serialize(Serializer& s) override
        //{
        //    BaseMapper.Serialize(s);
        //    SVArray(_ramPermission, 3);
        //    SVArray(_chrRegs, 6);
        //    SV(_chrMode);

        //    if (!s.IsSaving())
        //    {
        //        UpdateRamAccess();
        //    }
        //}

    }
}
