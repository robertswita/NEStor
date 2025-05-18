using System;

namespace NES
{
    class Mapper_001 : Mapper_000
    {
        int LoadRegister;
        int LoadRegisterCount;
        int ChrBankMode;
        int PrgBankMode;
        int ChrBank0;
        int ChrBank1;
        int PrgBank;

        public int Cycle;
        public override void Poke(int addr, byte data)
        {
            if (addr < 0x8000) return;
            if ((data & 0x80) != 0) // MSB is set, so reset serial loading
            {
                PrgBankMode = 3;
                UpdateState();
            }
            else if (Bus.Cpu.Cycle - Cycle != 1)
            {
                // Load data serially into load register. It arrives LSB first.
                LoadRegister |= (data & 0x1) << LoadRegisterCount;
                LoadRegisterCount++;
                if (LoadRegisterCount == 5) // After 5 writes, the register is ready.
                {
                    var targetRegister = (addr >> 13) & 0x03;
                    switch (targetRegister)
                    {
                        case 0: ControlRegister = LoadRegister; break;
                        case 1: ChrBank0 = LoadRegister; break;
                        case 2: ChrBank1 = LoadRegister; break;
                        case 3: PrgBank = LoadRegister; break;
                    }
                    UpdateState();
                }
            }
            Cycle = Bus.Cpu.Cycle;
        }

        int ControlRegister
        {
            set
            {
                switch (value & 3)
                {
                    case 0: Mirroring = MirrorType.OneScreenLo; break;
                    case 1: Mirroring = MirrorType.OneScreenHi; break;
                    case 2: Mirroring = MirrorType.Vertical; break;
                    case 3: Mirroring = MirrorType.Horizontal; break;
                }
                ChrBankMode = (value >> 4) & 1;
                PrgBankMode = (value >> 2) & 3;
            }
        }

        void UpdateState()
        {
            var bank0 = PrgBank;
            var bank1 = (Bus.PrgMemory.SwapBanks.Count - 1) & 0xF;
            switch (PrgBankMode)
            {
                case 0:
                case 1:
                    bank0 = PrgBank & ~1;
                    bank1 = PrgBank | 1;
                    break;
                case 2:
                    bank0 = 0;
                    bank1 = PrgBank;
                    break;
            }
            if (Bus.PrgMemory.SwapBanks.Count > 16 && (ChrBank0 & 0x10) != 0) // SURom
            {
                bank0 += 16;
                bank1 += 16;
            }
            Bus.PrgMemory.Swap(0, bank0);
            Bus.PrgMemory.Swap(1, bank1);
            Bus.ChrMemory.Swap(0, ChrBank0);
            Bus.ChrMemory.Swap(1, ChrBankMode != 0 ? ChrBank1 : ChrBank0 + 1);
            LoadRegister = 0;
            LoadRegisterCount = 0;
        }

    }
}
