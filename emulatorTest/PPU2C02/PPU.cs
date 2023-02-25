using System;
using emulatorTest.PPU2C02.Flags;

namespace emulatorTest.PPU2C02
{
    class PPU : Common
    {
        public Boolean FrameComplete = false;
        public Boolean DebugAtScanline = false;

        public readonly PPUInternalRegister VramAddr = new PPUInternalRegister();
        public readonly PPUInternalRegister TramAddr = new PPUInternalRegister();

        public readonly PPUControlFlags Control = new PPUControlFlags();
        public readonly PPUMaskFlags Mask = new PPUMaskFlags();
        public readonly PPUStatusFlags Status = new PPUStatusFlags();

        /// <summary>
        /// OAM (ObjectAttributeMemory)
        /// 64 sprites X (ObjectAttributeEntry)[Y, id, attribute, X]
        /// </summary>
        public Byte[] OAM = new Byte[64 * 4];

        public Boolean NMI = false;

        public UInt16 ScrollX = 0;
        public UInt16 ScrollY = 0;

        private Int16 _cycle = 0;
        private Int16 _scanline = 0;
        private readonly Bus _bus;

        // Nametable RAM
        private readonly Byte[][] _tblName = { new Byte[1024], new Byte[1024] };
        // 
        private readonly Byte[] _tblPalette = new Byte[32];

        // most of the times not used
        private readonly Byte[][] _tblPattern = { new Byte[4096], new Byte[4096] };

        // SPRITES
        private readonly UInt32[] _palScreen = new UInt32[0x40]; // 64

        private UInt32[] _screen = new UInt32[256 * 240];

        //USED FOR DEBUGGING ONLY (ARE FOR PRESENTATION PURPOSE ONLY)
        private readonly UInt32[][] _nameTable = { new UInt32[256 * 240], new UInt32[256 * 240] };
        private readonly UInt32[][] _patternTable = { new UInt32[128 * 128], new UInt32[128 * 128] };

        private Byte _addressLatch = 0x00;
        private Byte _ppuDataBuffer = 0x00;

        private Byte _fineX = 0x00; //fine X scrolling

        // Background
        private Byte _bgNextTileId = 0x00;
        private Byte _bgNextTileAttrib = 0x00;
        private Byte _bgNextTileLSB = 0x00;
        private Byte _bgNextTileMSB = 0x00;

        private UInt16 _bgShifterPatternLow = 0x0000;
        private UInt16 _bgShifterPatternHigh = 0x0000;
        private UInt16 _bgShifterAttribLow = 0x0000;
        private UInt16 _bgShifterAttribHigh = 0x0000;

        //Foreground
        private readonly Byte[] _spriteShifterPatternLow = new Byte[8];
        private readonly Byte[] _spriteShifterPatternHigh = new Byte[8];

        private Byte _oamAddr = 0x00;

        private readonly Byte[] _spriteScanline = new Byte[8 * 4]; // 8 sprites X (ObjectAttributeEntry)[Y, id, attribute, X]
        private Byte _spriteCount = 0x00;

        private Boolean _spriteZeroHitPossible = false;
        private Boolean _spriteZeroBeingRendered = false;

        private UInt16 _tempX = 0;
        private UInt16 _tempY = 0;

        UInt32 ToRGBA(uint r, uint g, uint b)
        {
            return r << 24 | g << 16 | b << 8 | 255;
        }

