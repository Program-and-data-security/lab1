using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Эмулятор_ФС
{
    public class UserList
    {
        private char[] name; // 14 байт | 7 символов

        private char[] password; // 14 байт | 7 символов

        private UInt16 userID; // 2 байта

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

        public char[] Password
        {
            set
            {
                password = new char[7];

                for (int i = 0; i < value.Length && i < password.Length; i++)
                {
                    password[i] = value[i];
                }
            }
            get { return password; }
        }

        public UInt16 UserID
        {
            set { userID = value; }
            get { return userID; }
        }

        public UInt16 GroupID
        {
            set { groupID = value; }
            get { return groupID; }
        }

        public UserList(char[] Name, char[] Password, UInt16 UserID, UInt16 GroupID)
        {
            this.Name = new char[7];

            for (int i = 0; i < Name.Length && i < this.Name.Length; i++)
            {
                this.Name[i] = Name[i];
            }

            this.Password = new char[7];

            for (int i = 0; i < Password.Length && i < this.Password.Length; i++)
            {
                this.Password[i] = Password[i];
            }

            this.UserID = UserID;
            this.GroupID = GroupID;
        }
    }
}
