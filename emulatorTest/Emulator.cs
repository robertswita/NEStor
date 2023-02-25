using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using emulatorTest.NESGamePak;

namespace emulatorTest
{
    class Emulator
    {
        private Cartridge _cartridge = null;
        private Bus _bus = null;
        private bool _isRunning = false;
        private bool _isPaused = false;
        private Thread emulatorThread = null;
        private bool _isPpuDebug = false;

        private bool _btnA, _btnB, _btnStart, _btnSelect, _btnUp, _btnDown, _btnLeft, _btnRight;

        Stopwatch stopwatch = Stopwatch.StartNew();
        int frameCount = 0;
        double timeElapsed = 0.0;

        private UInt32[] _screenFrame;


        UInt32[] debugPatternTable0;
        UInt32[] debugPatternTable1;
        UInt32[] debugNameTable;
        UInt32[] debugPalette = new UInt32[4 * 8];

        ushort _ppuScrollX;
        ushort _ppuScrollY;

        private int _fps;

        public Emulator()
        {

        }

        void Instance()
        {
            while (_isRunning)
            {
                if (_cartridge != null)
                {
                    if (_bus != null)
                    {
                        _bus.controller[0] = 0x00;
                        _bus.controller[0] |= _btnA ? (Byte)0x80 : (Byte)0x00;
                        _bus.controller[0] |= _btnB ? (Byte)0x40 : (Byte)0x00;
                        _bus.controller[0] |= _btnSelect ? (Byte)0x20 : (Byte)0x00;
                        _bus.controller[0] |= _btnStart ? (Byte)0x10 : (Byte)0x00;
                        _bus.controller[0] |= _btnUp ? (Byte)0x08 : (Byte)0x00;
                        _bus.controller[0] |= _btnDown ? (Byte)0x04 : (Byte)0x00;
                        _bus.controller[0] |= _btnLeft ? (Byte)0x02 : (Byte)0x00;
                        _bus.controller[0] |= _btnRight ? (Byte)0x01 : (Byte)0x00;

                        int frameRate = 60;
                        timeElapsed = stopwatch.Elapsed.TotalSeconds;

                        if (timeElapsed >= frameCount * (1.0 / frameRate))
                        {
                            do
                            {
                                _bus.Clock();

                                if (_isPpuDebug)
                                {
                                    if (_bus.PPU.DebugAtScanline == true)
                                    {
                                        debugPatternTable0 = _bus.PPU.GetPatternTable(0, 0);
                                        debugPatternTable1 = _bus.PPU.GetPatternTable(1, 0);
                                        debugNameTable = _bus.PPU.GetNameTables();
                                        _ppuScrollX = _bus.PPU.ScrollX;
                                        _ppuScrollY = _bus.PPU.ScrollY;

                                        debugPalette = new UInt32[4 * 8];
                                        for (Byte p = 0; p < 8; p++)
                                        {
                                            for (Byte col = 0; col < 4; col++)
                                            {
                                                debugPalette[(p * 4) + col] = _bus.PPU.GetColourFromPaletteRam(p, col);
                                            }
                                        }
                                    }
                                    _bus.PPU.DebugAtScanline = false;
                                }

                            } while (!_bus.PPU.FrameComplete);

                            _bus.PPU.FrameComplete = false;
                            _screenFrame = _bus.PPU.GetScreen();
                            frameCount++;
                        }
                        // Calculate the framerate every second
                        if (timeElapsed >= 1.0)
                        {
                            _fps = frameCount;
                            frameCount = 0;
                            timeElapsed = 0.0;
                            stopwatch.Restart();
                        }
                    }
                }
                else
                {
                    //demo?
                }
            }
        }

        public void Run()
        {
            emulatorThread = new Thread(new ThreadStart(Instance));
            //emulatorCore.SetApartmentState(ApartmentState.STA);
            emulatorThread.Start();
        }

        public bool InsertCartridge(string romPath)
        {
            try
            {
                _cartridge = new Cartridge(romPath);
                _bus = new Bus(ref _cartridge);
                _bus.Reset();

                return true;
            } catch
            {
                return false;
            }
        }

        public byte[] DebugGetRomHeader()
        {
            return _cartridge.BinHeader;
        }

        public void UpdateInputs(bool btnA, bool btnB, bool start, bool select, bool up, bool down, bool left, bool right)
        {
            _btnA = btnA;
            _btnB = btnB;
            _btnSelect = select;
            _btnStart = start;
            _btnUp = up;
            _btnDown = down;
            _btnLeft = left;
            _btnRight = right;
        }

        void Reset()
        {
            if(_isPaused)
            {
                DebugPause();
            }

            _bus.Reset();
        }

        void DebugPause()
        {
            _isPaused = !_isPaused;
            if (_isPaused)
            {
                emulatorThread.Suspend();
            }
            else
            {
                emulatorThread.Resume();
            }
        }

        public UInt32[] GetFourNametablesFrame()
        {
            return debugNameTable;
        }

        public UInt32[] GetPatternTableFrame(int t = 0)
        {
            if(t != 0)
            {
                return debugPatternTable1;
            } else
            {
                return debugPatternTable0;
            }
        }

        public bool GetMirroringType()
        {
            return _bus.PPU.GetMirroringType();
        }
    }
}
