using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NES
{
    class MemoryMap
    {
        public List<byte[]> Source;
        public byte[] Target;
        public byte[][] Banks;
        public int[] MapOrder;
        int bankSize;

        public MemoryMap(List<byte[]> source, byte[] target)
        {
            Source = source;
            Target = target;
        }
        public int BankSize
        {
            get { return bankSize; }
            set
            {
                bankSize = value;
                var bankCount = Target.Length / bankSize;
                Banks = new byte[bankCount][];
                for (int i = 0; i < bankCount; i++)
                {
                    Banks[i] = new byte[bankSize];
                    Array.Copy(Target, i * bankSize, Banks[i], 0, bankSize);
                }
                var bankSrcCount = Source.Count * Source[0].Length / bankSize;
                Source.Clear();
                MapOrder = new int[bankSrcCount];
                for (int i = 0; i < bankSrcCount; i++)
                {
                    Source.Add(Banks[i % Banks.Length]);
                    MapOrder[i] = i;
                }
            }
        }
        public void TransferBank(int targetIdx, int sourceIdx)
        {
            Source[MapOrder[sourceIdx]] = Banks[targetIdx % Banks.Length];
        }
    }
}
