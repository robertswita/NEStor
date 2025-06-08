using NES;
using System;

namespace NEStor.Core.Cartridge.Mappers
{
    class Mapper_002 : Mapper
    {
        int prgMask;
        int chrMask0;
        int chrMask1;
        int mirrorMask;
        int prgShift;
        int chrShift0;
        int chrShift1;
        public override void Reset()
        {
            switch (Id)
            {
                case 2:
                    prgMask = 0xF;
                    Bus.PrgMemory.Swap(1, -1);
                    break;
                case 3:
                case 185:
                    chrMask0 = 0xF;
                    break;
                case 7:
                    prgMask = 0xF;
                    mirrorMask = 0x10;
                    Bus.PrgMemory.BankSize = 0x8000;
                    break;
                case 11:
                    prgMask = 0xF;
                    chrMask0 = 0xF0;
                    Bus.PrgMemory.BankSize = 0x8000;
                    break;
                case 13:
                    chrMask1 = 3;
                    Bus.ChrMemory.SwapBanks.Clear();
                    Bus.ChrMemory.SwapBanks.Add(new byte[0x4000]);
                    Bus.ChrMemory.BankSize = 0x1000;
                    break;
                case 66:
                    prgMask = 0x30;
                    chrMask0 = 3;
                    Bus.PrgMemory.BankSize = 0x8000;
                    break;
                case 70:
                case 152:
                    prgMask = 0x70;
                    chrMask0 = 0xF;
                    mirrorMask = 0x80;
                    Bus.PrgMemory.Swap(1, -1);
                    break;
                case 78:
                    prgMask = 0x7;
                    chrMask0 = 0xF0;
                    mirrorMask = 8;
                    Bus.PrgMemory.Swap(1, -1);
                    break;
                case 87:
                    chrMask0 = 3;
                    PrgRamEnabled = false;
                    break;
                case 93:
                    prgMask = 0x70;
                    PrgRamEnabled = false;
                    Bus.PrgMemory.Swap(1, -1);
                    break;
                case 184:
                    chrMask0 = 7;
                    chrMask1 = 0x70;
                    PrgRamEnabled = false;
                    Bus.ChrMemory.BankSize = 0x1000;
                    break;
            }
            prgShift = GetShift(prgMask);
            chrShift0 = GetShift(chrMask0);
            chrShift1 = GetShift(chrMask1);
        }

        int GetShift(int mask)
        {
            var shift = 0;
            if (mask != 0)
                while ((mask & 1 << shift) == 0)
                    shift++;
            return shift;
        }
        public override void Poke(int addr, byte data)
        {
            if (PrgRamEnabled && addr < 0x8000) return;
            if (Id == 11 && addr == 0x8000) return;
            if (Id == 87) data = (byte)((data & 1) << 1 | (data & 2) >> 1);
            Bus.PrgMemory.Swap(0, (data & prgMask) >> prgShift);
            Bus.ChrMemory.Swap(0, (data & chrMask0) >> chrShift0);
            if (chrMask1 != 0)
            {
                int chrBank = (data & chrMask1) >> chrShift1;
                if (Id == 184) chrBank |= 0x80;
                Bus.ChrMemory.Swap(1, chrBank);
            }
            if (mirrorMask != 0)
            {
                if (Id == 78 && (SubId == 3 || AltMirroring))
                    Mirroring = (data & mirrorMask) != 0 ? MirrorType.Vertical: MirrorType.Horizontal;
                else
                    Mirroring = (data & mirrorMask) != 0 ? MirrorType.OneScreenHi : MirrorType.OneScreenLo;
            }
        }
    }
}
