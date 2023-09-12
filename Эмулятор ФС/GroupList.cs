using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Эмулятор_ФС
{
    public class GroupList
    {
        private char[] name; // 14 байт | 7 символов

        private UInt16 groupID; // 2 байта

        public char[] Name
        {
            set
            {
                name = new char[7];

                for (int i = 0; i < value.Length && i < name.Length; i++)
                {
                    name[i] = value[i];
                }
            }
            get { return name; }
        }

        public UInt16 GroupID
        {
            set { groupID = value; }
            get { return groupID; }
        }

        public GroupList(char[] Name, UInt16 GroupID)
        {
            this.Name = new char[7];

            for (int i = 0; i < Name.Length && i < this.Name.Length; i++)
            {
                this.Name[i] = Name[i];
            }

            this.GroupID = GroupID;
        }
    }
}
