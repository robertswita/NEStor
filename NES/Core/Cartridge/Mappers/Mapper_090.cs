using System;

namespace NEStor.Core.Cartridge.Mappers
{
    enum JyIrqSource
    {
        CpuClock = 0,
        PpuA12Rise = 1,
        PpuRead = 2,
        CpuWrite = 3
    };
    internal class Mapper_090 : Mapper
    {
        int[] _prgRegs = new int[4];
        byte[] _chrLowRegs = new byte[8];
        byte[] _chrHighRegs = new byte[8];
        byte[] _chrLatch = new byte[2];

        int _prgMode;
        bool _enablePrgAt6000;

        int _chrMode;
        bool _chrBlockMode;
        int _chrBlock;
        bool _mirrorChr;

        int _mirroringReg;
        bool _advancedNtControl;
        bool _disableNtRam;

        int _ntRamSelectBit;
        byte[] _ntLowRegs = new byte[8];
        byte[] _ntHighRegs = new byte[8];

        bool _irqEnabled;
        JyIrqSource _irqSource;
        int _irqCountDirection;
        bool _irqFunkyMode;
        byte _irqFunkyModeReg;
        bool _irqSmallPrescaler;
        int _irqPrescaler;
        int _irqCounter;
        int _irqXorReg;

        byte _multiplyValue1;
        byte _multiplyValue2;
        byte _regRamValue;

        //int _lastPpuAddr;
        byte[] WRamDefault;

        public override void Reset()
        {
            Bus.PrgMemory.BankSize = 0x2000;
            Bus.ChrMemory.BankSize = 0x400;
            WRamDefault = Bus.WRam;
            _chrLatch[0] = 0;
            _chrLatch[1] = 4;
            UpdateState();
        }

        //void Serialize(Serializer& s) override
        //{
        //    BaseMapper.Serialize(s);

        //    SVArray(_prgRegs, 4);
        //    SVArray(_chrLowRegs, 8);
        //    SVArray(_chrHighRegs, 8);
        //    SVArray(_ntLowRegs, 4);
        //    SVArray(_ntHighRegs, 4);

        //    SV(_chrLatch[0]);
        //    SV(_chrLatch[1]);
        //    SV(_prgMode);
        //    SV(_enablePrgAt6000);
        //    SV(_chrMode);
        //    SV(_chrBlockMode);
        //    SV(_chrBlock);
        //    SV(_mirrorChr);
        //    SV(_mirroringReg);
        //    SV(_advancedNtControl);
        //    SV(_disableNtRam);
        //    SV(_ntRamSelectBit);
        //    SV(_irqEnabled);
        //    SV(_irqSource);
        //    SV(_lastPpuAddr);
        //    SV(_irqCountDirection);
        //    SV(_irqFunkyMode);
        //    SV(_irqFunkyModeReg);
        //    SV(_irqSmallPrescaler);
        //    SV(_irqPrescaler);
        //    SV(_irqCounter);
        //    SV(_irqXorReg);
        //    SV(_multiplyValue1);
        //    SV(_multiplyValue2);
        //    SV(_regRamValue);

        //    if (!s.IsSaving())
        //    {
        //        UpdateState();
        //    }
        //}

        void UpdateState()
        {
            UpdatePrgState();
            UpdateChrState();
            UpdateMirroringState();
        }

        public static int InvertByteBits(int reg)
        {
            var result = 0;
            for (int i = 0; i < 7; i++)
                result |= (reg >> i & 1) << 6 - i;
            return result;
        }

