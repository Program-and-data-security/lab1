using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Эмулятор_ФС
{
    public class FSinfo
    {
        private UInt32 freeClastersCount; //            4 байта

        private UInt32 nextFreeClaster; //              4 байта

        private FATIndex FATIndex = new FATIndex();

        public UInt32 FreeClastersCount
        {
            set { freeClastersCount = value; }
            get { return freeClastersCount; }
        }

        public UInt32 NextFreeClaster
        {
            set { nextFreeClaster = value; }
            get { return nextFreeClaster; }
        }

        public FSinfo()
        {
            FreeClastersCount = FATIndex.EOC;
            NextFreeClaster = FATIndex.EOC;
        }

        public FSinfo(UInt32 FreeClastersCount, UInt32 NextFreeClaster)
        {
            this.FreeClastersCount = FreeClastersCount;
            this.NextFreeClaster = NextFreeClaster;
        }

        public void DeleteOneClaster()
        {
            FreeClastersCount -= 1;
        }

        public void AddOneCLaster()
        {
            FreeClastersCount += 1;
        }
    }
}
