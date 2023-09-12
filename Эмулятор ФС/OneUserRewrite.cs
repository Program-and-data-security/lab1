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
    public partial class OneUserRewrite : Form
    {
        UserList User;
        List<GroupList> GroupList;
        private FATIndex FATIndex = new FATIndex();

        public OneUserRewrite(ref UserList user, ref List<GroupList> groups)
        {
            InitializeComponent();

            User = user;
            GroupList = groups;

            textBox1.Text = String.Join("", user.Name).Replace("\0", "");

            string groupstr = "Unknown";

            foreach (GroupList group in GroupList)
            {
                if (group.GroupID == user.GroupID)
                {
                    groupstr = String.Join("", group.Name).Replace("\0", "");
                    break;
                }
            }

            comboBox1.Text = groupstr;

            foreach (GroupList group in GroupList)
            {
                if (group.Name[0] != '#')
                {
                    comboBox1.Items.Add(String.Join("", group.Name));
                }
            }

            textBox2.Text = String.Join("", user.Password);
        }

        private void textBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (FATIndex.BannedSymbols.Contains(e.KeyChar)) e.Handled = true;
            if ((sender as TextBox).Text.Length == 7 && e.KeyChar != 8) e.Handled = true;
            if (e.KeyChar == ' ') e.Handled = true;
            return;
        }

        private void comboBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (FATIndex.BannedSymbols.Contains(e.KeyChar)) e.Handled = true;
            if ((sender as ComboBox).Text.Length == 7 && e.KeyChar != 8) e.Handled = true;
            if (e.KeyChar == ' ') e.Handled = true;
            return;
        }

        private void OneUserRewrite_FormClosing(object sender, FormClosingEventArgs e)
        {
            var result = MessageBox.Show("Сохранить изменения?", "Сохранение", MessageBoxButtons.YesNo);

            if (result == DialogResult.Yes)
            {
                User.Name = textBox1.Text.ToCharArray();
                User.Password = textBox2.Text.ToCharArray();

                string groupName = comboBox1.Text;
                bool flag = false;

                UInt16 max = 0;

                foreach (GroupList group in GroupList)
                {
                    if (String.Join("", group.Name).Replace("\0", "") == groupName)
                    {
                        User.GroupID = group.GroupID;
                        flag = true;
                        break;
                    }

                    if (group.GroupID > max) max = group.GroupID;
                }

                if (!flag)
                {
                    GroupList.Add(new GroupList(comboBox1.Text.ToCharArray(), (UInt16)(max + 1)));

                    User.GroupID = (UInt16)(max + 1);
                }
            }
        }
    }
}
