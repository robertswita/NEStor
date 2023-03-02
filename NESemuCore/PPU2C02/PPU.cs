using emulatorTest.PPU2C02.Flags;
using System;
using System.Drawing;
using Common;

namespace emulatorTest.PPU2C02
{
    class PPU
    {
        public bool FrameComplete;
        public bool IsOddFrame;
        public bool DebugAtScanline;

        public readonly PPUInternalRegister VramAddr = new PPUInternalRegister();
        public readonly PPUInternalRegister TramAddr = new PPUInternalRegister();

        public readonly PPUControlFlags Control = new PPUControlFlags();
        public readonly PPUMaskFlags Mask = new PPUMaskFlags();
        public readonly PPUStatusFlags Status = new PPUStatusFlags();

        /// <summary>
        /// OAM (ObjectAttributeMemory)
        /// 64 sprites (ObjectAttributeEntry)[Y, id, attribute, X]
        /// </summary>
        public struct TSprite {
            public byte Y;          // Y position 
            public byte ID;         // ID of tile from pattern memory
            public byte Attribute;  // how sprite should be rendered
            public byte X;          // X position
        }
        public byte[] OAM = new byte[256];
        private byte _oamAddr;

        private readonly TSprite[] SpriteScanline = new TSprite[8];
        private byte _spriteCount;

        //public bool NMI;

        public int ScrollX;
        public int ScrollY;

        private int _cycle;
        private int _scanline;
        private readonly Bus _bus;

        // Nametable RAM
        private readonly byte[][] _tblName = { new byte[1024], new byte[1024] };
        // 
        private readonly byte[] _tblPalette = new byte[32];

        // most of the times not used
        private readonly byte[][] _tblPattern = { new byte[4096], new byte[4096] };

        // SPRITES
        private readonly uint[] _palScreen = new uint[0x40]; // 64

        public TPixmap Screen = new TPixmap(256, 240);

        //USED FOR DEBUGGING ONLY (ARE FOR PRESENTATION PURPOSE ONLY)
        private readonly uint[][] _nameTable = { new uint[256 * 240], new uint[256 * 240] };
        //private readonly uint[][] _patternTable = { new uint[128 * 128], new uint[128 * 128] };

        private byte _addressLatch;
        private byte _ppuDataBuffer;

        private int _fineX; //fine X scrolling

        // Background
        private byte _bgNextTileId;
        private byte _bgNextTileAttrib;
        private byte _bgNextTileLSB;
        private byte _bgNextTileMSB;

        private int _bgShifterPatternLow;
        private int _bgShifterPatternHigh;
        private int _bgShifterAttribLow;
        private int _bgShifterAttribHigh;

        //Foreground
        private byte[] _spriteShifterPatternLow = new byte[8];
        private byte[] _spriteShifterPatternHigh = new byte[8];

        private bool _spriteZeroHitPossible;

