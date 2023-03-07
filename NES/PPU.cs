using System;
using System.Drawing;
using Common;

namespace NES
{
    class PPU
    {
        public bool FrameComplete;
        public bool IsOddFrame;
        //public bool DebugAtScanline;

        public readonly PPUInternalRegister VramAddr = new PPUInternalRegister();
        public readonly PPUInternalRegister TramAddr = new PPUInternalRegister();

        public readonly PPUControlFlags Control = new PPUControlFlags();
        public readonly PPUMaskFlags Mask = new PPUMaskFlags();
        public readonly PPUStatusFlags Status = new PPUStatusFlags();

        /// <summary>
        /// OAM (ObjectAttributeMemory)
        /// 64 sprites (ObjectAttributeEntry)[Y, id, attribute, X]
        /// </summary>
        public struct TileRow {
            public byte Y;          // Y position 
            public byte ID;         // ID of tile from pattern memory
            public byte Attribute;  // how sprite should be rendered
            public byte X;          // X position
            public byte LSB;
            public byte MSB;
            public int GetPixel(int idx)
            {
                int pixel_lo = (LSB >> idx) & 1;
                int pixel_hi = (MSB >> idx) & 1;
                return pixel_hi << 1 | pixel_lo;
            }
        }
        public byte[] OAM = new byte[256];
        private byte _oamAddr;

        private readonly TileRow[] SpriteScanline = new TileRow[8];
        private byte _spriteCount;

        //public bool NMI;

        public Point Scroll;

        private int _cycle;
        private int _scanline;
        private readonly Bus _bus;

        // Nametable RAM
        private readonly byte[][] _tblName = { new byte[1024], new byte[1024] };
        // 
        private readonly byte[] _tblPalette = new byte[32];

        // most of the time, not used
        private readonly byte[][] _tblPattern = { new byte[4096], new byte[4096] };

        private readonly int[] _palScreen = new int[0x40]; // 64

        public TPixmap Screen = new TPixmap(256, 240);

        private byte _addressLatch;
        private byte _ppuDataBuffer;

        int FineXScroll;
        TileRow PrevTile;
        TileRow ActTile;
        TileRow NextTile;
        bool _spriteZeroHitPossible;

