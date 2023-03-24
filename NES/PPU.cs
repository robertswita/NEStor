using System;
using System.Drawing;
using Common;
using PPU;

namespace NES
{
    class PPU
    {
        public bool FrameComplete;
        //public bool DebugAtScanline;

        public InternalRegister VRAMaddress = new InternalRegister();
        InternalRegister NextVRAMaddress = new InternalRegister();
        public ControlRegister Control = new ControlRegister();
        public MaskRegister Mask = new MaskRegister();
        public StatusRegister Status = new StatusRegister();
        public float ClockRatio;
        public const int MaxX = 339;
        public int MaxY;


        /// <summary>
        /// OAM (ObjectAttributeMemory)
        /// 64 sprites (ObjectAttributeEntry)[Y, id, attribute, X]
        /// </summary>
        class TileRow {
            public byte Y;          // Y position 
            public byte ID;         // ID of tile from pattern memory
            public byte Attribute;  // how sprite should be rendered
            public byte X;          // X position
            public int LSB;
            public int MSB;
            public int GetPixel(int idx)
            {
                int lo = (LSB >> idx) & 0x1;
                int hi = (MSB >> idx) & 0x1;
                return hi << 1 | lo;
            }
        }

        public byte[] OAM = new byte[256];
        TileRow[] SpriteScanline = new TileRow[256];

        //public bool NMI;

        public int X;
        public int Y;
        Bus Bus;
        public byte OAMaddr;
        byte DataBuffer;
        byte[] Palette = new byte[32];
        int[] PalScreen = new int[64];

        public TPixmap Screen = new TPixmap(256, 240);
        public Point Scroll;

        TileRow PrevTile = new TileRow();
        TileRow ActTile = new TileRow();
        TileRow NextTile = new TileRow();
        TileRow SpriteZero;
        int[] TileLine = new int[8];