        public PPU(Bus bus)
        {
            _bus = bus;

            _palScreen[0x00] = ToRGBA(84, 84, 84);
            _palScreen[0x01] = ToRGBA(0, 30, 116);
            _palScreen[0x02] = ToRGBA(8, 16, 144);
            _palScreen[0x03] = ToRGBA(48, 0, 136);
            _palScreen[0x04] = ToRGBA(68, 0, 100);
            _palScreen[0x05] = ToRGBA(92, 0, 48);
            _palScreen[0x06] = ToRGBA(84, 4, 0);
            _palScreen[0x07] = ToRGBA(60, 24, 0);
            _palScreen[0x08] = ToRGBA(32, 42, 0);
            _palScreen[0x09] = ToRGBA(8, 58, 0);
            _palScreen[0x0A] = ToRGBA(0, 64, 0);
            _palScreen[0x0B] = ToRGBA(0, 60, 0);
            _palScreen[0x0C] = ToRGBA(0, 50, 60);
            _palScreen[0x0D] = ToRGBA(0, 0, 0);
            _palScreen[0x0E] = ToRGBA(0, 0, 0);
            _palScreen[0x0F] = ToRGBA(0, 0, 0);

            _palScreen[0x10] = ToRGBA(152, 150, 152);
            _palScreen[0x11] = ToRGBA(8, 76, 196);
            _palScreen[0x12] = ToRGBA(48, 50, 236);
            _palScreen[0x13] = ToRGBA(92, 30, 228);
            _palScreen[0x14] = ToRGBA(136, 20, 176);
            _palScreen[0x15] = ToRGBA(160, 20, 100);
            _palScreen[0x16] = ToRGBA(152, 34, 32);
            _palScreen[0x17] = ToRGBA(120, 60, 0);
            _palScreen[0x18] = ToRGBA(84, 90, 0);
            _palScreen[0x19] = ToRGBA(40, 114, 0);
            _palScreen[0x1A] = ToRGBA(8, 124, 0);
            _palScreen[0x1B] = ToRGBA(0, 118, 40);
            _palScreen[0x1C] = ToRGBA(0, 102, 120);
            _palScreen[0x1D] = ToRGBA(0, 0, 0);
            _palScreen[0x1E] = ToRGBA(0, 0, 0);
            _palScreen[0x1F] = ToRGBA(0, 0, 0);

            _palScreen[0x20] = ToRGBA(236, 238, 236);
            _palScreen[0x21] = ToRGBA(76, 154, 236);
            _palScreen[0x22] = ToRGBA(120, 124, 236);
            _palScreen[0x23] = ToRGBA(176, 98, 236);
            _palScreen[0x24] = ToRGBA(228, 84, 236);
            _palScreen[0x25] = ToRGBA(236, 88, 180);
            _palScreen[0x26] = ToRGBA(236, 106, 100);
            _palScreen[0x27] = ToRGBA(212, 136, 32);
            _palScreen[0x28] = ToRGBA(160, 170, 0);
            _palScreen[0x29] = ToRGBA(116, 196, 0);
            _palScreen[0x2A] = ToRGBA(76, 208, 32);
            _palScreen[0x2B] = ToRGBA(56, 204, 108);
            _palScreen[0x2C] = ToRGBA(56, 180, 204);
            _palScreen[0x2D] = ToRGBA(60, 60, 60);
            _palScreen[0x2E] = ToRGBA(0, 0, 0);
            _palScreen[0x2F] = ToRGBA(0, 0, 0);

            _palScreen[0x30] = ToRGBA(236, 238, 236);
            _palScreen[0x31] = ToRGBA(168, 204, 236);
            _palScreen[0x32] = ToRGBA(188, 188, 236);
            _palScreen[0x33] = ToRGBA(212, 178, 236);
            _palScreen[0x34] = ToRGBA(236, 174, 236);
            _palScreen[0x35] = ToRGBA(236, 174, 212);
            _palScreen[0x36] = ToRGBA(236, 180, 176);
            _palScreen[0x37] = ToRGBA(228, 196, 144);
            _palScreen[0x38] = ToRGBA(204, 210, 120);
            _palScreen[0x39] = ToRGBA(180, 222, 120);
            _palScreen[0x3A] = ToRGBA(168, 226, 144);
            _palScreen[0x3B] = ToRGBA(152, 226, 180);
            _palScreen[0x3C] = ToRGBA(160, 214, 228);
            _palScreen[0x3D] = ToRGBA(160, 162, 160);
            _palScreen[0x3E] = ToRGBA(0, 0, 0);
            _palScreen[0x3F] = ToRGBA(0, 0, 0);
        }

        public static byte ReverseBits(byte b)
        {
            //https://stackoverflow.com/questions/2602823/in-c-c-whats-the-simplest-way-to-reverse-the-order-of-bits-in-A-byte/2602885#2602885
            b = (byte)((b & 0xF0) >> 4 | (b & 0x0F) << 4);
            b = (byte)((b & 0xCC) >> 2 | (b & 0x33) << 2);
            b = (byte)((b & 0xAA) >> 1 | (b & 0x55) << 1);
            return b;
        }