        public PPU(Bus bus)
        {
            _bus = bus;

            _palScreen[0x00] = Color.FromArgb(84, 84, 84).ToArgb();
            _palScreen[0x01] = Color.FromArgb(0, 30, 116).ToArgb();
            _palScreen[0x02] = Color.FromArgb(8, 16, 144).ToArgb();
            _palScreen[0x03] = Color.FromArgb(48, 0, 136).ToArgb();
            _palScreen[0x04] = Color.FromArgb(68, 0, 100).ToArgb();
            _palScreen[0x05] = Color.FromArgb(92, 0, 48).ToArgb();
            _palScreen[0x06] = Color.FromArgb(84, 4, 0).ToArgb();
            _palScreen[0x07] = Color.FromArgb(60, 24, 0).ToArgb();
            _palScreen[0x08] = Color.FromArgb(32, 42, 0).ToArgb();
            _palScreen[0x09] = Color.FromArgb(8, 58, 0).ToArgb();
            _palScreen[0x0A] = Color.FromArgb(0, 64, 0).ToArgb();
            _palScreen[0x0B] = Color.FromArgb(0, 60, 0).ToArgb();
            _palScreen[0x0C] = Color.FromArgb(0, 50, 60).ToArgb();
            _palScreen[0x0D] = Color.FromArgb(0, 0, 0).ToArgb();
            _palScreen[0x0E] = Color.FromArgb(0, 0, 0).ToArgb();
            _palScreen[0x0F] = Color.FromArgb(0, 0, 0).ToArgb();

            _palScreen[0x10] = Color.FromArgb(152, 150, 152).ToArgb();
            _palScreen[0x11] = Color.FromArgb(8, 76, 196).ToArgb();
            _palScreen[0x12] = Color.FromArgb(48, 50, 236).ToArgb();
            _palScreen[0x13] = Color.FromArgb(92, 30, 228).ToArgb();
            _palScreen[0x14] = Color.FromArgb(136, 20, 176).ToArgb();
            _palScreen[0x15] = Color.FromArgb(160, 20, 100).ToArgb();
            _palScreen[0x16] = Color.FromArgb(152, 34, 32).ToArgb();
            _palScreen[0x17] = Color.FromArgb(120, 60, 0).ToArgb();
            _palScreen[0x18] = Color.FromArgb(84, 90, 0).ToArgb();
            _palScreen[0x19] = Color.FromArgb(40, 114, 0).ToArgb();
            _palScreen[0x1A] = Color.FromArgb(8, 124, 0).ToArgb();
            _palScreen[0x1B] = Color.FromArgb(0, 118, 40).ToArgb();
            _palScreen[0x1C] = Color.FromArgb(0, 102, 120).ToArgb();
            _palScreen[0x1D] = Color.FromArgb(0, 0, 0).ToArgb();
            _palScreen[0x1E] = Color.FromArgb(0, 0, 0).ToArgb();
            _palScreen[0x1F] = Color.FromArgb(0, 0, 0).ToArgb();

            _palScreen[0x20] = Color.FromArgb(236, 238, 236).ToArgb();
            _palScreen[0x21] = Color.FromArgb(76, 154, 236).ToArgb();
            _palScreen[0x22] = Color.FromArgb(120, 124, 236).ToArgb();
            _palScreen[0x23] = Color.FromArgb(176, 98, 236).ToArgb();
            _palScreen[0x24] = Color.FromArgb(228, 84, 236).ToArgb();
            _palScreen[0x25] = Color.FromArgb(236, 88, 180).ToArgb();
            _palScreen[0x26] = Color.FromArgb(236, 106, 100).ToArgb();
            _palScreen[0x27] = Color.FromArgb(212, 136, 32).ToArgb();
            _palScreen[0x28] = Color.FromArgb(160, 170, 0).ToArgb();
            _palScreen[0x29] = Color.FromArgb(116, 196, 0).ToArgb();
            _palScreen[0x2A] = Color.FromArgb(76, 208, 32).ToArgb();
            _palScreen[0x2B] = Color.FromArgb(56, 204, 108).ToArgb();
            _palScreen[0x2C] = Color.FromArgb(56, 180, 204).ToArgb();
            _palScreen[0x2D] = Color.FromArgb(60, 60, 60).ToArgb();
            _palScreen[0x2E] = Color.FromArgb(0, 0, 0).ToArgb();
            _palScreen[0x2F] = Color.FromArgb(0, 0, 0).ToArgb();

            _palScreen[0x30] = Color.FromArgb(236, 238, 236).ToArgb();
            _palScreen[0x31] = Color.FromArgb(168, 204, 236).ToArgb();
            _palScreen[0x32] = Color.FromArgb(188, 188, 236).ToArgb();
            _palScreen[0x33] = Color.FromArgb(212, 178, 236).ToArgb();
            _palScreen[0x34] = Color.FromArgb(236, 174, 236).ToArgb();
            _palScreen[0x35] = Color.FromArgb(236, 174, 212).ToArgb();
            _palScreen[0x36] = Color.FromArgb(236, 180, 176).ToArgb();
            _palScreen[0x37] = Color.FromArgb(228, 196, 144).ToArgb();
            _palScreen[0x38] = Color.FromArgb(204, 210, 120).ToArgb();
            _palScreen[0x39] = Color.FromArgb(180, 222, 120).ToArgb();
            _palScreen[0x3A] = Color.FromArgb(168, 226, 144).ToArgb();
            _palScreen[0x3B] = Color.FromArgb(152, 226, 180).ToArgb();
            _palScreen[0x3C] = Color.FromArgb(160, 214, 228).ToArgb();
            _palScreen[0x3D] = Color.FromArgb(160, 162, 160).ToArgb();
            _palScreen[0x3E] = Color.FromArgb(0, 0, 0).ToArgb();
            _palScreen[0x3F] = Color.FromArgb(0, 0, 0).ToArgb();
        }

        public int GetColourFromPaletteRam(int palette, int pixel)
        {
            byte colourIndex = PpuRead(0x3F00 + (palette << 2) + pixel);
            return _palScreen[colourIndex & 0x3F]; // looks like this Mask is redundant, based on what ppuRead returns
        }

        public static int Flipbyte(int b)
        {
            //https://stackoverflow.com/questions/2602823/in-c-c-whats-the-simplest-way-to-reverse-the-order-of-bits-in-A-byte/2602885#2602885
            b = (b & 0xF0) >> 4 | (b & 0x0F) << 4;
            b = (b & 0xCC) >> 2 | (b & 0x33) << 2;
            b = (b & 0xAA) >> 1 | (b & 0x55) << 1;
            return b;
        }

