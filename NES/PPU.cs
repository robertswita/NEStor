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
        public InternalRegister VRAMaddress = new InternalRegister();
        InternalRegister NextVRAMaddress = new InternalRegister();
        public ControlRegister Control = new ControlRegister();
        public MaskRegister Mask = new MaskRegister();
        public StatusRegister Status = new StatusRegister();
        public float ClockRatio;
        public const int MaxX = 339;
        public int MaxY;
        public int X;
        public int Y = -1;
        byte MemoryLatch;
        byte OpenBus;
        public byte[,] PaletteIndices = new byte[8, 4];
        public int[] Palette = new int[64];

        public TPixmap Screen = new TPixmap(256, 240);

        class TileRow {
            public byte Y;          // Y position 
            public int ID;         // ID of tile from pattern memory
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
            if ((X & 7) != 0) return;
            PrevTile = ActTile;
            ActTile = NextTile;
            NextTile = new TileRow();
            var tableNT = Bus.NameTable[VRAMaddress.NameTableY << 1 | VRAMaddress.NameTableX];
            NextTile.ID = Control.PatternBg << 8 | tableNT[VRAMaddress.CoarseY << 5 | VRAMaddress.CoarseX];
            NextTile.Attribute = tableNT[(VRAMaddress.CoarseY >> 2) << 3 | VRAMaddress.CoarseX >> 2 | 0x3C0];
            NextTile.Attribute >>= (VRAMaddress.CoarseY & 2) << 1 | VRAMaddress.CoarseX & 2;
            NextTile.ReadPattern(Bus, VRAMaddress.FineY);
            VRAMaddress.ScrollX += 8;
        }

        public int NextCycle() { return (int)(((Y + 1) * (MaxX + 2) + X + 1) / ClockRatio); }
        public void Tick()
        {
            if ((Mask.RenderBackground || Mask.RenderSprites))
            {
                if (Y == -1)
                {
                    if (X == 264)
                    {
                        VRAMaddress.ScrollX = NextVRAMaddress.ScrollX;
                        VRAMaddress.ScrollY = NextVRAMaddress.ScrollY;
                        ReadSpriteScanline();
                        X = 312;
                    }
                    else if (X >= 320 && X <= 335)
                        ReadNextTile();
                }
                else if (Y < Screen.Height)
                {
                    if (X < Screen.Width)
                    {
                        ReadNextTile();
                        DrawTileLine();
                    }
                    else if (X == 264)
                    {
                        VRAMaddress.ScrollY++;
                        VRAMaddress.ScrollX = NextVRAMaddress.ScrollX;
                        ReadSpriteScanline();
                        X = 312;
                    }
                    else if (X >= 320 && X <= 335)
                        ReadNextTile();
                }
            }
            X += 8;
            if (X >= MaxX)
            {
                X = 0;
                Y++;
                if (Y == Screen.Height + 1)
                {
                    Scroll.X = NextVRAMaddress.ScrollX;
                    Scroll.Y = NextVRAMaddress.ScrollY;
                    Status.VerticalBlank = true;
                    if (Control.EnableNMI)
                        Bus.CPU.IRQrequest = Bus.CPU.NMI;
                    Y = MaxY;
                    X = MaxX;
                }
                else if (Y > MaxY)
                {
                    Y = -1;
                    X = 264;
                    Status.VerticalBlank = false;
                    Status.SpriteZeroHit = false;
                    Status.SpriteOverflow = false;
                    FrameComplete = true;
                }
            }
        }

        private void DrawTileLine()
        {
            if ((X & 7) != 0) return;
            var renderBackground = Mask.RenderBackground && (X > 0 || Mask.RenderBackgroundLeft);
            var renderSprites = Mask.RenderSprites && (X > 0 || Mask.RenderSpritesLeft);
            for (int x = 0; x < 8; x++)
            {
                int pattern = 0;
                int palette = 0;
                if (renderBackground)
                {
                    var idx = 15 - (x + NextVRAMaddress.FineX);
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

        private void ReadSpriteScanline()
        {
            //OAMaddr = 0;
            SpriteZero = null;
            SpriteScanline = new TileRow[Screen.Width];
            var _spriteCount = 0;
            for (int i = 0; i < OAM.Length; i += 4)
            {
                var spriteSize = Control.SpriteSize16 ? 16: 8;
                var row = Y - OAM[i];
                if (row >= 0 && row < spriteSize)
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
                        row = spriteSize - 1 - row;
                    var tableNo = Control.PatternSprite;
                    if (spriteSize == 16)
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

        public byte CpuRead(int addr)
        {
            switch (addr)
            {
                case 2: // Status
                    OpenBus &= 0x1F;
                    OpenBus |= (byte)(Status.Register & 0xE0);
                    Status.VerticalBlank = false;
                    NextVRAMaddress.Latch = false;
                    break;
                case 4: // OAM Data
                    OpenBus = OAM[OAMaddr];
                    break;
                case 7: // PPU Data
                    addr = VRAMaddress.Register;
                    VRAMaddress.Register = addr + (Control.IncrementMode ? 32 : 1);
                    addr &= 0x3FFF;
                    OpenBus = MemoryLatch;
                    MemoryLatch = addr >= 0x2000 ?
                        Bus.NameTable[(addr >> 10) & 3][addr & 0x03FF] :
                        Bus.ChrTable[addr / Bus.ChrBankSize][addr % Bus.ChrBankSize];                       
                    if (addr >= 0x3F00)
                    {
                        if ((addr & 0x13) == 0x10) addr &= 0x0F; else addr &= 0x1F;
                        OpenBus = PaletteIndices[addr >> 2, addr & 3]; //& (Mask.Greyscale ? 0x30 : 0x3F));
                    }
                    break;
            }
            return OpenBus;
        }
        public void CpuWrite(int addr, byte data)
        {
            OpenBus = data;
            switch (addr)
            {
                case 0: // Control
                    Control.Register = OpenBus;
                    NextVRAMaddress.NameTableX = Control.NameTableX;
                    NextVRAMaddress.NameTableY = Control.NameTableY;
                    break;
                case 1: // Mask
                    Mask.Register = OpenBus;
                    break;
                case 2: // Status
                    Status.Register = OpenBus;
                    break;
                case 3: // OAM Address
                    OAMaddr = OpenBus;
                    break;
                case 4: // OAM Data
                    OAM[OAMaddr] = OpenBus;
                    OAMaddr++;
                    break;
                case 5: // Scroll
                    NextVRAMaddress.ReadScroll(OpenBus);
                    break;
                case 6: // PPU Address
                    NextVRAMaddress.ReadAddress(OpenBus);
                    if (!NextVRAMaddress.Latch)
                        VRAMaddress.Register = NextVRAMaddress.Register;
                    break;
                case 7: // PPU Data
                    addr = VRAMaddress.Register;
                    VRAMaddress.Register = addr + (Control.IncrementMode ? 32 : 1);
                    addr &= 0x3FFF;
                    if (addr <= 0x1FFF && Bus.Mapper.ChrRAMenabled)
                        Bus.ChrTable[addr / Bus.ChrBankSize][addr % Bus.ChrBankSize] = OpenBus;
                    else if (addr <= 0x3EFF)
                        Bus.NameTable[(addr >> 10) & 3][addr & 0x03FF] = OpenBus;
                    else if (addr <= 0x3FFF)
                    {
                        if ((addr & 0x13) == 0x10) addr &= 0xF; else addr &= 0x1F;
                        PaletteIndices[addr >> 2, addr & 3] = OpenBus;
                    }
                    break;
            }
        }

    }
}