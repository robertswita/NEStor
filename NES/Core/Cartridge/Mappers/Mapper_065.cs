using NES;

namespace NEStor.Core.Cartridge.Mappers
{
    class Mapper_065 : Mapper_004
    {
        public override void Poke(int addr, byte data)
        {
            switch (addr)
            {
                case 0x8000: // Bank Select
                    TargetRegister = data;// & 0xF;
                    Bus.PrgMemory.Swap(0, data);
                    break;
                case 0xB000: Bus.ChrMemory.Swap(0, data); break;
                case 0xB001: Bus.ChrMemory.Swap(1, data); break;
                case 0xB002: Bus.ChrMemory.Swap(2, data); break;
                case 0xB003: Bus.ChrMemory.Swap(3, data); break;
                case 0xB004: Bus.ChrMemory.Swap(4, data); break;
                case 0xB005: Bus.ChrMemory.Swap(5, data); break;
                case 0xB006: Bus.ChrMemory.Swap(6, data); break;
                case 0xB007: Bus.ChrMemory.Swap(7, data); break;
                case 0xA000: Bus.PrgMemory.Swap(1, data); break;
                case 0xC000: Bus.PrgMemory.Swap(2, data); break;
                case 0x9001:
                    Mirroring = (data & 0x40) == 0 ? MirrorType.Horizontal : MirrorType.Vertical;
                    break;
                case 0x9003:
                    IrqEnabled = false;
                    Bus.Cpu.MapperIrq.Acknowledge();
                    break;
                case 0x9004:
                    IrqMode = data & 1;
                    IrqCounter = IrqCounterLatch;
                    Bus.Cpu.MapperIrq.Acknowledge();
                    Bus.Cpu.MapperIrq.Start(IrqCounter);
                    break;
                case 0x9005:
                    IrqCounterLatch = IrqCounterLatch & 0x00FF | data << 8;
                    break;
                case 0x9006:
                    IrqCounterLatch = IrqCounterLatch & 0xFF00 | data;
                    IrqEnabled = true;
                    break;
                case 0xA001: // PRG RAM Protect
                             //PrgRAMenabled = (data & 0xC0) == 0x80;
                    break;
            }
        }

        public override void IrqScanline() { }

    }
}