        void UpdatePrgState()
        {
            var prgRegs = (int[])_prgRegs.Clone();
            var sizeMode = 2 - (_prgMode & 3);
            if (sizeMode < 0)
            {
                prgRegs = new int[_prgRegs.Length];
                for (int i = 0; i < _prgRegs.Length; i++)
                    prgRegs[i] = InvertByteBits(_prgRegs[i]);
                sizeMode = 0;
            }
            var prgBank = prgRegs[3];
            if ((_prgMode & 0x04) == 0)
                prgBank = -1 << sizeMode;
            if (_enablePrgAt6000)
                Bus.WRam = Bus.PrgMemory.SwapBanks[(prgRegs[3] + 1 << sizeMode) - 1];
            else
                Bus.WRam = WRamDefault;
            switch (sizeMode)
            {
                case 2:
                    for (int i = 0; i < prgRegs.Length; i++)
                        prgRegs[i] = prgBank + i;
                    break;
                case 1:
                    prgRegs[0] = prgRegs[1] << 1;
                    prgRegs[1] = prgRegs[0] | 1;
                    prgRegs[2] = prgBank;
                    prgRegs[3] = prgBank | 1;
                    break;
                case 0:
                    prgRegs[3] = prgBank;
                    break;
            }
            for (int i = 0; i < prgRegs.Length; i++)
                Bus.PrgMemory.Swap(i, prgRegs[i]);               
        }

        int GetChrReg(int index)
        {
            if (_chrMode >= 2 && _mirrorChr && (index == 2 || index == 3))
                index -= 2;
            if (_chrBlockMode)
            {
                byte mask = 0;
                byte shift = 0;
                switch (_chrMode)
                {
                    default:
                    case 0: mask = 0x1F; shift = 5; break;
                    case 1: mask = 0x3F; shift = 6; break;
                    case 2: mask = 0x7F; shift = 7; break;
                    case 3: mask = 0xFF; shift = 8; break;
                }
                return _chrLowRegs[index] & mask | _chrBlock << shift;
            }
            else
                return _chrLowRegs[index] | _chrHighRegs[index] << 8;
        }

        void UpdateChrState()
        {
            var chrRegs = new int[_chrLowRegs.Length];
            var sizeMode = 3 - _chrMode;
            for (int i = 0; i < chrRegs.Length; i += 1 << sizeMode)
            {
                chrRegs[i] = _chrMode == 1 ? GetChrReg(_chrLatch[i / 4]): GetChrReg(i);
                chrRegs[i] <<= sizeMode;
                for (int j = 0; j < 1 << sizeMode; j++)
                    chrRegs[i + j] = chrRegs[i] + j;
            }
            for (int i = 0; i < chrRegs.Length; i++)
                Bus.ChrMemory.Swap(i, chrRegs[i]);
        }

        void UpdateMirroringState()
        {
            //"Mapper 211 behaves as though N were always set (1), and mapper 090 behaves as though N were always clear(0)."
            if ((_advancedNtControl || Id == 211) && Id != 90)
                for (int i = 0; i < 4; i++)
                    Bus.NameTables.Swap(i, _ntLowRegs[i] & 0x01);
            else
                switch (_mirroringReg)
                {
                    case 0: Mirroring = MirrorType.Vertical; break;
                    case 1: Mirroring = MirrorType.Horizontal; break;
                    case 2: Mirroring = MirrorType.OneScreenLo; break;
                    case 3: Mirroring = MirrorType.OneScreenHi; break;
                }
        }

        public override byte Peek(int addr)
        {
            switch (addr & 0xF803)
            {
                case 0x5000: return 0; //Dip switches
                case 0x5800: return (byte)(_multiplyValue1 * _multiplyValue2);
                case 0x5801: return (byte)(_multiplyValue1 * _multiplyValue2 >> 8);
                case 0x5803: return _regRamValue;
            }
            return base.Peek(addr);
        }