        void ReadNextTileData(int dataIdx)
        {
            switch (dataIdx)
            {
                case 0:
                    NextTile.X = (byte)_cycle;
                    PrevTile = ActTile;
                    ActTile = NextTile;
                    NextTile = new TileRow();
                    NextTile.ID = PpuRead(0x2000 | VramAddr.reg & 0x0FFF);
                    break;
                case 2:
                    NextTile.Attribute = PpuRead(0x23C0 | VramAddr.reg & 0x0C00
                        | (VramAddr.coarse_y >> 2) << 3
                        | VramAddr.coarse_x >> 2);
                    NextTile.Attribute >>= (VramAddr.coarse_y & 0x02) << 1 | VramAddr.coarse_x & 0x02;
                    NextTile.Attribute &= 0x03;
                    break;
                case 4:
                    NextTile.LSB = PpuRead(Control.pattern_bg << 12 | NextTile.ID << 4 | VramAddr.fine_y);
                    break;
                case 6:
                    NextTile.MSB = PpuRead(Control.pattern_bg << 12 | NextTile.ID << 4 | VramAddr.fine_y | 8);
                    break;
                case 7:
                    IncrementCoarseX();
                    break;
            }
        }

        public void Clock()
        {
            if (Mask.render_background != 0 || Mask.render_sprites != 0)
            {
                if (_scanline == -1)
                {
                    if (_cycle >= 279 && _cycle <= 303)
                        TransferAddressY();
                    else if (_cycle >= 320 && _cycle <= 335)
                        ReadNextTileData(_cycle & 0x7);
                }
                else if (_scanline < 240)
                {
                    if (_cycle == -1 && _scanline == 0 && IsOddFrame)
                        _cycle = 0;
                    else if (_cycle >= 0 && _cycle <= 255)
                    {
                        ReadNextTileData(_cycle & 0x7);
                        Screen[_cycle, _scanline] = GetPixelColor();
                    }
                    else if (_cycle == 256)
                    {
                        IncrementCoarseY();
                        TransferAddressX();
                        ReadSpriteScanline();
                    }
                    else if (_cycle >= 320 && _cycle <= 335)
                        ReadNextTileData(_cycle & 0x7);
                }
            }
            _cycle++;
            if (_cycle >= 340)
            {
                _cycle = -1;
                _scanline++;
                if (_scanline == 241)
                {
                    Status.vertical_blank = 1;
                    if (Control.enable_nmi != 0)
                        _bus.CPU.IRQrequest = _bus.CPU.NMI;
                }
                else if (_scanline == 261)
                {
                    _scanline = -1;
                    Status.vertical_blank = 0;
                    Status.sprite_zero_hit = 0;
                    Status.sprite_overflow = 0;
                    IsOddFrame = !IsOddFrame;
                    FrameComplete = true;
                }
            }
        }

        private int GetPixelColor()
        {
            // --- Background
            int bg_pixel = 0;
            int bg_palette = 0;
            if (Mask.render_background != 0)
            {
                var idx = 15 - (_cycle - ActTile.X + FineXScroll);
                var patternTile = idx < 8 ? ActTile: PrevTile;
                if (idx >= 8) idx -= 8;
                bg_pixel = patternTile.GetPixel(idx);
                var attribTile = new TileRow();
                var attribIdx = idx & ~0x7;
                attribTile.LSB = patternTile.Attribute;
                attribTile.MSB = (byte)(patternTile.Attribute >> 1);
                bg_palette = attribTile.GetPixel(attribIdx);
            }
            int pixel = 0;
            int palette = 0;
            // --- Foreground
            bool fg_priority = false;
            if (Mask.render_sprites != 0)
            {
                for (int i = 0; i < _spriteCount; i++)
                {
                    var sprite = SpriteScanline[i];
                    int idx = 7 - (_cycle - sprite.X);
                    if (idx >= 0 && idx <= 7)
                    {
                        pixel = sprite.GetPixel(idx);
                        if (pixel != 0)
                        {
                            if (i == 0 && _spriteZeroHitPossible && bg_pixel > 0)
                                Status.sprite_zero_hit = 1;
                            palette = (sprite.Attribute & 0x03) + 0x04;
                            fg_priority = (sprite.Attribute & 0x20) == 0;
                            break;
                        }
                    }
                }
            }
            if (bg_pixel > 0 && (pixel == 0 || !fg_priority))
            {
                pixel = bg_pixel;
                palette = bg_palette;
            }
            return GetColourFromPaletteRam(palette, pixel);
        }