        public PPU(Bus bus)
        {
            _bus = bus;

            _palScreen[0x00] = (uint)Color.FromArgb(84, 84, 84).ToArgb();
            _palScreen[0x01] = (uint)Color.FromArgb(0, 30, 116).ToArgb();
            _palScreen[0x02] = (uint)Color.FromArgb(8, 16, 144).ToArgb();
            _palScreen[0x03] = (uint)Color.FromArgb(48, 0, 136).ToArgb();
            _palScreen[0x04] = (uint)Color.FromArgb(68, 0, 100).ToArgb();
            _palScreen[0x05] = (uint)Color.FromArgb(92, 0, 48).ToArgb();
            _palScreen[0x06] = (uint)Color.FromArgb(84, 4, 0).ToArgb();
            _palScreen[0x07] = (uint)Color.FromArgb(60, 24, 0).ToArgb();
            _palScreen[0x08] = (uint)Color.FromArgb(32, 42, 0).ToArgb();
            _palScreen[0x09] = (uint)Color.FromArgb(8, 58, 0).ToArgb();
            _palScreen[0x0A] = (uint)Color.FromArgb(0, 64, 0).ToArgb();
            _palScreen[0x0B] = (uint)Color.FromArgb(0, 60, 0).ToArgb();
            _palScreen[0x0C] = (uint)Color.FromArgb(0, 50, 60).ToArgb();
            _palScreen[0x0D] = (uint)Color.FromArgb(0, 0, 0).ToArgb();
            _palScreen[0x0E] = (uint)Color.FromArgb(0, 0, 0).ToArgb();
            _palScreen[0x0F] = (uint)Color.FromArgb(0, 0, 0).ToArgb();

            _palScreen[0x10] = (uint)Color.FromArgb(152, 150, 152).ToArgb();
            _palScreen[0x11] = (uint)Color.FromArgb(8, 76, 196).ToArgb();
            _palScreen[0x12] = (uint)Color.FromArgb(48, 50, 236).ToArgb();
            _palScreen[0x13] = (uint)Color.FromArgb(92, 30, 228).ToArgb();
            _palScreen[0x14] = (uint)Color.FromArgb(136, 20, 176).ToArgb();
            _palScreen[0x15] = (uint)Color.FromArgb(160, 20, 100).ToArgb();
            _palScreen[0x16] = (uint)Color.FromArgb(152, 34, 32).ToArgb();
            _palScreen[0x17] = (uint)Color.FromArgb(120, 60, 0).ToArgb();
            _palScreen[0x18] = (uint)Color.FromArgb(84, 90, 0).ToArgb();
            _palScreen[0x19] = (uint)Color.FromArgb(40, 114, 0).ToArgb();
            _palScreen[0x1A] = (uint)Color.FromArgb(8, 124, 0).ToArgb();
            _palScreen[0x1B] = (uint)Color.FromArgb(0, 118, 40).ToArgb();
            _palScreen[0x1C] = (uint)Color.FromArgb(0, 102, 120).ToArgb();
            _palScreen[0x1D] = (uint)Color.FromArgb(0, 0, 0).ToArgb();
            _palScreen[0x1E] = (uint)Color.FromArgb(0, 0, 0).ToArgb();
            _palScreen[0x1F] = (uint)Color.FromArgb(0, 0, 0).ToArgb();

            _palScreen[0x20] = (uint)Color.FromArgb(236, 238, 236).ToArgb();
            _palScreen[0x21] = (uint)Color.FromArgb(76, 154, 236).ToArgb();
            _palScreen[0x22] = (uint)Color.FromArgb(120, 124, 236).ToArgb();
            _palScreen[0x23] = (uint)Color.FromArgb(176, 98, 236).ToArgb();
            _palScreen[0x24] = (uint)Color.FromArgb(228, 84, 236).ToArgb();
            _palScreen[0x25] = (uint)Color.FromArgb(236, 88, 180).ToArgb();
            _palScreen[0x26] = (uint)Color.FromArgb(236, 106, 100).ToArgb();
            _palScreen[0x27] = (uint)Color.FromArgb(212, 136, 32).ToArgb();
            _palScreen[0x28] = (uint)Color.FromArgb(160, 170, 0).ToArgb();
            _palScreen[0x29] = (uint)Color.FromArgb(116, 196, 0).ToArgb();
            _palScreen[0x2A] = (uint)Color.FromArgb(76, 208, 32).ToArgb();
            _palScreen[0x2B] = (uint)Color.FromArgb(56, 204, 108).ToArgb();
            _palScreen[0x2C] = (uint)Color.FromArgb(56, 180, 204).ToArgb();
            _palScreen[0x2D] = (uint)Color.FromArgb(60, 60, 60).ToArgb();
            _palScreen[0x2E] = (uint)Color.FromArgb(0, 0, 0).ToArgb();
            _palScreen[0x2F] = (uint)Color.FromArgb(0, 0, 0).ToArgb();

            _palScreen[0x30] = (uint)Color.FromArgb(236, 238, 236).ToArgb();
            _palScreen[0x31] = (uint)Color.FromArgb(168, 204, 236).ToArgb();
            _palScreen[0x32] = (uint)Color.FromArgb(188, 188, 236).ToArgb();
            _palScreen[0x33] = (uint)Color.FromArgb(212, 178, 236).ToArgb();
            _palScreen[0x34] = (uint)Color.FromArgb(236, 174, 236).ToArgb();
            _palScreen[0x35] = (uint)Color.FromArgb(236, 174, 212).ToArgb();
            _palScreen[0x36] = (uint)Color.FromArgb(236, 180, 176).ToArgb();
            _palScreen[0x37] = (uint)Color.FromArgb(228, 196, 144).ToArgb();
            _palScreen[0x38] = (uint)Color.FromArgb(204, 210, 120).ToArgb();
            _palScreen[0x39] = (uint)Color.FromArgb(180, 222, 120).ToArgb();
            _palScreen[0x3A] = (uint)Color.FromArgb(168, 226, 144).ToArgb();
            _palScreen[0x3B] = (uint)Color.FromArgb(152, 226, 180).ToArgb();
            _palScreen[0x3C] = (uint)Color.FromArgb(160, 214, 228).ToArgb();
            _palScreen[0x3D] = (uint)Color.FromArgb(160, 162, 160).ToArgb();
            _palScreen[0x3E] = (uint)Color.FromArgb(0, 0, 0).ToArgb();
            _palScreen[0x3F] = (uint)Color.FromArgb(0, 0, 0).ToArgb();
        }

