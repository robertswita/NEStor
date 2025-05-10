using NES;
using System;

namespace NES
{
    class Mapper_048 : Mapper_004
    {
        public override void Reset()
        {
            Bus.PrgMemory.BankSize = 0x2000;
            Bus.ChrMemory.BankSize = 0x400;
            Bus.PrgMemory.Swap(2, -2);
            Bus.PrgMemory.Swap(3, -1);
        }

        public override void Poke(int addr, byte value)
        {
            switch (addr & 0xE003)
            {
                case 0x8000:
                    Bus.PrgMemory.Swap(0, value & 0x3F);
                    Mirroring = (value & 0x40) != 0 ? MirrorType.Horizontal : MirrorType.Vertical;
                    break;
                case 0x8001:
                    Bus.PrgMemory.Swap(1, value & 0x3F);
                    break;
                case 0x8002:
                    Bus.ChrMemory.Swap(0, value * 2);
                    Bus.ChrMemory.Swap(1, value * 2 + 1);
                    break;
                case 0x8003:
                    Bus.ChrMemory.Swap(2, value * 2);
                    Bus.ChrMemory.Swap(3, value * 2 + 1);
                    break;
                case 0xA000:
                case 0xA001:
                case 0xA002:
                case 0xA003:
                    Bus.ChrMemory.Swap(4 + (addr & 0x03), value);
                    break;
                case 0xC000:
                    //Flintstones expects either $C000 or $C001 to clear the irq flag
                    Bus.Cpu.MapperIrq.Acknowledge();

                    IrqCounterLatch = (value ^ 0xFF);// + (_isFlintstones ? 0 : 1);
                    break;
                case 0xC001:
                    //Flintstones expects either $C000 or $C001 to clear the irq flag
                    Bus.Cpu.MapperIrq.Acknowledge();

                    IrqCounter = 0;
                    Reload = true;
                    break;
                case 0xC002:
                    IrqEnabled = true;
                    break;
                case 0xC003:
                    IrqEnabled = false;
                    Bus.Cpu.MapperIrq.Acknowledge();
                    break;

                case 0xE000:
                    Mirroring = (value & 0x40) != 0 ? MirrorType.Horizontal : MirrorType.Vertical;
                    break;
            }
        }
    }
}
