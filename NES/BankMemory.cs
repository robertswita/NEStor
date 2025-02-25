using System;
using System.Collections.Generic;

namespace NES
{
    class BankMemory
    {
        public List<byte[]> Banks = new List<byte[]>();
        public List<byte[]> SwapBanks = new List<byte[]>();
        int bankSize;
        public int BankSize
        {
            get { return bankSize; }
            set
            {
                bankSize = SwapBanks[0].Length;
                var expanded = new byte[SwapBanks.Count * bankSize];
                for (int i = 0; i < SwapBanks.Count; i++)
                    Array.Copy(SwapBanks[i], 0, expanded, i * bankSize, bankSize);
                bankSize = value;
                var bankCount = expanded.Length / bankSize;
                SwapBanks.Clear();
                for (int i = 0; i < bankCount; i++)
                {
                    SwapBanks.Add(new byte[bankSize]);
                    Array.Copy(expanded, i * bankSize, SwapBanks[i], 0, bankSize);
                }
                bankCount = Banks.Count * Banks[0].Length / bankSize;
                Banks.Clear();
                for (int i = 0; i < bankCount; i++)
                    Banks.Add(SwapBanks[i % SwapBanks.Count]);
            }
        }

        public byte this[int addr]
        {
            get { return Banks[addr / bankSize][addr % bankSize]; }
            set { Banks[addr / bankSize][addr % bankSize] = value; }
        }

        public void Swap(int bankIdx, int swapBankIdx)
        {
            if (swapBankIdx < 0) swapBankIdx += SwapBanks.Count;
            Banks[bankIdx] = SwapBanks[swapBankIdx % SwapBanks.Count];
        }
    }
}
