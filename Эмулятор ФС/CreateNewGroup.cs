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
    public partial class CreateNewGroup : Form
    {
        List<GroupList> GroupList;
        private FATIndex FATIndex = new FATIndex();

        public CreateNewGroup(ref List<GroupList> groupList)
        {
            InitializeComponent();

            GroupList = groupList;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(textBox1.Text))
            {
                if (textBox1.Text == "root")
                {
                    MessageBox.Show("Имя не может быть root", "Ошибка", MessageBoxButtons.OK);
                    return;
                }

                // Считывание имени
                string name = textBox1.Text;

                UInt16 groupid = 0;

                // Проверка наличия пользователя с таким именем и нахождение максимального id
                for (int i = 0; i < GroupList.Count; i++)
                {
                    if (name == String.Join("", GroupList[i].Name).Replace("\0", ""))
                    {
                        MessageBox.Show("Такая группа уже есть, введите несуществующую группу", "Ошибка", MessageBoxButtons.OK);
                        return;
                    }

                    if (groupid < GroupList[i].GroupID) groupid = GroupList[i].GroupID;
                }

                GroupList.Add(new GroupList(name.ToCharArray(), (UInt16)(groupid + 1)));

                this.Close();
            }
            else MessageBox.Show("Поле с именем пусто", "Ошибка", MessageBoxButtons.OK);
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
    }
}
