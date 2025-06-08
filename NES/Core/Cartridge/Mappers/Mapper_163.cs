using System;
using System.Collections.Generic;

namespace NEStor.Core.Cartridge.Mappers
{
    class Mapper_163 : Mapper
    {
        byte[] _registers = new byte[5];
        bool _toggle = false;
        bool _autoSwitchCHR = false;

        void UpdateState()
        {
            var prgPage = _registers[0] & 0x0F | (_registers[2] & 0x0F) << 4;
            _autoSwitchCHR = (_registers[0] & 0x80) == 0x80;
            Bus.PrgMemory.Swap(0, prgPage);
        }

        //void Serialize(Serializer& s) override
        //{
        //    BaseMapper.Serialize(s);
        //    SVArray(_registers, 5);
        //    SV(_toggle);
        //    SV(_autoSwitchCHR);
        //}

        public override void Reset()
        {
            //"Initial value of this register is 1, initial value of "trigger" is 0."
            _toggle = true;
            _registers[4] = 0;
            Bus.PrgMemory.BankSize = 0x8000;
            Bus.ChrMemory.BankSize = 0x1000;
            Bus.ChrMemory.Swap(1, 0);
        }

        public override void Poke(int addr, byte value)
        {
            if (addr >= 0x5000 && addr <= 0x5FFF)
            {
                //"(Address is masked with 0x7300, except for 5101)"
                if (addr == 0x5101)
                {
                    if (_registers[4] != 0 && value == 0)
                    {
                        //"If the value of this register is changed from nonzero to zero, "trigger" is toggled (XORed with 1)"
                        _toggle = !_toggle;
                    }
                    _registers[4] = value;
                }
                else if (addr == 0x5100 && value == 6)
                {
                    Bus.PrgMemory.Swap(0, 3);
                }
                else
                {
                    switch (addr & 0x7300)
                    {
                        case 0x5000:
                            _registers[0] = value;
                            if ((_registers[0] & 0x80) == 0 && Bus.Ppu.Y < 128)
                            {
                                Bus.ChrMemory.Swap(0, 0);
                                Bus.ChrMemory.Swap(1, 1);
                            }
                            UpdateState();
                            break;
                        case 0x5100:
                            _registers[1] = value;
                            if (value == 6)
                            {
                                Bus.PrgMemory.Swap(0, 3);
                            }
                            break;
                        case 0x5200:
                            _registers[2] = value;
                            UpdateState();
                            break;
                        case 0x5300: _registers[3] = value; break;
                    }
                }
            }
        }

        public override byte Peek(int addr)
        {
            //Copy protection stuff - based on FCEUX's implementation
            switch (addr & 0x7700)
            {
                case 0x5100:
                    return (byte)(_registers[3] | _registers[1] | _registers[0] | _registers[2] ^ 0xFF);

                case 0x5500:
                    if (_toggle)
                    {
                        return (byte)(_registers[3] | _registers[0]);
                    }
                    return 0;
            }
            return 4;
        }

        public override void OnSpritelineStart()
        {
            if (_autoSwitchCHR)
            {
                if (Bus.Ppu.Y == 239)
                {
                    Bus.ChrMemory.Swap(0, 0);
                    Bus.ChrMemory.Swap(1, 0);
                }
                else if (Bus.Ppu.Y == 127)
                {
                    Bus.ChrMemory.Swap(0, 1);
                    Bus.ChrMemory.Swap(1, 1);
                }
            }
        }
    }
}
