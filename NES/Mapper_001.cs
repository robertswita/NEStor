using System;
using System.Collections.Generic;
using System.Text;

namespace NES
{
    //CPU $6000-$7FFF: 8 KB PRG RAM bank, (optional)
    //CPU $8000-$BFFF: 16 KB PRG ROM bank, either switchable or fixed to the first bank
    //CPU $C000 -$FFFF: 16 KB PRG ROM bank, either fixed to the last bank or switchable
    //PPU $0000 -$0FFF: 4 KB switchable CHR bank
    //PPU $1000 -$1FFF: 4 KB switchable CHR bank

    class Mapper_001 : Mapper_000
    {
        static int CTRL_MIRRORING = 0x03;
        //static int CTRL_PRG_SWAP_LOW = 0x04;
        //static int CTRL_PRG_SWAP_16K = 0x08;
        //static int CTRL_CHR_SWAP_4K = 0x10;
        //static int CTRL_WRITE_RESET = CTRL_PRG_SWAP_LOW | CTRL_PRG_SWAP_16K;
        //static int CTRL_HARD_RESET = CTRL_WRITE_RESET | CTRL_MIRRORING;
        //static int PRG0_WRAM_DISABLED = 0x10;

		int LoadRegister;
		int LoadRegisterCount;
        int ControlRegister;
        int ChrBankMode;
        int PrgBankMode;
        bool SkipNext;
        int ChrBank0;
        int ChrBank1;
        int PrgBank;

        public override void Reset()
        {
            base.Reset();
			LoadRegister = 0;
			LoadRegisterCount = 0;
            UpdateControl(0xC);
        }

        public override void CpuMapWrite(int addr, byte data)
        {
            if ((data & 0x80) != 0)
            {
                // MSB is set, so reset serial loading
                LoadRegister = 0;
                LoadRegisterCount = 0;
                ControlRegister |= 0x0C;
                SkipNext = Bus.cpu.DummyAccess;
            }
            else
            {
                if (SkipNext)
                {
                    SkipNext = false;
                    return;
                }
                // Load data in serially into load register
                // It arrives LSB first, so implant this at
                // bit 5. After 5 writes, the register is ready
                LoadRegister |= (data & 0x1) << LoadRegisterCount;
                LoadRegisterCount++;
                if (LoadRegisterCount == 5)
                {
                    var targetRegister = (addr >> 13) & 0x03;
                    switch (targetRegister)
                    {
                        case 0: UpdateControl(LoadRegister); break;
                        case 1: UpdateChr0(LoadRegister); break;
                        case 2: UpdateChr1(LoadRegister); break;
                        case 3: UpdatePrg(LoadRegister); break;
                    }
                    LoadRegister = 0x00;
                    LoadRegisterCount = 0;
                }
            }
        }

        void UpdateControl(int register)
        {
            ControlRegister = register;
            switch (ControlRegister & CTRL_MIRRORING)
            {
                case 0: Mirroring = MirrorType.OneScreenLo; break;
                case 1: Mirroring = MirrorType.OneScreenHi; break;
                case 2: Mirroring = MirrorType.Vertical; break;
                case 3: Mirroring = MirrorType.Horizontal; break;
            }
            ChrBankMode = (ControlRegister >> 4) & 1;
            PrgBankMode = (ControlRegister >> 2) & 3;
            UpdateChr0(ChrBank0);
            UpdateChr1(ChrBank1);
            UpdatePrg(PrgBank);
        }

        void UpdateChr0(int register)
        {
            ChrBank0 = register;
            if (ChrBankMode != 0) // 4K CHR Bank
                ChrMap[0] = ChrBank0;
            else // 8K CHR Bank
            {
                ChrMap[0] = ChrBank0 & ~0x1;
                ChrMap[1] = ChrBank0 | 0x1;
            }
            HandleSURomVariant(register);
        }

        void UpdateChr1(int register)
        {
            ChrBank1 = register;
            if (ChrBankMode != 0)
            {
                ChrMap[1] = register;
                //HandleSURomVariant(register);
            }
        }
        bool IsOuterBank;
        private void HandleSURomVariant(int register)
        {
            var isOuterBank = false;
            if (PrgMap.Banks.Length > 16 && (register & 0x10) != 0)
                isOuterBank = true;
            if (IsOuterBank != isOuterBank)
            {
                IsOuterBank = isOuterBank;
                UpdatePrg(PrgBank);
            }
        }

        void UpdatePrg(int register)
        {
            PrgBank = register;
            var bank0 = register;
            var bank1 = (PrgMap.Banks.Length - 1) & 0xF;
            switch (PrgBankMode)
            {
                case 0:
                case 1:
                    bank0 = register & ~0x1;
                    bank1 = register | 0x1;
                    break;
                case 2:
                    bank0 = 0;
                    bank1 = register;
                    break;
            }
            if (IsOuterBank)
            {
                bank0 += 16;
                bank1 += 16;
            }
            PrgMap[0] = bank0;
            PrgMap[1] = bank1;
        }

    }
}
