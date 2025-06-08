using System;

namespace NEStor.Core.Ppu
{
    // $2005 | W   | PPUSCROLL
    //       |     | There are two scroll registers, vertical and horizontal,
    //       |     | which are both written via this port. The first value written
    //       |     | will go into the Vertical Scroll Register (unless it is > 239,
    //       |     | then it will be ignored). The second value will appear in the
    //       |     | Horizontal Scroll Register. The Name Tables are assumed to be
    //       |     | arranged in the following way:
    //       |     |
    //       |     |           +-----------+-----------+
    //       |     |           | 2 ($2800) | 3 ($2C00) |
    //       |     |           +-----------+-----------+
    //       |     |           | 0 ($2000) | 1 ($2400) |
    //       |     |           +-----------+-----------+
    //       |     |
    //       |     | When scrolled, the picture may span over several Name Tables.
    //       |     | Remember, though, that because of the mirroring, there are
    //       |     | only 2 real Name Tables, not 4.
    public struct VramRegister
    {
        public bool A12Toggled;
        public int FineX;
        int fineY;
        public int FineY
        {
            get => fineY;
            set
            {
                A12Toggled = (fineY & 1) == 0 && (value & 1) == 1;
                fineY = value;
            }
        }
        public int CoarseX;
        public int CoarseY;
        public int NameTableX;
        public int NameTableY;
        public bool Latched;

        public int Address
        {
            get
            {
                return FineY << 12 | NameTableY << 11 | NameTableX << 10 | CoarseY << 5 | CoarseX;
            }
            set
            {
                CoarseX = value & 0x1F;
                CoarseY = value >> 5 & 0x1F;
                NameTableX = value >> 10 & 0x1;
                NameTableY = value >> 11 & 0x1;
                FineY = value >> 12 & 0x7;
            }
        }

        public int ScrollX
        {
            get
            {
                return NameTableX << 8 | CoarseX << 3 | FineX;
            }
            set
            {
                FineX = value & 7;
                CoarseX = value >> 3 & 0x1F;
                NameTableX = value >> 8 & 0x1;
            }
        }

        public int ScrollY
        {
            get
            {
                return NameTableY << 8 | CoarseY << 3 | FineY;
            }
            set
            {
                FineY = value & 7;
                CoarseY = value >> 3 & 0x1F;
                NameTableY = value >> 8 & 0x1;
            }
        }

        public void SetAddress(int value)
        {
            Latched = !Latched;
            if (Latched)
                Address = value << 8;
            else
            {
                CoarseX = value & 0x1F;
                CoarseY |= value >> 5 & 0x7;
            }
        }

        public void SetScroll(int value)
        {
            Latched = !Latched;
            if (Latched)
            {
                FineX = value & 0x7;
                CoarseX = value >> 3;
            }
            else
            {
                FineY = value & 0x7;
                CoarseY = value >> 3;
            }
        }
    }
}
