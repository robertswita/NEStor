using NEStor.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NEStor.Core.Cartridge.Mappers
{
    internal class Mapper_085 : Mapper
    {
        //unique_ptr<Vrc7Audio> _audio;
        //unique_ptr<VrcIrq> _irq;
        byte _controlFlags;
        byte[] _chrRegisters = new byte[8];
        int IrqReloadValue;
        int IrqCounter;
        bool IrqEnabled;
        bool IrqEnabledAfterAck;
        bool IrqCycleMode;

        void UpdatePrgRamAccess()
        {
            //SetCpuMemoryMapping(
            //    0x6000,
            //    0x7FFF,
            //    0,
            //    HasBattery() ? PrgMemoryType.SaveRam : PrgMemoryType.WorkRam,
            //    (_controlFlags & 0x80) ? MemoryAccessType.ReadWrite : MemoryAccessType.NoAccess
            //);
        }

        //	int GetPrgPageSize() override { return 0x2000; }
        //	int GetChrPageSize() override { return 0x0400; }
        //bool EnableCpuClockHook() override
        //{ return true; }

        public override void Reset()
        {
            Bus.PrgMemory.BankSize = 0x2000;
            Bus.ChrMemory.BankSize = 0x400;
            //_audio.reset(new Vrc7Audio(_console));
            //_irq.reset(new VrcIrq(_console));

            //_irq->Reset();
            Bus.PrgMemory.Swap(3, -1);

            UpdateState(); //disable wram, set mirroring mode
        }

        //void Reset(bool softReset) override
        //{
        //    _audio->Reset();
        //}

        //void Serialize(Serializer& s) override
        //{
        //    BaseMapper.Serialize(s);
        //    SV(_controlFlags);
        //    SVArray(_chrRegisters, 8);
        //    SV(_irq);
        //    SV(_audio);

        //    if (!s.IsSaving())
        //    {
        //        UpdateState();
        //    }
        //}

        //void ProcessCpuClock() override
        //{
        //    BaseProcessCpuClock();
        //    _irq->ProcessCpuClock();
        //    _audio->Clock();
        //}

        void UpdateState()
        {
            switch (_controlFlags & 0x03)
            {
                case 0: Mirroring = MirrorType.Vertical; break;
                case 1: Mirroring = MirrorType.Horizontal; break;
                case 2: Mirroring = MirrorType.OneScreenLo; break;
                case 3: Mirroring = MirrorType.OneScreenHi; break;
            }
            UpdatePrgRamAccess();

            //_audio->SetMuteAudio((_controlFlags & 0x40) != 0);
        }

        public override void Poke(int addr, byte value)
        {
            if ((addr & 0x10) != 0 && (addr & 0xF010) != 0x9010)
            {
                addr |= 0x08;
                addr &= ~0x10;
            }

            switch (addr & 0xF038)
            {
                case 0x8000: Bus.PrgMemory.Swap(0, value & 0x3F); break;
                case 0x8008: Bus.PrgMemory.Swap(1, value & 0x3F); break;
                case 0x9000: Bus.PrgMemory.Swap(2, value & 0x3F); break;

                //case 0x9010: case 0x9030: _audio->WriteReg(addr, value); break;

                case 0xA000: Bus.ChrMemory.Swap(0, value); break;
                case 0xA008: Bus.ChrMemory.Swap(1, value); break;
                case 0xB000: Bus.ChrMemory.Swap(2, value); break;
                case 0xB008: Bus.ChrMemory.Swap(3, value); break;
                case 0xC000: Bus.ChrMemory.Swap(4, value); break;
                case 0xC008: Bus.ChrMemory.Swap(5, value); break;
                case 0xD000: Bus.ChrMemory.Swap(6, value); break;
                case 0xD008: Bus.ChrMemory.Swap(7, value); break;

                case 0xE000: _controlFlags = value; UpdateState(); break;

                case 0xE008: IrqReloadValue = value; break;
                case 0xF000:
                    IrqEnabledAfterAck = (value & 0x01) == 0x01;
                    IrqEnabled = (value & 0x02) == 0x02;
                    IrqCycleMode = (value & 0x04) == 0x04;
                    Bus.Cpu.MapperIrq.Acknowledge();
                    if (IrqEnabled)
                    {
                        IrqCounter = (byte)(0xFF - IrqReloadValue);
                        if (!IrqCycleMode)
                            IrqCounter = (int)(IrqCounter * (Bus.ModelParams.PpuMaxX + 2) / Bus.Ppu.ClockRatio);
                        Bus.Cpu.MapperIrq.Start(IrqCounter);
                    }
                    break;
                case 0xF008:
                    IrqEnabled = IrqEnabledAfterAck;
                    Bus.Cpu.MapperIrq.Acknowledge(); 
                    break;
            }
        }

    }
}
