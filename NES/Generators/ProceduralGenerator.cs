using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NesCore.Audio.Generators
{
    public abstract class ProceduralGenerator: WaveGenerator
    {
        public bool LengthEnabled { get; protected set; }
        public int LengthValue { get; set; }

        public int TimerPeriod { get; protected set; }
        public int TimerValue { get; protected set; }

        public void StepLength()
        {
            if (LengthEnabled && LengthValue > 0)
                LengthValue--;
        }

        protected static readonly byte[] lengthTable = {
            10, 254, 20, 2, 40, 4, 80, 6, 160, 8, 60, 10, 14, 12, 26, 14,
            12, 16, 24, 18, 48, 20, 96, 22, 192, 24, 72, 26, 16, 28, 32, 30
        };

        protected static readonly byte[][] dutyTable = {
            new byte[] {0, 1, 0, 0, 0, 0, 0, 0},
            new byte[] {0, 1, 1, 0, 0, 0, 0, 0},
            new byte[] {0, 1, 1, 1, 1, 0, 0, 0},
            new byte[] {1, 0, 0, 1, 1, 1, 1, 1},
        };

        public override void SaveState(BinaryWriter binaryWriter)
        {
            base.SaveState(binaryWriter);

            binaryWriter.Write(LengthEnabled);
            binaryWriter.Write((byte)LengthValue);

            binaryWriter.Write((ushort)TimerPeriod);
            binaryWriter.Write((ushort)TimerValue);

        }

        public override void LoadState(BinaryReader binaryReader)
        {
            base.LoadState(binaryReader);

            LengthEnabled = binaryReader.ReadBoolean();
            LengthValue = binaryReader.ReadByte();

            TimerPeriod = binaryReader.ReadUInt16();
            TimerValue = binaryReader.ReadUInt16();
        }

    }
}
