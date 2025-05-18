using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NES
{
    internal class Mapper_016 : Mapper
    {
        bool _irqEnabled = false;
        int _irqCounter = 0;
        int _irqReload = 0;
        int _prgPage = 0;
        int _prgBankSelect = 0;
        byte[] _chrRegs = new byte[8];
        //shared_ptr<DatachBarcodeReader> _barcodeReader;
        //unique_ptr<BaseEeprom24C0X> _standardEeprom;
        //unique_ptr<BaseEeprom24C0X> _extraEeprom;

        public override void Reset()
        {
            //Only allow reads from 0x6000 to 0x7FFF
            //RemoveRegisterRange(0x8000, 0xFFFF, MemoryOperation::Read);
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
                //if (SubId == 0 || (SubId == 5 && _romInfo.Header.GetSaveRamSize() == 256))
                //{
                //    //Connect a 256-byte EEPROM for iNES roms, and when submapper 5 + 256 bytes of save ram in header
                //    _standardEeprom.reset(new Eeprom24C02(_console));
                //}
            }

            //if (Id != 16)
            //{
            //    //"For iNES Mapper 153 (with SRAM), the writeable ports must only be mirrored across $8000-$FFFF."
            //    //"Mappers 157 and 159 do not need to support the FCG-1 and -2 and so should only mirror the ports across $8000-$FFFF."
            //    if (Id == 153)
            //    {
            //        //Mapper 153 has regular save ram from $6000-$7FFF, need to remove the register for both read & writes
            //        //RemoveRegisterRange(0x6000, 0x7FFF, MemoryOperation::Any);
            //    }
            //    else
            //    {
            //        //RemoveRegisterRange(0x6000, 0x7FFF, MemoryOperation::Write);
            //        //PrgRamEnabled = false;
            //    }
            //}
            //else if (Id == 16 && _romInfo.SubMapperID == 4)
            //{
            //    RemoveRegisterRange(0x8000, 0xFFFF, MemoryOperation::Write);
            //}
            //else if (Id == 16 && _romInfo.SubMapperID == 5)
            //{
            //    RemoveRegisterRange(0x6000, 0x7FFF, MemoryOperation::Write);
            //}

            if (Id == 153)
                Array.Fill<byte>(Bus.WRam, 0xFF);
            else
                PrgRamEnabled = false;
            Bus.PrgMemory.Swap(1, -1);
        }

        public override byte Peek(int addr)
        {
            byte output = 0;
            //if (_barcodeReader)
            //    output |= _barcodeReader->GetOutput();
            //if (_extraEeprom && _standardEeprom)
            //    output |= (_standardEeprom->Read() && _extraEeprom->Read()) << 4;
            //else if (_standardEeprom)
            //    output |= (_standardEeprom->Read() << 4);
            return output;// | _console->GetMemoryManager()->GetOpenBus(0xE7);
        }

        public override void Poke(int addr, byte value)
        {
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
                    if (Id == 153 || Bus.PrgMemory.SwapBanks.Count >= 0x20)
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
                    _prgPage = value & 0xF;
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
                    _irqEnabled = (value & 0x01) == 0x01;
                    //Wiki claims there is no reload value, however this seems to be the only way to make Famicom Jump II - Saikyou no 7 Nin work properly 
                    //if (Id != 16 || SubId != 4)
                    {
                        //"On the LZ93D50 (Submapper 5), writing to this register also copies the latch to the actual counter."
                        _irqCounter = _irqReload;
                    }
                    Bus.Cpu.MapperIrq.Acknowledge();
                    if (_irqEnabled)
                        Bus.Cpu.MapperIrq.Start(_irqCounter);
                    break;
                case 0x0B:
                    if (Id != 16 || SubId != 4)
                        //"On the LZ93D50 (Submapper 5), these registers instead modify a latch that will only be copied to the actual counter when register $800A is written to."
                        _irqReload = (_irqReload & 0xFF00) | value;
                    else
                        //"On the FCG-1/2 (Submapper 4), writing to these two registers directly modifies the counter itself; all such games therefore disable counting before changing the counter value."
                        _irqCounter = (_irqCounter & 0xFF00) | value;
                    break;
                case 0x0C:
                    if (Id != 16 || SubId != 4)
                        _irqReload = (_irqReload & 0xFF) | (value << 8);
                    else
                        _irqCounter = (_irqCounter & 0xFF) | (value << 8);
                    break;
                case 0x0D:
                    if (Id == 153)
                    {
                        //SetCpuMemoryMapping(0x6000, 0x7FFF, 0, HasBattery() ? PrgMemoryType::SaveRam : PrgMemoryType::WorkRam, value & 0x20 ? MemoryAccessType::ReadWrite : MemoryAccessType::NoAccess);
                    }
                    else
                    {
                        //byte scl = (value & 0x20) >> 5;
                        //byte sda = (value & 0x40) >> 6;
                        //if (_standardEeprom)
                        //{
                        //    _standardEeprom->Write(scl, sda);
                        //}
                        //if (_extraEeprom)
                        //{
                        //    _extraEeprom->WriteSda(sda);
                        //}
                    }
                    break;
            }
            if (addr >= 0x6000 && addr < 0x8000)
                Bus.WRam[addr & 0x1FFF] = value;
        }
    }
}
