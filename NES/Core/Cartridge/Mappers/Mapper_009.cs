using NEStor.Core.Ppu;
using System;

namespace NEStor.Core.Cartridge.Mappers
{
    class Mapper_009: Mapper
    {
        int LeftLatch = 1;
        int RightLatch = 1;
        int[] LeftChrPage = new int[2];
        int[] RightChrPage = new int[2];
        public override void Reset()
        {
            Bus.ChrMemory.BankSize = 0x1000;
            if (Id == 10)
            {
                Bus.PrgMemory.BankSize = 0x4000;
                Bus.PrgMemory.Swap(1, -1);
            }
            else
            {
                Bus.PrgMemory.BankSize = 0x2000;
                Bus.PrgMemory.Swap(1, -3);
                Bus.PrgMemory.Swap(2, -2);
                Bus.PrgMemory.Swap(3, -1);
            }
        }
        public override void Poke(int addr, byte data)
        {
            switch (addr & 0xF000)
            {
                case 0xA000:
                    Bus.PrgMemory.Swap(0, data & 0xF);
                    break;
                case 0xB000:
                    LeftChrPage[0] = data & 0x1F;
                    Bus.ChrMemory.Swap(0, LeftChrPage[LeftLatch]);
                    break;
                case 0xC000:
                    LeftChrPage[1] = data & 0x1F;
                    Bus.ChrMemory.Swap(0, LeftChrPage[LeftLatch]);
                    break;
                case 0xD000:
                    RightChrPage[0] = data & 0x1F;
                    Bus.ChrMemory.Swap(1, RightChrPage[RightLatch]);
                    break;
                case 0xE000:
                    RightChrPage[1] = data & 0x1F;
                    Bus.ChrMemory.Swap(1, RightChrPage[RightLatch]);
                    break;
                case 0xF000:
                    Mirroring = (data & 1) != 0 ? MirrorType.Horizontal : MirrorType.Vertical;
                    break;
            }
        }

        public override void OnFetchTile(TileRow tile)
        {
            var needChrUpdate = true;
            var addr = tile.ID << 4 | Bus.Ppu.Vram.FineY | 8;
            if (Id == 10)
                addr &= 0xFFF8;
            if (addr == 0x0FD8) 
                LeftLatch = 0;
            else if (addr == 0x0FE8) 
                LeftLatch = 1;
            else if ((addr & 0xFFF8) == 0x1FD8) 
                RightLatch = 0;
            else if ((addr & 0xFFF8) == 0x1FE8) 
                RightLatch = 1;
            else needChrUpdate = false;
            if (needChrUpdate)
            {
                Bus.ChrMemory.Swap(0, LeftChrPage[LeftLatch]);
                Bus.ChrMemory.Swap(1, RightChrPage[RightLatch]);
            }
        }

    }
}
