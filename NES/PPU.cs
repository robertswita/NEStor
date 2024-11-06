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
        public VRAMregister VRAM = new VRAMregister();
        VRAMregister NextVRAM = new VRAMregister();
        public ControlRegister Control = new ControlRegister();
        public MaskRegister Mask = new MaskRegister();
        public StatusRegister Status = new StatusRegister();
        public float ClockRatio = 3;
        public const int MaxX = 339;
        public int MaxY = 260;
        public int X = -1;
        public int Y = -1;
        public byte OpenBus;
        //public byte[,] PaletteIndices = new byte[8, 4];
        public byte[] PaletteIndices = new byte[32];
        public int[] Palette = new int[64];
        public TPixmap Screen = new TPixmap(256, 240);
        /// <summary>
        /// OAM (ObjectAttributeMemory)
        /// 64 sprite TileRows (ObjectAttributeEntry)[Y, ID, attribute, X]
        /// </summary>
        public byte OAMDMAaddr; 
        public byte OAMaddr;
        public byte[] OAM = new byte[256];
        TileRow[] SpriteScanline = new TileRow[256];
        TileRow PrevTile = new TileRow();
        TileRow ActTile = new TileRow();
        TileRow NextTile = new TileRow();
        TileRow SpriteZero;
        int[] TileLine = new int[8];
        bool PreventVBL;
        bool NMIEnabled;
        public Point FrameScroll;
        public bool OddFrame;
        public int Drift;

        public PPU(Bus bus)
        {
            Bus = bus;
            LoadPalette(NEStor.Properties.Resources.DefaultPalette);
        }

        public void LoadPalette(Bitmap pal)
        {
            var paletteMap = new TPixmap(Palette.Length / 4, 4);
            paletteMap.Image = pal;
            Array.Copy(paletteMap.Pixels, Palette, Palette.Length);
        }

        public TPixmap GetPaletteMap()
        {
            var paletteMap = new TPixmap(Palette.Length / 4, 4);
            Array.Copy(Palette, paletteMap.Pixels, Palette.Length);
            return paletteMap;
        }

        public int GetPixel(int palette, int pattern)
        {
            var mask = Mask.Greyscale ? 0x30 : 0x3F;
            return Palette[PaletteIndices[palette << 2 | pattern] & mask];
        }
        public TPixmap GetTileMap(TileRow tile)
        {
            var tileMap = new TPixmap(8, 8);
            for (var row = 0; row < 8; row++)
            {
                tile.ReadPattern(Bus, row);
                for (var col = 0; col < 8; col++)
                    tileMap[col, row] = GetPixel(tile.Attribute, tile.GetPatternPixel(7 - col));
            }
            return tileMap;
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
            if (X == 320) 
                Bus.mapper.OnSpritelineEnd();
            PrevTile = ActTile;
            ActTile = NextTile;
            NextTile = GetBgTile(VRAM.NameTableY << 1 | VRAM.NameTableX, VRAM.CoarseY, VRAM.CoarseX);
            NextTile.ReadPattern(Bus, VRAM.FineY);
            VRAM.ScrollX += 8;
        }

        public TileRow GetBgTile(int idx, int y, int x)
        {
            var tile = new TileRow();
            var table = Bus.nameTable[idx];
            tile.ID = Control.PatternBg << 8 | table[y << 5 | x];
            tile.Attribute = table[(y >> 2) << 3 | x >> 2 | 0x3C0];
            Bus.mapper.OnFetchTile(tile);
            tile.Attribute >>= (y & 2) << 1 | x & 2;
            tile.Attribute &= 3;
            return tile;
        }

        public float Tick()
        {
            var renderEnabled = (Mask.RenderBackground || Mask.RenderSprites) & OddFrame;
            if (Y == -1)
            {
                if (X == 0)
                {
                    Status.SpriteZeroHit = false;
                    Status.SpriteOverflow = false;
                    Status.VerticalBlank = false;
                    PreventVBL = false;
                    OpenBus = 0;
                    Bus.mapper.OnSpritelineEnd();
                    Bus.mapper.IRQScanline();
                    X = 256;
                }
                else if (X == 256)
                {
                    if (renderEnabled)
                    {
                        VRAM = NextVRAM;
                        ReadSpriteScanline(Y);
                    }
                    X = 320;
                }
                else if (X >= 320 && X < 336)
                {
                    if (renderEnabled)
                        ReadNextTile();
                    X += 8;
                }
                else if (X == 337 && Bus.SystemType == SystemType.NTSC && renderEnabled && OddFrame)
                {
                    Drift--;
                    X++;
                }
                else
                    X++;
            }
            else if (Y < Screen.Height)
            {
                if (X < Screen.Width && (X & 7) == 0)
                {
                    if (X == 0)
                    {
                        VRAM.A12Toggled = false;
                        Bus.mapper.IRQScanline();
                    }
                    if (renderEnabled)
                    {
                        ReadNextTile();
                        DrawTileLine(Y, X);
                    }
                    X += 8;
                }
                else if (X == 256)
                {
                    if (renderEnabled)
                    {
                        VRAM.ScrollY++;
                        if (VRAM.CoarseY == 0 && VRAM.FineY == 0)
                            VRAM.NameTableY ^= 1;
                        else if (VRAM.CoarseY == 30)
                            VRAM.ScrollY += 16;
                        VRAM.ScrollX = NextVRAM.ScrollX;
                        ReadSpriteScanline(Y);
                    }
                    X = 320;
                }
                else if (X >= 320 && X < 336)
                {
                    if (renderEnabled)
                        ReadNextTile();
                    X += 8;
                }
                else
                    X++;
            }
            else if (Y == Screen.Height + 1)
            {
                if (X == 0)
                {
                    FrameScroll.X = NextVRAM.ScrollX;
                    FrameScroll.Y = NextVRAM.ScrollY;
                    if (!PreventVBL)
                        Status.VerticalBlank = true;
                    X = 2;
                }
                else if (X == 2)
                {
                    //condition ensures correct NMI disable delay when done near time when VBL
                    // is set - BLARGG test 08 - nmi_off_timing.rom
                    NMIEnabled = Control.EnableNMI;
                    X = 14;
                }
                else if (X == 14)
                {
                    if (NMIEnabled && !PreventVBL)
                    {
                        //Bus.cpu.NMIDelay = 1;
                        Bus.cpu.NMI();
                    }
                    Y = MaxY;
                    X = MaxX;
                }
                else
                    X++;
            }
            else
                X = MaxX + 1;
            if (X > MaxX)
            {
                X = -1;
                Y++;
                if (Y > MaxY)
                {
                    Y = -1;
                    var maxDot = (MaxX + 2) * (MaxY + 2) + Drift;
                    Drift = maxDot % 3;
                    OddFrame = !OddFrame;
                    FrameComplete = true;
                    return maxDot / ClockRatio;
                }
            }
            var nextDot = (MaxX + 2) * (Y + 1) + X + 1 + Drift;
            return nextDot / ClockRatio;
        }

        public float TotalCycles()
        {
            var maxDot = (MaxX + 2) * (MaxY + 2) + Drift;
            return maxDot / ClockRatio;
        }

        private void DrawTileLine(int Y, int X)
        {
            //if (!OddFrame) { return; }
            var renderBackground = Mask.RenderBackground && (X > 0 || Mask.RenderBackgroundLeft);
            var renderSprites = Mask.RenderSprites && (X > 0 || Mask.RenderSpritesLeft);
            for (int x = 0; x < 8; x++)
            {
                int pattern = 0;
                int palette = 0;
                if (renderBackground)
                {
                    var idx = 15 - (x + NextVRAM.FineX);
                    var patternTile = idx < 8 ? ActTile : PrevTile;
                    pattern = patternTile.GetPatternPixel(idx & 7);
                    if (pattern > 0)
                        palette = patternTile.Attribute;
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
            Bus.mapper.OnSpritelineStart();
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
                        Status.SpriteOverflow = true;
                    _spriteCount++;
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
                }
            }
        }

        public byte Peek(int addr)
        {
            int data = OpenBus;
            switch (addr)
            {
                case 2: // Status
                    data &= 0x1F;
                    data |= (byte)(Status.Register & 0xE0);
                    Status.VerticalBlank = false;
                    NextVRAM.Latched = false;
                    // Reading PPUSTATUS one clock before the start of vertical blank will read as clear
                    // and never set the flag or generate an NMI for that frame
                    PreventVBL = (Y == Screen.Height + 1 && X >= 0 && X <= 2);
                    break;
                case 4: // OAM Data
                    data = OAM[OAMaddr];
                    //data = (byte)(OAM[OAMaddr] & 0xE3);
                    break;
                case 7: // PPU Data
                    data = VRAMdata;
                    break;
            }
            OpenBus = (byte)data;
            return OpenBus;
        }
        public void Poke(int addr, byte data)
        {
            OpenBus = data;
            switch (addr)
            {
                case 0: // Control
                    var prevNMI = Control.EnableNMI;
                    Control.Register = data;
                    NextVRAM.NameTableX = Control.NameTableX;
                    NextVRAM.NameTableY = Control.NameTableY;
                    // condition ensures correct NMI occurrence when enabled near time
                    // VBL flag is cleared - BLARGG test 07-nmi_on_timing.rom
                    bool supressNMI = (Y == -1 && X == 0);
                    if (!prevNMI && Control.EnableNMI && Status.VerticalBlank && !supressNMI)
                    {
                        Bus.cpu.NMIDelay = 1;
                        Bus.cpu.NMI();
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
                    NextVRAM.SetScroll(data);
                    break;
                case 6: // PPU Address
                    NextVRAM.SetAddress(data);
                    if (!NextVRAM.Latched)
                    {
                        VRAM = NextVRAM;
                        Bus.mapper.IRQScanline();
                    }
                    break;
                case 7: // PPU Data
                    VRAMdata = data;
                    break;
            }
        }

        byte VRAMdata
        {
            get
            {
                var addr = VRAM.Address;
                VRAM.Address = addr + Control.Increment;
                Bus.mapper.IRQScanline();
                int data = Control.DataLatch;
                addr &= 0x3FFF;
                if (addr >= 0x3F00)
                {
                    var idx = (addr & 3) == 0 ? addr & 0xF : addr & 0x1F;
                    data = PaletteIndices[idx] | OpenBus & 0xC0;
                }
                Control.DataLatch = addr >= 0x2000 ?
                    Bus.nameTable[(addr >> 10) & 3][addr & 0x03FF] :
                    Bus.GetPattern(addr);
                return (byte)data;
            }
            set
            {
                var addr = VRAM.Address;
                VRAM.Address = addr + Control.Increment;
                Bus.mapper.IRQScanline();
                addr &= 0x3FFF;
                if (addr <= 0x1FFF && Bus.mapper.ChrRAMenabled)
                    Bus.SetPattern(addr, value);
                else if (addr <= 0x3EFF)
                    Bus.nameTable[(addr >> 10) & 3][addr & 0x03FF] = value;
                else if (addr <= 0x3FFF)
                {
                    var idx = (addr & 3) == 0 ? addr & 0xF : addr & 0x1F;
                    PaletteIndices[idx] = value;
                }
            }
        }

    }
}