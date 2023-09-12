using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Эмулятор_ФС
{
    public partial class Users : Form
    {
        List<UserList> UserList;
        List<GroupList> GroupList;
        UInt16 GroupID;
        UInt16 OwnerID;

        public Users(ref List<UserList> userLists, ref List<GroupList> groupList, UInt16 groupID, UInt16 ownerID)
        {
            InitializeComponent();

            UserList = userLists;

            GroupList = groupList;

            GroupID = groupID;

            OwnerID = ownerID;

            PrintUser();

            if (GroupID != 0) panel1.Enabled = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                CreateNewUsers createNewUsers = new CreateNewUsers(ref UserList, ref GroupList);
                createNewUsers.ShowDialog();

                PrintUser();
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
                if (userTable.SelectedRows[0].Index == -1) return;

                string name = userTable.SelectedRows[0].Cells[0].Value.ToString();

                if (name.Replace("\0", "") == "root") return;

                foreach (UserList user in UserList)
                {
                    string userstr = String.Join("", user.Name);

                    if (userstr == name)
                    {
                        if (user.UserID == OwnerID)
                        {
                            MessageBox.Show("Нельзя удалить самого себя из системы", "Ошибка", MessageBoxButtons.OK);
                            return;
                        }

                        name = '#' + name.TrimStart(name.First());

                        user.Name = name.ToCharArray();

                        break;
                    }
                }

                PrintUser();
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
                if (userTable.SelectedRows[0].Index == -1) return;

                string name = userTable.SelectedRows[0].Cells[0].Value.ToString();

                if (name.Replace("\0", "") == "root") return;

                UserList checkUser;

                for (int i = 0; i < UserList.Count; i++)
                {
                    string userstr = String.Join("", UserList[i].Name);

                    if (userstr == name)
                    {
                        checkUser = UserList[i];

                        OneUserRewrite oneUser = new OneUserRewrite(ref checkUser, ref GroupList);
                        oneUser.ShowDialog();

                        UserList[i] = checkUser;
                    }
                }

                PrintUser();
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message, "Ошибка", MessageBoxButtons.OK);
                return;
            }
        }

        private void PrintUser()
        {
            try
            {
                userTable.Rows.Clear();

                foreach (UserList user in UserList)
                {
                    if (user.Name[0] == '#') continue;

                    string userstr = String.Join("", user.Name);

                    string groupstr = "Unknown";

                    foreach (GroupList group in GroupList)
                    {
                        if (group.GroupID == user.GroupID)
                        {
                            if (group.Name[0] == '#') continue;

                            groupstr = String.Join("", group.Name);

                            break;
                        }
                    }

                    userTable.Rows.Add(userstr, groupstr);
                }
            }
            catch (Exception exc)
            {
                throw exc;
            }
        }
    }
}
