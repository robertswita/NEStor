using System;
using System.Drawing;
using Common;
using PPU;

namespace NES
{
    class PPU
    {
        Bus Bus;
        public bool FrameComplete;
        public InternalRegister Address = new InternalRegister();
        InternalRegister NextAddress = new InternalRegister();
        public ControlRegister Control = new ControlRegister();
        public MaskRegister Mask = new MaskRegister();
        public StatusRegister Status = new StatusRegister();
        public float ClockRatio;
        public const int MaxX = 339;
        public int MaxY;
        public int X = -1;
        public int Y = -1;
        byte MemoryLatch;
        public byte OpenBus;
        public byte[,] PaletteIndices = new byte[8, 4];
        public int[] Palette = new int[64];
        public TPixmap Screen = new TPixmap(256, 240);

        public class TileRow {
            public byte Y;          // Y position 
            public int ID;          // ID of tile from pattern memory
            public byte Attribute;  // how sprite should be rendered
            public byte X;          // X position
            public int LSB;
            public int MSB;

            public void ReadPattern(Bus bus, int row)
            {
                var address = ID << 4 | row;
                var table = bus.ChrTable[address / bus.ChrBankSize];
                var tileLine = address % bus.ChrBankSize;
                LSB = table[tileLine];
                MSB = table[tileLine | 0x8];
            }
            public int GetPatternPixel(int idx)
            {
                int lo = (LSB >> idx) & 0x1;
                int hi = (MSB >> idx) & 0x1;
                return hi << 1 | lo;
            }
        }
        /// <summary>
        /// OAM (ObjectAttributeMemory)
        /// 64 sprite TileRows (ObjectAttributeEntry)[Y, ID, attribute, X]
        /// </summary>
        public byte OAMaddr;
        public byte[] OAM = new byte[256];
        TileRow[] SpriteScanline = new TileRow[256];
        TileRow PrevTile = new TileRow();
        TileRow ActTile = new TileRow();
        TileRow NextTile = new TileRow();
        TileRow SpriteZero;
        int[] TileLine = new int[8];
        public Point Scroll;

        public PPU(Bus bus)
        {
            Bus = bus;
            LoadPalette(Properties.Resources.DefaultPalette);
        }

        public void LoadPalette(Bitmap pal)
        {
            var paletteMap = new TPixmap(Palette.Length / 4, 4);
            paletteMap.Image = pal;
            Array.Copy(paletteMap.Pixels, Palette, Palette.Length);
        }

        public int GetPixel(int palette, int pattern)
        {
            return Palette[PaletteIndices[palette, pattern] & 0x3F];
        }

        public static int Flipbyte(int b)
        {
            b = (b & 0xF0) >> 4 | (b & 0x0F) << 4;
            b = (b & 0xCC) >> 2 | (b & 0x33) << 2;
            b = (b & 0xAA) >> 1 | (b & 0x55) << 1;
            return b;
        }

        void ReadNextTile()
        {
            PrevTile = ActTile;
            ActTile = NextTile;
            NextTile = new TileRow();
            var tableNT = Bus.NameTable[Address.NameTableY << 1 | Address.NameTableX];
            NextTile.ID = Control.PatternBg << 8 | tableNT[Address.CoarseY << 5 | Address.CoarseX];
            NextTile.Attribute = tableNT[(Address.CoarseY >> 2) << 3 | Address.CoarseX >> 2 | 0x3C0];
            NextTile.Attribute >>= (Address.CoarseY & 2) << 1 | Address.CoarseX & 2;
            NextTile.ReadPattern(Bus, Address.FineY);
            Address.ScrollX += 8;
        }

