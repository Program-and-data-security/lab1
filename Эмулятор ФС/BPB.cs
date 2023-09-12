using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Эмулятор_ФС
{
    public class BPB
    {
        private UInt16 sectorsByteCount; // 2048 стандартно     2 байта

        private Byte sectorsPerClaster; // 1 стандартно         1 байт

        private Byte fATCount; // 2 стандартно                  1 байт

        private UInt32 fATSize; // 268 435 450 стандартно       4 байта

        private FATIndex FATIndex = new FATIndex();

        public UInt16 SectorsByteCount
        {
            set { sectorsByteCount = value; }
            get { return sectorsByteCount; }
        }

        public Byte SectorsPerClaster
        {
            set { sectorsPerClaster = value; }
            get { return sectorsPerClaster; }
        }

        public Byte FATCount
        {
            set { fATCount = value; }
            get { return fATCount; }
        }

        public UInt32 FATSize
        {
            set { fATSize = value; }
            get { return fATSize; }
        }

        public BPB()
        {
            SectorsByteCount = 2048;
            SectorsPerClaster = 1;
            FATCount = 2;
            FATSize = FATIndex.MaxFAT;
        }

        public BPB(UInt16 SectorsByteCount, Byte SectorsPerClaster)
        {
            this.SectorsByteCount = SectorsByteCount;
            this.SectorsPerClaster = SectorsPerClaster;
            FATCount = 2;
            FATSize = FATIndex.MaxFAT;
        }

        public BPB(UInt32 FATSize)
        {
            SectorsByteCount = 2048;
            SectorsPerClaster = 1;
            FATCount = 2;
            this.FATSize = FATSize;
        }

        public BPB(UInt16 SectorsByteCount, Byte SectorsPerClaster, Byte FATCount, UInt32 FATSize)
        {
            this.SectorsByteCount = SectorsByteCount;
            this.SectorsPerClaster = SectorsPerClaster;
            this.FATCount = FATCount;
            this.FATSize = FATSize;
        }
    }
}