        private void ReadSpriteScanline()
        {
            _spriteCount = 0;
            _spriteZeroHitPossible = false;
            for (int i = 0; i < OAM.Length; i += 4)
            {
                var sprite = new TileRow();
                sprite.Y = OAM[i];
                sprite.ID = OAM[i + 1];
                sprite.Attribute = OAM[i + 2];
                sprite.X = OAM[i + 3];
                var spriteSize = Control.sprite_size == 0 ? 8 : 16;
                var diff = _scanline - sprite.Y;
                if (diff >= 0 && diff < spriteSize)
                {
                    if (_spriteCount == 8)
                    {
                        Status.sprite_overflow = 1;
                        break;
                    }
                    if (i == 0) // Is this SpriteZero?
                        _spriteZeroHitPossible = true;
                    var flipX = (sprite.Attribute & 0x40) != 0;
                    var flipY = (sprite.Attribute & 0x80) != 0;
                    var row_offset = _scanline - sprite.Y;
                    int sprite_pattern_addr;
                    if (Control.sprite_size == 0) // 8x8 sprite mode
                        sprite_pattern_addr = Control.pattern_sprite << 12 | sprite.ID << 4;
                    else // 8x16 sprite mode
                    {
                        sprite_pattern_addr = (sprite.ID & 0x01) << 12;
                        if (flipY && row_offset < 8 || !flipY && row_offset >= 8)
                            sprite_pattern_addr |= ((sprite.ID & 0xFE) + 1) << 4;
                        else
                            sprite_pattern_addr |= (sprite.ID & 0xFE) << 4;
                    }
                    if (flipY)
                        sprite_pattern_addr |= (7 - row_offset) & 0x07;
                    else
                        sprite_pattern_addr |= row_offset & 0x07;
                    sprite.LSB = PpuRead(sprite_pattern_addr);
                    sprite.MSB = PpuRead(sprite_pattern_addr + 8);
                    if (flipX)
                    {
                        sprite.LSB = (byte)Flipbyte(sprite.LSB);
                        sprite.MSB = (byte)Flipbyte(sprite.MSB);
                    }
                    SpriteScanline[_spriteCount] = sprite;
                    _spriteCount++;
                }
            }
            //for (int i = _spriteCount; i < SpriteScanline.Length; i++)
            //{
            //    SpriteScanline[i].Y = 0xFF;
            //    SpriteScanline[i].ID = 0xFF;
            //    SpriteScanline[i].Attribute = 0xFF;
            //    SpriteScanline[i].X = 0xFF;
            //}
        }

        public void Reset()
        {
            FineXScroll = 0x00;
            _addressLatch = 0x00;
            _ppuDataBuffer = 0x00;
            _scanline = 0;
            _cycle = -1;
            ActTile = new TileRow();
            NextTile = new TileRow();
            Status.reg = 0x00;
            Mask.reg = 0x00;
            Control.reg = 0x00;
            VramAddr.reg = 0x0000;
            TramAddr.reg = 0x0000;

            //debug
            Scroll.X = 0;
            Scroll.Y = 0;
        }

        TPixmap ReadTile(int tileIdx, int palette)
        {
            TPixmap tile = new TPixmap(8, 8);
            for (var row = 0; row < 8; row++)
            {
                var tileRow = new TileRow();
                var address = (tileIdx << 4) + row;
                tileRow.LSB = PpuRead(address);
                tileRow.MSB = PpuRead(address + 8);
                for (var col = 0; col < 8; col++)
                    tile[col, row] = GetColourFromPaletteRam(palette, tileRow.GetPixel(7 - col));
            }
            return tile;
        }