        public void Clock()
        {
            if (_scanline >= -1 && _scanline < 240)
            {
                // --------- BACKGROUND RENDERING ---------

                if (_scanline == 0 && _cycle == 0)
                {
                    // "Odd Frame" _cycle skip
                    _cycle = 1;
                }

                if (_scanline == -1 && _cycle == 1)
                {
                    Status.vertical_blank = 0;
                    Status.sprite_zero_hit = 0;
                    Status.sprite_overflow = 0;

                    for (int i = 0; i < 8; i++)
                    {
                        _spriteShifterPatternLow[i] = 0;
                        _spriteShifterPatternHigh[i] = 0;
                    }
                }

                if ((_cycle >= 2 && _cycle < 258) || (_cycle >= 321 && _cycle < 338))
                {
                    UpdateShifters();

                    switch ((_cycle - 1) % 8)
                    {
                        case 0:
                            LoadBackgroundShifters();
                            _bgNextTileId = PpuRead((UInt16)(0x2000 | (VramAddr.reg & 0x0FFF)));
                            if (_cycle == 321 && _scanline == -1)
                            {
                                DebugSetLastFramePpuScroll(VramAddr.reg, _fineX, false);
                            }
                            break;
                        case 2:
                            _bgNextTileAttrib = PpuRead((UInt16)(0x23C0 | (VramAddr.nametable_y << 11)
                                | (VramAddr.nametable_x << 10)
                                | ((VramAddr.coarse_y >> 2) << 3)
                                | (VramAddr.coarse_x >> 2)));

                            if ((VramAddr.coarse_y & 0x02) != 0) _bgNextTileAttrib >>= 4;
                            if ((VramAddr.coarse_x & 0x02) != 0) _bgNextTileAttrib >>= 2;
                            _bgNextTileAttrib &= 0x03;
                            break;
                        case 4:
                            _bgNextTileLSB = PpuRead((UInt16)((Control.pattern_background << 12)
                                + ((UInt16)_bgNextTileId << 4)
                                + (VramAddr.fine_y) + 0));
                            break;
                        case 6:
                            _bgNextTileMSB = PpuRead((UInt16)((Control.pattern_background << 12)
                                + ((UInt16)_bgNextTileId << 4)
                                + (VramAddr.fine_y) + 8));
                            break;
                        case 7:
                            IncrementScrollX();
                            break;
                    }
                }

                if (_cycle == 256)
                {
                    IncrementScrollY();
                }

                if (_cycle == 257)
                {
                    LoadBackgroundShifters();
                    TransferAddressX();
                }

                if (_cycle == 338 || _cycle == 340)
                {
                    _bgNextTileId = PpuRead((UInt16)(0x2000 | (VramAddr.reg & 0x0FFF)));
                }

                if (_scanline == -1 && _cycle >= 280 && _cycle < 305)
                {
                    TransferAddressY();
                }


                // --------- FOREGROUND RENDERING ---------
                if (_cycle == 257 && _scanline >= 0)
                {
                    // faster clear using memset, DarcyThomas answer
                    // https://stackoverflow.com/questions/6150097/initialize-A-byte-array-to-A-certain-value-other-than-the-default-null
                    // or https://stackoverflow.com/questions/1897555/what-is-the-equivalent-of-memset-in-c
                    for (int i = 0; i < _spriteScanline.Length; i++)
                    {
                        _spriteScanline[i] = 0xFF; //DEBUG_THIS
                    }

                    _spriteCount = 0;

                    for (Byte i = 0; i < 8; i++)
                    {
                        _spriteShifterPatternLow[i] = 0;
                        _spriteShifterPatternHigh[i] = 0;
                    }

                    Byte nOAMEntry = 0;
                    _spriteZeroHitPossible = false;
                    while (nOAMEntry < 64 && _spriteCount < 9)
                    {
                        Int16 diff = (Int16)((Int16)_scanline - (Int16)OAM[(nOAMEntry * 4) + 0]);
                        if (diff >= 0 && diff < (Control.sprite_size != 0 ? 16 : 8))
                        {
                            if (nOAMEntry == 0)
                            {
                                _spriteZeroHitPossible = true;
                            }

                            if (_spriteCount < 8)
                            {
                                // copy sprite from OAM to scanLine
                                _spriteScanline[(_spriteCount * 4) + 0] = OAM[(nOAMEntry * 4) + 0]; // Y
                                _spriteScanline[(_spriteCount * 4) + 1] = OAM[(nOAMEntry * 4) + 1]; // id
                                _spriteScanline[(_spriteCount * 4) + 2] = OAM[(nOAMEntry * 4) + 2]; // attribute
                                _spriteScanline[(_spriteCount * 4) + 3] = OAM[(nOAMEntry * 4) + 3]; // X
                                _spriteCount++;
                            }
                        }
                        nOAMEntry++;
                    }
                    Status.sprite_overflow = _spriteCount > 8 ? (Byte)0b1 : (Byte)0b0;

                }

                if (_cycle == 340)
                {
                    for (Byte i = 0; i < _spriteCount; i++)
                    {
                        Byte sprite_pattern_bits_lo, sprite_pattern_bits_hi;
                        UInt16 sprite_pattern_addr_lo, sprite_pattern_addr_hi;

                        if (Control.sprite_size == 0)
                        {
                            // 8x8 sprite mode
                            if ((_spriteScanline[(i * 4) + 2] & 0x80) == 0)
                            {
                                sprite_pattern_addr_lo = (UInt16)((Control.pattern_sprite << 12)
                                    | (_spriteScanline[(i * 4) + 1] << 4)
                                    | (_scanline - _spriteScanline[(i * 4) + 0]));
                            }
                            else
                            {
                                sprite_pattern_addr_lo = (UInt16)((Control.pattern_sprite << 12)
                                    | (_spriteScanline[(i * 4) + 1] << 4)
                                    | (7 - (_scanline - _spriteScanline[(i * 4) + 0])));
                            }
                        }
                        else
                        {
                            // 8x16 sprite mode
                            if ((_spriteScanline[(i * 4) + 2] & 0x80) == 0)
                            {
                                if (_scanline - _spriteScanline[(i * 4) + 0] < 8)
                                {
                                    sprite_pattern_addr_lo = (UInt16)(((_spriteScanline[(i * 4) + 1] & 0x01) << 12)
                                        | ((_spriteScanline[(i * 4) + 1] & 0xFE) << 4)
                                        | ((_scanline - _spriteScanline[(i * 4) + 0]) & 0x07));
                                }
                                else
                                {
                                    sprite_pattern_addr_lo = (UInt16)(((_spriteScanline[(i * 4) + 1] & 0x01) << 12)
                                        | (((_spriteScanline[(i * 4) + 1] & 0xFE) + 1) << 4)
                                        | ((_scanline - _spriteScanline[(i * 4) + 0]) & 0x07));
                                }
                            }
                            else
                            {
                                if (_scanline - _spriteScanline[(i * 4) + 0] < 8)
                                {
                                    sprite_pattern_addr_lo = (UInt16)(((_spriteScanline[(i * 4) + 1] & 0x01) << 12)
                                        | (((_spriteScanline[(i * 4) + 1] & 0xFE) + 1) << 4)
                                        | (7 - (_scanline - _spriteScanline[(i * 4) + 0]) & 0x07));
                                }
                                else
                                {
                                    sprite_pattern_addr_lo = (UInt16)(((_spriteScanline[(i * 4) + 1] & 0x01) << 12)
                                        | ((_spriteScanline[(i * 4) + 1] & 0xFE) << 4)
                                        | (7 - (_scanline - _spriteScanline[(i * 4) + 0]) & 0x07));
                                }
                            }
                        }

                        sprite_pattern_addr_hi = (UInt16)(sprite_pattern_addr_lo + 8);
                        sprite_pattern_bits_lo = PpuRead(sprite_pattern_addr_lo);
                        sprite_pattern_bits_hi = PpuRead(sprite_pattern_addr_hi);

                        if ((_spriteScanline[(i * 4) + 2] & 0x40) != 0)
                        {
                            // flip horizontally
                            sprite_pattern_bits_lo = ReverseBits(sprite_pattern_bits_lo);
                            sprite_pattern_bits_hi = ReverseBits(sprite_pattern_bits_hi);
                        }

                        _spriteShifterPatternLow[i] = sprite_pattern_bits_lo;
                        _spriteShifterPatternHigh[i] = sprite_pattern_bits_hi;
                    }
                }
            }

            //    if (_cycle == 340)
            //    {
            //        for (var i = 0; i < _spriteCount; i++)
            //        {
            //            int sprite_pattern_addr_lo;
            //            var row_offset = _scanline - _spriteScanline[4 * i];
            //            var flipped = (_spriteScanline[4 * i + 2] & 0x80) != 0;
            //            if (Control.sprite_size == 0) // 8x8 sprite mode
            //                sprite_pattern_addr_lo = Control.pattern_sprite << 12 | _spriteScanline[4 * i + 1] << 4;
            //            else // 8x16 sprite mode
            //            {
            //                sprite_pattern_addr_lo = (_spriteScanline[4 * i + 1] & 0x01) << 12;
            //                if (flipped && row_offset < 8 || !flipped && row_offset >= 8)
            //                    sprite_pattern_addr_lo |= ((_spriteScanline[4 * i + 1] & 0xFE) + 1) << 4;
            //                else
            //                    sprite_pattern_addr_lo |= (_spriteScanline[4 * i + 1] & 0xFE) << 4;
            //            }
            //            if (flipped)
            //                sprite_pattern_addr_lo |= (7 - row_offset) & 0x07;
            //            else
            //                sprite_pattern_addr_lo |= row_offset & 0x07;
            //
            //            var sprite_pattern_addr_hi = sprite_pattern_addr_lo + 8;
            //            _spriteShifterPatternLow[i] = PpuRead((ushort)sprite_pattern_addr_lo);
            //            _spriteShifterPatternHigh[i] = PpuRead((ushort)sprite_pattern_addr_hi);
            //            if ((_spriteScanline[(i * 4) + 2] & 0x40) != 0) // flip horizontally
            //            {
            //                _spriteShifterPatternLow[i] = ReverseBits(_spriteShifterPatternLow[i]);
            //                _spriteShifterPatternHigh[i] = ReverseBits(_spriteShifterPatternHigh[i]);
            //            }
            //        }
            //    }
            //}

            if (_scanline == 240)
            {

            }

            if (_scanline >= 241 && _scanline < 261)
            {
                if (_scanline == 241 && _cycle == 1)
                {
                    Status.vertical_blank = 1;
                    if (Control.enable_nmi != 0)
                    {
                        NMI = true;
                    }
                }
            }


            // --- Background
            Byte bg_pixel = 0x00;   // bg Pixel
            Byte bg_palette = 0x00; // bg Palette

            if (Mask.render_background != 0)
            {
                UInt16 bit_mux = (UInt16)(0x8000 >> _fineX);

                Byte p0_pixel = (_bgShifterPatternLow & bit_mux) > 0 ? (Byte)1 : (Byte)0;
                Byte p1_pixel = (_bgShifterPatternHigh & bit_mux) > 0 ? (Byte)1 : (Byte)0;

                bg_pixel = (Byte)((p1_pixel << 1) | p0_pixel);

                Byte bg_pal0 = (_bgShifterAttribLow & bit_mux) > 0 ? (Byte)1 : (Byte)0;
                Byte bg_pal1 = (_bgShifterAttribHigh & bit_mux) > 0 ? (Byte)1 : (Byte)0;

                bg_palette = (Byte)((bg_pal1 << 1) | bg_pal0);
            }

            // --- Foreground
            Byte fg_pixel = 0x00;
            Byte fg_palette = 0x00;
            Byte fg_priority = 0x00;

            if (Mask.render_sprites != 0)
            {
                _spriteZeroBeingRendered = false;

                for (Byte i = 0; i < _spriteCount; i++)
                {
                    if (_spriteScanline[(i * 4) + 3] == 0)
                    {
                        Byte fg_pixel_lo = (_spriteShifterPatternLow[i] & 0x80) > 0 ? (Byte)0b1 : (Byte)0b0;
                        Byte fg_pixel_hi = (_spriteShifterPatternHigh[i] & 0x80) > 0 ? (Byte)0b1 : (Byte)0b0;
                        fg_pixel = (Byte)((fg_pixel_hi << 1) | fg_pixel_lo);

                        fg_palette = (Byte)((_spriteScanline[(i * 4) + 2] & 0x03) + 0x04);
                        fg_priority = (_spriteScanline[(i * 4) + 2] & 0x20) == 0 ? (Byte)0b1 : (Byte)0b0;

                        if (fg_pixel != 0)
                        {
                            if (i == 0)
                            {
                                _spriteZeroBeingRendered = true;
                            }
                            break;
                        }
                    }
                }
            }

            Byte pixel = 0x00;   // FINAL Pixel
            Byte palette = 0x00; // FINAL Palette

            if (bg_pixel == 0 && fg_pixel == 0)
            {
                pixel = 0x00;
                palette = 0x00;
            }
            else if (bg_pixel == 0 && fg_pixel > 0)
            {
                pixel = fg_pixel;
                palette = fg_palette;
            }
            else if (bg_pixel > 0 && fg_pixel == 0)
            {
                pixel = bg_pixel;
                palette = bg_palette;
            }
            else if (bg_pixel > 0 && fg_pixel > 0)
            {
                if (fg_priority != 0)
                {
                    pixel = fg_pixel;
                    palette = fg_palette;
                }
                else
                {
                    pixel = bg_pixel;
                    palette = bg_palette;
                }

                if (_spriteZeroHitPossible && _spriteZeroBeingRendered)
                {
                    if ((Mask.render_background & Mask.render_sprites) != 0)
                    {
                        if (~(Mask.render_background_left | Mask.render_sprites_left) != 0)
                        {
                            if (_cycle >= 9 && _cycle < 258)
                            {
                                Status.sprite_zero_hit = 1;
                            }
                        }
                        else
                        {
                            if (_cycle >= 1 && _cycle < 258)
                            {
                                Status.sprite_zero_hit = 1;
                            }
                        }
                    }
                }
            }

            SetPixel(ref _screen, 256, (Int16)(_cycle - 0x0001), _scanline, GetColourFromPaletteRam(palette, pixel), 240);

            _cycle++;
            if (_cycle >= 341)
            {
                _cycle = 0;
                _scanline++;
                //debug
                if (_scanline == 241)
                {
                    DebugAtScanline = true;
                    if (_tempX != ScrollX)
                    {
                        Console.WriteLine("X:" + ScrollX + ", Y:" + ScrollY);
                        _tempX = ScrollX;
                    }
                    else if (_tempY != ScrollY)
                    {
                        Console.WriteLine("X:" + ScrollX + ", Y:" + ScrollY);
                        _tempY = ScrollY;
                    }
                }
                if (_scanline >= 261)
                {
                    _scanline = -1;
                    FrameComplete = true;
                }
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

        public UInt32[] GetScreen()
        {
            return _screen;
        }

        /// <summary>
        /// Debug view of the specific pattern table for given palette
        /// </summary>
        /// <param name="i"></param>
        /// <param name="palette"></param>
        /// <returns></returns>
        public UInt32[] GetPatternTable(Byte i, Byte palette)
        {
            for (UInt16 nTileY = 0; nTileY < 16; nTileY++)
            {
                for (UInt16 nTileX = 0; nTileX < 16; nTileX++)
                {
                    UInt16 nOffset = (UInt16)(nTileY * 256 + nTileX * 16); // byte offset
                    for (UInt16 row = 0; row < 8; row++)
                    {
                        Byte tile_lsb = PpuRead((UInt16)(i * 0x1000 + nOffset + row + 0));
                        Byte tile_msb = PpuRead((UInt16)(i * 0x1000 + nOffset + row + 8));

                        for (UInt16 col = 0; col < 8; col++)
                        {
                            Byte pixel = (Byte)((tile_lsb & 0x01) + ((tile_msb & 0x01) << 1));
                            //Byte pixel = (Byte)((tile_lsb & 1) | (tile_msb >> 7));
                            tile_lsb >>= 1;
                            tile_msb >>= 1;

                            SetPixel(ref _patternTable[i], 128, nTileX * 8 + (7 - col), nTileY * 8 + row, GetColourFromPaletteRam(palette, pixel));
                        }
                    }
                }
            }

            return _patternTable[i];
        }

        // TO JEST PROWIZORYCZNY WYGLAD, PALETY MOGA SIE NIE ZGADZAC, ale
        // co do umieszczania kafelka to juz tak(bo tylko te informacje sa w danym momencie w RAM'ie) :)
        /// <summary>
        /// Debug view of the specific name table
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public UInt32[] GetNameTable(Byte i)
        {
            //rgba
            //UInt32 red = 0xFF000000;
            //UInt32 green = 0x00FF0000;
            //UInt32 blue = 0x0000FF00;
            //UInt32 yellow = 0xFFFF0000;
            //32x30
            for (UInt16 nTileY = 0; nTileY < 30; nTileY++)
            {
                for (UInt16 nTileX = 0; nTileX < 32; nTileX++)
                {
                    if(nTileX == 31 && nTileY == 29)
                    {
                        int asd = 1;
                    }
                    //0x0 - AttrGroup:0x0, Attr:0x0, Data:AA, Top-Left
                    //0x1 - AttrGroup:0x0, Attr:0x0, Data:AA, Top-Left
                    //0x2 - AttrGroup:0x0, Attr:0x1, Data:AA, Top-Right
                    //0x3 - AttrGroup:0x0, Attr:0x1, Data:AA, Top-Right
                    //0x4 - AttrGroup:0x1, Attr:0x2, Data:AA, Top-Left

                    //1x0 - AttrGroup:0x0, Attr:0x0, Data:AA, Top-Left

                    //2x0 - AttrGroup:0x0, Attr:1x0, Data:AA, Bottom-Left

                    //4x0 - AttrGroup:1x0, Attr:2x0, Data:00, Top-Left

                    //UInt16 nOffset = (UInt16)(nTileY * 256 + nTileX * 32); // byte offset

                    Byte attr_group_grid_x = (Byte)(nTileX / 4); Byte attr_grid_x = (Byte)(nTileX / 2);
                    Byte attr_group_grid_y = (Byte)(nTileY / 4); Byte attr_grid_y = (Byte)(nTileY / 2);
                    Byte attr_grid_index = (Byte)(attr_group_grid_y * 8 + attr_group_grid_x);
                    Byte attr_grid_data = _tblName[i][0x3C0 + attr_grid_index];
                    Byte palette;

                    if ((Byte)((attr_grid_y + 1) % 2) == 0)
                    {
                        //even
                        if ((Byte)((attr_grid_x + 1) % 2) == 0)
                            palette = (Byte)((attr_grid_data >> 6) & 0b11);    //even/ even //bottom right
                        else
                            palette = (Byte)((attr_grid_data >> 4) & 0b11);    //even/ odd  //bottom left
                    }
                    else
                    {
                        if ((Byte)((attr_grid_x + 1) % 2) == 0)
                            palette = (Byte)((attr_grid_data >> 2) & 0b11);    // odd / even //top right
                        else
                            palette = (Byte)((attr_grid_data >> 0) & 0b11);    // odd / even //top Left
                    }

                    Byte tile_index = _tblName[i][nTileY * 32 + nTileX];
                    for (UInt16 row = 0; row < 8; row++)
                    {
                        //name table 0 0x2000 (Byte TILE_ID(only from second half?)) 
                            //attr 0   0x23C0
                        //name table 1 0x23FF
                            //attr 1   0x27C0

                        Byte tile_lsb = PpuRead((UInt16)(0x1000 + (tile_index << 4) + row + 0));
                        Byte tile_msb = PpuRead((UInt16)(0x1000 + (tile_index << 4) + row + 8));

                        for (UInt16 col = 0; col < 8; col++)
                        {
                            Byte pixel = (Byte)((tile_lsb & 0x01) + ((tile_msb & 0x01) << 1));
                            //Byte pixel = (Byte)((tile_lsb & 1) | (tile_msb >> 7));
                            tile_lsb >>= 1;
                            tile_msb >>= 1;

                            //UInt32 debugCol = 0xFFFFFF00;

                            //switch (palette)
                            //{
                            //    case 0b00:
                            //        debugCol = red; break;
                            //    case 0b01:
                            //        debugCol = green; break;
                            //    case 0b10:
                            //        debugCol = blue; break;
                            //    case 0b11:
                            //        debugCol = yellow; break;
                            //}

                            //SetPixel(ref _nameTable[i], 256, nTileX * 8 + (7 - col), nTileY * 8 + row, debugCol);
                            SetPixel(ref _nameTable[i], 256, nTileX * 8 + (7 - col), nTileY * 8 + row, GetColourFromPaletteRam(palette, pixel));
                        }
                    }
                }
            }

            return _nameTable[i];
        }

        public Boolean GetMirroringType()
        {
            return _bus.Cartridge.Mirror() == NESGamePak.Mapper.MIRROR.VERTICAL;
        }

        /// <summary>
        /// Debug view of the two Name Tables including mirroring
        /// </summary>
        /// <returns></returns>
        public UInt32[] GetNameTables()
        {
            UInt32[] FourScreens;
            if (_bus.Cartridge.Mirror() == NESGamePak.Mapper.MIRROR.VERTICAL)
            {
                UInt32[] TwoScreens = Join1DArraysWithKnownSize(GetNameTable(0), GetNameTable(1), 256, 240, false);
                FourScreens = Join1DArraysWithKnownSize(TwoScreens, TwoScreens, 512, 240, true);
            } else
            {
                UInt32[] TwoScreens = Join1DArraysWithKnownSize(GetNameTable(0), GetNameTable(1), 256, 240, true);
                FourScreens = Join1DArraysWithKnownSize(TwoScreens, TwoScreens, 256, 480, false);
            }
            
            return FourScreens;
        }

        public UInt32 GetColourFromPaletteRam(Byte palette, Byte pixel)
        {
            UInt16 address = (UInt16)(0x3F00 + (palette << 2) + pixel);
            Byte colourIndex = PpuRead(address);
            return _palScreen[colourIndex & 0x3F]; // looks like this Mask is redundant, based on what ppuRead returns
        }

        // CPU <---> PPU
        public Byte CpuRead(UInt16 addr, Boolean bReadOnly)
        {
            Byte data = 0x00;

            switch (addr)
            {
                case 0x0000: // Control
                    break;
                case 0x0001: // Mask
                    break;
                case 0x0002: // Status
                    data = (Byte)((Status.reg & 0xE0) | (_ppuDataBuffer & 0x1F));
                    Status.vertical_blank = 0;
                    _addressLatch = 0;
                    break;
                case 0x0003: // OAM Address
                    //"doesnt make any sens to read from the addr register"
                    break;
                case 0x0004: // OAM Data
                    data = OAM[_oamAddr];
                    break;
                case 0x0005: // Scroll
                    break;
                case 0x0006: // PPU Address
                    break;
                case 0x0007: // PPU Data
                    data = _ppuDataBuffer;
                    _ppuDataBuffer = PpuRead(VramAddr.reg);

                    if (VramAddr.reg >= 0x3F00) data = _ppuDataBuffer;
                    VramAddr.reg += (UInt16)((Control.increment_mode != 0 ? 32 : 1));
                    break;
            }


            return data;
        }

        public void CpuWrite(UInt16 addr, Byte data)
        {
            switch (addr)
            {
                case 0x0000: // Control
                    Control.reg = data;
                    TramAddr.nametable_x = Control.nametable_x;
                    TramAddr.nametable_y = Control.nametable_y;
                    break;
                case 0x0001: // Mask
                    Mask.reg = data; // DEBUG
                    break;
                case 0x0002: // Status
                    break;
                case 0x0003: // OAM Address
                    _oamAddr = data;
                    break;
                case 0x0004: // OAM Data
                    OAM[_oamAddr] = data;
                    break;
                case 0x0005: // Scroll
                    if (_addressLatch == 0)
                    {
                        _fineX = (Byte)(data & 0x07);
                        TramAddr.coarse_x = (UInt16)(data >> 3);
                        _addressLatch = 1;
                    }
                    else
                    {
                        TramAddr.fine_y = (Byte)(data & 0x07);
                        TramAddr.coarse_y = (UInt16)(data >> 3);
                        _addressLatch = 0;
                    }
                    break;
                case 0x0006: // PPU Address
                    if (_addressLatch == 0)
                    {
                        TramAddr.reg = (UInt16)((UInt16)((data & 0x3F) << 8) | (TramAddr.reg & 0x00FF));
                        _addressLatch = 1;

                        // SCROLL DEBUG v2
                        DebugSetLastFramePpuScroll(VramAddr.reg, _fineX, false);
                    }
                    else
                    {
                        TramAddr.reg = (UInt16)((TramAddr.reg & 0xFF00) | data);
                        VramAddr.reg = TramAddr.reg; // might cause problems!
                        _addressLatch = 0;
                    }
                    break;
                case 0x0007: // PPU Data
                    PpuWrite(VramAddr.reg, data);
                    VramAddr.reg += (UInt16)(Control.increment_mode != 0 ? 32 : 1);
                    break;
            }
        }

        // PPU <---> PPU_BUS
        private Byte PpuRead(UInt16 addr, Boolean bReadOnly = true)
        {
            Byte data = 0x00;
            addr &= 0x3FFF;

            if (_bus.Cartridge.PpuRead(addr, ref data))
            {

            }
            else if (addr >= 0x0000 && addr <= 0x1FFF)
            {
                //_temp for cartridge or debug
                data = _tblPattern[(addr & 0x1000) >> 12][addr & 0x0FFF];
            }
            else if (addr >= 0x2000 && addr <= 0x3EFF)
            {
                addr &= 0x0FFF;

                if (_bus.Cartridge.Mirror() == NESGamePak.Mapper.MIRROR.VERTICAL)
                {
                    // Vertical
                    if (addr >= 0x0000 & addr <= 0x03FF)
                        data = _tblName[0][addr & 0x03FF];
                    if (addr >= 0x0400 & addr <= 0x07FF)
                        data = _tblName[1][addr & 0x03FF];
                    if (addr >= 0x0800 & addr <= 0x0BFF)
                        data = _tblName[0][addr & 0x03FF];
                    if (addr >= 0x0C00 & addr <= 0x0FFF)
                        data = _tblName[1][addr & 0x03FF];
                }
                else if (_bus.Cartridge.Mirror() == NESGamePak.Mapper.MIRROR.HORIZONTAL)
                {
                    // Horizontal
                    if (addr >= 0x0000 & addr <= 0x03FF)
                        data = _tblName[0][addr & 0x03FF];
                    if (addr >= 0x0400 & addr <= 0x07FF)
                        data = _tblName[0][addr & 0x03FF];
                    if (addr >= 0x0800 & addr <= 0x0BFF)
                        data = _tblName[1][addr & 0x03FF];
                    if (addr >= 0x0C00 & addr <= 0x0FFF)
                        data = _tblName[1][addr & 0x03FF];
                }
            }
            else if (addr >= 0x3F00 && addr <= 0x3FFF)
            {
                addr &= 0x001F;
                switch (addr)
                {
                    case 0x0010:
                        addr = 0x0000; break;
                    case 0x0014:
                        addr = 0x0004; break;
                    case 0x0018:
                        addr = 0x0008; break;
                    case 0x001C:
                        addr = 0x000C; break;
                    default:
                        break;
                }
                data = (Byte)(_tblPalette[addr] & (Mask.greyscale != 0 ? 0x30 : 0x3F));
            }

            return data;
        }

        private void PpuWrite(UInt16 addr, Byte data)
        {
            addr &= 0x3FFF;

            if (_bus.Cartridge.PpuWrite(addr, data))
            {

            }
            else if (addr >= 0x0000 && addr <= 0x1FFF)
            {
                _tblPattern[(addr & 0x1000) >> 12][addr & 0x0FFF] = data;
            }
            else if (addr >= 0x2000 && addr <= 0x3EFF)
            {
                addr &= 0x0FFF;

                if (_bus.Cartridge.Mirror() == NESGamePak.Mapper.MIRROR.VERTICAL)
                {
                    // Vertical
                    if (addr >= 0x0000 & addr <= 0x03FF)
                        _tblName[0][addr & 0x03FF] = data;
                    if (addr >= 0x0400 & addr <= 0x07FF)
                        _tblName[1][addr & 0x03FF] = data;
                    if (addr >= 0x0800 & addr <= 0x0BFF)
                        _tblName[0][addr & 0x03FF] = data;
                    if (addr >= 0x0C00 & addr <= 0x0FFF)
                        _tblName[1][addr & 0x03FF] = data;
                }
                else if (_bus.Cartridge.Mirror() == NESGamePak.Mapper.MIRROR.HORIZONTAL)
                {
                    // Horizontal
                    if (addr >= 0x0000 & addr <= 0x03FF)
                        _tblName[0][addr & 0x03FF] = data;
                    if (addr >= 0x0400 & addr <= 0x07FF)
                        _tblName[0][addr & 0x03FF] = data;
                    if (addr >= 0x0800 & addr <= 0x0BFF)
                        _tblName[1][addr & 0x03FF] = data;
                    if (addr >= 0x0C00 & addr <= 0x0FFF)
                        _tblName[1][addr & 0x03FF] = data;
                }
            }
            else if (addr >= 0x3F00 && addr <= 0x3FFF)
            {
                addr &= 0x001F;
                switch (addr)
                {
                    case 0x0010:
                        addr = 0x0000; break;
                    case 0x0014:
                        addr = 0x0004; break;
                    case 0x0018:
                        addr = 0x0008; break;
                    case 0x001C:
                        addr = 0x000C; break;
                    default:
                        break;
                }
                _tblPalette[addr] = data;
            }
        }

        private void DebugSetLastFramePpuScroll(UInt16 reg, Byte fine_x, bool horizontalScrollOnly)
        {
            ScrollX = (UInt16)(((reg & 0x1F) << 3) | fine_x | (((reg & 0x400) != 0) ? 0x100 : 0));
            if (!horizontalScrollOnly)
            {
                ScrollY = (UInt16)((((reg & 0x3E0) >> 2) | ((reg & 0x7000) >> 12)) + (((reg & 0x800) != 0) ? 240 : 0));
            }
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

                DebugSetLastFramePpuScroll(VramAddr.reg, _fineX, true);
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
        }

        /// <summary>
        /// Prime the "in-effect" background tile shifters ready for outputting next
	    /// 8 pixels in scanline.
        /// </summary>
        private void LoadBackgroundShifters()
        {
            _bgShifterPatternLow = (UInt16)((_bgShifterPatternLow & 0xFF00) | _bgNextTileLSB);
            _bgShifterPatternHigh = (UInt16)((_bgShifterPatternHigh & 0xFF00) | _bgNextTileMSB);

            _bgShifterAttribLow = (UInt16)((_bgShifterAttribLow & 0xFF00) | ((_bgNextTileAttrib & 0b01) != 0 ? 0xFF : 0x00));
            _bgShifterAttribHigh = (UInt16)((_bgShifterAttribHigh & 0xFF00) | ((_bgNextTileAttrib & 0b10) != 0 ? 0xFF : 0x00));
        }

        /// <summary>
        /// Every cycle the shifters storing pattern and attribute information shift
        /// their contents by 1 bit. This is because every cycle, the output progresses
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

            if (Mask.render_sprites != 0 && _cycle >= 1 && _cycle < 258)
            {
                for (int i = 0; i < _spriteCount; i++)
                {
                    int sprite_x = (i * 4) + 3;
                    if (_spriteScanline[sprite_x] > 0)
                    {
                        _spriteScanline[sprite_x]--;
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