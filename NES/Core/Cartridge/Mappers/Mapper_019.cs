using NES;
using System;
using System.Collections.Generic;

namespace NEStor.Core.Cartridge.Mappers
{
    class Mapper_019 : Mapper
    {
        enum NamcoVariant
        {
            Namco163,
            Namco175,
            Namco340,
            Unknown,
        };

        //unique_ptr<Namco163Audio> _audio;
        static int AudioRamSize = 0x80;
        byte[] AudioRam = new byte[AudioRamSize];
        int _ramPosition;
        bool _autoIncrement;
        bool _disableSound;
        NamcoVariant _variant;
        bool _notNamco340;
        bool _autoDetectVariant;
        byte _writeProtect;
        bool _lowChrNtMode;
        bool _highChrNtMode;
        int _irqCounter;
        //public byte[][] CRAM = new byte[32][];
        public int[] CRAM_PAGE = new int[8];
        bool IrqEnabled;

        void SetVariant(NamcoVariant variant)
        {
            if (_autoDetectVariant)
            {
                if (!_notNamco340 || variant != NamcoVariant.Namco340)
                {
                    _variant = variant;
                }
            }
        }

        void UpdateSaveRamAccess()
        {
            //PrgMemoryType memType = HasBattery() ? PrgMemoryType.SaveRam : PrgMemoryType.WorkRam;
            //if (_variant == NamcoVariant.Namco163)
            //{
            //    bool globalWriteEnable = (_writeProtect & 0x40) == 0x40;
            //    SetCpuMemoryMapping(0x6000, 0x67FF, 0, memType, globalWriteEnable && (_writeProtect & 0x01) == 0x00 ? MemoryAccessType.ReadWrite : MemoryAccessType.Read);
            //    SetCpuMemoryMapping(0x6800, 0x6FFF, 1, memType, globalWriteEnable && (_writeProtect & 0x02) == 0x00 ? MemoryAccessType.ReadWrite : MemoryAccessType.Read);
            //    SetCpuMemoryMapping(0x7000, 0x77FF, 2, memType, globalWriteEnable && (_writeProtect & 0x04) == 0x00 ? MemoryAccessType.ReadWrite : MemoryAccessType.Read);
            //    SetCpuMemoryMapping(0x7800, 0x7FFF, 3, memType, globalWriteEnable && (_writeProtect & 0x08) == 0x00 ? MemoryAccessType.ReadWrite : MemoryAccessType.Read);
            //}
            //else if (_variant == NamcoVariant.Namco175)
            //{
            //    SetCpuMemoryMapping(0x6000, 0x7FFF, 0, memType, (_writeProtect & 0x01) == 0x01 ? MemoryAccessType.ReadWrite : MemoryAccessType.Read);
            //}
            //else
            //{
            //    SetCpuMemoryMapping(0x6000, 0x7FFF, 0, memType, MemoryAccessType.NoAccess);
            //}
        }

        public override void Reset()
        {
            //_audio.reset(new Namco163Audio(_console));
            Bus.PrgMemory.BankSize = 0x2000;
            Bus.ChrMemory.BankSize = 0x400;
            //for (int i = 0; i < 8; i++)
            //    Bus.ChrMemory.SwapBanks.Add(new byte[0x400]);
            PrgRamEnabled = false;
            ChrRamEnabled = true;
            //for (int i = 0; i < CRAM.Length; i++)
            //{
            //    CRAM[i] = new byte[0x400];
            //    //if (i < Bus.ChrMemory.SwapBanks.Count)
            //    //    Array.Copy(Bus.ChrMemory.SwapBanks[i], CRAM[i], 0x400);
            //}
            switch (Id)
            {
                case 19:
                    _variant = NamcoVariant.Namco163;
                    //if (_romInfo.DatabaseInfo.Board == "NAMCOT-163")
                    //{
                    //    _variant = NamcoVariant.Namco163;
                    //    _autoDetectVariant = false;
                    //}
                    //else if (_romInfo.DatabaseInfo.Board == "NAMCOT-175")
                    //{
                    //    _variant = NamcoVariant.Namco175;
                    //    _autoDetectVariant = false;
                    //}
                    //else if (_romInfo.DatabaseInfo.Board == "NAMCOT-340")
                    //{
                    //    _variant = NamcoVariant.Namco340;
                    //    _autoDetectVariant = false;
                    //}
                    //else
                    //{
                    //    _autoDetectVariant = true;
                    //}
                    break;
                case 210:
                    switch (SubId)
                    {
                        case 0: _variant = NamcoVariant.Unknown; _autoDetectVariant = true; break;
                        case 1: _variant = NamcoVariant.Namco175; _autoDetectVariant = false; break;
                        case 2: _variant = NamcoVariant.Namco340; _autoDetectVariant = false; break;
                    }
                    break;
            }

            _notNamco340 = false;

            _writeProtect = 0;
            _lowChrNtMode = false;
            _highChrNtMode = false;
            _irqCounter = 0;

            //AddRegisterRange(0x4800, 0x5FFF, MemoryOperation.Any);
            //RemoveRegisterRange(0x6000, 0xFFFF, MemoryOperation.Read);

            //Bus.PrgMemory.Swap(2, -2);
            Bus.PrgMemory.Swap(3, -1);
            UpdateSaveRamAccess();

            //if (HasBattery())
            //{
            //    vector<byte> batteryContent(_saveRamSize +Namco163Audio.AudioRamSize, 0);
            //    _emu->GetBatteryManager()->LoadBattery(".sav", batteryContent.data(), (uint32_t)batteryContent.size());

            //    memcpy(_saveRam, batteryContent.data(), _saveRamSize);
            //    memcpy(_audio->GetInternalRam(), batteryContent.data() + _saveRamSize, Namco163Audio.AudioRamSize);
            //}
        }

