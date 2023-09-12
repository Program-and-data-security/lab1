using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices.ComTypes;

namespace Эмулятор_ФС
{
    public partial class StartMenu : Form
    {
        bool demoMode = false;

        public StartMenu()
        {
            InitializeComponent();

            demoMode = CheckRegistry.CheckIsDemo();

            if (demoMode)
                CheckRegistry.CheckTrialPeriod();
        }

        private void FileFormatting_Click(object sender, EventArgs e)
        {
            try
            {
                this.Hide();

                using (FIleFormattingForm fIleFormattingForm = new FIleFormattingForm(demoMode))
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

                using (EnterUser enterUser = new EnterUser(demoMode))
                    enterUser.ShowDialog();

                this.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (TrialRemove form = new TrialRemove())
            {
                form.ShowDialog();
            }
        }
    }
}
