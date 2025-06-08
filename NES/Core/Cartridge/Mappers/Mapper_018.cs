using NES;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NEStor.Core.Cartridge.Mappers
{
    internal class Mapper_018 : Mapper
    {
        static int[] _irqMask = new int[] { 0xFFFF, 0x0FFF, 0x00FF, 0x000F };

        int[] _prgBanks = new int[3];
        int[] _chrBanks = new int[8];
        int[] _irqReloadValue = new int[4];
        int _irqCounter;
        byte _irqCounterSize;
        bool _irqEnabled;

        public override void Reset()
        {
            Bus.PrgMemory.BankSize = 0x2000;
            Bus.ChrMemory.BankSize = 0x400;
            Bus.PrgMemory.Swap(3, -1);
        }

        //virtual void Serialize(Serializer& s) override
        //{
        //	BaseMapper.Serialize(s);

        //	SVArray(_prgBanks, 3);
        //	SVArray(_chrBanks, 8);
        //	SVArray(_irqReloadValue, 4);
        //	SV(_irqCounter);
        //	SV(_irqCounterSize);
        //	SV(_irqEnabled);
        //}

        void SetMirroring(int value)
        {
            switch (value)
            {
                case 0: Mirroring = MirrorType.Horizontal; break;
                case 1: Mirroring = MirrorType.Vertical; break;
                case 2: Mirroring = MirrorType.OneScreenLo; break;
                case 3: Mirroring = MirrorType.OneScreenHi; break;
            }
        }

        void UpdatePrgBank(byte bankNumber, byte value, bool updateUpperBits)
        {
            if (updateUpperBits)
            {
                _prgBanks[bankNumber] = _prgBanks[bankNumber] & 0x0F | value << 4;
            }
            else
            {
                _prgBanks[bankNumber] = _prgBanks[bankNumber] & 0xF0 | value;
            }

            Bus.PrgMemory.Swap(bankNumber, _prgBanks[bankNumber]);
        }

        void UpdateChrBank(byte bankNumber, byte value, bool updateUpperBits)
        {
            if (updateUpperBits)
            {
                _chrBanks[bankNumber] = _chrBanks[bankNumber] & 0x0F | value << 4;
            }
            else
            {
                _chrBanks[bankNumber] = _chrBanks[bankNumber] & 0xF0 | value;
            }

            Bus.ChrMemory.Swap(bankNumber, _chrBanks[bankNumber]);
        }

        //virtual void ProcessCpuClock() override
        //{
        //	BaseProcessCpuClock();

        //	//Clock irq counter every memory read/write (each cpu cycle either reads or writes memory)
        //	ClockIrqCounter();
        //}

        void ReloadIrqCounter()
        {
            _irqCounter = _irqReloadValue[0] | _irqReloadValue[1] << 4 | _irqReloadValue[2] << 8 | _irqReloadValue[3] << 12;
        }

        //void ClockIrqCounter()
        //{
        //	if(_irqEnabled) {
        //		int counter = _irqCounter & _irqMask[_irqCounterSize];

        //		if(--counter == 0) {
        //			_console->GetCpu()->SetIrqSource(IRQSource.External);
        //		}

        //		_irqCounter = (_irqCounter & ~_irqMask[_irqCounterSize]) | (counter & _irqMask[_irqCounterSize]);
        //	}
        //}

        public override void Poke(int addr, byte value)
        {
            bool updateUpperBits = (addr & 0x01) == 0x01;
            value &= 0x0F;

            switch (addr & 0xF003)
            {
                case 0x8000: case 0x8001: UpdatePrgBank(0, value, updateUpperBits); break;
                case 0x8002: case 0x8003: UpdatePrgBank(1, value, updateUpperBits); break;
                case 0x9000: case 0x9001: UpdatePrgBank(2, value, updateUpperBits); break;

                case 0xA000: case 0xA001: UpdateChrBank(0, value, updateUpperBits); break;
                case 0xA002: case 0xA003: UpdateChrBank(1, value, updateUpperBits); break;
                case 0xB000: case 0xB001: UpdateChrBank(2, value, updateUpperBits); break;
                case 0xB002: case 0xB003: UpdateChrBank(3, value, updateUpperBits); break;
                case 0xC000: case 0xC001: UpdateChrBank(4, value, updateUpperBits); break;
                case 0xC002: case 0xC003: UpdateChrBank(5, value, updateUpperBits); break;
                case 0xD000: case 0xD001: UpdateChrBank(6, value, updateUpperBits); break;
                case 0xD002: case 0xD003: UpdateChrBank(7, value, updateUpperBits); break;

                case 0xE000:
                case 0xE001:
                case 0xE002:
                case 0xE003:
                    _irqReloadValue[addr & 0x03] = value;
                    break;

                case 0xF000:
                    Bus.Cpu.MapperIrq.Acknowledge();
                    ReloadIrqCounter();
                    break;

                case 0xF001:
                    Bus.Cpu.MapperIrq.Acknowledge();
                    _irqEnabled = (value & 0x01) != 0;
                    value >>= 1;
                    _irqCounterSize = 0;
                    while (value > 0)
                    {
                        _irqCounterSize++;
                        value >>= 1;
                    }
                    if (_irqEnabled)
                    {
                        _irqCounter &= _irqMask[_irqCounterSize];
                        Bus.Cpu.MapperIrq.Start(_irqCounter);
                    }
                    break;

                case 0xF002:
                    SetMirroring(value & 0x03);
                    break;

                case 0xF003:
                    //Expansion audio, not supported yet
                    break;
            }
        }
    }
}