        public bool OddFrame;
        public int Drift;
        public float Tick()
        {
            var renderEnabled = Mask.RenderBackground || Mask.RenderSprites;
            if (X <= 2 || renderEnabled)
            {
                if (Y == -1)
                {
                    if (X == 0)
                    {
                        Status.VerticalBlank = false;
                        Status.SpriteZeroHit = false;
                        Status.SpriteOverflow = false;
                        PreventVBL = false;
                        NMIoccurred = false;
                        OpenBus = 0;
                        X = 256;
                    }
                    else if (X == 264)
                    {
                        Address = NextAddress;
                        ReadSpriteScanline(Y);
                        X = 312;
                    }
                    else if (X >= 320 && X <= 335)
                        ReadNextTile();
                }
                else if (Y < Screen.Height)
                {
                    if (X < Screen.Width && (X & 7) == 0 && renderEnabled)
                    {
                        ReadNextTile();
                        DrawTileLine(Y, X);
                    }
                    else if (X == 264)
                    {
                        Address.ScrollY++;
                        Address.ScrollX = NextAddress.ScrollX;
                        ReadSpriteScanline(Y);
                        X = 312;
                    }
                    else if (X >= 320 && X <= 335)
                        ReadNextTile();
                }
                else if (Y == Screen.Height)
                    X = MaxX;
                else if (Y == Screen.Height + 1)
                {
                    if (X == 0)
                    {
                        Scroll.X = NextAddress.ScrollX;
                        Scroll.Y = NextAddress.ScrollY;
                        if (!PreventVBL)
                        {
                            Status.VerticalBlank = true;
                        }
                    }
                    if (X == 2)
                    {
                        if (!PreventVBL)
                        {
                            Bus.CPU.IRQDelay = 1;
                            StartNMI();
                        }
                        Y = MaxY;
                        X = MaxX - 1;
                    }
                }
            }

            if (X >= 8 && X < MaxX - 3)
                X += 8;
            else
                X++;
            if (X > MaxX)
            {
                X = -1;
                Y++;
                if (Y > MaxY)
                {
                    var maxDot = (MaxX + 2) * (MaxY + 2) + Drift;
                    Drift = maxDot % 3;
                    OddFrame = !OddFrame;
                    if (Bus.SystemType == SystemType.NTSC && renderEnabled && OddFrame)
                    {
                        Drift--;
                        X++;
                    }
                    Y = -1;
                    FrameComplete = true;
                    return maxDot / ClockRatio;
                }
            }
            var nextDot = (MaxX + 2) * (Y + 1) + X + 1 + Drift;
            return nextDot / ClockRatio;
        }

        private void DrawTileLine(int Y, int X)
        {
            var renderBackground = Mask.RenderBackground && (X > 0 || Mask.RenderBackgroundLeft);
            var renderSprites = Mask.RenderSprites && (X > 0 || Mask.RenderSpritesLeft);
            for (int x = 0; x < 8; x++)
            {
                int pattern = 0;
                int palette = 0;
                if (renderBackground)
                {
                    var idx = 15 - (x + NextAddress.FineX);
                    var patternTile = idx < 8 ? ActTile : PrevTile;
                    pattern = patternTile.GetPatternPixel(idx & 7);
                    if (pattern > 0)
                        palette = patternTile.Attribute & 0x03;
                }
                if (renderSprites)
                {
                    var pos = X + x;
                    var sprite = SpriteScanline[pos];
                    if (sprite != null)
                    {
                        if (sprite == SpriteZero && pattern > 0 && pos < Screen.Width - 1)
                            Status.SpriteZeroHit = true;
                        var fg_priority = (sprite.Attribute & 0x20) == 0;
                        if (fg_priority || pattern == 0)
                        {
                            int idx = pos - sprite.X;
                            pattern = sprite.GetPatternPixel(7 - idx);
                            palette = sprite.Attribute & 0x03 | 0x04;
                        }
                    }
                }
                TileLine[x] = GetPixel(palette, pattern);
            }
            Array.Copy(TileLine, 0, Screen.Pixels, Y * Screen.Width + X, TileLine.Length);
        }