        public uint GetColourFromPaletteRam(int palette, int pixel)
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

        public void Clock()
        {
            if (_scanline < 240)
            {
                if (_scanline == 0 && _cycle == 0 && IsOddFrame)
                    _cycle = 1;
                if (_cycle >= 2 && _cycle < 258 || _cycle >= 321 && _cycle < 338)
                {
                    UpdateShifters();
                    switch ((_cycle - 1) & 0x7)
                    {
                        case 0:
                            LoadBackgroundShifters();
                            _bgNextTileId = PpuRead(0x2000 | VramAddr.reg & 0x0FFF);
                            break;
                        case 2:
                            _bgNextTileAttrib = PpuRead(0x23C0 | VramAddr.reg & 0x0C00
                                | (VramAddr.coarse_y >> 2) << 3
                                | VramAddr.coarse_x >> 2);
                            if ((VramAddr.coarse_y & 0x02) != 0) _bgNextTileAttrib >>= 4;
                            if ((VramAddr.coarse_x & 0x02) != 0) _bgNextTileAttrib >>= 2;
                            _bgNextTileAttrib &= 0x03;
                            break;
                        case 4:
                            _bgNextTileLSB = PpuRead(Control.pattern_bg << 12 | _bgNextTileId << 4 | VramAddr.fine_y);
                            break;
                        case 6:
                            _bgNextTileMSB = PpuRead(Control.pattern_bg << 12 | _bgNextTileId << 4 | VramAddr.fine_y | 8);
                            break;
                        case 7:
                            IncrementScrollX();
                            break;
                    }
                }
                if (_cycle == 256)
                    IncrementScrollY();
                if (_cycle == 257)
                {
                    LoadBackgroundShifters();
                    TransferAddressX();
                    if (_scanline >= 0)
                        ReadSpriteScanline();
                }
                if (_cycle >= 280 && _cycle < 305 && _scanline == -1)
                    TransferAddressY();
                //if (_cycle == 338 || _cycle == 340)
                //    _bgNextTileId = PpuRead(0x2000 | VramAddr.reg & 0x0FFF);
                if (_cycle == 340)
                    LoadSpriteShifters();
                if (_cycle > 0 && _cycle < 257 && _scanline >= 0)
                    Screen[_cycle - 1, _scanline] = GetPixelColor();
            }
            _cycle++;
            if (_cycle >= 341)
            {
                _cycle = 0;
                _scanline++;
                if (_scanline == 241)
                {
                    Status.vertical_blank = 1;
                    if (Control.enable_nmi != 0)
                        _bus.CPU.IRQrequest = _bus.CPU.NMI;
                }
                if (_scanline >= 261)
                {
                    _scanline = -1;
                    Status.vertical_blank = 0;
                    Status.sprite_zero_hit = 0;
                    Status.sprite_overflow = 0;
                    //_spriteShifterPatternLow = new byte[8];
                    //_spriteShifterPatternHigh = new byte[8];
                    IsOddFrame = !IsOddFrame;
                    FrameComplete = true;
                }
            }
        }

