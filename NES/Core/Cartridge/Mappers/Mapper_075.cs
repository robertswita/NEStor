using NES;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NEStor.Core.Cartridge.Mappers
{
    internal class Mapper_075 : Mapper
    {
        int[] _chrBanks = new int[2];

        public override void Reset()
        {
            Bus.PrgMemory.BankSize = 0x2000;
            Bus.ChrMemory.BankSize = 0x1000;
            Bus.PrgMemory.Swap(3, -1);
        }

        //void Serialize(Serializer& s) override
        //{
        //    BaseMapper.Serialize(s);
        //    SV(_chrBanks[0]);
        //    SV(_chrBanks[1]);
        //}

        public override void Poke(int addr, byte value)
        {
            //TODO: Create a setting to enable/disable oversized PRG
            bool allowOversizedPrg = true;
            int prgMask = allowOversizedPrg ? 0xFF : 0x0F;
            switch (addr & 0xF000)
            {
                case 0x8000: Bus.PrgMemory.Swap(0, value & prgMask); break;
                case 0x9000:
                    if (Mirroring != MirrorType.FourScreens)
                        //"The mirroring bit is ignored if the cartridge is wired for 4-screen VRAM, as is typical for Vs. System games using the VRC1."
                        Mirroring = (value & 1) != 0 ? MirrorType.Horizontal : MirrorType.Vertical;
                    _chrBanks[0] = _chrBanks[0] & 0x0F | value << 3 & 0x10;
                    _chrBanks[1] = _chrBanks[1] & 0x0F | value << 2 & 0x10;
                    Bus.ChrMemory.Swap(0, _chrBanks[0]);
                    Bus.ChrMemory.Swap(1, _chrBanks[1]);
                    break;
                case 0xA000: Bus.PrgMemory.Swap(1, value & prgMask); break;
                case 0xC000: Bus.PrgMemory.Swap(2, value & prgMask); break;
                case 0xE000:
                case 0xF000:
                    var chrBank = addr == 0xE000 ? 0 : 1;
                    _chrBanks[chrBank] = _chrBanks[chrBank] & 0x10 | value & 0x0F;
                    Bus.ChrMemory.Swap(chrBank, _chrBanks[chrBank]);
                    break;
            }

        }
    }
}
