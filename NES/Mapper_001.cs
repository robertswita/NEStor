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

        public override void Reset()
        {
            base.Reset();
            ControlRegister = 0x1C;
			LoadRegister = 0;
			LoadRegisterCount = 0;
            //Bus.WRAM = new byte[0x2000];
        }

        public override void CpuMapWrite(int addr, byte data)
        {
            if ((data & 0x80) != 0)
            {
                // MSB is set, so Reset serial loading
                LoadRegister = 0x00;
                LoadRegisterCount = 0;
                ControlRegister |= 0x0C;
            }
            else
            {
                // Load data in serially into load register
                // It arrives LSB first, so implant this at
                // bit 5. After 5 writes, the register is ready
                LoadRegister >>= 1;
                LoadRegister |= (data & 0x1) << 4;
                LoadRegisterCount++;
                if (LoadRegisterCount == 5)
                {
                    var targetRegister = (addr >> 13) & 0x03;
                    switch (targetRegister)
                    {
                        case 0: UpdateNmt(LoadRegister); break;
                        case 1: UpdateChr0(LoadRegister); break;
                        case 2: UpdateChr1(LoadRegister); break;
                        case 3: UpdatePrg(LoadRegister); break;
                    }
                    LoadRegister = 0x00;
                    LoadRegisterCount = 0;
                }
            }
        }

        void UpdateNmt(int register)
        {
            ControlRegister = register;
            switch (ControlRegister & CTRL_MIRRORING)
            {
                case 0: Mirroring = VRAMmirroring.OneScreenLo; break;
                case 1: Mirroring = VRAMmirroring.OneScreenHi; break;
                case 2: Mirroring = VRAMmirroring.Vertical; break;
                case 3: Mirroring = VRAMmirroring.Horizontal; break;
            }
        }

        void UpdateChr0(int register)
        {
            int mode = ControlRegister >> 4 & 0x1;
            if (mode == 0) // 8K CHR Bank at PPU 0x0000
            {
                ChrMap.TransferBank(register & ~0x1, 0);
                ChrMap.TransferBank(register | 0x1, 1);
                //Bus.PatternTable[0] = CHRBanks[register & ~0x1];
                //Bus.PatternTable[1] = CHRBanks[register | 0x1];
            }
            else // 4K CHR Bank at PPU 0x0000
                //Bus.PatternTable[0] = CHRBanks[register];
                ChrMap.TransferBank(register, 0);
        }

        void UpdateChr1(int register)
        {
            int mode = ControlRegister >> 4 & 0x1;
            if (mode != 0)
                //Bus.PatternTable[1] = CHRBanks[register];
                ChrMap.TransferBank(register, 1);
        }

        void UpdatePrg(int register)
        {
            var mode = ControlRegister >> 2 & 0x3;
            switch (mode)
            {
                case 0:
                case 1:
                    //Bus.PRGMemory[0] = PRGBanks[register & ~0x1];
                    //Bus.PRGMemory[1] = PRGBanks[register | 0x1];
                    PrgMap.TransferBank(register & ~0x1, 0);
                    PrgMap.TransferBank(register | 0x1, 1);
                    break;
                case 2:
                    //Bus.PRGMemory[0] = PRGBanks[0];
                    //Bus.PRGMemory[1] = PRGBanks[register];
                    PrgMap.TransferBank(0, 0);
                    PrgMap.TransferBank(register, 1);
                    break;
                case 3:
                    //Bus.PRGMemory[0] = PRGBanks[register];
                    //Bus.PRGMemory[1] = PRGBanks[PRGBanks.Length - 1];
                    PrgMap.TransferBank(register, 0);
                    PrgMap.TransferBank(PrgMap.Banks.Length - 1, 1);
                    break;
            }
        }

    }
}
