using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace Эмулятор_ФС
{
    public partial class PropertiesEntry : Form
    {
        FATEmul fATEmul;

        private Entry entry;
        private List<UserList> UserList;
        private List<GroupList> GroupList;

        private bool check = false;

        private PermissionsClass PermissionsClass = new PermissionsClass();
        private FATIndex FATIndex = new FATIndex();

        private string[] DataColumn;

        public PropertiesEntry(Entry entry, List<UserList> UserList, List<GroupList> GroupList, FATEmul fATEmul, string[] column, bool change)
        {
            InitializeComponent();

            this.fATEmul = fATEmul;

            this.entry = entry;

            this.UserList = UserList;

            this.GroupList = GroupList;

            this.DataColumn = column;

            textBox1.Text = String.Join("", entry.Name).Split('\0')[0];

            label4.Text = entry.FileOfSize.ToString();

            char[] owner = "Unknown".ToCharArray();

            foreach (UserList str in UserList)
            {
                if (str.UserID == entry.OwnerID && str.Name[0] != '#')
                {
                    owner = str.Name;
                    break;
                }
            }

            char[] group = "Unknown".ToCharArray();

            foreach (GroupList str in GroupList)
            {
                if (str.GroupID == entry.GroupID && str.Name[0] != '#')
                {
                    group = str.Name;
                    break;
                }
            }

            foreach (UserList userone in UserList)
            {
                if (userone.Name[0] != '#') comboBox1.Items.Add(String.Join("", userone.Name));
            }

            foreach (GroupList groupone in GroupList)
            {
                if (groupone.Name[0] != '#') comboBox2.Items.Add(String.Join("", groupone.Name));
            }

            comboBox1.Text = String.Join("", owner).Replace("\0", "");

            comboBox2.Text = String.Join("", group).Replace("\0", "");

            var c = DateTimeOffset.FromUnixTimeSeconds(entry.CreateDate);

            string create = c.ToLocalTime().ToString();

            c = DateTimeOffset.FromUnixTimeSeconds(entry.EditDate);

            string edit = c.ToLocalTime().ToString();

            label10.Text = create;

            label12.Text = edit;

            UInt16 permissions = entry.Permissions;

            if ((permissions & PermissionsClass.CatalogBit) == 0) label17.Text = "Файл";
            else label17.Text = "Папка";

            if ((permissions & PermissionsClass.Read_Owner) == PermissionsClass.Read_Owner) checkBox1.Checked = true;
            if ((permissions & PermissionsClass.Write_Owner) == PermissionsClass.Write_Owner) checkBox2.Checked = true;
            if ((permissions & PermissionsClass.Execute_Owner) == PermissionsClass.Execute_Owner) checkBox3.Checked = true;

            if ((permissions & PermissionsClass.Read_Group) == PermissionsClass.Read_Group) checkBox4.Checked = true;
            if ((permissions & PermissionsClass.Write_Group) == PermissionsClass.Write_Group) checkBox5.Checked = true;
            if ((permissions & PermissionsClass.Execute_Group) == PermissionsClass.Execute_Group) checkBox6.Checked = true;

            if ((permissions & PermissionsClass.Read_Other) == PermissionsClass.Read_Other) checkBox7.Checked = true;
            if ((permissions & PermissionsClass.Write_Other) == PermissionsClass.Write_Other) checkBox8.Checked = true;
            if ((permissions & PermissionsClass.Execute_Other) == PermissionsClass.Execute_Other) checkBox9.Checked = true;

            if (!change || entry.Name[0] == '%')
            {
                groupBox1.Enabled = false;
                comboBox1.Enabled = false;
                comboBox2.Enabled = false;
                textBox1.ReadOnly = true;
            }

            check = false;

            label2.Text = textBox1.Text.Length.ToString();
        }

        private void nameInput_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (FATIndex.BannedSymbols.Contains(e.KeyChar)) e.Handled = true;
            if (textBox1.Text.Length >= 21 && e.KeyChar != '\b') e.Handled = true;
            label2.Text = textBox1.Text.Length.ToString();
            check = true;
            return;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            check = true;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            check = true;
        }

        private void PropertiesEntry_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (check)
            {
                var result = MessageBox.Show("Сохранить внесенные изменения?", "Сохранение", MessageBoxButtons.YesNo);

                if (result == DialogResult.Yes)
                {

                    if (String.IsNullOrEmpty(textBox1.Text))
                    {
                        MessageBox.Show("Нельзя оставлять пустое имя!", "Сохранение", MessageBoxButtons.OK);
                        e.Cancel = true;
                        return;
                    }

                    if (DataColumn.Contains(textBox1.Text))
                    {
                        MessageBox.Show("Такой элемент уже есть!", "Ошибка", MessageBoxButtons.OK);
                        e.Cancel = true;
                        return;
                    }

                    bool flag = false;

                    string owner = comboBox1.Text;

                    UInt16 ownerid = entry.OwnerID;

                    if (!String.IsNullOrEmpty(owner))
                    {
                        foreach (UserList str in UserList)
                        {
                            if (owner == String.Join("", str.Name).Replace("\0", ""))
                            {
                                ownerid = str.UserID;
                                flag = true;
                                break;
                            }
                        }

                        if (!flag)
                        {
                            MessageBox.Show("Такой пользователь не найден!", "Ошибка", MessageBoxButtons.OK);
                            e.Cancel = true;
                            return;
                        }
                    }

                    flag = false;

                    string group = comboBox2.Text;

                    UInt16 groupid = entry.GroupID;

                    if (!String.IsNullOrEmpty(group))
                    {
                        foreach (GroupList str in GroupList)
                        {
                            if (group == String.Join("", str.Name).Replace("\0", ""))
                            {
                                groupid = str.GroupID;
                                flag = true;
                                break;
                            }
                        }

                        if (!flag)
                        {
                            MessageBox.Show("Такая группа не найдена!", "Ошибка", MessageBoxButtons.OK);
                            e.Cancel = true;
                            return;
                        }
                    }

                    UInt16 permissions = 0;

                    if (checkBox1.Checked == true) permissions |= PermissionsClass.Read_Owner;
                    if (checkBox4.Checked == true) permissions |= PermissionsClass.Read_Group;
                    if (checkBox7.Checked == true) permissions |= PermissionsClass.Read_Other;

                    if (checkBox2.Checked == true) permissions |= PermissionsClass.Write_Owner;
                    if (checkBox5.Checked == true) permissions |= PermissionsClass.Write_Group;
                    if (checkBox8.Checked == true) permissions |= PermissionsClass.Write_Other;

                    if (checkBox3.Checked == true) permissions |= PermissionsClass.Execute_Owner;
                    if (checkBox6.Checked == true) permissions |= PermissionsClass.Execute_Group;
                    if (checkBox9.Checked == true) permissions |= PermissionsClass.Execute_Other;

                    if (label17.Text == "Файл")
                    {
                        entry = new FileEntry(textBox1.Text.ToCharArray(), permissions, entry.FirstClusterIndex, entry.FileOfSize, ownerid, groupid, entry.EditDate, entry.CreateDate);
                    }
                    else
                    {
                        permissions |= PermissionsClass.CatalogBit;
                        entry = new CatalogEntry(textBox1.Text.ToCharArray(), permissions, entry.FirstClusterIndex, ownerid, groupid, entry.EditDate, entry.CreateDate);
                    }
                }
            }

            fATEmul.Entry = entry;
        }
    }
}
