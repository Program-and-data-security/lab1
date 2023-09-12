using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Эмулятор_ФС
{
    abstract public class Entry
    {
        private char[] name; //                     42 байт | 21 символ

        private UInt16 permissions; //              2 байта

        private UInt32 firstClusterIndex; //        4 байта

        private UInt32 fileOfSize; //               4 байта

        private UInt16 ownerID; //                  2 байта

        private UInt16 groupID; //                  2 байта

        private UInt32 editDate; //                 4 байта

        private UInt32 createDate; //               4 байта

        private PermissionsClass permissionsClass = new PermissionsClass();

        public char[] Name
        {
            set
            {
                name = new char[21];
                for (int i = 0; i < value.Length && i < name.Length; i++)
                {
                    name[i] = value[i];
                }
            }
            get { return name; }
        }

        public UInt16 Permissions
        {
            set { permissions = value; }
            get { return permissions; }
        }

        public UInt32 FirstClusterIndex
        {
            set { firstClusterIndex = value; }
            get { return firstClusterIndex; }
        }

        public UInt32 FileOfSize
        {
            set { fileOfSize = value; }
            get { return fileOfSize; }
        }

        public UInt16 OwnerID
        {
            set { ownerID = value; }
            get { return ownerID; }
        }

        public UInt16 GroupID
        {
            set { groupID = value; }
            get { return groupID; }
        }

        public UInt32 EditDate
        {
            set { editDate = value; }
            get { return editDate; }
        }

        public UInt32 CreateDate
        {
            set { createDate = value; }
            get { return createDate; }
        }

        public PermissionsClass PermissionsClassIndex
        {
            get
            {
                return permissionsClass;
            }
        }
    }
}