        public override void Poke(int addr, byte value)
        {
            if (addr < 0x8000)
            {
                switch (addr & 0xF803)
                {
                    case 0x5800: _multiplyValue1 = value; break;
                    case 0x5801: _multiplyValue2 = value; break;
                    case 0x5803: _regRamValue = value; break;
                }
            }
            else
            {
                switch (addr & 0xF007)
                {
                    case 0x8000:
                    case 0x8001:
                    case 0x8002:
                    case 0x8003:
                    case 0x8004:
                    case 0x8005:
                    case 0x8006:
                    case 0x8007:
                        _prgRegs[addr & 0x03] = value & 0x7F;
                        break;

                    case 0x9000:
                    case 0x9001:
                    case 0x9002:
                    case 0x9003:
                    case 0x9004:
                    case 0x9005:
                    case 0x9006:
                    case 0x9007:
                        _chrLowRegs[addr & 0x07] = value;
                        break;

                    case 0xA000:
                    case 0xA001:
                    case 0xA002:
                    case 0xA003:
                    case 0xA004:
                    case 0xA005:
                    case 0xA006:
                    case 0xA007:
                        _chrHighRegs[addr & 0x07] = value;
                        break;

                    case 0xB000:
                    case 0xB001:
                    case 0xB002:
                    case 0xB003:
                        _ntLowRegs[addr & 0x03] = value;
                        break;

                    case 0xB004:
                    case 0xB005:
                    case 0xB006:
                    case 0xB007:
                        _ntHighRegs[addr & 0x03] = value;
                        break;

                    case 0xC000:
                        _irqEnabled = (value & 1) != 0;
                        if (!_irqEnabled)
                            Bus.Cpu.MapperIrq.Acknowledge();
                        break;

                    case 0xC001:
                        _irqCountDirection = value >> 6 & 0x03;
                        _irqFunkyMode = (value & 0x08) == 0x08;
                        _irqSmallPrescaler = (value >> 2 & 0x01) == 0x01;
                        _irqSource = (JyIrqSource)(value & 0x03);
                        break;

                    case 0xC002:
                        _irqEnabled = false;
                        Bus.Cpu.MapperIrq.Acknowledge();
                        break;

                    case 0xC003: 
                        _irqEnabled = true;
                        var mask = _irqSmallPrescaler ? 0x07 : 0xFF;
                        var prescaler = _irqPrescaler & mask;
                        //_irqPrescaler = (_irqPrescaler & ~mask) | (prescaler & mask);
                        if (_irqSource == JyIrqSource.CpuClock)
                        {
                            if (_irqCountDirection == 1)
                                Bus.Cpu.MapperIrq.Start((256 - _irqCounter) * prescaler);
                            if (_irqCountDirection == 2)
                                Bus.Cpu.MapperIrq.Start(_irqCounter * prescaler);
                        }
                        break;
                    case 0xC004: _irqPrescaler = value ^ _irqXorReg; break;
                    case 0xC005: _irqCounter = value ^ _irqXorReg; break;
                    case 0xC006: _irqXorReg = value; break;
                    case 0xC007: _irqFunkyModeReg = value; break;

                    case 0xD000:
                        _prgMode = value & 0x07;
                        _chrMode = value >> 3 & 0x03;
                        _advancedNtControl = (value & 0x20) == 0x20;
                        _disableNtRam = (value & 0x40) == 0x40;
                        _enablePrgAt6000 = (value & 0x80) == 0x80;
                        break;

                    case 0xD001: _mirroringReg = value & 0x03; break;
                    case 0xD002: _ntRamSelectBit = value & 0x80; break;

                    case 0xD003:
                        _mirrorChr = (value & 0x80) == 0x80;
                        _chrBlockMode = (value & 0x20) == 0x00;
                        _chrBlock = (value & 0x18) >> 2 | value & 0x01;
                        break;

                }
            }

            UpdateState();
        }

        public override void IrqScanline()
        {
            if (_irqSource == JyIrqSource.PpuA12Rise && Bus.Ppu.Vram.A12Toggled)
            {
                _irqCounter--;
                if (_irqCounter == 0 && _irqEnabled)
                    Bus.Cpu.MapperIrq.Start(0);
            }
        }

        public override void OnSpritelineStart()
        {
            Bus.Ppu.Vram.A12Toggled = true;
            if (Bus.Ppu.Control.PatternBg == 0)
                IrqScanline();
            Bus.Ppu.Vram.A12Toggled = false;
        }