        //void Serialize(Serializer& s) override
        //{
        //    BaseMapper.Serialize(s);

        //    SV(_audio);
        //    SV(_variant);
        //    SV(_notNamco340);
        //    SV(_autoDetectVariant);
        //    SV(_writeProtect);
        //    SV(_lowChrNtMode);
        //    SV(_highChrNtMode);
        //    SV(_irqCounter);

        //    if (!s.IsSaving())
        //    {
        //        UpdateSaveRamAccess();
        //    }
        //}

        //void SaveBattery() override
        //{
        //    if (HasBattery())
        //    {
        //        vector<byte> batteryContent(_saveRamSize +Namco163Audio.AudioRamSize, 0);
        //        memcpy(batteryContent.data(), _saveRam, _saveRamSize);
        //        memcpy(batteryContent.data() + _saveRamSize, _audio->GetInternalRam(), Namco163Audio.AudioRamSize);

        //        _emu->GetBatteryManager()->SaveBattery(".sav", batteryContent.data(), (uint32_t)batteryContent.size());
        //    }
        //}

        //void ProcessCpuClock() override
        //{
        //    BaseProcessCpuClock();

        //    if (_irqCounter & 0x8000 && (_irqCounter & 0x7FFF) != 0x7FFF)
        //    {
        //        _irqCounter++;
        //        if ((_irqCounter & 0x7FFF) == 0x7FFF)
        //        {
        //            _console->GetCpu()->SetIrqSource(IRQSource.External);
        //        }
        //    }

        //    if (_variant == NamcoVariant.Namco163)
        //    {
        //        _audio->Clock();
        //    }
        //}

        //void WriteRam(int addr, byte value) override
        //{
        //    if (addr >= 0x6000 && addr <= 0x7FFF)
        //    {
        //        _notNamco340 = true;
        //        if (_variant == NamcoVariant.Namco340)
        //        {
        //            SetVariant(NamcoVariant.Unknown);
        //        }
        //    }
        //    BaseMapper.WriteRam(addr, value);
        //}

        public override byte Peek(int addr)
        {
            switch (addr & 0xF800)
            {
                case 0x4800:
                    var value = AudioRam[_ramPosition];
                    if (_autoIncrement)
                        _ramPosition = _ramPosition + 1 & 0x7F;
                    return value;
                case 0x5000:
                    {
                        var counter = Bus.Cpu.MapperIrq.Delay + Bus.Cpu.MapperIrq.CyclesPassed;
                        return (byte)(counter & 0xFF);
                    }
                case 0x5800:
                    {
                        var counter = Bus.Cpu.MapperIrq.Delay + Bus.Cpu.MapperIrq.CyclesPassed;
                        return (byte)(counter >> 8 & 0x7F);
                    }
                default: return base.Peek(addr);
            }
        }

