using System;
using emulatorTest.NESGamePak;

namespace emulatorTest
{
    class Bus
    {
        public Byte[] Ram = new byte[(UInt16)64 * (UInt16)1024];

        public Byte[] InternalRAM = new byte[(UInt16)8 * (UInt16)1024];

        public CPU6502.CPU CPU;

        public PPU2C02.PPU PPU;

        public Cartridge Cartridge;


        private UInt32 nSystemClockCounter = 0;

        public Bus(ref Cartridge cartridge)
        {
            Cartridge = cartridge;
            CPU = new CPU6502.CPU(this);
            PPU = new PPU2C02.PPU(this); 
        }

        private readonly Byte[] controller_state = new Byte[2];
        public Byte[] controller = new Byte[2];

        public void CpuWrite(ushort addr, Byte data)
        {
            if (Cartridge.CpuWrite(addr, data))
            {

            }   
            else if (addr >= 0x0000 && addr <= 0x1FFF) // 2K Internal RAM
            {
                InternalRAM[addr & 0x07FF] = data; // _mirror by masking first block range
            }
            else if (addr >= 0x2000 && addr <= 0x3FFF) // NES PPU registers
            {
                
                // Mirrors of $2000-2007 (repeats every 8 bytes)
                PPU.CpuWrite((UInt16)(addr & 0x0007), data);
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

        public Byte CpuRead(ushort addr, bool bReadOnly)
        {
            Byte data = 0x00;

            if (Cartridge.CpuRead(addr, ref data))
            {

            }
            else if (addr >= 0x0000 && addr <= 0x1FFF) // 2K Internal RAM
            {
                data = InternalRAM[addr & 0x07FF]; // _mirror by masking first block range
            }
            else if (addr >= 0x2000 && addr <= 0x3FFF) // NES PPU registers
            {
                
                // Mirrors of $2000-2007 (repeats every 8 bytes)
                data = PPU.CpuRead((UInt16)(addr & 0x0007), bReadOnly);
            }
            else if (addr >= 0x4016 && addr <= 0x4017)
            {
                data = (controller_state[addr & 0x0001] & 0x80) > 0 ? (Byte)0x01 : (Byte)0x00;
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

        private Byte DMAPage = 0x00;
        private Byte DMAAddr = 0x00;
        private Byte DMAData = 0x00;

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
                            DMAData = CpuRead((UInt16)(DMAPage << 8 | DMAAddr), false);

                        }
                        else
                        {
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
                else
                {
                    CPU.Clock();
                }
            }

            if (PPU.NMI)
            {
                PPU.NMI = false;
                CPU.NMI();
            }

            nSystemClockCounter++;
        }
    }
}