        public PPU(Bus bus)
        {
            Bus = bus;

            PalScreen[0x00] = Color.FromArgb(84, 84, 84).ToArgb();
            PalScreen[0x01] = Color.FromArgb(0, 30, 116).ToArgb();
            PalScreen[0x02] = Color.FromArgb(8, 16, 144).ToArgb();
            PalScreen[0x03] = Color.FromArgb(48, 0, 136).ToArgb();
            PalScreen[0x04] = Color.FromArgb(68, 0, 100).ToArgb();
            PalScreen[0x05] = Color.FromArgb(92, 0, 48).ToArgb();
            PalScreen[0x06] = Color.FromArgb(84, 4, 0).ToArgb();
            PalScreen[0x07] = Color.FromArgb(60, 24, 0).ToArgb();
            PalScreen[0x08] = Color.FromArgb(32, 42, 0).ToArgb();
            PalScreen[0x09] = Color.FromArgb(8, 58, 0).ToArgb();
            PalScreen[0x0A] = Color.FromArgb(0, 64, 0).ToArgb();
            PalScreen[0x0B] = Color.FromArgb(0, 60, 0).ToArgb();
            PalScreen[0x0C] = Color.FromArgb(0, 50, 60).ToArgb();
            PalScreen[0x0D] = Color.FromArgb(0, 0, 0).ToArgb();
            PalScreen[0x0E] = Color.FromArgb(0, 0, 0).ToArgb();
            PalScreen[0x0F] = Color.FromArgb(0, 0, 0).ToArgb();

            PalScreen[0x10] = Color.FromArgb(152, 150, 152).ToArgb();
            PalScreen[0x11] = Color.FromArgb(8, 76, 196).ToArgb();
            PalScreen[0x12] = Color.FromArgb(48, 50, 236).ToArgb();
            PalScreen[0x13] = Color.FromArgb(92, 30, 228).ToArgb();
            PalScreen[0x14] = Color.FromArgb(136, 20, 176).ToArgb();
            PalScreen[0x15] = Color.FromArgb(160, 20, 100).ToArgb();
            PalScreen[0x16] = Color.FromArgb(152, 34, 32).ToArgb();
            PalScreen[0x17] = Color.FromArgb(120, 60, 0).ToArgb();
            PalScreen[0x18] = Color.FromArgb(84, 90, 0).ToArgb();
            PalScreen[0x19] = Color.FromArgb(40, 114, 0).ToArgb();
            PalScreen[0x1A] = Color.FromArgb(8, 124, 0).ToArgb();
            PalScreen[0x1B] = Color.FromArgb(0, 118, 40).ToArgb();
            PalScreen[0x1C] = Color.FromArgb(0, 102, 120).ToArgb();
            PalScreen[0x1D] = Color.FromArgb(0, 0, 0).ToArgb();
            PalScreen[0x1E] = Color.FromArgb(0, 0, 0).ToArgb();
            PalScreen[0x1F] = Color.FromArgb(0, 0, 0).ToArgb();

            PalScreen[0x20] = Color.FromArgb(236, 238, 236).ToArgb();
            PalScreen[0x21] = Color.FromArgb(76, 154, 236).ToArgb();
            PalScreen[0x22] = Color.FromArgb(120, 124, 236).ToArgb();
            PalScreen[0x23] = Color.FromArgb(176, 98, 236).ToArgb();
            PalScreen[0x24] = Color.FromArgb(228, 84, 236).ToArgb();
            PalScreen[0x25] = Color.FromArgb(236, 88, 180).ToArgb();
            PalScreen[0x26] = Color.FromArgb(236, 106, 100).ToArgb();
            PalScreen[0x27] = Color.FromArgb(212, 136, 32).ToArgb();
            PalScreen[0x28] = Color.FromArgb(160, 170, 0).ToArgb();
            PalScreen[0x29] = Color.FromArgb(116, 196, 0).ToArgb();
            PalScreen[0x2A] = Color.FromArgb(76, 208, 32).ToArgb();
            PalScreen[0x2B] = Color.FromArgb(56, 204, 108).ToArgb();
            PalScreen[0x2C] = Color.FromArgb(56, 180, 204).ToArgb();
            PalScreen[0x2D] = Color.FromArgb(60, 60, 60).ToArgb();
            PalScreen[0x2E] = Color.FromArgb(0, 0, 0).ToArgb();
            PalScreen[0x2F] = Color.FromArgb(0, 0, 0).ToArgb();

            PalScreen[0x30] = Color.FromArgb(236, 238, 236).ToArgb();
            PalScreen[0x31] = Color.FromArgb(168, 204, 236).ToArgb();
            PalScreen[0x32] = Color.FromArgb(188, 188, 236).ToArgb();
            PalScreen[0x33] = Color.FromArgb(212, 178, 236).ToArgb();
            PalScreen[0x34] = Color.FromArgb(236, 174, 236).ToArgb();
            PalScreen[0x35] = Color.FromArgb(236, 174, 212).ToArgb();
            PalScreen[0x36] = Color.FromArgb(236, 180, 176).ToArgb();
            PalScreen[0x37] = Color.FromArgb(228, 196, 144).ToArgb();
            PalScreen[0x38] = Color.FromArgb(204, 210, 120).ToArgb();
            PalScreen[0x39] = Color.FromArgb(180, 222, 120).ToArgb();
            PalScreen[0x3A] = Color.FromArgb(168, 226, 144).ToArgb();
            PalScreen[0x3B] = Color.FromArgb(152, 226, 180).ToArgb();
            PalScreen[0x3C] = Color.FromArgb(160, 214, 228).ToArgb();
            PalScreen[0x3D] = Color.FromArgb(160, 162, 160).ToArgb();
            PalScreen[0x3E] = Color.FromArgb(0, 0, 0).ToArgb();
            PalScreen[0x3F] = Color.FromArgb(0, 0, 0).ToArgb();
        }

        public int GetColorFromPalette(int palette, int pixel)
        {
            return PalScreen[PpuRead(0x3F00 + (palette << 2) + pixel)];
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
            //if ((X & 7) != 0) return;
            PrevTile = ActTile;
            ActTile = NextTile;
            NextTile = new TileRow();
            var tableNT = Bus.NameTable[VRAMaddress.NameTableY << 1 | VRAMaddress.NameTableX];
            NextTile.ID = tableNT[VRAMaddress.CoarseY << 5 | VRAMaddress.CoarseX];
            NextTile.Attribute = tableNT[(VRAMaddress.CoarseY >> 2) << 3 | VRAMaddress.CoarseX >> 2 | 0x3C0];
            NextTile.Attribute >>= (VRAMaddress.CoarseY & 2) << 1 | VRAMaddress.CoarseX & 2;
            var tableCHR = Bus.PatternTable[Control.PatternBg];
            var tileLine = NextTile.ID << 4 | VRAMaddress.FineY;
            NextTile.LSB = tableCHR[tileLine];
            NextTile.MSB = tableCHR[tileLine | 0x8];
            VRAMaddress.ScrollX += 8;
        }

        public int NextCycle() { return (int)(((Y + 1) * (MaxX + 2) + X + 1) / ClockRatio); }

