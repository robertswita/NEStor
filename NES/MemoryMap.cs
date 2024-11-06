using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NES
{
    class MemoryMap
    {
        public List<byte[]> Target;
        public byte[] Source;
        public byte[][] Banks;

        public MemoryMap(List<byte[]> target, byte[] source)
        {
            Target = target;
            Source = source;
        }

        int bankSize;
        public int BankSize
        {
            get { return bankSize; }
            set
            {
                bankSize = value;
                var bankCount = Source.Length / bankSize;
                Banks = new byte[bankCount][];
                for (int i = 0; i < bankCount; i++)
                {
                    Banks[i] = new byte[bankSize];
                    Array.Copy(Source, i * bankSize, Banks[i], 0, bankSize);
                }
                var bankTargetCount = Target.Count * Target[0].Length / bankSize;
                Target.Clear();
                for (int i = 0; i < bankTargetCount; i++)
                    Target.Add(Banks[i % Banks.Length]);
            }
        }

        public int this[int targetIdx]
        {
            set
            {
                if (value < 0) value += Banks.Length;
                Target[targetIdx] = Banks[value % Banks.Length];
            }
        }

    }
}
