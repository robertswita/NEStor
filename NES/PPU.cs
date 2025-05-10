using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime;
using System.Windows.Forms;
using Common;
using PPU;

namespace NES
{
    class PPU
    {
        Bus Bus;
        public bool FrameComplete;
        public VramRegister Vram = new VramRegister();
        VramRegister NextVram = new VramRegister();
        public ControlRegister Control = new ControlRegister();
        public MaskRegister Mask = new MaskRegister();
        public StatusRegister Status = new StatusRegister();
        public float ClockRatio = 3;
        public int MaxX = Bus.ModelParams.PpuMaxX;
        public int MaxY = 260;
        public int X;
        public int Y = -1;
        byte OpenBus;
        public byte[] PaletteIndices = new byte[32];
        public int[] Palette = new int[64];
        public TPixmap Screen = new TPixmap(256, 240);
        public byte OamDmaAddr; 
        public byte OamAddr;
        public byte[] Oam = new byte[256];
        public TileRow[] SpriteScanline = new TileRow[256];
        public List<TileRow>[] SpriteLines;
        TileRow PrevTile = new TileRow();
        TileRow ActTile = new TileRow();
        TileRow NextTile = new TileRow();
        TileRow SpriteZero;
        int[] TileLine = new int[8];
        bool PreventVBL;
        public Point FrameScroll;
        public bool OddFrame;
        public int Drift = 2;

        public PPU(Bus bus)
        {
            Bus = bus;
            LoadPalette(NEStor.Properties.Resources.DefaultPalette);
            SpriteLines = new List<TileRow>[Screen.Height];
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
            PrevTile = ActTile;
            ActTile = NextTile;
            NextTile = GetBgTile(Vram.NameTableY << 1 | Vram.NameTableX, Vram.CoarseY, Vram.CoarseX);
            //NextTile.ReadPattern(Bus, Vram.FineY);
            Vram.ScrollX += 8;
        }

        public TileRow GetBgTile(int idx, int y, int x)
        {
            var tile = new TileRow();
            var table = Bus.NameTables.Banks[idx];
            tile.ID = Control.PatternBg << 8 | table[y << 5 | x];
            tile.Attribute = table[(y >> 2) << 3 | x >> 2 | 0x3C0];
            //Bus.Mapper.OnFetchTile(tile);
            tile.ReadPattern(Bus, Vram.FineY);
            tile.Attribute >>= (y & 2) << 1 | x & 2;
            tile.Attribute &= 3;
            return tile;
        }

        public void Step()
        {
            while ((int)NextCycle <= Bus.Cpu.Cycle)
                FrameStep();
        }

