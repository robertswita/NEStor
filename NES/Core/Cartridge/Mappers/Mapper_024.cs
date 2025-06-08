using NEStor.Core;
using System;
using System.Collections.Generic;

namespace NEStor.Core.Cartridge.Mappers
{
    internal class Mapper_024 : Mapper
    {
        //unique_ptr<VrcIrq> _irq;
        //unique_ptr<Vrc6Audio> _audio;

        //VRCVariant _model;
        float Version;
        byte _bankingMode = 0;
        byte[] _chrRegisters = new byte[8];
        byte IrqReloadValue;

        void UpdatePrgRamAccess()
        {
            //SetCpuMemoryMapping(0x6000, 0x7FFF, 0, HasBattery() ? PrgMemoryType.SaveRam : PrgMemoryType.WorkRam, (_bankingMode & 0x80) ? MemoryAccessType.ReadWrite : MemoryAccessType.NoAccess);
        }

        //	int GetPrgPageSize() override { return 0x2000; }
        //	int GetChrPageSize() override { return 0x0400; }
        //bool EnableCpuClockHook() override
        //{ return true; }

        public override void Reset()
        {
            //_audio.reset(new Vrc6Audio(_console));
            //_irq.reset(new VrcIrq(_console));

            //_irq->Reset();
            //_audio->Reset();
            Bus.PrgMemory.BankSize = 0x2000;
            Bus.ChrMemory.BankSize = 0x400;
            Bus.PrgMemory.Swap(3, -1);
        }

        //void Serialize(Serializer& s) override
        //{
        //    BaseMapper.Serialize(s);
        //    SVArray(_chrRegisters, 8);
        //    SV(_irq);
        //    SV(_audio);
        //    SV(_bankingMode);

        //    if (!s.IsSaving())
        //    {
        //        UpdatePrgRamAccess();
        //        UpdatePpuBanking();
        //    }
        //}

        //void ProcessCpuClock() override
        //{
        //    BaseProcessCpuClock();
        //    _irq->ProcessCpuClock();
        //    _audio->Clock();
        //}

        void SetPpuMapping(byte bank, int page)
        {
            Bus.NameTables.Banks[bank] = Bus.ChrMemory.SwapBanks[page];

            //SetPpuMemoryMapping(0x2000 + bank * 0x400, 0x23FF + bank * 0x400, page);
            //SetPpuMemoryMapping(0x3000 + bank * 0x400, 0x33FF + bank * 0x400, page);
        }

