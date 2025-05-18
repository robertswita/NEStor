using System;

namespace NES
{
    class Mapper_021 : Mapper
    {
        bool IsVrc2;
        int[] PrgRegs = new int[2];
        int PrgMode;
        byte[] HiChrRegs = new byte[8];
        byte[] LoChrRegs = new byte[8];
        byte Latch;
        int IrqReloadValue;
        int IrqCounter;
        bool IrqEnabled;
        bool IrqEnabledAfterAck;
        bool IrqCycleMode;

        public override void Reset()
        {
            Bus.PrgMemory.BankSize = 0x2000;
            Bus.ChrMemory.BankSize = 0x400;
            Bus.PrgMemory.Swap(2, -2);
            Bus.PrgMemory.Swap(3, -1);
            IsVrc2 = Id == 22 | Id == 23 && SubId != 2 | Id == 25 && SubId == 3;
            UpdateState();

            //RemoveRegisterRange(0, 0xFFFF, MemoryOperation.Read);
            //if (!_useHeuristics && _variant <= VRCVariant.VRC2c && _workRamSize == 0 && _saveRamSize == 0)
            //{
            //    AddRegisterRange(0x6000, 0x7FFF, MemoryOperation.Any);
            //}
        }

        void UpdateState()
        {
            for (int i = 0; i < 8; i++)
            {
                int page = LoChrRegs[i] | (HiChrRegs[i] << 4);
                if (Id == 22)
                    page >>= 1;
                Bus.ChrMemory.Swap(i, page);
            }
            Bus.PrgMemory.Swap(1, PrgRegs[1]);
            var target = new int[2];
            target[PrgMode] = 2;
            Bus.PrgMemory.Swap(target[0], -2);
            Bus.PrgMemory.Swap(target[1], PrgRegs[0]);
        }

        public override byte Peek(int addr)
        {
            //Microwire interface ($6000-$6FFF) (VRC2 only)
            return Latch;// | (_console->GetMemoryManager()->GetOpenBus() & 0xFE);
        }

        public override void Poke(int addr, byte data)
        {
            if (addr < 0x8000)
            {
                //Microwire interface ($6000-$6FFF) (VRC2 only)
                Latch = (byte)(data & 0x01);
                return;
            }
            addr = TranslateAddress(addr);
            if (addr >= 0x8000 && addr <= 0x8003)
                PrgRegs[0] = data & 0x1F;
            else if (addr >= 0x9000 && addr <= 0x9003)
            {
                if (IsVrc2)
                    Mirroring = (data & 1) == 0 ? MirrorType.Vertical : MirrorType.Horizontal;
                else
                {
                    if (addr <= 0x9001)
                        switch (data)
                        {
                            case 0: Mirroring = MirrorType.Vertical; break;
                            case 1: Mirroring = MirrorType.Horizontal; break;
                            case 2: Mirroring = MirrorType.OneScreenLo; break;
                            case 3: Mirroring = MirrorType.OneScreenHi; break;
                        }
                    else
                        PrgMode = (data >> 1) & 1;
                }
            }
            else if (addr >= 0xA000 && addr <= 0xA003)
                PrgRegs[1] = data & 0x1F;
            else if (addr >= 0xB000 && addr <= 0xE003)
            {
                int regNumber = ((addr >> 12 & 7) - 3) << 1 | (addr >> 1) & 1;
                if ((addr & 1) == 0)
                    LoChrRegs[regNumber] = (byte)(data & 0x0F);
                else
                    HiChrRegs[regNumber] = (byte)(data & 0x1F);
            }
            else if (addr == 0xF000)
                IrqReloadValue = (IrqReloadValue & 0xF0) | (data & 0x0F);
            else if (addr == 0xF001)
                IrqReloadValue = (IrqReloadValue & 0x0F) | ((data & 0x0F) << 4);
            else if (addr == 0xF002)
            {
                IrqEnabledAfterAck = (data & 0x01) == 0x01;
                IrqEnabled = (data & 0x02) == 0x02;
                IrqCycleMode = (data & 0x04) == 0x04;
                Bus.Cpu.MapperIrq.Acknowledge();
                if (IrqEnabled)
                {
                    IrqCounter = (byte)(0xFF - IrqReloadValue);
                    if (!IrqCycleMode)
                        IrqCounter = (int)(IrqCounter * (Bus.ModelParams.PpuMaxX + 2) / Bus.Ppu.ClockRatio);
                    if (!IsVrc2)
                        Bus.Cpu.MapperIrq.Start(IrqCounter);
                }
            }
            else if (addr == 0xF003)
            {
                IrqEnabled = IrqEnabledAfterAck;
                Bus.Cpu.MapperIrq.Acknowledge();
            }
            UpdateState();
        }

        int TranslateAddress(int addr)
        {
            int a0 = 0, a1 = 0;
            switch (Id)
            {
                case 23:
                case 27:
                    a0 = (addr | addr >> 2) & 0x01;
                    a1 = (addr >> 1 | addr >> 3) & 0x01;
                    break;

                case 22:
                case 25:
                    a0 = (addr >> 1 | addr >> 3) & 0x01;
                    a1 = (addr | addr >> 2) & 0x01;
                    break;

                case 21:
                    a0 = (addr >> 1 | addr >> 6) & 0x01;
                    a1 = (addr >> 2 | addr >> 7) & 0x01;
                    break;
            }
            return addr & 0xF000 | a1 << 1 | a0;
        }

    }
}
