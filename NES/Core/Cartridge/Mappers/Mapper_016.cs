using NES;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace NEStor.Core.Cartridge.Mappers
{
    class Mapper_016 : Mapper
    {
        class Eeprom24C0X
        {
            enum Mode { Idle, Address, Read, Write, SendAck, WaitAck, ChipAddress };
            Mode _mode;
            Mode _nextMode;
            int _chipAddress;
            int _address;
            int _data;
            byte _counter;
            int _output;
            byte _prevScl;
            byte _prevSda;
            byte[] _romData = new byte[256];
            void WriteBit(ref int dest, bool bit)
            {
                if (_counter < 8)
                {
                    var mask = 1 << 7 - _counter;
                    if (bit)
                        dest |= mask;
                    else
                        dest &= ~mask;
                    _counter++;
                }
            }

            void ReadBit()
            {
                if (_counter < 8)
                {
                    _output = _data >> 7 - _counter & 1;
                    _counter++;
                }
            }

            public int Read() { return _output; }

            public void Write(byte scl, byte sda)
            {
                if ((_prevScl & scl & sda) < _prevSda)
                {
                    //"START is identified by a high to low transition of the SDA line while the clock SCL is *stable* in the high state"
                    _mode = Mode.ChipAddress;
                    _counter = 0;
                    _output = 1;
                }
                else if ((_prevScl & scl & sda) > _prevSda)
                {
                    //"STOP is identified by a low to high transition of the SDA line while the clock SCL is *stable* in the high state"
                    _mode = Mode.Idle;
                    _output = 1;
                }
                else if (scl > _prevScl)
                {
                    //Clock rise
                    switch (_mode)
                    {
                        default: break;
                        case Mode.ChipAddress: WriteBit(ref _chipAddress, sda != 0); break;
                        case Mode.Address: WriteBit(ref _address, sda != 0); break;
                        case Mode.Read: ReadBit(); break;
                        case Mode.Write: WriteBit(ref _data, sda != 0); break;
                        case Mode.SendAck: _output = 0; break;
                        case Mode.WaitAck:
                            if (sda != 0)
                            {
                                _nextMode = Mode.Read;
                                _data = _romData[_address];
                            }
                            break;
                    }
                }
                else if (scl < _prevScl)
                {
                    //Clock fall
                    switch (_mode)
                    {
                        case Mode.ChipAddress:
                            //"Upon a correct compare the X24C02 outputs an acknowledge on the SDA line"
                            if (_counter == 8)
                            {
                                if ((_chipAddress & 0xA0) == 0xA0)
                                {
                                    _mode = Mode.SendAck;
                                    _counter = 0;
                                    _output = 1;

                                    //"The last bit of the slave address defines the operation to
                                    //be performed. When set to one a read operation is
                                    //selected, when set to zero a write operation is selected"
                                    if ((_chipAddress & 0x01) != 0)
                                    {
                                        //"Current Address Read"
                                        //"Upon receipt of the slave address with the R/W
                                        //bit set to one, the X24C02 issues an acknowledge 
                                        //and transmits the eight bit word during the next eight clock cycles"
                                        _nextMode = Mode.Read;
                                        _data = _romData[_address];
                                    }
                                    else
                                    {
                                        _nextMode = Mode.Address;
                                    }
                                }
                                else
                                {
                                    //This chip wasn't selected, go back to idle mode
                                    _mode = Mode.Idle;
                                    _counter = 0;
                                    _output = 1;
                                }
                            }
                            break;

                        case Mode.Address:
                            if (_counter == 8)
                            {
                                //Finished receiving all 8 bits of the address, send an ack and then starting writing the value
                                _counter = 0;
                                _mode = Mode.SendAck;
                                _nextMode = Mode.Write;
                                _output = 1;
                            }
                            break;

                        case Mode.Read:
                            if (_counter == 8)
                            {
                                //Finished sending all 8 bits, wait for an ack
                                _mode = Mode.WaitAck;
                                _address = _address + 1 & 0xFF;
                            }
                            break;

                        case Mode.Write:
                            if (_counter == 8)
                            {
                                //Finished receiving all 8 bits, send an ack
                                _counter = 0;
                                _mode = Mode.SendAck;
                                _nextMode = Mode.Write;
                                _romData[_address] = (byte)_data;
                                _address = _address + 1 & 0xFF;
                            }
                            break;

                        case Mode.SendAck:
                        case Mode.WaitAck:
                            _mode = _nextMode;
                            _counter = 0;
                            _output = 1;
                            break;

                        default:
                            break;
                    }
                }
                _prevScl = scl;
                _prevSda = sda;
            }


            void WriteScl(byte scl)
            {
                Write(scl, _prevSda);
            }

            void WriteSda(byte sda)
            {
                Write(_prevScl, sda);
            }
        }
        bool _irqEnabled = false;
        int _irqCounter = 0;
        int _irqReload = 0;
        int _prgPage = 0;
        int _prgBankSelect = 0;
        byte[] _chrRegs = new byte[8];
        //shared_ptr<DatachBarcodeReader> _barcodeReader;
        Eeprom24C0X _standardEeprom;
        //unique_ptr<BaseEeprom24C0X> _extraEeprom;

        public override void Reset()
        {
            //Only allow reads from 0x6000 to 0x7FFF
            //RemoveRegisterRange(0x8000, 0xFFFF, MemoryOperation.Read);
            Bus.PrgMemory.BankSize = 0x4000;
            Bus.ChrMemory.BankSize = 0x400;
            if (Id == 157)
            {
                //"Mapper 157 is used for Datach Joint ROM System boards"
                //_barcodeReader.reset(new DatachBarcodeReader(_console));
                //_console->GetControlManager()->AddSystemControlDevice(_barcodeReader);

                //Datach Joint ROM System
                //"It contains an internal 256-byte serial EEPROM (24C02) that is shared among all Datach games."
                //"One game, Battle Rush: Build up Robot Tournament, has an additional external 128-byte serial EEPROM (24C01) on the game cartridge."
                //"The NES 2.0 header's PRG-NVRAM field will only denote whether the game cartridge has an additional 128-byte serial EEPROM"
                //if (!IsNes20() || _romInfo.Header.GetSaveRamSize() == 128)
                //{
                //    _extraEeprom.reset(new Eeprom24C01(_console));
                //}

                ////All mapper 157 games have an internal 256-byte EEPROM
                //_standardEeprom.reset(new Eeprom24C02(_console));
            }
            else if (Id == 159)
            {
                //LZ93D50 with 128 byte serial EEPROM (24C01)
                //_standardEeprom.reset(new Eeprom24C01(_console));
            }
            else if (Id == 16)
            {
                //"INES Mapper 016 submapper 4: FCG-1/2 ASIC, no serial EEPROM, banked CHR-ROM"
                //"INES Mapper 016 submapper 5: LZ93D50 ASIC and no or 256-byte serial EEPROM, banked CHR-ROM"

                //Add a 256 byte serial EEPROM (24C02)
                if (SubId == 0 || SubId == 5 && SaveRamSize == 256)
                {
                    //Connect a 256-byte EEPROM for iNES roms, and when submapper 5 + 256 bytes of save ram in header
                    _standardEeprom = new Eeprom24C0X();
                }
            }

            //if (Id != 16)
            //{
            //    //"For iNES Mapper 153 (with SRAM), the writeable ports must only be mirrored across $8000-$FFFF."
            //    //"Mappers 157 and 159 do not need to support the FCG-1 and -2 and so should only mirror the ports across $8000-$FFFF."
            //    if (Id == 153)
            //    {
            //        //Mapper 153 has regular save ram from $6000-$7FFF, need to remove the register for both read & writes
            //        //RemoveRegisterRange(0x6000, 0x7FFF, MemoryOperation.Any);
            //    }
            //    else
            //    {
            //        //RemoveRegisterRange(0x6000, 0x7FFF, MemoryOperation.Write);
            //        //PrgRamEnabled = false;
            //    }
            //}
            //else if (Id == 16 && _romInfo.SubMapperID == 4)
            //{
            //    RemoveRegisterRange(0x8000, 0xFFFF, MemoryOperation.Write);
            //}
            //else if (Id == 16 && _romInfo.SubMapperID == 5)
            //{
            //    RemoveRegisterRange(0x6000, 0x7FFF, MemoryOperation.Write);
            //}

            if (Id == 153)
                Array.Fill<byte>(Bus.WRam, 0xFF);
            else
                PrgRamEnabled = false;
            Bus.PrgMemory.Swap(1, -1);
        }

        public override byte Peek(int addr)
        {
            var output = 0;
            //if (_barcodeReader)
            //    output |= _barcodeReader->GetOutput();
            //if (_extraEeprom && _standardEeprom)
            //    output |= (_standardEeprom->Read() && _extraEeprom->Read()) << 4;
            //else if (_standardEeprom)
                output |= _standardEeprom.Read() << 4;
            return (byte)output;// | _console->GetMemoryManager()->GetOpenBus(0xE7);
        }

        public override void Poke(int addr, byte value)
        {
            if (addr >= 0x8000 && Id == 16 && SubId == 4) return;
            //if (addr >= 0x6000 && addr < 0x8000 && Id == 16 && SubId == 5)
            //{
            //    //Bus.WRam[addr & 0x1FFF] = value;
            //    return;
            //}
            switch (addr & 0xF)
            {
                case 0x00:
                case 0x01:
                case 0x02:
                case 0x03:
                case 0x04:
                case 0x05:
                case 0x06:
                case 0x07:
                    _chrRegs[addr & 0x07] = value;
                    if (Id == 153 || Id == 16 && Bus.PrgMemory.SwapBanks.Count >= 0x20)
                    {
                        _prgBankSelect = 0;
                        for (int i = 0; i < 8; i++)
                            _prgBankSelect |= _chrRegs[i] << 4 & 0x10;
                        Bus.PrgMemory.Swap(0, _prgBankSelect | _prgPage);
                        Bus.PrgMemory.Swap(1, _prgBankSelect | 0xF);
                    }
                    else if (!ChrRamEnabled && Id != 157)
                        Bus.ChrMemory.Swap(addr & 0x07, value);
                    //if (_extraEeprom && Id == 157 && (addr & 0x0F) <= 3)
                    //{
                    //    _extraEeprom->WriteScl((value >> 3) & 0x01);
                    //}
                    break;
                case 0x08:
                    _prgPage = value;// & 0xF;
                    Bus.PrgMemory.Swap(0, _prgBankSelect | _prgPage);
                    break;
                case 0x09:
                    switch (value & 0x03)
                    {
                        case 0: Mirroring = MirrorType.Vertical; break;
                        case 1: Mirroring = MirrorType.Horizontal; break;
                        case 2: Mirroring = MirrorType.OneScreenLo; break;
                        case 3: Mirroring = MirrorType.OneScreenHi; break;
                    }
                    break;
                case 0x0A:
                    _irqEnabled = (value & 1) != 0;
                    Bus.Cpu.MapperIrq.Acknowledge();
                    if ((value & 1) != 0)
                        Bus.Cpu.MapperIrq.Start(_irqCounter);
                    break;
                case 0x0B:
                    _irqCounter = _irqCounter & 0xFF00 | value;
                    break;
                case 0x0C:
                    _irqCounter = _irqCounter & 0xFF | value << 8;
                    break;
                case 0x0D:
                    if (Id == 153)
                    {
                        //SetCpuMemoryMapping(0x6000, 0x7FFF, 0, HasBattery() ? PrgMemoryType.SaveRam : PrgMemoryType.WorkRam, value & 0x20 ? MemoryAccessType.ReadWrite : MemoryAccessType.NoAccess);
                    }
                    else
                    {
                        var scl = (value & 0x20) >> 5;
                        var sda = (value & 0x40) >> 6;
                        if (_standardEeprom != null)
                            _standardEeprom.Write((byte)scl, (byte)sda);
                        //if (_extraEeprom)
                        //{
                        //    _extraEeprom->WriteSda(sda);
                        //}
                    }
                    break;
            }

        }
    }
}
