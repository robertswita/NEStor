using System;
using emulatorTest.NESGamePak;
using System.Threading;
using System.Diagnostics;
using System.Reflection;
using System.IO;

namespace emulatorTest
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

            Threads threads = new Threads();
            Thread t1 = new Thread(new ThreadStart(threads.GUI));
            t1.SetApartmentState(ApartmentState.STA);
            t1.Start();
            while (threads.dziala == false)
            {
               Thread.Sleep(1);
            }


            Console.BackgroundColor = ConsoleColor.Black;
            
            Run(ref threads);
        }

        public static void Run(ref Threads threads)
        {
            Cartridge Cartridge = new Cartridge(ROM_PATH);

            threads.form.FormINESHeader.BinRomHeader = Cartridge.BinHeader;

            if (!Cartridge.ImageValid())
                Console.WriteLine("Invalid image!");

            Bus Bus = new Bus(ref Cartridge);

            Bus.Reset();

            threads.form.UpdateName(ROM_PATH);

            bool close = false;

            Stopwatch stopwatch = Stopwatch.StartNew();
            int frameCount = 0;
            double timeElapsed = 0.0;

            while (!close)
            {
                if (threads.form.reset)
                {
                    threads.form.reset = false;
                    Bus.Reset();
                }

                while (threads.form.pause == true && !threads.form.IsDisposed)
                {
                    Thread.Sleep(100);
                    // wait
                }

                if (threads.form.newRomFile)
                {
                    threads.form.newRomFile = false;
                    ROM_PATH = threads.form.romPath;
                    Cartridge = new Cartridge(ROM_PATH);
                    threads.form.FormINESHeader.BinRomHeader = Cartridge.BinHeader;
                    Bus = new Bus(ref Cartridge);
                    Bus.Reset();
                    threads.form.UpdateName(ROM_PATH);
                }

                if (threads.form.IsDisposed)
                {
                    close = true;
                }
                

                Bus.controller[0] = 0x00;
                Bus.controller[0] |= threads.form.btnA ? (Byte)0x80 : (Byte)0x00;
                Bus.controller[0] |= threads.form.btnB ? (Byte)0x40 : (Byte)0x00;
                Bus.controller[0] |= threads.form.btnSelect ? (Byte)0x20 : (Byte)0x00;
                Bus.controller[0] |= threads.form.btnStart ? (Byte)0x10 : (Byte)0x00;
                Bus.controller[0] |= threads.form.btnUp ? (Byte)0x08 : (Byte)0x00;
                Bus.controller[0] |= threads.form.btnDown ? (Byte)0x04 : (Byte)0x00;
                Bus.controller[0] |= threads.form.btnLeft ? (Byte)0x02 : (Byte)0x00;
                Bus.controller[0] |= threads.form.btnRight ? (Byte)0x01 : (Byte)0x00;

                int frameRate = 60;
                timeElapsed = stopwatch.Elapsed.TotalSeconds;

                if (timeElapsed >= frameCount * (1.0 / frameRate))
                {
                    do
                    {
                        Bus.Clock();

                        if (threads.form.debug)
                        {
                            if (Bus.PPU.DebugAtScanline == true)
                            {
                                UInt32[] debugPatternTable0 = Bus.PPU.GetPatternTable(0, 0);
                                //UInt32[] newDebugPatternTable0 = new UInt32[128 * 128];
                                //FilpVerticallyOptimized(ref debugPatternTable0, ref newDebugPatternTable0, 128, 128);

                                UInt32[] debugPatternTable1 = Bus.PPU.GetPatternTable(1, 0);
                                //UInt32[] newDebugPatternTable1 = new UInt32[128 * 128];
                                //FilpVerticallyOptimized(ref debugPatternTable1, ref newDebugPatternTable1, 128, 128);

                                threads.form.UpdateDebugViewPatternTable(ref debugPatternTable0, ref debugPatternTable1);


                                UInt32[] debugNameTable = Bus.PPU.GetNameTables();
                                //UInt32[] newDebugNameTable = new UInt32[256 * 2 * 240 * 2];
                                //FilpVerticallyOptimized(ref debugNameTable, ref newDebugNameTable, 256 * 2, 240 * 2);
                                threads.form.UpdateDebugViewNameTable(ref debugNameTable, Bus.PPU.ScrollX, Bus.PPU.ScrollY, Bus.PPU.GetMirroringType());

                                UInt32[] debugPalette = new UInt32[4 * 8];
                                for (Byte p = 0; p < 8; p++)
                                {
                                    for (Byte col = 0; col < 4; col++)
                                    {
                                        debugPalette[(p * 4) + col] = Bus.PPU.GetColourFromPaletteRam(p, col);
                                    }
                                }
                                threads.form.UpdateDebugViewPalette(ref debugPalette);
                            }
                            Bus.PPU.DebugAtScanline = false;
                        }

                    } while (!Bus.PPU.FrameComplete);

                    Bus.PPU.FrameComplete = false;


                    UInt32[] frame = Bus.PPU.GetScreen();
                    //UInt32[] newFrame = new UInt32[256 * 240];
                    //FilpVerticallyOptimized(ref frame, ref newFrame, 256, 240);
                    threads.form.UpdateView(ref frame);
                    frameCount++;
                }
                // Calculate the framerate every second
                if (timeElapsed >= 1.0)
                {
                    threads.form.UpdateFps(frameCount);
                    frameCount = 0;
                    timeElapsed = 0.0;
                    stopwatch.Restart();
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