        /// <summary>
        /// Debug view of the specific pattern table for given palette
        /// </summary>
        /// <param name="idx"></param>
        /// <param name="palette"></param>
        /// <returns></returns>
        public TPixmap GetPattern(int idx, byte palette)
        {
            var pattern = new TPixmap(128, 0);
            for (var nTileY = 0; nTileY < 16; nTileY++)
            {
                var patternRow = new TPixmap(0, 8);
                for (var nTileX = 0; nTileX < 16; nTileX++)
                {
                    var tile = ReadTile(256 * idx + nTileY * 16 + nTileX, palette);
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
        public TPixmap GetNameTable(int idx)
        {
            var nameTable = new TPixmap(256, 0);
            for (var nTileY = 0; nTileY < 30; nTileY++)
            {
                var nameTableRow = new TPixmap(0, 8);
                for (var nTileX = 0; nTileX < 32; nTileX++)
                {
                    var attr_group_grid_x = nTileX / 4;
                    var attr_group_grid_y = nTileY / 4;
                    var attr_grid_index = attr_group_grid_y * 8 + attr_group_grid_x;
                    var attr_grid_data = _tblName[idx][0x3C0 + attr_grid_index];
                    var attr_grid_x = (nTileX / 2) & 1;
                    var attr_grid_y = (nTileY / 2) & 1;
                    int palette = attr_grid_data >> 2 * (2 * attr_grid_y + attr_grid_x);
                    int tile_index = _tblName[idx][nTileY * 32 + nTileX] + 256;
                    var tile = ReadTile(tile_index, palette & 0x3);
                    nameTableRow = nameTableRow.HorzCat(tile);
                }
                nameTable = nameTable.VertCat(nameTableRow);
            }
            return nameTable;
        }

        // Timing hack grafted from fogleman's nes emulator.
        //
        // Some games fail to boot because of the CPU being instruction-cycle accurate,
        // instead of sub-instruction-cycle accurate.
        //bool nmi_previous;
        //int nmi_delay;
        //void NMIChange()
        //{ // hack
        //    NMI = Control.enable_nmi != 0 && Status.vertical_blank != 0;
        //    if (NMI && !nmi_previous)
        //    {
        //        nmi_delay = 15;
        //    }
        //    nmi_previous = NMI;
        //}

        // CPU <---> PPU
        public byte CpuRead(int addr)
        {
            int data = 0;
            switch (addr)
            {
                case 0: // Control
                    data = Control.reg;
                    break;
                case 1: // Mask
                    data = Mask.reg;
                    break;
                case 2: // Status
                    data = Status.reg & 0xE0 | _ppuDataBuffer & 0x1F;
                    _addressLatch = 0;
                    Status.vertical_blank = 0;
                    //data = Status.reg;
                    break;
                case 3: // OAM Address
                    // doesn't make any sense to read from the addr register
                    break;
                case 4: // OAM Data
                    data = OAM[_oamAddr];
                    break;
                case 5: // Scroll
                    break;
                case 6: // PPU Address
                    break;
                case 7: // PPU Data
                    data = _ppuDataBuffer;
                    _ppuDataBuffer = PpuRead(VramAddr.reg);
                    if (VramAddr.reg >= 0x3F00) data = _ppuDataBuffer;
                    VramAddr.reg += Control.increment_mode != 0 ? 32 : 1;
                    break;
            }
            return (byte)data;
        }

        public void CpuWrite(int addr, byte data)
        {
            switch (addr)
            {
                case 0: // Control
                    Control.reg = data;
                    TramAddr.nametable_x = Control.nametable_x;
                    TramAddr.nametable_y = Control.nametable_y;
                    break;
                case 1: // Mask
                    Mask.reg = data; // DEBUG
                    break;
                case 2: // Status
                    Status.reg = data;
                    break;
                case 3: // OAM Address
                    _oamAddr = data;
                    break;
                case 4: // OAM Data
                    OAM[_oamAddr] = data;
                    break;
                case 5: // Scroll
                    if (_addressLatch == 0)
                    {
                        Scroll.X = data;
                        FineXScroll = data & 0x7;
                        TramAddr.coarse_x = data >> 3;
                        _addressLatch = 1;
                    }
                    else
                    {
                        Scroll.Y = data;
                        TramAddr.fine_y = data & 0x7;
                        TramAddr.coarse_y = data >> 3;
                        _addressLatch = 0;
                    }
                    break;
                case 6: // PPU Address
                    if (_addressLatch == 0)
                    {
                        TramAddr.reg = (data & 0x3F) << 8 | TramAddr.reg & 0x00FF;
                        _addressLatch = 1;
                    }
                    else
                    {
                        TramAddr.reg = TramAddr.reg & 0xFF00 | data;
                        VramAddr.reg = TramAddr.reg; // might cause problems!
                        _addressLatch = 0;
                    }
                    break;
                case 7: // PPU Data
                    PpuWrite(VramAddr.reg, data);
                    VramAddr.reg += Control.increment_mode != 0 ? 32 : 1;
                    break;
            }
        }

        // PPU <---> PPU_BUS
        private byte PpuRead(int addr, bool bReadOnly = true)
        {
            byte data = 0x00;
            addr &= 0x3FFF;

            if (_bus.Cartridge.PpuRead((ushort)addr, ref data)) 
                return data;
            if (addr <= 0x1FFF)
            {
                //_temp for cartridge or debug
                data = _tblPattern[(addr & 0x1000) >> 12][addr & 0x0FFF];
            }
            else if (addr <= 0x3EFF)
            {
                addr &= 0x0FFF;
                if (_bus.Cartridge.Mirror() == MIRROR.VERTICAL)
                {
                    if (addr / 0x0400 % 2 == 0)
                        data = _tblName[0][addr & 0x03FF];
                    else
                        data = _tblName[1][addr & 0x03FF];
                }
                else if (_bus.Cartridge.Mirror() == MIRROR.HORIZONTAL)
                {
                    if (addr < 0x0400 * 2)
                        data = _tblName[0][addr & 0x03FF];
                    else
                        data = _tblName[1][addr & 0x03FF];
                }
            }
            else
            {
                addr &= 0x001F;
                if (addr == 0x0010 || addr == 0x0014 || addr == 0x0018 || addr == 0x001C)
                    addr &= 0xF;
                data = (byte)(_tblPalette[addr] & (Mask.greyscale != 0 ? 0x30 : 0x3F));
            }
            return data;
        }

        private void PpuWrite(int addr, byte data)
        {
            addr &= 0x3FFF;

            if (_bus.Cartridge.PpuWrite((ushort)addr, data)) return;
            if (addr <= 0x1FFF)
            {
                _tblPattern[(addr & 0x1000) >> 12][addr & 0x0FFF] = data;
            }
            else if (addr <= 0x3EFF)
            {
                addr &= 0x0FFF;
                if (_bus.Cartridge.Mirror() == MIRROR.VERTICAL)
                {
                    if (addr / 0x0400 % 2 == 0)
                        _tblName[0][addr & 0x03FF] = data;
                    else
                        _tblName[1][addr & 0x03FF] = data;
                }
                else if (_bus.Cartridge.Mirror() == MIRROR.HORIZONTAL)
                {
                    if (addr < 0x0400 * 2)
                        _tblName[0][addr & 0x03FF] = data;
                    else
                        _tblName[1][addr & 0x03FF] = data;
                }
            }
            else
            {
                addr &= 0x001F;
                if (addr == 0x0010 || addr == 0x0014 || addr == 0x0018 || addr == 0x001C)
                    addr &= 0xF;
                _tblPalette[addr] = data;
            }
        }

        /// <summary>
        /// Increment the background tile "pointer" one tile/column horizontally
        /// </summary>
        private void IncrementCoarseX()
        {
            if (Mask.render_background != 0 || Mask.render_sprites != 0)
            {
                if (VramAddr.coarse_x == 31)
                {
                    VramAddr.coarse_x = 0;
                    VramAddr.nametable_x = (UInt16)~VramAddr.nametable_x;
                }
                else
                {
                    VramAddr.coarse_x++;
                }
            }
        }

        /// <summary>
        /// Increment the background tile "pointer" one scanline vertically
        /// </summary>
        private void IncrementCoarseY()
        {
            if (VramAddr.fine_y < 7)
            {
                VramAddr.fine_y++;
            }
            else
            {
                VramAddr.fine_y = 0;

                if (VramAddr.coarse_y == 29)
                {
                    VramAddr.coarse_y = 0;
                    VramAddr.nametable_y = (UInt16)~VramAddr.nametable_y;
                }
                else if (VramAddr.coarse_y == 31)
                {
                    VramAddr.coarse_y = 0;
                }
                else
                {
                    VramAddr.coarse_y++;
                }
            }
        }

        /// <summary>
        /// Transfer the temporarily stored horizontal nametable access information
        /// into the "pointer". Note that fine x scrolling is not part of the "pointer"
        /// addressing mechanism
        /// </summary>
        private void TransferAddressX()
        {
            VramAddr.nametable_x = TramAddr.nametable_x;
            VramAddr.coarse_x = TramAddr.coarse_x;
        }

        /// <summary>
        /// Transfer the temporarily stored vertical nametable access information
        /// into the "pointer". Note that fine y scrolling is part of the "pointer"
        /// addressing mechanism
        /// </summary>
        private void TransferAddressY()
        {
            VramAddr.fine_y = TramAddr.fine_y;
            VramAddr.nametable_y = TramAddr.nametable_y;
            VramAddr.coarse_y = TramAddr.coarse_y;
        }

    }
}