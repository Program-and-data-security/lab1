using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Эмулятор_ФС
{
    internal class FileClaster
    {
        private char[] fileInfo;

        private UInt16 clasterSize;

        public char[] FileInfo
        {
            set
            {
                for (int i = 0; i < value.Length && i < clasterSize; i++)
                {
                    this.fileInfo[i] = value[i];
                }
            }
            get { return fileInfo; }
        }

        public UInt16 ClasterSize
        {
            set { clasterSize = value; }
            get { return clasterSize; }
        }

        public FileClaster()
        {
            FileInfo = new char[2048];

            ClasterSize = 2048;
        }

        public FileClaster(BPB BPB)
        {
            ClasterSize = (UInt16)(BPB.SectorsPerClaster * BPB.SectorsByteCount);
            FileInfo = new char[ClasterSize];
        }

        public FileClaster(BPB BPB, char[] FileInfo)
        {
            ClasterSize = (UInt16)(BPB.SectorsPerClaster * BPB.SectorsByteCount);
            this.FileInfo = new char[ClasterSize];

            for (int i = 0; i < FileInfo.Length && i < ClasterSize; i++)
            {
                this.FileInfo[i] = FileInfo[i];
            }
        }
    }
}
