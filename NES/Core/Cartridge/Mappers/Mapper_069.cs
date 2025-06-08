using System;
using System.Collections.Generic;

namespace NEStor.Core.Cartridge.Mappers
{
    internal class Mapper_069 : Mapper
    {
        //unique_ptr<Sunsoft5bAudio> _audio;
        int _command;
        byte _workRamValue;
        List<byte[]> WRAMBanks;
        bool _irqEnabled;
        bool _irqCounterEnabled;
        int _irqCounter;

        public override void Reset()
        {
            //_audio.reset(new Sunsoft5bAudio(_console));
            Bus.PrgMemory.BankSize = 0x2000;
            Bus.ChrMemory.BankSize = 0x400;
            Bus.PrgMemory.Swap(3, -1);
            WRAMBanks = new List<byte[]>();
            for (int i = 0; i < 4; i++)
                WRAMBanks.Add(new byte[0x2000]);
            UpdateWorkRam();
        }

        //void Serialize(Serializer& s) override
        //{
        //    BaseMapper.Serialize(s);
        //    SV(_audio);
        //    SV(_command);
        //    SV(_workRamValue);
        //    SV(_irqEnabled);
        //    SV(_irqCounterEnabled);
        //    SV(_irqCounter);
        //    if (!s.IsSaving())
        //    {
        //        UpdateWorkRam();
        //    }
        //}

        //void ProcessCpuClock() override
        //{
        //    BaseProcessCpuClock();

        //    if (_irqCounterEnabled)
        //    {
        //        _irqCounter--;
        //        if (_irqCounter == 0xFFFF)
        //        {
        //            if (_irqEnabled)
        //            {
        //                _console->GetCpu()->SetIrqSource(IRQSource.External);
        //            }
        //        }
        //    }

        //    _audio->Clock();
        //}

        void UpdateWorkRam()
        {
            if ((_workRamValue & 0x40) != 0)
                Bus.WRam = WRAMBanks[_workRamValue & 0x3F];
            else
                Bus.WRam = Bus.PrgMemory.SwapBanks[_workRamValue % Bus.PrgMemory.SwapBanks.Count];
        }

        public override void Poke(int addr, byte value)
        {
            switch (addr & 0xE000)
            {
                case 0x8000:
                    _command = value & 0x0F;
                    break;
                case 0xA000:
                    switch (_command)
                    {
                        case 0:
                        case 1:
                        case 2:
                        case 3:
                        case 4:
                        case 5:
                        case 6:
                        case 7:
                            Bus.ChrMemory.Swap(_command, value);
                            break;

                        case 8:
                            {
                                _workRamValue = value;
                                UpdateWorkRam();
                                break;
                            }

                        case 9:
                        case 0xA:
                        case 0xB:
                            Bus.PrgMemory.Swap(_command - 9, value & 0x3F);
                            break;

                        case 0xC:
                            switch (value & 0x03)
                            {
                                case 0: Mirroring = MirrorType.Vertical; break;
                                case 1: Mirroring = MirrorType.Horizontal; break;
                                case 2: Mirroring = MirrorType.OneScreenLo; break;
                                case 3: Mirroring = MirrorType.OneScreenHi; break;
                            }
                            break;

                        case 0xD:
                            _irqEnabled = (value & 0x01) == 0x01;
                            _irqCounterEnabled = (value & 0x80) == 0x80;
                            Bus.Cpu.MapperIrq.Acknowledge();
                            if (_irqEnabled)
                                Bus.Cpu.MapperIrq.Start(_irqCounter - 1);
                            break;

                        case 0xE:
                            _irqCounter = _irqCounter & 0xFF00 | value;
                            break;

                        case 0xF:
                            _irqCounter = _irqCounter & 0xFF | value << 8;
                            break;
                    }
                    break;

                case 0xC000:
                case 0xE000:
                    //_audio->WriteRegister(addr, value);
                    break;
            }
        }

    }
}
