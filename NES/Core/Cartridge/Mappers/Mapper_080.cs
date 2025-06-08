using NES;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NEStor.Core.Cartridge.Mappers
{
    internal class Mapper_080 : Mapper
    {
        bool _alternateMirroring = false;
        byte _ramPermission = 0;

        void UpdateRamAccess()
        {
            //SetCpuMemoryMapping(0x7F00, 0x7FFF, 0, HasBattery() ? PrgMemoryType::SaveRam : PrgMemoryType::WorkRam, _ramPermission == 0xA3 ? MemoryAccessType::ReadWrite : MemoryAccessType::NoAccess);
        }

        //	int GetPrgPageSize() override { return 0x2000; }
        //	int GetChrPageSize() override { return 0x0400; }
        //int RegisterStartAddress() override
        //{ return 0x7EF0; }
        //int RegisterEndAddress() override
        //{ return 0x7EFF; }

        //uint32_t GetWorkRamSize() override
        //{ return 0x100; }
        //uint32_t GetWorkRamPageSize() override
        //{ return 0x100; }
        //uint32_t GetSaveRamSize() override
        //{ return 0x100; }
        //uint32_t GetSaveRamPageSize() override
        //{ return 0x100; }

        //bool ForceSaveRamSize() override
        //{ return HasBattery(); }
        //bool ForceWorkRamSize() override
        //{ return !HasBattery(); }

        public override void Reset()
        {
            _ramPermission = 0;
            Bus.PrgMemory.BankSize = 0x2000;
            Bus.ChrMemory.BankSize = 0x400;
            Bus.PrgMemory.Swap(3, -1);
            PrgRamEnabled = false;
            UpdateRamAccess();
        }

        //void WriteRam(int addr, byte value) override
        //{
        //    if ((addr & 0xFF00) == 0x7F00)
        //    {
        //        //Mirror writes to the top/bottom - the mapper only has 128 bytes of ram, mirrored once.
        //        //The current BaseMapper code doesn't support mapping blocks smaller than 256 bytes,
        //        //so doing this at least ensures it behaves like a mirrored 128-byte block of ram
        //        BaseMapper::WriteRam(addr ^ 0x80, value);
        //    }
        //    BaseMapper::WriteRam(addr, value);
        //}

        public override void Poke(int addr, byte value)
        {
            if ((addr & 0xFFF0) != 0x7EF0) return;
            addr &= 0xF;
            switch (addr)
            {
                case 0:
                    Bus.ChrMemory.Swap(0, value);
                    Bus.ChrMemory.Swap(1, value + 1);
                    if (_alternateMirroring)
                    {
                        Bus.NameTables.Swap(0, value >> 7);
                        Bus.NameTables.Swap(1, value >> 7);
                    }
                    break;
                case 1:
                    Bus.ChrMemory.Swap(2, value);
                    Bus.ChrMemory.Swap(3, value + 1);
                    if (_alternateMirroring)
                    {
                        Bus.NameTables.Swap(2, value >> 7);
                        Bus.NameTables.Swap(3, value >> 7);
                    }
                    break;

                case 2: Bus.ChrMemory.Swap(4, value); break;
                case 3: Bus.ChrMemory.Swap(5, value); break;
                case 4: Bus.ChrMemory.Swap(6, value); break;
                case 5: Bus.ChrMemory.Swap(7, value); break;

                case 6:
                case 7:
                    if (!_alternateMirroring)
                        Mirroring = (value & 1) != 0 ? MirrorType.Vertical : MirrorType.Horizontal;
                    break;

                case 8:
                case 9:
                    _ramPermission = value;
                    UpdateRamAccess();
                    break;

                case 0xA:
                case 0xB:
                case 0xC:
                case 0xD:
                case 0xE:
                case 0xF:
                    Bus.PrgMemory.Swap(addr - 0xA >> 1, value);
                    break;
            }
        }

        //void Serialize(Serializer& s) override
        //{
        //    BaseMapper::Serialize(s);
        //    SV(_ramPermission);

        //    if (!s.IsSaving())
        //    {
        //        UpdateRamAccess();
        //    }
        //}

    }
}