        private void ReadSpriteScanline(int y)
        {
            OAMaddr = 0;
            SpriteZero = null;
            SpriteScanline = new TileRow[Screen.Width];
            var _spriteCount = 0;
            for (int i = 0; i < OAM.Length; i += 4)
            {
                var row = y - OAM[i];
                if (row >= 0 && row < Control.SpriteSize)
                {
                    if (_spriteCount == 8)
                    {
                        Status.SpriteOverflow = true;
                        break;
                    }
                    var sprite = new TileRow();
                    sprite.Y = OAM[i];
                    sprite.ID = OAM[i + 1];
                    sprite.Attribute = OAM[i + 2];
                    sprite.X = OAM[i + 3];
                    if (i == 0)
                        SpriteZero = sprite;
                    var flipX = (sprite.Attribute & 0x40) != 0;
                    var flipY = (sprite.Attribute & 0x80) != 0;
                    if (flipY)
                        row = Control.SpriteSize - 1 - row;
                    var tableNo = Control.PatternSprite;
                    if (Control.SpriteSize == 16)
                    {
                        tableNo = sprite.ID & 1;
                        sprite.ID &= 0xFE;
                        if (row > 7)
                        {
                            sprite.ID++;
                            row &= 7;
                        }
                    }
                    sprite.ID |= tableNo << 8;
                    sprite.ReadPattern(Bus, row);
                    if (flipX)
                    {
                        sprite.LSB = Flipbyte(sprite.LSB);
                        sprite.MSB = Flipbyte(sprite.MSB);
                    }
                    for (int j = 0; j < 8; j++)
                    {
                        var x = sprite.X + j;
                        if (x >= SpriteScanline.Length) break;
                        if (SpriteScanline[x] != null) continue;
                        if (sprite.GetPatternPixel(7 - j) != 0)
                            SpriteScanline[x] = sprite;
                    }
                    _spriteCount++;
                }
            }
            Bus.Mapper.PPUSync();
        }

        public TPixmap GetPaletteMap()
        {
            var paletteMap = new TPixmap(Palette.Length / 4, 4);
            Array.Copy(Palette, paletteMap.Pixels, Palette.Length);
            return paletteMap;
        }

        TPixmap GetTileMap(int tableIdx, int tileIdx, int palette)
        {
            tileIdx |= tableIdx << 8;
            var tileMap = new TPixmap(8, 8);
            for (var row = 0; row < 8; row++)
            {
                var tileRow = new TileRow();
                tileRow.ID = tileIdx;
                tileRow.ReadPattern(Bus, row);
                for (var col = 0; col < 8; col++)
                    tileMap[col, row] = GetPixel(palette, tileRow.GetPatternPixel(7 - col));
            }
            return tileMap;
        }

        /// <summary>
        /// Debug view of the specific pattern table for given palette
        /// </summary>
        /// <param name="idx"></param>
        /// <param name="palette"></param>
        /// <returns></returns>
        public TPixmap GetPatternTableMap(int idx, byte palette)
        {
            var pattern = new TPixmap(128, 0);
            for (var y = 0; y < 16; y++)
            {
                var patternRow = new TPixmap(0, 8);
                for (var x = 0; x < 16; x++)
                {
                    var tile = GetTileMap(idx, y * 16 + x, palette);
                    patternRow = patternRow.HorzCat(tile);
                }
                pattern = pattern.VertCat(patternRow);
            }
            return pattern;
        }

        /// <summary>
        /// Dump of the name table memory (information about the tiles is incomplete)
        /// </summary>
        /// <param name="idx"></param>
        /// <returns></returns>
        public TPixmap GetNameTableMap(int idx)
        {
            var nameTable = new TPixmap(256, 0);
            for (var y = 0; y < 30; y++)
            {
                var nameTableRow = new TPixmap(0, 8);
                for (var x = 0; x < 32; x++)
                {
                    var table = Bus.NameTable[idx];
                    var attribute = table[(y >> 2) << 3 | x >> 2 | 0x3C0];
                    attribute >>= (y & 2) << 1 | x & 2;
                    var tileIdx = Bus.NameTable[idx][y << 5 | x];
                    var tile = GetTileMap(Control.PatternBg, tileIdx, attribute & 3); 
                    nameTableRow = nameTableRow.HorzCat(tile);
                }
                nameTable = nameTable.VertCat(nameTableRow);
            }
            return nameTable;
        }

