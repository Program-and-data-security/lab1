using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Эмулятор_ФС
{
    internal class FATNext
    {
        private UInt32 next = fATIndex.NOT_USING; // 268 435 454 стандартно NOT_USING

        static private FATIndex fATIndex = new FATIndex();

        public UInt32 Next
        {
            set
            {
                try
                {
                    next = value;
                }
                catch (Exception e)
                {
                    throw new Exception(e.Message);
                }
            }
            get { return next; }
        }

        public FATNext()
        {
            Next = fATIndex.NOT_USING;
        }

        public FATNext(UInt32 Next)
        {
            this.Next = Next;
        }
    }
}
