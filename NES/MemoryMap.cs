using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NES
{
    class MemoryMap
    {
        public List<byte[]> Sources = new List<byte[]>();
        public List<byte[]> Targets = new List<byte[]>();
        public byte[][] Banks;
        public int[] MapOrder;
        int SourceSize;
        int TargetSize;
        int bankSize;

        public MemoryMap(int sourceSize, int targetSize)
        {
            SourceSize = sourceSize;
            TargetSize = targetSize;
        }
        public int BankSize
        {
            get { return bankSize; }
            set
            {
                bankSize = value;
                var bankCount = Targets.Count * TargetSize / bankSize;
                Banks = new byte[bankCount][];
                MapOrder = new int[bankCount];
                for (int i = 0; i < bankCount; i++)
                {
                    Banks[i] = new byte[bankSize];
                    var pos = i * bankSize;
                    Array.Copy(Targets[pos / TargetSize], pos % TargetSize, Banks[i], 0, bankSize);
                    MapOrder[i] = i;
                }
            }
        }
        public void TransferBank(int targetIdx, int sourceIdx)
        {
            var pos = MapOrder[sourceIdx] * bankSize;
            if (targetIdx > Banks.Length)
                targetIdx %= Banks.Length;
            Array.Copy(Banks[targetIdx], 0, Sources[pos / SourceSize], pos % SourceSize, bankSize);
        }
    }
}
