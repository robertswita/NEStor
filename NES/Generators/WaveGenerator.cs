using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NesCore.Audio.Generators
{
    public abstract class WaveGenerator
    {
        public bool Enabled { get; set; }

        public abstract byte Control { set; }

        public abstract byte Output { get; }

        public abstract void StepTimer();

        public virtual void SaveState(BinaryWriter binaryWriter)
        {
            binaryWriter.Write(Enabled);
        }

        public virtual void LoadState(BinaryReader binaryReader)
        {
            Enabled = binaryReader.ReadBoolean();
        }

    }
}