        //public override void OnSpritelineEnd()
        //{
        //    Bus.Ppu.Vram.A12Toggled = true;
        //    if (Bus.Ppu.Control.PatternBg == 1)
        //        IrqScanline();
        //    Bus.Ppu.Vram.A12Toggled = false;
        //}


        //void ProcessCpuClock() override
        //{
        //    BaseProcessCpuClock();

        //    if (_irqSource == JyIrqSource.CpuClock || (_irqSource == JyIrqSource.CpuWrite && _console->GetCpu()->IsCpuWrite()))
        //    {
        //        TickIrqCounter();
        //    }
        //}

        //byte MapperReadVram(int addr, MemoryOperationType type) override
        //{
        //    if (_irqSource == JyIrqSource.PpuRead && type == MemoryOperationType.PpuRenderingRead)
        //    {
        //        TickIrqCounter();
        //    }

        //    if (addr >= 0x2000)
        //    {
        //        //This behavior only affects reads, not writes.
        //        //Additional info: https://forums.nesdev.com/viewtopic.php?f=3&t=17198
        //        if ((_advancedNtControl || _romInfo.MapperID == 211) && _romInfo.MapperID != 90)
        //        {
        //            byte ntIndex = ((addr & 0x2FFF) - 0x2000) / 0x400;
        //            if (_disableNtRam || (_ntLowRegs[ntIndex] & 0x80) != (_ntRamSelectBit & 0x80))
        //            {
        //                int chrPage = _ntLowRegs[ntIndex] | (_ntHighRegs[ntIndex] << 8);
        //                uint32_t chrOffset = chrPage * 0x400 + (addr & 0x3FF);
        //                if (_chrRomSize > chrOffset)
        //                {
        //                    return _chrRom[chrOffset];
        //                }
        //                else
        //                {
        //                    return 0;
        //                }
        //            }
        //        }
        //    }

        //    return InternalReadVram(addr);
        //}

        //void NotifyVramAddressChange(int addr) override
        //{
        //    if (_irqSource == JyIrqSource.PpuA12Rise && (addr & 0x1000) && !(_lastPpuAddr & 0x1000))
        //    {
        //        TickIrqCounter();
        //    }
        //    _lastPpuAddr = addr;

        //    if (_romInfo.MapperID == 209)
        //    {
        //        switch (addr & 0x2FF8)
        //        {
        //            case 0x0FD8:
        //            case 0x0FE8:
        //                _chrLatch[addr >> 12] = addr >> 4 & ((addr >> 10 & 0x04) | 0x02);
        //                UpdateChrState();
        //                break;
        //        }
        //    }
        //}

        //void TickIrqCounter()
        //{
        //    bool clockIrqCounter = false;
        //    byte mask = _irqSmallPrescaler ? 0x07 : 0xFF;
        //    byte prescaler = _irqPrescaler & mask;
        //    if (_irqCountDirection == 0x01)
        //    {
        //        prescaler++;
        //        if ((prescaler & mask) == 0)
        //        {
        //            clockIrqCounter = true;
        //        }
        //    }
        //    else if (_irqCountDirection == 0x02)
        //    {
        //        if (--prescaler == 0)
        //        {
        //            clockIrqCounter = true;
        //        }
        //    }
        //    _irqPrescaler = (_irqPrescaler & ~mask) | (prescaler & mask);

        //    if (clockIrqCounter)
        //    {
        //        if (_irqCountDirection == 0x01)
        //        {
        //            _irqCounter++;
        //            if (_irqCounter == 0 && _irqEnabled)
        //            {
        //                _console->GetCpu()->SetIrqSource(IRQSource.External);
        //            }
        //        }
        //        else if (_irqCountDirection == 0x02)
        //        {
        //            _irqCounter--;
        //            if (_irqCounter == 0xFF && _irqEnabled)
        //            {
        //                _console->GetCpu()->SetIrqSource(IRQSource.External);
        //            }
        //        }
        //    }
        //}
    }
}