        bool PreventVBL;
        bool NMIoccurred;
        void StartNMI()
        {
            if (!NMIoccurred && Control.EnableNMI)
            {
                Bus.CPU.NMI();
                NMIoccurred = true;
            }
        }
        public byte CpuRead(int addr)
        {
            int data = OpenBus;
            switch (addr)
            {
                case 2: // Status
                    data &= 0x1F;
                    data |= (byte)(Status.Register & 0xE0);
                        Status.VerticalBlank = false;
                    NextAddress.Latched = false;
                    NMIoccurred = false;
                    // Reading PPUSTATUS one clock before the start of vertical blank will read as clear
                    // and never set the flag or generate an NMI for that frame
                    if (Y == Screen.Height + 1 && X >= 0 && X < 3)
                    {
                        if (X == 0)
                            PreventVBL = true;
                        //Control.EnableNMI = false;
                    }
                    break;
                case 4: // OAM Data
                    data = OAM[OAMaddr];
                    //data = (byte)(OAM[OAMaddr] & 0xE3);
                    break;
                case 7: // PPU Data
                    data = ReadVRAM();
                    // MMC3 clocks using A12
                    //addr = self.scroll.read_addr();
                    //self.mapper_mut().ppu_bus_write(addr, val);
                    //Bus.Mapper.PPUSync();
                    break;
            }
            OpenBus = (byte)data;
            return OpenBus;
        }
        public void CpuWrite(int addr, byte data)
        {
            OpenBus = data;
            switch (addr)
            {
                case 0: // Control
                    Control.Register = data;
                    NextAddress.NameTableX = Control.NameTableX;
                    NextAddress.NameTableY = Control.NameTableY;

                    // condition ensures correct NMI occurrence when enabled near time
                    // VBL flag is cleared - BLARGG test 07-nmi_on_timing.rom
                    bool supressNMI = Y == -1 && X == 0;

                    // condition ensures correct NMI disable delay when done near time when VBL
                    // is set - BLARGG test 08-nmi_off_timing.rom
                    //if (Y == Screen.Height + 1 && X > 2)
                    //{
                    //    Control.EnableNMI = true;
                    //}

                    if (!Control.EnableNMI)
                        NMIoccurred = false;
                    if (Status.VerticalBlank && !supressNMI)
                    {
                        Bus.CPU.IRQDelay = 2;
                        StartNMI();
                    }
                    break;
                case 1: // Mask
                    Mask.Register = data;
                    break;
                case 3: // OAM Address
                    OAMaddr = data;
                    break;
                case 4: // OAM Data
                    OAM[OAMaddr] = data;
                    OAMaddr++;
                    break;
                case 5: // Scroll
                    NextAddress.SetScroll(data);
                    break;
                case 6: // PPU Address
                    NextAddress.SetAddress(data);
                    if (!NextAddress.Latched)
                        Address = NextAddress;
                    //Bus.Mapper.PPUSync();
                    break;
                case 7: // PPU Data
                    WriteVRAM(data);
                    //Bus.Mapper.PPUSync();
                    break;
            }
        }

        byte ReadVRAM()
        {
            var addr = Address.Register;
            Address.Register = addr + Control.Increment;
            addr &= 0x3FFF;
            int data = MemoryLatch;
            MemoryLatch = addr >= 0x2000 ?
                Bus.NameTable[(addr >> 10) & 3][addr & 0x03FF] :
                Bus.ChrTable[addr / Bus.ChrBankSize][addr % Bus.ChrBankSize];
            if (addr >= 0x3F00)
            {
                if ((addr & 0x13) == 0x10) addr &= 0x0F; else addr &= 0x1F;
                data = PaletteIndices[addr >> 2, addr & 3] | OpenBus & 0xC0; //& (Mask.Greyscale ? 0x30 : 0x3F));
            }
            return (byte)data;
        }
        void WriteVRAM(byte data)
        {
            var addr = Address.Register;
            Address.Register = addr + Control.Increment;
            addr &= 0x3FFF;
            if (addr <= 0x1FFF && Bus.Mapper.ChrRAMenabled)
                Bus.ChrTable[addr / Bus.ChrBankSize][addr % Bus.ChrBankSize] = data;
            else if (addr <= 0x3EFF)
                Bus.NameTable[(addr >> 10) & 3][addr & 0x03FF] = data;
            else if (addr <= 0x3FFF)
            {
                if ((addr & 0x13) == 0x10) addr &= 0xF; else addr &= 0x1F;
                PaletteIndices[addr >> 2, addr & 3] = data;
            }
        }


    }
}