        public override void Poke(int addr, byte value)
        {
            addr &= 0xF800;

            switch (addr)
            {
                case 0x4800:
                    SetVariant(NamcoVariant.Namco163);
                    //_audio->WriteRegister(addr, value);
                    AudioRam[_ramPosition] = value;
                    if (_autoIncrement)
                    {
                        _ramPosition = _ramPosition + 1 & 0x7F;
                    }
                    break;

                case 0x5000:
                    SetVariant(NamcoVariant.Namco163);
                    _irqCounter = _irqCounter & 0x7F00 | value;
                    Bus.Cpu.MapperIrq.Acknowledge();
                    if (IrqEnabled)
                        Bus.Cpu.MapperIrq.Start(0x7FFF - _irqCounter);
                    break;

                case 0x5800:
                    SetVariant(NamcoVariant.Namco163);
                    _irqCounter = _irqCounter & 0x00FF | (value & 0x7F) << 8;
                    IrqEnabled = (value & 0x80) != 0;
                    Bus.Cpu.MapperIrq.Acknowledge();
                    //if ((_irqCounter & 0x8000) != 0 && (_irqCounter & 0x7FFF) != 0x7FFF)
                    if (IrqEnabled)
                        Bus.Cpu.MapperIrq.Start(0x7FFF - _irqCounter);
                    break;

                case 0x8000:
                case 0x8800:
                case 0x9000:
                case 0x9800:
                    {
                        int bankNumber = addr - 0x8000 >> 11;
                        if (!_lowChrNtMode && value >= 0xE0 && _variant == NamcoVariant.Namco163)
                        {
                            //Bus.NameTables.Banks[bankNumber] = Bus.ChrMemory.SwapBanks[value & 1];
                            //Bus.ChrMemory.Swap(bankNumber, value & 0x01);//, ChrMemoryType.NametableRam);
                            Bus.ChrMemory.Banks[bankNumber] = Bus.NameTables.SwapBanks[value & 1];
                            //Bus.ChrMemory.Banks[bankNumber] = CRAM[value & 0x1F];
                        }
                        else
                        {
                            Bus.ChrMemory.Swap(bankNumber, value);
                        }
                        break;
                    }

                case 0xA000:
                case 0xA800:
                case 0xB000:
                case 0xB800:
                    {
                        int bankNumber = (addr - 0xA000 >> 11) + 4;
                        if (!_highChrNtMode && value >= 0xE0 && _variant == NamcoVariant.Namco163)
                        {
                            //Bus.ChrMemory.Swap(bankNumber, value & 0x01);//, ChrMemoryType.NametableRam);
                            //Bus.ChrMemory.Banks[bankNumber] = CRAM[value & 0x1F];
                            Bus.ChrMemory.Banks[bankNumber] = Bus.NameTables.SwapBanks[value & 1];
                        }
                        else
                        {
                            Bus.ChrMemory.Swap(bankNumber, value);
                        }
                        break;
                    }

                case 0xC000:
                case 0xC800:
                case 0xD000:
                case 0xD800:
                    if (addr >= 0xC800)
                    {
                        SetVariant(NamcoVariant.Namco163);
                    }
                    else if (_variant != NamcoVariant.Namco163)
                    {
                        SetVariant(NamcoVariant.Namco175);
                    }

                    if (_variant == NamcoVariant.Namco175)
                    {
                        _writeProtect = value;
                        UpdateSaveRamAccess();
                    }
                    else
                    {
                        int bankNumber = addr - 0xC000 >> 11; // + 8;
                        if (value >= 0xE0)
                        {
                            Bus.NameTables.Swap(bankNumber, value & 0x01);
                            //Bus.NameTables.Banks[bankNumber] = CRAM[value & 0x1];
                        }
                        else
                        {
                            //Bus.ChrMemory.Swap(bankNumber, value);
                            Bus.NameTables.Banks[bankNumber] = (byte[])Bus.ChrMemory.SwapBanks[value].Clone();
                        }
                    }
                    break;

                case 0xE000:
                    if ((value & 0x80) == 0x80)
                    {
                        SetVariant(NamcoVariant.Namco340);
                    }
                    else if ((value & 0x40) == 0x40 && _variant != NamcoVariant.Namco163)
                    {
                        SetVariant(NamcoVariant.Namco340);
                    }

                    Bus.PrgMemory.Swap(0, value & 0x3F);

                    if (_variant == NamcoVariant.Namco340)
                    {
                        switch ((value & 0xC0) >> 6)
                        {
                            case 0: Mirroring = MirrorType.OneScreenLo; break;
                            case 1: Mirroring = MirrorType.Vertical; break;
                            case 2: Mirroring = MirrorType.Horizontal; break;
                            case 3: Mirroring = MirrorType.OneScreenHi; break;
                        }
                    }
                    else if (_variant == NamcoVariant.Namco163)
                    {
                        //_audio->WriteRegister(addr, value);
                        _disableSound = (value & 0x40) == 0x40;
                    }
                    break;

                case 0xE800:
                    Bus.PrgMemory.Swap(1, value & 0x3F);
                    if (_variant == NamcoVariant.Namco163)
                    {
                        _lowChrNtMode = (value & 0x40) == 0x40;
                        _highChrNtMode = (value & 0x80) == 0x80;
                    }
                    break;

                case 0xF000:
                    Bus.PrgMemory.Swap(2, value & 0x3F);
                    break;

                case 0xF800:
                    SetVariant(NamcoVariant.Namco163);
                    if (_variant == NamcoVariant.Namco163)
                    {
                        _writeProtect = value;
                        UpdateSaveRamAccess();

                        //_audio->WriteRegister(addr, value);
                        _ramPosition = value & 0x7F;
                        _autoIncrement = (value & 0x80) == 0x80;
                    }
                    break;
            }
            if (addr >= 0x6000 && addr < 0x8000)
            {
                Bus.WRam[addr & 0x1FFF] = value;
            }

        }
    }
}
