using Common;
using emulatorTest.NESGamePak;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;

namespace emulatorTest
{
    class Program
    {
        static string ROM_PATH = ".\\Zelda II - The Adventure of Link (USA).nes";
        //static string ROM_PATH = "demo.nes";

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
                Bus.controller[0] |= threads.form.btnA ? (byte)0x80 : (byte)0x00;
                Bus.controller[0] |= threads.form.btnB ? (byte)0x40 : (byte)0x00;
                Bus.controller[0] |= threads.form.btnSelect ? (byte)0x20 : (byte)0x00;
                Bus.controller[0] |= threads.form.btnStart ? (byte)0x10 : (byte)0x00;
                Bus.controller[0] |= threads.form.btnUp ? (byte)0x08 : (byte)0x00;
                Bus.controller[0] |= threads.form.btnDown ? (byte)0x04 : (byte)0x00;
                Bus.controller[0] |= threads.form.btnLeft ? (byte)0x02 : (byte)0x00;
                Bus.controller[0] |= threads.form.btnRight ? (byte)0x01 : (byte)0x00;

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
                                var ppuViewer = threads.form.FormPPUViewer;
                                ppuViewer.glViewPatternTable0.FrameBuffer = Bus.PPU.GetPattern(0, 0);
                                ppuViewer.glViewPatternTable1.FrameBuffer = Bus.PPU.GetPattern(1, 0);
                                //if (!ppuViewer.Visible)
                                //{
                                //    threads.form.debug = false;
                                //}
                                bool mirrorType = Bus.Cartridge.Mirror() == Mapper.MIRROR.VERTICAL;
                                ppuViewer.UpdateMirroringType(mirrorType);
                                TPixmap fourScreens;
                                if (mirrorType)
                                {
                                    var twoScreens = Bus.PPU.GetNameTable(0).HorzCat(Bus.PPU.GetNameTable(1));
                                    fourScreens = twoScreens.VertCat(twoScreens);
                                }
                                else
                                {
                                    var twoScreens = Bus.PPU.GetNameTable(0).VertCat(Bus.PPU.GetNameTable(1));
                                    fourScreens = twoScreens.HorzCat(twoScreens);
                                }
                                ppuViewer.glViewNametable.FrameBuffer = fourScreens;
                                var palettes = new TPixmap[8];
                                for (var i = 0; i < 8; i++)
                                {
                                    var pal = new TPixmap(4, 1);
                                    for (var col = 0; col < 4; col++)
                                        pal.Pixels[col] = Bus.PPU.GetColourFromPaletteRam(i, col);
                                    palettes[i] = pal;
                                }
                                ppuViewer.glViewPaletteTile0.FrameBuffer = palettes[0];
                                ppuViewer.glViewPaletteTile1.FrameBuffer = palettes[1];
                                ppuViewer.glViewPaletteTile2.FrameBuffer = palettes[2];
                                ppuViewer.glViewPaletteTile3.FrameBuffer = palettes[3];
                                ppuViewer.glViewPaletteSprite0.FrameBuffer = palettes[4];
                                ppuViewer.glViewPaletteSprite1.FrameBuffer = palettes[5];
                                ppuViewer.glViewPaletteSprite2.FrameBuffer = palettes[6];
                                ppuViewer.glViewPaletteSprite3.FrameBuffer = palettes[7];
                            }
                            Bus.PPU.DebugAtScanline = false;
                        }

                    } while (!Bus.PPU.FrameComplete);

                    Bus.PPU.FrameComplete = false;


                    //var frame = Bus.PPU.GetScreen();
                    //threads.form.UpdateView(ref frame);
                    threads.form.GLView.FrameBuffer = Bus.PPU.Screen;
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

        //static void FlipVerticallyOptimized(ref UInt32[] frame, ref UInt32[] newFrame, int width, int height)
        //{
        //    unsafe
        //    {
        //        fixed (UInt32* pFrame = frame)
        //        fixed (UInt32* pNewFrame = newFrame)
        //        {
        //            UInt32* pSrc = pFrame + (height - 1) * width;
        //            UInt32* pDst = pNewFrame;

        //            for (int y = 0; y < height; y++)
        //            {
        //                for (int x = 0; x < width; x++)
        //                {
        //                    *pDst++ = *pSrc++;
        //                }
        //                pSrc -= width * 2;
        //            }
        //        }
        //    }
        //}
    }
}
