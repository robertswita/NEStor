using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NES
{
    class Mapper_005: Mapper_004
    {
        public int[] CHRBanksA;
        public int[] CHRBanksB = new int[] { 0, 1, 2, 3, 0, 1, 2, 3 };
        bool LastChrA;
        int ChrHigh;
        int ExtRAMmode;
        byte[] ExtRAM = new byte[0x400];
        List<byte[]> WRAMBanks = new List<byte[]>();
        public TileRow FillTile = new TileRow();
        int Factor1 = 0xFF, Factor2 = 0xFF;
        int IRQStatus;

        public override void Reset()
        {
            base.Reset();
            for (int i = 0; i < 8; i++)
                WRAMBanks.Add(new byte[0x2000]);
            Bus.WRAM = WRAMBanks[0];
            CHRBanksA = ChrBanks;
            LastChrA = true;
            PRGmode = 3;
        }
        public override void IRQScanline()
        {
            //var ppuRendering = Bus.PPU.Y < 240 && (Bus.PPU.Mask.RenderBackground || Bus.PPU.Mask.RenderSprites);
            //if (ppuRendering)
            {
                if (Bus.PPU.Y < 0 || Bus.PPU.Y >= 240)
                {
                    IRQStatus = 0;
                    IRQCounter = -1;
                    Bus.CPU.IRQList.Clear();
                    //UpdateChr(LastChrA);
                }
                else
                {
                    IRQStatus |= 0x40;
                    IRQCounter++;
                    if (IRQCounter == IRQLatch)
                    {
                        IRQStatus |= 0x80;
                        if (IRQEnabled)
                            Bus.CPU.IRQ();
                    }
                }
            }
        }
        public override void OnSpritelineStart()
        {
            if (LastChrA || Bus.PPU.Control.SpriteSize == 16)
                UpdateChr(true);
            else
                UpdateChr(false);
            //if (Bus.PPU.Control.SpriteSize == 16)
            //    UpdateChr(true);
        }

        public override void OnSpritelineEnd()
        {
            //IRQScanline();
            if (!LastChrA || Bus.PPU.Control.SpriteSize == 16)
                UpdateChr(false);
            else
                UpdateChr(true);
            //if (Bus.PPU.Control.SpriteSize == 16)
            //    UpdateChr(false);
        }
        public override byte CpuMapRead(int addr)
        {
            byte data = 0;
            if (addr == 0x5204)
            {
                data = (byte)IRQStatus;
                IRQStatus = 0;
                //Bus.CPU.IRQList.Clear();
            }
            else if (addr == 0x5205)
                data = (byte)(Factor1 * Factor2);
            else if (addr == 0x5206)
                data = (byte)((Factor1 * Factor2) >> 8);
            else if (addr >= 0x5C00 && addr <= 0x5FFF)
                data = ExtRAM[addr & 0x3FF];
            return data;
        }

        public override void CpuMapWrite(int addr, byte data)
		{
            if (addr == 0x5100)
            {
                var mode = data & 3;
                if (mode != PRGmode)
                {
                    PRGmode = mode;
                    UpdatePrg();
                }
            }
            else if (addr == 0x5101)
            {
                var mode = data & 3;
                if (mode != CHRmode)
                {
                    CHRmode = mode;
                    if (Bus.PPU.Control.SpriteSize != 16 || Bus.PPU.Y >= 240)
                    //if (Bus.PPU.Control.SpriteSize != 16)
                    {
                        UpdateChr(LastChrA);
                    }
                }
            }
            else if (addr == 0x5104)
                ExtRAMmode = data & 0x3;
            else if (addr == 0x5105)
            {
                for (int i = 0; i < 4; i++)
                {
                    var bank = (data >> 2 * i) & 3;
                    if (bank < 2)
                        Bus.NameTable[i] = NameTableBanks[bank];
                    else if (bank == 2)
                        Bus.NameTable[i] = ExtRAMmode < 2 ? ExtRAM : NameTableBanks[2];
                    else
                    {
                        Array.Fill(NameTableBanks[3], (byte)FillTile.ID, 0, 0x3C0);
                        Array.Fill(NameTableBanks[3], (byte)FillTile.Attribute, 0x3C0, 0x40);
                        Bus.NameTable[i] = NameTableBanks[3];
                    }
                }
            }
            else if (addr == 0x5106)
                FillTile.ID = data;
            else if (addr == 0x5107)
            {
                FillTile.Attribute = 0;
                for (int i = 0; i < 4; i++)
                    FillTile.Attribute |= (byte)((data & 3) << 2 * i);
            }
            else if (addr == 0x5113)
                Bus.WRAM = WRAMBanks[data & 0x7];
            else if (addr >= 0x5114 && addr <= 0x5117)
            {
                addr &= 0x3;
                if (PrgBanks[addr] != data)
                {
                    PrgBanks[addr] = data;
                    UpdatePrg();
                }
            }
            else if (addr >= 0x5120 && addr <= 0x5127)
            {
                var bank = data | (ChrHigh << 8);
                addr &= 0x7;
                if (!LastChrA || CHRBanksA[addr] != bank)
                {
                    CHRBanksA[addr] = bank;
                    LastChrA = true;
                    if (Bus.PPU.Control.SpriteSize != 16 || Bus.PPU.Y >= 240)
                        UpdateChr(LastChrA);
                }
            }
            else if (addr >= 0x5128 && addr <= 0x512B)
            {
                var bank = data | (ChrHigh << 8);
                addr &= 0x3;
                if (LastChrA || CHRBanksB[addr] != bank)
                {
                    CHRBanksB[addr] = bank;
                    CHRBanksB[addr + 4] = bank;
                    LastChrA = false;
                    if (Bus.PPU.Control.SpriteSize != 16 || Bus.PPU.Y >= 240)
                        UpdateChr(LastChrA);
                }
            }
            else if (addr == 0x5130)
                ChrHigh = (data & 3);
            else if (addr == 0x5203)
                IRQLatch = data;
            else if (addr == 0x5204)
                IRQEnabled = (data & 0x80) != 0;
            else if (addr == 0x5205)
                Factor1 = data;
            else if (addr == 0x5206)
                Factor2 = data;
            else if (addr >= 0x5C00 && addr < 0x6000)
                ExtRAM[addr & 0x3FF] = data;
        }

        void SwapPrgBank(int target, int bank)
        {
            if ((bank & 0x80) == 0)
                PrgMap.Target[target] = WRAMBanks[bank & 7];
            else
                PrgMap[target] = bank & 0x7F;
        }

        void UpdatePrg()
        {
            switch (PRGmode)
            {
                case 0:
                    var bank = PrgBanks[3] & ~3;
                    PrgMap[0] = bank;
                    PrgMap[1] = bank + 1;
                    PrgMap[2] = bank + 2;
                    PrgMap[3] = bank + 3;
                    break;
                case 1:
                    SwapPrgBank(0, PrgBanks[1] & ~1);
                    SwapPrgBank(1, PrgBanks[1] | 1);
                    PrgMap[2] = PrgBanks[3] & ~1;
                    PrgMap[3] = PrgBanks[3] | 1;
                    break;
                case 2:
                    SwapPrgBank(0, PrgBanks[1] & ~1);
                    SwapPrgBank(1, PrgBanks[1] | 1);
                    SwapPrgBank(2, PrgBanks[2]);
                    PrgMap[3] = PrgBanks[3];
                    break;
                case 3:
                    SwapPrgBank(0, PrgBanks[0]);
                    SwapPrgBank(1, PrgBanks[1]);
                    SwapPrgBank(2, PrgBanks[2]);
                    PrgMap[3] = PrgBanks[3];
                    break;
            }
        }

        void UpdateChr(bool lastChrA)
        {
            var chrBanks = lastChrA ? CHRBanksA : CHRBanksB;
            var size = 1 << (3 - CHRmode);
            for (int i = 0; i < chrBanks.Length; i++)
            {
                var y = i / size + 1;
                var x = i % size;
                ChrMap[i] = size * chrBanks[size * y - 1] + x;
            }
        }

        public override void OnFetchTile(TileRow tile) 
        {
            if (ExtRAMmode == 1)
            {
                var tileID = tile.ID;
                var extData = ExtRAM[Bus.PPU.VRAM.CoarseY << 5 | Bus.PPU.VRAM.CoarseX];
                var extChrBank = extData & 0x3F;
                var attr = extData >> 6;
                extChrBank |= ChrHigh << 6;
                extChrBank <<= 2;
                tileID >>= 8;
                tileID <<= 2;
                for (int i = 0; i < 4; i++)
                {
                    //if (tileID + i >= 8) break;
                    ChrMap[tileID + i] = extChrBank + i;
                    attr |= attr << 2 * i;
                }
                tile.Attribute = (byte)attr;
            }
        }


    }
}
