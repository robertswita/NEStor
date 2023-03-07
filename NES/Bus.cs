using System;
using NES.NESGamePak;

namespace NES
{
    class Bus
    {
        /*
        64 * 1024 = total bus size  // range: 65536 * 8 bit = 524288 bit = 64 kB

        soooo apparently Cartridge space starts from $4020 ...

        
        addresses works in such way that 0[00]0 0000 ... determine what chip is used and its also used to Enabling that chip

        CPU BUS (16 bit)
        $0000
         2K RAM
        $07FF
        $8000
         3 mirrors of the RAM above
        $1FFF
        $2000
         PPU
        $2007
        ?
        $4000
         APU (Audio)
         $4016 Controller 1
        $4017 Controller 2
        ?
        $8000
         [Cartridge] Program RAM or ROM Memory (32 kB) ("more" with mappers)
        $FFFF

        PPU BUS (14 bit?)
        $0000
         [Cartridge] Character RAM or ROM Memory (8 kB) ("more" with mappers)?
        $1FFF
        $2000 PPUCTRL
         $2001 PPUMASK
         $2002 PPUSTATUS
         ?
         $2006 PPUDATA
         $2007 PPUADDR
         VRAM (SRAM 2 kB, is it really exactly 2 kB?)
         $3F00
          Palette RAM
         $3F1F
         ?
        $3FFF


        */

        public byte[] Ram = new byte[(UInt16)64 * (UInt16)1024];

        //**** MEMORY *****

        // 8 * 1024 = range: 8192 * 8bit = 65536 bit = 8 kB
        // 8kb / (1 + 3 mirrors) = 2 kB
        public byte[] InternalRAM = new byte[8 * 1024];

        public CPU CPU;
        public bool CPUInSleep;

        public PPU PPU;

        public Cartridge Cartridge;


        private UInt32 nSystemClockCounter = 0;

        public Bus(ref Cartridge cartridge)
        {
            Cartridge = cartridge;
            CPU = new CPU(this);
            PPU = new PPU(this); 
        }

        private readonly byte[] controller_state = new byte[2];
        public byte[] controller = new byte[2];

        public void CpuWrite(int addr, byte data)
        {
            if (Cartridge.CpuWrite(addr, data))
            {

            }   
            else if (addr <= 0x1FFF) // 2K Internal RAM
            {
                InternalRAM[addr & 0x07FF] = data; // _mirror by masking first block range
            }
            else if (addr <= 0x3FFF) // NES PPU registers
            {
                
                // Mirrors of $2000-2007 (repeats every 8 bytes)
                PPU.CpuWrite(addr & 0x0007, data);
            }
            else if (addr == 0x4014)
            {
                // DMA
                DMAPage = data;
                DMAAddr = 0x00;
                DMATransfer = true;
            }
            else if (addr >= 0x4016 && addr <= 0x4017)
            {
                controller_state[addr & 0x0001] = controller[addr & 0x0001];
            }

        }

        public byte CpuRead(int addr)
        {
            byte data = 0x00;

            if (Cartridge.CpuRead(addr, ref data))
            {

            }
            else if (addr <= 0x1FFF) // 2K Internal RAM
            {
                data = InternalRAM[addr & 0x07FF]; // _mirror by masking first block range
            }
            else if (addr <= 0x3FFF) // NES PPU registers
            {
                
                // Mirrors of $2000-2007 (repeats every 8 bytes)
                data = PPU.CpuRead(addr & 0x0007);
            }
            else if (addr >= 0x4016 && addr <= 0x4017)
            {
                data = (controller_state[addr & 0x0001] & 0x80) > 0 ? (byte)0x01 : (byte)0x00;
                controller_state[addr & 0x0001] <<= 1;
            }

            //throw new Exception("Memory address out of bounds! ADDR: " + addr);
            return data;
        }

        public void Reset()
        {
            Cartridge.Reset();
            CPU.Reset();
            PPU.Reset();
            nSystemClockCounter = 0;
            DMAPage = 0x00;
            DMAAddr = 0x00;
            DMAData = 0x00;
            DMADummy = true;
            DMATransfer = false;
        }

        private byte DMAPage = 0x00;
        private byte DMAAddr = 0x00;
        private byte DMAData = 0x00;

        bool DMATransfer = false;
        bool DMADummy = true;

        public int DMATransfer_Benchmark = 0;

        public void Clock()
        {
            PPU.Clock();

            if (nSystemClockCounter % 3 == 0)
            {
                // whenever DMA transfer occures, CPU is suspended until data is transfered
                if (DMATransfer)
                {
                    if (DMADummy)
                    {
                        if (nSystemClockCounter % 2 == 1)
                        {
                            DMADummy = false;
                        }
                    }
                    else
                    {
                        if (nSystemClockCounter % 2 == 0) //even _cycle
                        {
                            DMAData = CpuRead(DMAPage << 8 | DMAAddr);
                        }
                        else
                        {
                            //PPU.CpuWrite(3, DMAAddr);
                            //PPU.CpuWrite(4, DMAData);
                            PPU.OAM[DMAAddr] = DMAData;
                            DMAAddr++;

                            if (DMAAddr == 0x00)
                            {
                                // transfer finished
                                DMATransfer = false;
                                DMADummy = true;
                            }
                        }
                    }
                }
                else if (!CPUInSleep)
                {
                    CPU.Clock();
                }
            }

            //if (PPU.NMI)
            //{
            //    PPU.NMI = false;
            //    CPU.NMI();
            //}
            //if (CPU.DoNMI)
            //{
            //    CPU.DoNMI = false;
            //    CPU.NMI();
            //}

            nSystemClockCounter++;
        }
    }
}
