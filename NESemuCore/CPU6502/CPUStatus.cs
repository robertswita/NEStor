using System;
using System.Collections.Generic;
using System.Text;

namespace emulatorTest.CPU6502
{
    public class CPUStatus
    {
        public int reg;
        void SetBit(int f, bool value)
        {
            if (value)
                reg |= f;
            else
                reg &= ~f;
        }

        bool GetBit(int f)
        {
            return (reg & f) != 0;
        }

        public bool Carry
        {
            get { return GetBit(0x1); }
            set { SetBit(0x1, value); }
        }
        public bool Zero
        {
            get { return GetBit(0x2); }
            set { SetBit(0x2, value); }
        }
        public bool IRQoff
        {
            get { return GetBit(0x4); }
            set { SetBit(0x4, value); }
        }
        public bool Decimal
        {
            get { return GetBit(0x8); }
            set { SetBit(0x8, value); }
        }
        public bool Break
        {
            get { return GetBit(0x10); }
            set { SetBit(0x10, value); SetBit(0x20, true); }
        }
        private bool Unused
        {
            get { return GetBit(0x20); }
            set { SetBit(0x20, value); }
        }
        public bool Overflow
        {
            get { return GetBit(0x40); }
            set { SetBit(0x40, value); }
        }
        public bool Negative
        {
            get { return GetBit(0x80); }
            set { SetBit(0x80, value); }
        }
    }
}
