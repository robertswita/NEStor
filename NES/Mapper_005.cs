using System;
using System.Collections.Generic;

namespace NES
{
    class Mapper_005 : Mapper_004
    {
        public int[] CHRBanksA;
        public int[] CHRBanksB = new int[] { 0, 1, 2, 3, 0, 1, 2, 3 };
        bool LastChrA;
        int ChrHigh;
        int ExtRAMmode;
        byte[] ExtRAM = new byte[0x400];
        List<byte[]> WRAMBanks;
        public TileRow FillTile = new TileRow();
        int Factor1 = 0xFF, Factor2 = 0xFF;
        int IRQStatus;

        public override void Reset()
        {
            base.Reset();
            WRAMBanks = new List<byte[]>();
            for (int i = 0; i < 8; i++)
                WRAMBanks.Add(new byte[0x2000]);
            Bus.WRam = WRAMBanks[0];
            CHRBanksA = ChrBanks;
            LastChrA = true;
            PrgMode = 3;
        }
        public override void IrqScanline()
        {
            //var ppuRendering = Bus.PPU.Y < 240 && (Bus.PPU.Mask.RenderBackground || Bus.PPU.Mask.RenderSprites);
            //if (ppuRendering)
            {
                if (Bus.Ppu.Y < 0 || Bus.Ppu.Y >= 240)
                {
                    IRQStatus = 0;
                    IrqCounter = -1;
                    Bus.Cpu.MapperIrq.Acknowledge();
                    //UpdateChr(LastChrA);
                }
                else
                {
                    IRQStatus |= 0x40;
                    IrqCounter++;
                    if (IrqCounter == IrqCounterLatch)
                    {
                        IRQStatus |= 0x80;
                        if (IrqEnabled)
                            Bus.Cpu.MapperIrq.Start();
                    }
                }
            }
        }
        public override void OnSpritelineStart()
        {
            if (LastChrA || Bus.Ppu.Control.SpriteSize == 16)
                UpdateChr(true);
            else
                UpdateChr(false);
            //if (Bus.PPU.Control.SpriteSize == 16)
            //    UpdateChr(true);
        }

        public override void OnSpritelineEnd()
        {
            //IRQScanline();
            if (!LastChrA || Bus.Ppu.Control.SpriteSize == 16)
                UpdateChr(false);
            else
                UpdateChr(true);
            //if (Bus.PPU.Control.SpriteSize == 16)
            //    UpdateChr(false);
        }
        public override byte Peek(int addr)
        {
            byte data = 0;
            if (addr == 0x5010)
                Bus.Cpu.DmcIrq.Acknowledge();
            else if (addr == 0x5204)
            {
                data = (byte)IRQStatus;
                IRQStatus = 0;
                Bus.Cpu.MapperIrq.Acknowledge();
            }
            else if (addr == 0x5205)
                data = (byte)(Factor1 * Factor2);
            else if (addr == 0x5206)
                data = (byte)((Factor1 * Factor2) >> 8);
            else if (addr >= 0x5C00 && addr <= 0x5FFF)
                data = ExtRAM[addr & 0x3FF];
            return data;
        }

        public override void Poke(int addr, byte data)
        {
            if (addr == 0x5100)
            {
                var mode = data & 3;
                if (mode != PrgMode)
                {
                    PrgMode = mode;
                    UpdatePrg();
                }
            }
            else if (addr == 0x5101)
            {
                var mode = data & 3;
                if (mode != ChrMode)
                {
                    ChrMode = mode;
                    if (Bus.Ppu.Control.SpriteSize != 16 || Bus.Ppu.Y >= 240)
                        UpdateChr(LastChrA);
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
                        Bus.NameTables.Swap(i, bank);
                    else if (bank == 2)
                        Bus.NameTables.Banks[i] = ExtRAMmode < 2 ? ExtRAM : Bus.NameTables.SwapBanks[2];
                    else
                    {
                        Array.Fill(Bus.NameTables.SwapBanks[3], (byte)FillTile.ID, 0, 0x3C0);
                        Array.Fill(Bus.NameTables.SwapBanks[3], (byte)FillTile.Attribute, 0x3C0, 0x40);
                        Bus.NameTables.Swap(i, 3);
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
                Bus.WRam = WRAMBanks[data & 0x7];
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
                    if (Bus.Ppu.Control.SpriteSize != 16 || Bus.Ppu.Y >= 240)
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
                    if (Bus.Ppu.Control.SpriteSize != 16 || Bus.Ppu.Y >= 240)
                        UpdateChr(LastChrA);
                }
            }
            else if (addr == 0x5130)
                ChrHigh = (data & 3);
            else if (addr == 0x5203)
                IrqCounterLatch = data;
            else if (addr == 0x5204)
                IrqEnabled = (data & 0x80) != 0;
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
                Bus.PrgMemory.Banks[target] = WRAMBanks[bank & 7];
            else
                Bus.PrgMemory.Swap(target, bank & 0x7F);
        }

        void UpdatePrg()
        {
            switch (PrgMode)
            {
                case 0:
                    var bank = PrgBanks[3] & ~3;
                    Bus.PrgMemory.Swap(0, bank);
                    Bus.PrgMemory.Swap(1, bank + 1);
                    Bus.PrgMemory.Swap(2, bank + 2);
                    Bus.PrgMemory.Swap(3, bank + 3);
                    break;
                case 1:
                    SwapPrgBank(0, PrgBanks[1] & ~1);
                    SwapPrgBank(1, PrgBanks[1] | 1);
                    Bus.PrgMemory.Swap(2, PrgBanks[3] & ~1);
                    Bus.PrgMemory.Swap(3, PrgBanks[3] | 1);
                    break;
                case 2:
                    SwapPrgBank(0, PrgBanks[1] & ~1);
                    SwapPrgBank(1, PrgBanks[1] | 1);
                    SwapPrgBank(2, PrgBanks[2]);
                    Bus.PrgMemory.Swap(3, PrgBanks[3]);
                    break;
                case 3:
                    SwapPrgBank(0, PrgBanks[0]);
                    SwapPrgBank(1, PrgBanks[1]);
                    SwapPrgBank(2, PrgBanks[2]);
                    Bus.PrgMemory.Swap(3, PrgBanks[3]);
                    break;
            }
        }

        void UpdateChr(bool lastChrA)
        {
            var chrBanks = lastChrA ? CHRBanksA : CHRBanksB;
            var size = 1 << (3 - ChrMode);
            for (int i = 0; i < chrBanks.Length; i++)
            {
                var y = i / size + 1;
                var x = i % size;
                Bus.ChrMemory.Swap(i, size * chrBanks[size * y - 1] + x);
            }
        }

        public override void OnFetchTile(TileRow tile)
        {
            if (ExtRAMmode == 1)
            {
                var tileID = tile.ID;
                var extData = ExtRAM[Bus.Ppu.Vram.CoarseY << 5 | Bus.Ppu.Vram.CoarseX];
                var extChrBank = extData & 0x3F;
                var attr = extData >> 6;
                extChrBank |= ChrHigh << 6;
                extChrBank <<= 2;
                tileID >>= 8;
                tileID <<= 2;
                for (int i = 0; i < 4; i++)
                {
                    //if (tileID + i >= 8) break;
                    Bus.ChrMemory.Swap(tileID + i, extChrBank + i);
                    attr |= attr << 2 * i;
                }
                tile.Attribute = (byte)attr;
            }
        }


    }
}
