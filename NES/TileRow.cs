using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NES
{
    class TileRow
    {
        public byte Y;          // Y position 
        public int ID;          // ID of tile from pattern memory
        public byte Attribute;  // how sprite should be rendered
        public byte X;          // X position
        public int LSB;
        public int MSB;

        internal void ReadPattern(Bus bus, int row)
        {
            var address = ID << 4 | row;
            //var table = bus.ChrTable[address / bus.ChrBankSize];
            //var tileLine = address % bus.ChrBankSize;
            LSB = bus.GetPattern(address);// table[tileLine];
            MSB = bus.GetPattern(address | 8);// table[tileLine | 0x8];
        }
        public int GetPatternPixel(int idx)
        {
            int lo = (LSB >> idx) & 0x1;
            int hi = (MSB >> idx) & 0x1;
            return hi << 1 | lo;
        }
    }
}