        public void Clock()
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
                else if (Y < 240)
                {
                    if (X < 256)
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
                if (Y == 241)
                {
                    Scroll.X = NextVRAMaddress.ScrollX;
                    Scroll.Y = NextVRAMaddress.ScrollY;
                    Status.VerticalBlank = true;
                    if (Control.EnableNMI)
                        Bus.CPU.IRQrequest = Bus.CPU.NMI;
                    Y = MaxY;
                    X = MaxX;
                    return;
                }
                else if (Y > MaxY)
                {
                    Y = -1;
                    X = 264;
                    Status.VerticalBlank = false;
                    Status.SpriteZeroHit = false;
                    Status.SpriteOverflow = false;
                    FrameComplete = true;
                    return;
                }
            }
        }

        private void DrawTileLine()
        {
            //if ((X & 7) != 0) return;
            for (int x = 0; x < 8; x++)
            {
                int pattern = 0;
                int palette = 0;
                if (Mask.RenderBackground)
                {
                    var idx = 15 - (x + NextVRAMaddress.FineX);
                    var patternTile = idx < 8 ? ActTile : PrevTile;
                    pattern = patternTile.GetPixel(idx & 7);
                    if (pattern > 0)
                        palette = patternTile.Attribute & 0x03;
                }
                if (Mask.RenderSprites)
                {
                    var sprite = SpriteScanline[X + x];
                    if (sprite != null)
                    {
                        if (sprite == SpriteZero && pattern > 0)
                            Status.SpriteZeroHit = true;
                        var fg_priority = (sprite.Attribute & 0x20) == 0;
                        if (fg_priority || pattern == 0)
                        {
                            int idx = X + x - sprite.X;
                            pattern = sprite.GetPixel(7 - idx);
                            palette = sprite.Attribute & 0x03 | 0x04;
                        }
                    }
                }
                TileLine[x] = PalScreen[Palette[palette << 2 | pattern] & 0x3F];
            }
            Array.Copy(TileLine, 0, Screen.Pixels, Y * Screen.Width + X, TileLine.Length);
        }

        private void ReadSpriteScanline()
        {
            SpriteZero = null;
            SpriteScanline = new TileRow[256];
            var _spriteCount = 0;
            for (int i = 0; i < OAM.Length; i += 4)
            {
                var sprite = new TileRow();
                sprite.Y =          OAM[i];
                sprite.ID =         OAM[i + 1];
                sprite.Attribute =  OAM[i + 2];
                sprite.X =          OAM[i + 3];
                var spriteSize = Control.SpriteSize == 0 ? 8 : 16;
                if (spriteSize > 8)
                    spriteSize = 16;
                var row = Y - sprite.Y;
                if (row >= 0 && row < spriteSize)
                {
                    if (_spriteCount == 8)
                    {
                        Status.SpriteOverflow = true;
                        break;
                    }
                    if (i == 0)
                        SpriteZero = sprite;
                    var flipX = (sprite.Attribute & 0x40) != 0;
                    var flipY = (sprite.Attribute & 0x80) != 0;
                    if (flipY)
                        row = spriteSize - 1 - row;
                    var tableNo = Control.PatternSprite;
                    var tileNo = sprite.ID;
                    if (Control.SpriteSize > 0) // 8x16 sprite mode
                    {
                        tableNo = tileNo & 1;
                        tileNo &= 0xFE;
                        if (row > 7)
                        {
                            tileNo++;
                            row &= 7;
                        }
                    }
                    var tableCHR = Bus.PatternTable[tableNo];
                    sprite.LSB = tableCHR[tileNo << 4 | row];
                    sprite.MSB = tableCHR[tileNo << 4 | row | 0x8];
                    if (flipX)
                    {
                        sprite.LSB = (byte)Flipbyte(sprite.LSB);
                        sprite.MSB = (byte)Flipbyte(sprite.MSB);
                    }
                    for (int j = 0; j < 8; j++)
                    {
                        var x = sprite.X + j;
                        if (x >= SpriteScanline.Length) break;
                        if (SpriteScanline[x] != null) continue;
                        if (sprite.GetPixel(7 - j) != 0)
                            SpriteScanline[x] = sprite;
                    }
                    _spriteCount++;
                }
            }
            Bus.Mapper.PPUSync();
        }

        public void Reset()
        {
            DataBuffer = 0x00;
            Y = -1;
            X = 0;
            Status.Register = 0;
            Mask.Register = 0;
            Control.Register = 0;
            VRAMaddress.Register = 0;
            NextVRAMaddress.Register = 0;
            //debug
            Scroll.X = 0;
            Scroll.Y = 0;
        }

