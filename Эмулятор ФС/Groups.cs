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
    public partial class Groups : Form
    {
        List<GroupList> GroupList;
        UInt16 GroupID;

        public Groups(ref List<GroupList> groupList, UInt16 groupID)
        {
            InitializeComponent();

            GroupList = groupList;

            GroupID = groupID;

            PrintGroup();

            if (GroupID != 0) panel1.Enabled = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                CreateNewGroup createNewGroup = new CreateNewGroup(ref GroupList);
                createNewGroup.ShowDialog();

                PrintGroup();
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message, "Ошибка", MessageBoxButtons.OK);
                return;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                if (groupTable.SelectedRows[0].Index == -1) return;

                string name = groupTable.SelectedRows[0].Cells[0].Value.ToString();

                if (name.Replace("\0", "") == "root") return;

                foreach (GroupList group in GroupList)
                {
                    string groupstr = String.Join("", group.Name);

                    if (groupstr == name)
                    {
                        name = '#' + name.TrimStart(name.First());

                        group.Name = name.ToCharArray();

                        break;
                    }
                }

                PrintGroup();
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message, "Ошибка", MessageBoxButtons.OK);
                return;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                if (groupTable.SelectedRows[0].Index == -1) return;

                string name = groupTable.SelectedRows[0].Cells[0].Value.ToString();

                if (name.Replace("\0", "") == "root") return;

                GroupList chechGroup;

                for (int i = 0; i < GroupList.Count; i++)
                {
                    string userstr = String.Join("", GroupList[i].Name);

                    if (userstr == name)
                    {
                        chechGroup = GroupList[i];

                        OneGroupRewrite oneGroup = new OneGroupRewrite(ref chechGroup, ref GroupList);
                        oneGroup.ShowDialog();

                        GroupList[i] = chechGroup;
                    }
                }

                PrintGroup();
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message, "Ошибка", MessageBoxButtons.OK);
                return;
            }
        }

        private void PrintGroup()
        {
            try
            {
                groupTable.Rows.Clear();

                foreach (GroupList group in GroupList)
                {
                    if (group.Name[0] == '#') continue;

                    string groupstr = String.Join("", group.Name);

                    groupTable.Rows.Add(groupstr);
                }
            }
            catch (Exception exc)
            {
                throw exc;
            }
        }
    }
}
