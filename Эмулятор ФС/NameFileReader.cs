using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace Эмулятор_ФС
{
    public partial class NameFileReader : Form
    {
        string[] Names;

        FATEmul form;

        FATIndex FATIndex = new FATIndex();

        public NameFileReader(string[] names, FATEmul form)
        {
            InitializeComponent();

            Names = names;

            this.form = form;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(nameInput.Text) || nameInput.Text.Length > 21)
            { 
                MessageBox.Show("Ошибка ввода названия!", "Ошибка", MessageBoxButtons.OK);
                return;
            }

            string name = nameInput.Text;

            for (int i = 0; i < Names.Length; i++)
            {
                if (Names[i] == name)
                {
                    MessageBox.Show("Такое имя уже есть в папке!", "Ошибка", MessageBoxButtons.OK);
                    return;
                }
            }

            form.tempString = name;
            this.Close();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void nameInput_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (FATIndex.BannedSymbols.Contains(e.KeyChar)) e.Handled = true;
            return;
        }
    }
}
