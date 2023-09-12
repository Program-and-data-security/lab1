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
    public partial class OneGroupRewrite : Form
    {
        GroupList Group;
        List<GroupList> GroupList;
        private FATIndex FATIndex = new FATIndex();

        string oldName;

        public OneGroupRewrite(ref GroupList group, ref List<GroupList> groups)
        {
            InitializeComponent();

            Group = group;
            GroupList = groups;

            textBox1.Text = String.Join("", group.Name).Replace("\0", "");

            oldName = String.Join("", group.Name).Replace("\0", "");
        }

        private void textBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (FATIndex.BannedSymbols.Contains(e.KeyChar)) e.Handled = true;
            if ((sender as TextBox).Text.Length == 7 && e.KeyChar != 8) e.Handled = true;
            if (e.KeyChar == ' ') e.Handled = true;
            return;
        }

        private void OneUserRewrite_FormClosing(object sender, FormClosingEventArgs e)
        {
            var result = DialogResult.No;

            if (oldName != textBox1.Text) result = MessageBox.Show("Сохранить изменения?", "Сохранение", MessageBoxButtons.YesNo);

            if (result == DialogResult.Yes)
            {
                string name = textBox1.Text;

                foreach (GroupList group in GroupList)
                {
                    if (name == String.Join("", group.Name).Replace("\0", ""))
                    {
                        MessageBox.Show("Такая группа уже есть", "Ошибка", MessageBoxButtons.OK);
                        return;
                    }
                }

                Group.Name = name.ToCharArray();
            }
        }
    }
}
