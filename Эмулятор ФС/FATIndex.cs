using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Эмулятор_ФС
{
    internal class FATIndex
    {
        public UInt32 EOC;
        public UInt32 NOT_USING;
        public UInt32 MaxFAT;
        public Byte EmptyEntry;

        public char[] NullClaster;
        public char[] NullEntry;

        public byte[] NullClasterByte;
        public byte[] NullEntryByte;

        public byte[] NullName;

        public char[] BannedSymbols;

        public byte[] EmptyEntryArray;

        public char[] Alphabet;

        public FATIndex()
        {
            EOC = 0x0FFFFFFF;
            NOT_USING = 0x00000000;
            MaxFAT = 0x0FFFFFFA;

            EmptyEntryArray = new byte[1];
            EmptyEntryArray[0] = BitConverter.GetBytes('#')[0];

            EmptyEntry = EmptyEntryArray[0];

            NullClaster = new char[2048];

            NullEntry = new char[64];

            NullClasterByte = new byte[2048];

            NullEntryByte = new byte[64];

            NullName = new byte[42];

            BannedSymbols = new char[] { '$', '@', '%', '&', '/', '\\', '|', '~', '[', ']', '{', '}', '^', '#', ':', ';', '\'', '"' };

            Alphabet = new char[] { '1', '2', '3', '4', '5', '6', '7', '8', '9', '0', 'q', 'w', 'e', 'r', 't', 'y', 'u', 'i', 'o', 'p', 'a', 's', 'd', 'f', 'g', 'h', 'j', 'k', 'l', 'z', 'x', 'c', 'v', 'b', 'n', 'm' };
        }
    }
}