        private uint GetPixelColor()
        {
            // --- Background
            int bg_pixel = 0;
            int bg_palette = 0;
            if (Mask.render_background != 0)
            {
                int bit_mux = 0x8000 >> _fineX;
                int p0_pixel = (_bgShifterPatternLow & bit_mux) > 0 ? 1 : 0;
                int p1_pixel = (_bgShifterPatternHigh & bit_mux) > 0 ? 1 : 0;
                bg_pixel = (p1_pixel << 1) | p0_pixel;
                int bg_pal0 = (_bgShifterAttribLow & bit_mux) > 0 ? 1 : 0;
                int bg_pal1 = (_bgShifterAttribHigh & bit_mux) > 0 ? 1 : 0;
                bg_palette = (bg_pal1 << 1) | bg_pal0;
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
                    if (sprite.X == 0)
                    {
                        int pixel_lo = (_spriteShifterPatternLow[i] & 0x80) > 0 ? 1 : 0;
                        int pixel_hi = (_spriteShifterPatternHigh[i] & 0x80) > 0 ? 1 : 0;
                        pixel = (pixel_hi << 1) | pixel_lo;
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
                var sprite = new TSprite();
                sprite.Y = OAM[i];
                sprite.ID = OAM[i + 1];
                sprite.Attribute = OAM[i + 2];
                sprite.X = OAM[i + 3];
                var spriteSize = Control.sprite_size == 0 ? 8 : 16;
                var diff = _scanline - sprite.Y;
                if (diff >= 0 && diff < spriteSize)
                {
                    if (_spriteCount < 8)
                    {
                        if (i == 0) // Is this SpriteZero?
                            _spriteZeroHitPossible = true;
                        SpriteScanline[_spriteCount] = sprite;
                        _spriteCount++;
                    }
                    else
                    {
                        Status.sprite_overflow = 1;
                        break;
                    }
                }
            }
            for (int i = _spriteCount; i < SpriteScanline.Length; i++)
            {
                SpriteScanline[i].Y = 0xFF;
                SpriteScanline[i].ID = 0xFF;
                SpriteScanline[i].Attribute = 0xFF;
                SpriteScanline[i].X = 0xFF;
            }
        }

        public void Reset()
        {
            _fineX = 0x00;
            _addressLatch = 0x00;
            _ppuDataBuffer = 0x00;
            _scanline = 0;
            _cycle = 0;
            _bgNextTileId = 0x00;
            _bgNextTileAttrib = 0x00;
            _bgNextTileLSB = 0x00;
            _bgNextTileMSB = 0x00;
            _bgShifterPatternLow = 0x0000;
            _bgShifterPatternHigh = 0x0000;
            _bgShifterAttribLow = 0x0000;
            _bgShifterAttribHigh = 0x0000;
            Status.reg = 0x00;
            Mask.reg = 0x00;
            Control.reg = 0x00;
            VramAddr.reg = 0x0000;
            TramAddr.reg = 0x0000;

            //debug
            ScrollX = 0;
            ScrollY = 0;
        }

        TPixmap ReadTile(int tileIdx, int palette)
        {
            TPixmap tile = new TPixmap(8, 8);
            for (var row = 0; row < 8; row++)
            {
                var address = (tileIdx << 4) + row;
                var tile_lsb = PpuRead(address);
                var tile_msb = PpuRead(address + 8);
                for (var col = 7; col >= 0; col--)
                {
                    var pixel = 2 * (tile_msb & 0x01) + (tile_lsb & 0x01);
                    tile_lsb >>= 1;
                    tile_msb >>= 1;
                    tile[col, row] = GetColourFromPaletteRam(palette, pixel);
                }
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

        //public bool GetMirroringType()
        //{
        //    return _bus.Cartridge.Mirror() == NESGamePak.Mapper.MIRROR.VERTICAL;
        //}

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
                        _fineX = data & 0x7;
                        TramAddr.coarse_x = data >> 3;
                        _addressLatch = 1;
                    }
                    else
                    {
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
                if (_bus.Cartridge.Mirror() == NESGamePak.Mapper.MIRROR.VERTICAL)
                {
                    if (addr <= 0x03FF)
                        data = _tblName[0][addr & 0x03FF];
                    else if (addr <= 0x07FF)
                        data = _tblName[1][addr & 0x03FF];
                    else if (addr <= 0x0BFF)
                        data = _tblName[0][addr & 0x03FF];
                    else
                        data = _tblName[1][addr & 0x03FF];
                }
                else if (_bus.Cartridge.Mirror() == NESGamePak.Mapper.MIRROR.HORIZONTAL)
                {
                    if (addr <= 0x03FF)
                        data = _tblName[0][addr & 0x03FF];
                    else if (addr <= 0x07FF)
                        data = _tblName[0][addr & 0x03FF];
                    else if (addr <= 0x0BFF)
                        data = _tblName[1][addr & 0x03FF];
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
                if (_bus.Cartridge.Mirror() == NESGamePak.Mapper.MIRROR.VERTICAL)
                {
                    if (addr <= 0x03FF)
                        _tblName[0][addr & 0x03FF] = data;
                    else if (addr <= 0x07FF)
                        _tblName[1][addr & 0x03FF] = data;
                    else if (addr <= 0x0BFF)
                        _tblName[0][addr & 0x03FF] = data;
                    else
                        _tblName[1][addr & 0x03FF] = data;
                }
                else if (_bus.Cartridge.Mirror() == NESGamePak.Mapper.MIRROR.HORIZONTAL)
                {
                    if (addr <= 0x03FF)
                        _tblName[0][addr & 0x03FF] = data;
                    else if (addr <= 0x07FF)
                        _tblName[0][addr & 0x03FF] = data;
                    else if (addr <= 0x0BFF)
                        _tblName[1][addr & 0x03FF] = data;
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

        private void DebugSetLastFramePpuScroll(int reg, int fine_x, bool horizontalScrollOnly)
        {
            ScrollX = (reg & 0x1F) << 3 | fine_x;
            if ((reg & 0x400) != 0)
                ScrollX |= 0x100;
            if (!horizontalScrollOnly)
                ScrollY = (reg & 0x3E0) >> 2 | (reg & 0x7000) >> 12 | ((reg & 0x800) != 0 ? 240 : 0);
        }

        void SetScrollX()
        {
            var reg = VramAddr.reg;
            ScrollX = (reg & 0x1F) << 3 | _fineX;
            if ((reg & 0x400) != 0)
                ScrollX |= 0x100;
        }

        /// <summary>
        /// Increment the background tile "pointer" one tile/column horizontally
        /// </summary>
        private void IncrementScrollX()
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
        private void IncrementScrollY()
        {
            if (Mask.render_background != 0 || Mask.render_sprites != 0)
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
        }

        /// <summary>
        /// Transfer the temporarily stored horizontal nametable access information
        /// into the "pointer". Note that fine x scrolling is not part of the "pointer"
        /// addressing mechanism
        /// </summary>
        private void TransferAddressX()
        {
            if (Mask.render_background != 0 || Mask.render_sprites != 0)
            {
                VramAddr.nametable_x = TramAddr.nametable_x;
                VramAddr.coarse_x = TramAddr.coarse_x;
            }
        }

        /// <summary>
        /// Transfer the temporarily stored vertical nametable access information
        /// into the "pointer". Note that fine y scrolling is part of the "pointer"
        /// addressing mechanism
        /// </summary>
        private void TransferAddressY()
        {
            if (Mask.render_background != 0 || Mask.render_sprites != 0)
            {
                VramAddr.fine_y = TramAddr.fine_y;
                VramAddr.nametable_y = TramAddr.nametable_y;
                VramAddr.coarse_y = TramAddr.coarse_y;
            }
            if (_cycle == 304)
            {
                var _tempX = ScrollX;
                var _tempY = ScrollY;
                DebugSetLastFramePpuScroll(VramAddr.reg, _fineX, false);
                DebugAtScanline = true;
                if (_tempX != ScrollX || _tempY != ScrollY)
                    Console.WriteLine("X:" + ScrollX + ", Y:" + ScrollY);
            }
        }

        /// <summary>
        /// Prime the "in-effect" background tile shifters ready for outputting next
	    /// 8 pixels in scanline.
        /// </summary>
        private void LoadBackgroundShifters()
        {
            _bgShifterPatternLow = _bgShifterPatternLow & 0xFF00 | _bgNextTileLSB;
            _bgShifterPatternHigh = _bgShifterPatternHigh & 0xFF00 | _bgNextTileMSB;

            _bgShifterAttribLow = _bgShifterAttribLow & 0xFF00 | ((_bgNextTileAttrib & 0b01) != 0 ? 0xFF : 0x00);
            _bgShifterAttribHigh = _bgShifterAttribHigh & 0xFF00 | ((_bgNextTileAttrib & 0b10) != 0 ? 0xFF : 0x00);
        }

        private void LoadSpriteShifters()
        {
            for (var i = 0; i < _spriteCount; i++)
            {
                var sprite = SpriteScanline[i];
                var flipX = (sprite.Attribute & 0x40) != 0;
                var flipY = (sprite.Attribute & 0x80) != 0;
                var row_offset = _scanline - sprite.Y;
                int sprite_pattern_addr_lo;
                if (Control.sprite_size == 0) // 8x8 sprite mode
                    sprite_pattern_addr_lo = Control.pattern_sprite << 12 | sprite.ID << 4;
                else // 8x16 sprite mode
                {
                    sprite_pattern_addr_lo = (sprite.ID & 0x01) << 12;
                    if (flipY && row_offset < 8 || !flipY && row_offset >= 8)
                        sprite_pattern_addr_lo |= ((sprite.ID & 0xFE) + 1) << 4;
                    else
                        sprite_pattern_addr_lo |= (sprite.ID & 0xFE) << 4;
                }
                if (flipY)
                    sprite_pattern_addr_lo |= (7 - row_offset) & 0x07;
                else
                    sprite_pattern_addr_lo |= row_offset & 0x07;
                var sprite_pattern_addr_hi = sprite_pattern_addr_lo + 8;
                _spriteShifterPatternLow[i] = PpuRead(sprite_pattern_addr_lo);
                _spriteShifterPatternHigh[i] = PpuRead(sprite_pattern_addr_hi);
                if (flipX)
                {
                    _spriteShifterPatternLow[i] = (byte)Flipbyte(_spriteShifterPatternLow[i]);
                    _spriteShifterPatternHigh[i] = (byte)Flipbyte(_spriteShifterPatternHigh[i]);
                }
            }
        }

        /// <summary>
        /// Every cycle the shifters storing pattern and attribute information shift
        /// their contents by 1 bit. This is because in every cycle, the output progresses
        /// by 1 pixel. This means relatively, the state of the shifter is in sync
        /// with the pixels being drawn for that 8 pixel section of the scanline.
        /// </summary>
        private void UpdateShifters()
        {
            if (Mask.render_background != 0)
            {
                // Shifting background tile pattern row
                _bgShifterPatternLow <<= 1;
                _bgShifterPatternHigh <<= 1;

                // Shifting palette attributes by 1
                _bgShifterAttribLow <<= 1;
                _bgShifterAttribHigh <<= 1;
            }

            if (Mask.render_sprites != 0 && _cycle < 258)
            {
                for (int i = 0; i < _spriteCount; i++)
                {
                    if (SpriteScanline[i].X > 0)
                    {
                        SpriteScanline[i].X--;
                    }
                    else
                    {
                        _spriteShifterPatternLow[i] <<= 1;
                        _spriteShifterPatternHigh[i] <<= 1;
                    }
                }
            }
        }
    }
}