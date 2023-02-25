using System;
using NESemuCore.NESGamePak;
using System.Threading;
using System.Diagnostics;
using System.Reflection;
using System.IO;

namespace NESemuCore
{
    class Program
    {
        //static string ROM_PATH = ".\\Zelda II - The Adventure of Link (USA).nes";
        static string ROM_PATH = "demo.nes";

        static void Main(string[] args)
        {
            Console.WriteLine("Assembly Location: " + Assembly.GetCallingAssembly().Location);

            ROM_PATH = Path.GetDirectoryName(Assembly.GetCallingAssembly().Location) + "\\demo.nes";

            if (args.Length == 0)
            {
                Console.WriteLine("No arguments provided, using default rom path");
            }
            else
            {
                for (int i = 0; i < args.Length; i++)
                {
                    switch (args[i])
                    {
                        case "-r":
                        case "--rom":
                            i++;
                            ROM_PATH = args[i];
                            break;
                    }
                }
            }
            Console.WriteLine("ROM_PATH: " + ROM_PATH);

            WinGUI gui = new WinGUI();
            Thread guiThread = new Thread(new ThreadStart(gui.GUI));
            guiThread.SetApartmentState(ApartmentState.STA);
            guiThread.Start();
            while (!gui.isReady())
            {
               Thread.Sleep(1);
            }
            
            Run(ref gui);
        }

        public static void Run(ref WinGUI guiThread)
        {
            Cartridge Cartridge = new Cartridge(ROM_PATH);

            guiThread.form.FormINESHeader.BinRomHeader = Cartridge.BinHeader;

            if (!Cartridge.ImageValid())
                Console.WriteLine("Invalid image!");

            Bus Bus = new Bus(ref Cartridge);

            Bus.Reset();

            guiThread.form.UpdateName(ROM_PATH);

            bool close = false;

            Stopwatch stopwatch = Stopwatch.StartNew();
            int frameCount = 0;
            double timeElapsed = 0.0;

            while (!close)
            {
                if (guiThread.form.reset)
                {
                    guiThread.form.reset = false;
                    Bus.Reset();
                }

                if (guiThread.form.newRomFile)
                {
                    guiThread.form.newRomFile = false;
                    ROM_PATH = guiThread.form.romPath;
                    Cartridge = new Cartridge(ROM_PATH);
                    guiThread.form.FormINESHeader.BinRomHeader = Cartridge.BinHeader;
                    Bus = new Bus(ref Cartridge);
                    Bus.Reset();
                    guiThread.form.UpdateName(ROM_PATH);
                }

                if (guiThread.form.IsDisposed)
                {
                    close = true;
                }

                if (guiThread.form.pause == true && !guiThread.form.IsDisposed)
                {
                    Thread.Sleep(100);
                }
                else
                {
                    Bus.controller[0] = 0x00;
                    Bus.controller[0] |= guiThread.form.btnA ? (Byte)0x80 : (Byte)0x00;
                    Bus.controller[0] |= guiThread.form.btnB ? (Byte)0x40 : (Byte)0x00;
                    Bus.controller[0] |= guiThread.form.btnSelect ? (Byte)0x20 : (Byte)0x00;
                    Bus.controller[0] |= guiThread.form.btnStart ? (Byte)0x10 : (Byte)0x00;
                    Bus.controller[0] |= guiThread.form.btnUp ? (Byte)0x08 : (Byte)0x00;
                    Bus.controller[0] |= guiThread.form.btnDown ? (Byte)0x04 : (Byte)0x00;
                    Bus.controller[0] |= guiThread.form.btnLeft ? (Byte)0x02 : (Byte)0x00;
                    Bus.controller[0] |= guiThread.form.btnRight ? (Byte)0x01 : (Byte)0x00;

                    int frameRate = 60;
                    timeElapsed = stopwatch.Elapsed.TotalSeconds;

                    if (timeElapsed >= frameCount * (1.0 / frameRate))
                    {
                        do
                        {
                            Bus.Clock();

                            if (guiThread.form.debug)
                            {
                                if (Bus.PPU.DebugAtScanline == true)
                                {
                                    UInt32[] debugPatternTable0 = Bus.PPU.GetPatternTable(0, 0);

                                    UInt32[] debugPatternTable1 = Bus.PPU.GetPatternTable(1, 0);

                                    guiThread.form.UpdateDebugViewPatternTable(ref debugPatternTable0, ref debugPatternTable1);

                                    UInt32[] debugNameTable = Bus.PPU.GetNameTables();
                                    guiThread.form.UpdateDebugViewNameTable(ref debugNameTable, Bus.PPU.ScrollX, Bus.PPU.ScrollY, Bus.PPU.GetMirroringType());

                                    UInt32[] debugPalette = new UInt32[4 * 8];
                                    for (Byte p = 0; p < 8; p++)
                                    {
                                        for (Byte col = 0; col < 4; col++)
                                        {
                                            debugPalette[(p * 4) + col] = Bus.PPU.GetColourFromPaletteRam(p, col);
                                        }
                                    }
                                    guiThread.form.UpdateDebugViewPalette(ref debugPalette);
                                }
                                Bus.PPU.DebugAtScanline = false;
                            }

                        } while (!Bus.PPU.FrameComplete);

                        Bus.PPU.FrameComplete = false;


                        UInt32[] frame = Bus.PPU.GetScreen();
                        guiThread.form.UpdateView(ref frame);
                        frameCount++;
                    }

                    // Calculate the framerate every second
                    if (timeElapsed >= 1.0)
                    {
                        guiThread.form.UpdateFps(frameCount);
                        frameCount = 0;
                        timeElapsed = 0.0;
                        stopwatch.Restart();
                    }
                }
            }
        }

        static void FilpVerticallyOptimized(ref UInt32[] frame, ref UInt32[] newFrame, int width, int height)
        {
            unsafe
            {
                fixed (UInt32* pFrame = frame)
                fixed (UInt32* pNewFrame = newFrame)
                {
                    UInt32* pSrc = pFrame + (height - 1) * width;
                    UInt32* pDst = pNewFrame;

                    for (int y = 0; y < height; y++)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            *pDst++ = *pSrc++;
                        }
                        pSrc -= width * 2;
                    }
                }
            }
        }
    }
}