        void UpdatePpuBanking()
        {
            var sizeModeMask = (_bankingMode & 0x20) != 0 ? 1 : 0;
            var mode = _bankingMode & 3;
            for (int i = 0; i < 8; i++)
            {
                int chrBank = _chrRegisters[i];
                if (mode == 1 || mode > 1 && i > 3)
                {
                    chrBank &= 0xFF & ~sizeModeMask;
                    if ((i & 1) != 0) chrBank = _chrRegisters[i - 1] | sizeModeMask;
                }
                Bus.ChrMemory.Swap(i, chrBank);
            }
            if ((_bankingMode & 0x10) != 0) //CHR ROM nametables
            {
                switch (_bankingMode & 0x2F)
                {
                    case 0x20:
                    case 0x27:
                        SetPpuMapping(0, _chrRegisters[6] & 0xFE);
                        SetPpuMapping(1, _chrRegisters[6] & 0xFE | 1);
                        SetPpuMapping(2, _chrRegisters[7] & 0xFE);
                        SetPpuMapping(3, _chrRegisters[7] & 0xFE | 1);
                        break;

                    case 0x23:
                    case 0x24:
                        SetPpuMapping(0, _chrRegisters[6] & 0xFE);
                        SetPpuMapping(1, _chrRegisters[7] & 0xFE);
                        SetPpuMapping(2, _chrRegisters[6] & 0xFE | 1);
                        SetPpuMapping(3, _chrRegisters[7] & 0xFE | 1);
                        break;

                    case 0x28:
                    case 0x2F:
                        SetPpuMapping(0, _chrRegisters[6] & 0xFE);
                        SetPpuMapping(1, _chrRegisters[6] & 0xFE);
                        SetPpuMapping(2, _chrRegisters[7] & 0xFE);
                        SetPpuMapping(3, _chrRegisters[7] & 0xFE);
                        break;

                    case 0x2B:
                    case 0x2C:
                        SetPpuMapping(0, _chrRegisters[6] & 0xFE | 1);
                        SetPpuMapping(1, _chrRegisters[7] & 0xFE | 1);
                        SetPpuMapping(2, _chrRegisters[6] & 0xFE | 1);
                        SetPpuMapping(3, _chrRegisters[7] & 0xFE | 1);
                        break;

                    default:
                        switch (_bankingMode & 0x07)
                        {
                            case 0:
                            case 6:
                            case 7:
                                SetPpuMapping(0, _chrRegisters[6]);
                                SetPpuMapping(1, _chrRegisters[6]);
                                SetPpuMapping(2, _chrRegisters[7]);
                                SetPpuMapping(3, _chrRegisters[7]);
                                break;

                            case 1:
                            case 5:
                                SetPpuMapping(0, _chrRegisters[4]);
                                SetPpuMapping(1, _chrRegisters[5]);
                                SetPpuMapping(2, _chrRegisters[6]);
                                SetPpuMapping(3, _chrRegisters[7]);
                                break;

                            case 2:
                            case 3:
                            case 4:
                                SetPpuMapping(0, _chrRegisters[6]);
                                SetPpuMapping(1, _chrRegisters[7]);
                                SetPpuMapping(2, _chrRegisters[6]);
                                SetPpuMapping(3, _chrRegisters[7]);
                                break;
                        }
                        break;
                }
            }
            else
            {
                //Regular nametables (CIRAM)
                switch (_bankingMode & 0x2F)
                {
                    case 0x20:
                    case 0x27:
                        Mirroring = MirrorType.Vertical;
                        break;
                    case 0x23:
                    case 0x24:
                        Mirroring = MirrorType.Horizontal;
                        break;
                    case 0x28:
                    case 0x2F:
                        Mirroring = MirrorType.OneScreenLo;
                        break;
                    case 0x2B:
                    case 0x2C:
                        Mirroring = MirrorType.OneScreenHi;
                        break;

                    default:
                        switch (_bankingMode & 0x07)
                        {
                            case 0:
                            case 6:
                            case 7:
                                Bus.NameTables.Swap(0, _chrRegisters[6] & 0x01);
                                Bus.NameTables.Swap(1, _chrRegisters[6] & 0x01);
                                Bus.NameTables.Swap(2, _chrRegisters[7] & 0x01);
                                Bus.NameTables.Swap(3, _chrRegisters[7] & 0x01);
                                break;

                            case 1:
                            case 5:
                                Bus.NameTables.Swap(0, _chrRegisters[4] & 0x01);
                                Bus.NameTables.Swap(1, _chrRegisters[5] & 0x01);
                                Bus.NameTables.Swap(2, _chrRegisters[6] & 0x01);
                                Bus.NameTables.Swap(3, _chrRegisters[7] & 0x01);
                                break;

                            case 2:
                            case 3:
                            case 4:
                                Bus.NameTables.Swap(0, _chrRegisters[6] & 0x01);
                                Bus.NameTables.Swap(1, _chrRegisters[7] & 0x01);
                                Bus.NameTables.Swap(2, _chrRegisters[6] & 0x01);
                                Bus.NameTables.Swap(3, _chrRegisters[7] & 0x01);
                                break;
                        }
                        break;
                }
            }
            UpdatePrgRamAccess();
        }

        public override void Poke(int addr, byte value)
        {
            if (Id == 26) // Version b
            {
                addr = addr & 0xFFFC | (addr & 0x01) << 1 | (addr & 0x02) >> 1;
            }

            switch (addr & 0xF003)
            {
                case 0x8000:
                case 0x8001:
                case 0x8002:
                case 0x8003:
                    var prgBank = (value & 0x0F) << 1;
                    Bus.PrgMemory.Swap(0, prgBank);
                    Bus.PrgMemory.Swap(1, prgBank | 1);
                    break;

                case 0x9000:
                case 0x9001:
                case 0x9002:
                case 0x9003:
                case 0xA000:
                case 0xA001:
                case 0xA002:
                case 0xB000:
                case 0xB001:
                case 0xB002:
                    //_audio->WriteRegister(addr, value);
                    break;

                case 0xB003:
                    _bankingMode = value;
                    UpdatePpuBanking();
                    break;

                case 0xC000:
                case 0xC001:
                case 0xC002:
                case 0xC003:
                    Bus.PrgMemory.Swap(2, value & 0x1F);
                    break;

                case 0xD000:
                case 0xD001:
                case 0xD002:
                case 0xD003:
                    _chrRegisters[addr & 0x03] = value;
                    UpdatePpuBanking();
                    break;

                case 0xE000:
                case 0xE001:
                case 0xE002:
                case 0xE003:
                    _chrRegisters[4 + (addr & 0x03)] = value;
                    UpdatePpuBanking();
                    break;

                case 0xF000: IrqReloadValue = value; break;

                case 0xF001:
                    //IrqEnabledAfterAck = (value & 0x01) == 0x01;
                    var IrqEnabled = (value & 0x02) == 0x02;
                    var IrqCycleMode = (value & 0x04) == 0x04;
                    Bus.Cpu.MapperIrq.Acknowledge();
                    if (IrqEnabled)
                    {
                        var IrqCounter = 0xFF - IrqReloadValue;
                        if (!IrqCycleMode)
                            IrqCounter = (int)(IrqCounter * (Bus.ModelParams.PpuMaxX + 2) / Bus.Ppu.ClockRatio);
                        Bus.Cpu.MapperIrq.Start(IrqCounter);
                    }
                    break;

                case 0xF002: Bus.Cpu.MapperIrq.Acknowledge(); break;
            }
        }

    }
}