        TPixmap GetTileMap(int tableIdx, int tileIdx, int palette)
        {
            var table = Bus.PatternTable[tableIdx];
            TPixmap tileMap = new TPixmap(8, 8);
            for (var row = 0; row < 8; row++)
            {
                var tileRow = new TileRow();
                var address = tileIdx << 4 | row;
                tileRow.LSB = table[address];
                tileRow.MSB = table[address | 8];
                for (var col = 0; col < 8; col++)
                    tileMap[col, row] = GetColorFromPalette(palette, tileRow.GetPixel(7 - col));
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

        // TO JEST PROWIZORYCZNY WYGLAD, PALETY MOGA SIE NIE ZGADZAC, ale
        // co do umieszczania kafelka to juz tak(bo tylko te informacje sa w danym momencie w RAM'ie) :)
        /// <summary>
        /// Debug view of the specific name table
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

        // CPU <---> PPU
        public byte CpuRead(int addr)
        {
            int data = 0;
            switch (addr)
            {
                case 0: // Control
                    data = Control.Register;
                    break;
                case 1: // Mask
                    data = Mask.Register;
                    break;
                case 2: // Status
                    data = Status.Register & 0xE0 | DataBuffer & 0x1F;
                    Status.VerticalBlank = false;
                    VRAMaddress.Latch = false;
                    break;
                case 3: // OAM Address
                    // doesn't make any sense to read from the addr register
                    break;
                case 4: // OAM Data
                    data = OAM[OAMaddr];
                    break;
                case 5: // Scroll
                    break;
                case 6: // PPU Address
                    break;
                case 7: // PPU Data
                    var reg = VRAMaddress.Register;
                    var nextData = PpuRead(reg);
                    if (reg >= 0x3F00)
                        data = DataBuffer;
                    else
                    {
                        data = DataBuffer;
                        DataBuffer = nextData;
                    }
                    VRAMaddress.Register = reg + (Control.IncrementMode ? 32 : 1);
                    break;
            }
            return (byte)data;
        }

        public void CpuWrite(int addr, byte data)
        {
            switch (addr)
            {
                case 0: // Control
                    Control.Register = data;
                    NextVRAMaddress.NameTableX = Control.NameTableX;
                    NextVRAMaddress.NameTableY = Control.NameTableY;
                    break;
                case 1: // Mask
                    Mask.Register = data; // DEBUG
                    break;
                case 2: // Status
                    Status.Register = data;
                    break;
                case 3: // OAM Address
                    OAMaddr = data;
                    break;
                case 4: // OAM Data
                    OAM[OAMaddr] = data;
                    break;
                case 5: // Scroll
                    NextVRAMaddress.ReadScroll(data);
                    break;
                case 6: // PPU Address
                    NextVRAMaddress.ReadAddress(data);
                    VRAMaddress.Register = NextVRAMaddress.Register;
                    break;
                case 7: // PPU Data
                    PpuWrite(VRAMaddress.Register, data);
                    VRAMaddress.Register += Control.IncrementMode ? 32 : 1;
                    break;
            }
        }

        // PPU <---> PPU_BUS
        private byte PpuRead(int addr)
        {
            byte data = 0x00;
            addr &= 0x3FFF;

            if (addr <= 0x1FFF)
            {
                data = Bus.PatternTable[addr >> 12][addr & 0xFFF];
            }
            else if (addr <= 0x3EFF)
            {
                data = Bus.NameTable[(addr >> 10) & 3][addr & 0x03FF];
            }
            else if (addr <= 0x3FFF)
            {
                addr &= 0x1F;
                if (addr >= 16 && addr % 4 == 0)
                    addr -= 16;
                data = (byte)(Palette[addr] & (Mask.Greyscale ? 0x30 : 0x3F));
            }
            return data;
        }

        private void PpuWrite(int addr, byte data)
        {
            addr &= 0x3FFF;

            if (addr <= 0x1FFF)
            {
                Bus.PatternTable[addr >> 12][addr & 0xFFF] = data;
            }
            else if (addr <= 0x3EFF)
            {
                Bus.NameTable[(addr >> 10) & 3][addr & 0x03FF] = data;
            }
            else if (addr <= 0x3FFF)
            {
                addr &= 0x1F;
                if (addr >= 16 && addr % 4 == 0)
                    addr -= 16;
                Palette[addr] = data;
            }
        }

    }
}