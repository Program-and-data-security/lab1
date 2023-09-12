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
    public partial class StartMenu : Form
    {
        public StartMenu()
        {
            InitializeComponent();
        }

        private void FileFormatting_Click(object sender, EventArgs e)
        {
            try
            {
                this.Hide();

                using (FIleFormattingForm fIleFormattingForm = new FIleFormattingForm())
                    if (fIleFormattingForm != null) fIleFormattingForm.ShowDialog();

                this.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK);
            }
        }

        private void UserInput_Click(object sender, EventArgs e)
        {
            try
            {
                this.Hide();

                using (EnterUser enterUser = new EnterUser())
                    enterUser.ShowDialog();

                this.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK);
            }
        }
    }
}
