using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Эмулятор_ФС
{
    public partial class CreateNewUsers : Form
    {
        static private readonly string path = "File.fat"; // путь к файлу с данными

        private List<UserList> UserList;
        private List<GroupList> GroupList;

        private FATIndex FATIndex = new FATIndex();

        public CreateNewUsers(ref List<UserList> userlist, ref List<GroupList> grouplist)
        {
            InitializeComponent();

            UserList = userlist;
            GroupList = grouplist;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(textBox1.Text) && !String.IsNullOrEmpty(textBox2.Text) && !String.IsNullOrEmpty(textBox3.Text))
            {
                if (textBox1.Text == "root")
                {
                    MessageBox.Show("Имя не может быть root", "Ошибка", MessageBoxButtons.OK);
                    return;
                }

                if (textBox3.Text == "root")
                {
                    MessageBox.Show("Группа не может быть root", "Ошибка", MessageBoxButtons.OK);
                    return;
                }

                // Считывание имени
                char[] name = new char[7];

                char[] temp = textBox1.Text.ToCharArray();

                for (int i = 0; i < temp.Length && i < name.Length; i++)
                {
                    name[i] = temp[i];
                }

                UInt16 userid = 0;

                // Проверка наличия пользователя с таким именем и нахождение максимального id
                for (int i = 0; i < UserList.Count; i++)
                {
                    if (CompareArrays(UserList[i].Name, name))
                    {
                        MessageBox.Show("Такой пользователь уже есть, введите несуществующего пользователя", "Ошибка", MessageBoxButtons.OK);
                        return;
                    }

                    if (userid < UserList[i].UserID) userid = UserList[i].UserID;
                }

                userid += 1;

                // Считывание пароля
                char[] password = new char[7];

                temp = textBox2.Text.ToCharArray();

                for (int i = 0; i < temp.Length && i < password.Length; i++)
                {
                    password[i] = temp[i];
                }

                // Считывание группы
                char[] group = new char[7];

                temp = textBox3.Text.ToCharArray();

                for (int i = 0; i < temp.Length && i < group.Length; i++)
                {
                    group[i] = temp[i];
                }

                UInt16 groupid = 0;

                // Поиск группы с введенным именем
                for (int i = 0; i < GroupList.Count; i++)
                {
                    if (CompareArrays(GroupList[i].Name, group))
                    {
                        groupid = GroupList[i].GroupID;
                        break;
                    }

                    if (groupid < GroupList[i].GroupID) groupid = GroupList[i].GroupID;

                    if (i == GroupList.Count - 1) // Если такой группы нет, то она создается
                    {
                        groupid += 1;
                        GroupList.Add(new GroupList(group, groupid));
                        break;
                    }
                }

                UserList.Add(new UserList(name, password, userid, groupid));

                this.Close();
            }
            else MessageBox.Show("Какое-то из полей не заполнено", "Ошибка", MessageBoxButtons.OK);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void textBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (FATIndex.BannedSymbols.Contains(e.KeyChar)) e.Handled = true;
            if ((sender as TextBox).Text.Length == 7 && e.KeyChar != 8) e.Handled = true;
            if ((sender as TextBox).Text.Length != 7 && e.KeyChar == ' ') e.Handled = true;
            return;
        }

        private bool CompareArrays(char[] array1, char[] array2)
        {
            if (array1.Length != array2.Length) return false;

            for (int i = 0; i < array1.Length; i++)
            {
                if (array1[i] != array2[i]) return false;
            }

            return true;
        }
    }
}
