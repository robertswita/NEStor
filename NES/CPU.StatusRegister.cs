using System;
using System.Collections.Generic;
using System.Text;

namespace CPU
{
    public class StatusRegister
    {
        public bool Carry;
        public bool Zero;
        public bool IRQoff;
        public bool Decimal;
        public bool Break;
        public bool Unused;
        public bool Overflow;
        public bool Negative;
        public int Register
        {
            get
            {
                var result = 0;
                if (Carry)      result |= 0x01;
                if (Zero)       result |= 0x02;
                if (IRQoff)     result |= 0x04;
                if (Decimal)    result |= 0x08;
                if (Break)      result |= 0x10;
                if (Unused)     result |= 0x20;
                if (Overflow)   result |= 0x40;
                if (Negative)   result |= 0x80;
                return result;
            }
            set
            {
                Carry =     (value & 0x01) != 0;
                Zero =      (value & 0x02) != 0;
                IRQoff =    (value & 0x04) != 0;
                Decimal =   (value & 0x08) != 0;
                Break =     (value & 0x10) != 0;
                Unused =    true;
                Overflow =  (value & 0x40) != 0;
                Negative =  (value & 0x80) != 0;
            }
        }
    }
}
