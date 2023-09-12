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
    public partial class TrialRemove : Form
    {
        public TrialRemove()
        {
            InitializeComponent();
        }

        private void checkCode_Click(object sender, EventArgs e)
        {
            if (codeBox.Text == "remove")
            {
                CheckRegistry.DeleteApplicationRegistryFolder();
            }
            if (codeBox.Text == "1234567890")
            {
                CheckRegistry.RemoveDemoTrial();
            }
            if (codeBox.Text == "startdemo")
            {
                CheckRegistry.ResetDemoTrial();
            }
        }
    }
}