        float NextCycle;
        public void FrameStep()
        {
            var renderEnabled = Mask.RenderBackground || Mask.RenderSprites;
            if (Y == -1)
            {
                if (X == 0)
                {
                    Status.SpriteZeroHit = false;
                    Status.SpriteOverflow = false;
                    Status.VerticalBlank = false;
                    PreventVBL = false;
                    OpenBus = 0;
                    Bus.Mapper.OnSpritelineEnd();
                    Bus.Mapper.IrqScanline();
                    X = 256;
                }
                else if (X == 256)
                {
                    if (renderEnabled)
                    {
                        Vram = NextVram;
                        Bus.Mapper.OnSpritelineStart();
                        ReadSpriteLines();
                    }
                    X = 320;
                }
                else if (X >= 320 && X < 336)
                {
                    if (renderEnabled)
                    {
                        if (X == 320) Bus.Mapper.OnSpritelineEnd();
                        ReadNextTile();
                    }
                    X += 8;
                }
                else if (X == 336) X++;
                else if (X == 337)
                {
                    if (Bus.SystemType == SystemType.NTSC && renderEnabled && OddFrame)
                        Drift--;
                    Y = 0; X = 0;
                }
            }
            else if (Y < Screen.Height)
            {
                if (X < Screen.Width && (X & 7) == 0)
                {
                    if (renderEnabled)
                    {
                        if (X == 0)
                        {
                            Vram.A12Toggled = false;
                            Bus.Mapper.IrqScanline();
                        }
                        ReadNextTile();
                    }
                    DrawTileLine(Y, X);
                    X += 8;
                }
                else if (X == 256)
                {
                    if (renderEnabled)
                    {
                        Vram.ScrollY++;
                        if (Vram.CoarseY == 0 && Vram.FineY == 0)
                            Vram.NameTableY ^= 1;
                        else if (Vram.CoarseY == 30)
                            Vram.ScrollY += 16;
                        Vram.ScrollX = NextVram.ScrollX;
                        Bus.Mapper.OnSpritelineStart();
                        ReadSpriteScanline();
                    }
                    X = 320;
                }
                else if (X >= 320 && X < 336)
                {
                    if (renderEnabled)
                    {
                        if (X == 320) Bus.Mapper.OnSpritelineEnd();
                        ReadNextTile();
                    }
                    X += 8;
                }
                else { Y++; X = 0; }
            }
            else if (Y == Screen.Height) { Y++; X = -1; }
            else if (Y == Screen.Height + 1)
            {
                if (X == -1) X++;
                else if (X == 0)
                {
                    FrameScroll.X = NextVram.ScrollX;
                    FrameScroll.Y = NextVram.ScrollY;
                    if (!PreventVBL)
                        Status.VerticalBlank = true;
                    X = 2;
                }
                else if (X == 2) //NMI disable delay - BLARGG test 08 - nmi_off_timing
                {
                    if (Control.EnableNMI && !PreventVBL)
                        Bus.Cpu.Nmi.Start(1);
                    Y = MaxY + 1; X = -1;
                }
            }
            else if (Y > MaxY)
            {
                FrameComplete = true;
                var cyclesCount = (MaxX + 2) * (MaxY + 2);
                Drift = (cyclesCount + Drift) % 3;
                OddFrame = !OddFrame;
                Bus.Cpu.ResetCycles();
                Y = -1; X = 0;
            }
            NextCycle = ((MaxX + 2) * (Y + 1) + X + 1 + Drift) / ClockRatio;
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
                    var idx = 15 - (x + NextVram.FineX);
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

        void ReadSpriteLines()
        {
            SpriteLines = new List<TileRow>[Screen.Height];
            for (int i = 0; i < Oam.Length; i += 4)
            {
                var sprite = new TileRow();
                if (i == OamAddr)
                    SpriteZero = sprite;
                sprite.Y = Oam[i];
                sprite.ID = Oam[i + 1];
                sprite.Attribute = Oam[i + 2];
                sprite.X = Oam[i + 3];
                for (int row = sprite.Y; row < sprite.Y + Control.SpriteSize; row++)
                {
                    if (row >= SpriteLines.Length) break;
                    if (SpriteLines[row] == null) SpriteLines[row] = new List<TileRow>();
                    SpriteLines[row].Add(sprite);
                }
            }
            //SpriteScanline = new TileRow[Screen.Width];
        }

        void ReadSpriteScanline()
        {
            SpriteScanline = new TileRow[Screen.Width];
            OamAddr = 0;
            var line = SpriteLines[Y];
            if (line == null) return;
            if (line.Count > 8)
                Status.SpriteOverflow = true;
            foreach (var sprite in line)
            {
                var row = Y - sprite.Y;
                var flipX = (sprite.Attribute & 0x40) != 0;
                var flipY = (sprite.Attribute & 0x80) != 0;
                if (flipY)
                    row = Control.SpriteSize - 1 - row;
                var tableNo = Control.PatternSprite;
                var id = sprite.ID;
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
                sprite.ID = id;
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

        private void ReadSpriteScanline(int y)
        {
            SpriteZero = null;
            SpriteScanline = new TileRow[Screen.Width];
            var spriteCount = 0;
            OamAddr = 0;
            for (int i = OamAddr; i < Oam.Length; i += 4)
            {
                var row = y - Oam[i];
                if (row >= 0 && row < Control.SpriteSize)
                {
                    var sprite = new TileRow();
                    if (i == OamAddr)
                        SpriteZero = sprite;
                    sprite.Y = Oam[i];
                    sprite.ID = Oam[i + 1];
                    sprite.Attribute = Oam[i + 2];
                    sprite.X = Oam[i + 3];
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
                    spriteCount++;
                    if (spriteCount > 8)
                        Status.SpriteOverflow = true;
                }
            }
            OamAddr = 0;
        }

        public byte Peek(int addr)
        {
            int data = OpenBus;
            switch (addr)
            {
                case 2:
                    data &= 0x1F;
                    data |= (byte)(Status.Register & 0xE0);
                    Status.VerticalBlank = false;
                    NextVram.Latched = false;
                    // Reading PPUSTATUS one clock before the start of vertical blank will read as clear
                    // and never set the flag or generate an NMI for that frame
                    //PreventVBL = (Y == Screen.Height + 1 && X >= 0 && X <= 2);
                    //var vBlankCycle = (MaxX + 2) * (Screen.Height + 2) + Drift + 3;
                    //var actCycle = (Bus.Cpu.Cycle) * ClockRatio;
                    //PreventVBL = actCycle > vBlankCycle && actCycle < vBlankCycle + 4;
                    PreventVBL = Y == Screen.Height + 1 && X >= 0;
                    break;
                case 4:
                    if (Mask.RenderSprites && Y >= 0 && Y < Screen.Height)
                    {
                        data = 0xFF;
                        if (X > 256 && X < 320)
                        {
                            var line = SpriteLines[Y];
                            var x = X - 256;
                            var idx = x / 8;
                            if (idx < line.Count)
                            {
                                var sprite = line[idx];
                                switch (x % 8)
                                {
                                    case 0: data = sprite.Y; break;
                                    case 1: data = sprite.ID; break;
                                    case 2: data = sprite.Attribute; break;
                                    case 3: data = sprite.X; break;
                                }
                            }
                        }
                    }
                    else
                        data = Oam[OamAddr];
                    if (OamAddr % 4 == 2)
                        data &= 0xE3;
                    break;
                case 7:
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
                case 0:
                    var prevNMI = Control.EnableNMI;
                    Control.Register = data;
                    NextVram.NameTableX = Control.NameTableX;
                    NextVram.NameTableY = Control.NameTableY;
                    // NMI occurrence when enabled - BLARGG test 07-nmi_on_timing
                    bool supressNMI = Y == -1 && X == 0;
                    if (!prevNMI && Control.EnableNMI && Status.VerticalBlank && !supressNMI)
                        Bus.Cpu.Nmi.Start(2);
                    break;
                case 1:
                    Mask.Register = data;
                    break;
                case 3:
                    OamAddr = data;
                    break;
                case 4:
                    Oam[OamAddr] = data;
                    OamAddr++;
                    break;
                case 5:
                    NextVram.SetScroll(data);
                    break;
                case 6:
                    NextVram.SetAddress(data);
                    if (!NextVram.Latched)
                    {
                        Vram = NextVram;
                        Bus.Mapper.IrqScanline();
                    }
                    break;
                case 7:
                    VRAMdata = data;
                    break;
            }
        }

        byte VRAMdata
        {
            get
            {
                var addr = Vram.Address;
                Vram.Address = addr + Control.Increment;
                Bus.Mapper.IrqScanline();
                int data = Control.DataLatch;
                addr &= 0x3FFF;
                if (addr >= 0x3F00)
                {
                    var idx = (addr & 3) == 0 ? addr & 0xF : addr & 0x1F;
                    data = PaletteIndices[idx] | OpenBus & 0xC0;
                }
                Control.DataLatch = addr >= 0x2000 ? Bus.NameTables[addr & 0xFFF] : Bus.ChrMemory[addr];
                return (byte)data;
            }
            set
            {
                var addr = Vram.Address;
                Vram.Address = addr + Control.Increment;
                Bus.Mapper.IrqScanline();
                addr &= 0x3FFF;
                if (addr <= 0x1FFF && Bus.Mapper.ChrRamEnabled)
                    Bus.ChrMemory[addr] = value;
                else if (addr <= 0x3EFF)
                    Bus.NameTables[addr & 0xFFF] = value;
                else if (addr <= 0x3FFF)
                {
                    var idx = (addr & 3) == 0 ? addr & 0xF : addr & 0x1F;
                    PaletteIndices[idx] = value;
                }
            }
        }

    }
}