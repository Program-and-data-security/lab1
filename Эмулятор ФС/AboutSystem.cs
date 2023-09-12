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
    public partial class AboutSystem : Form
    {
        UInt32 all;
        UInt32 free;

        public AboutSystem(UInt32 AllSize, UInt32 FreeSize)
        {
            InitializeComponent();

            all = AllSize;
            free = FreeSize;

            progressBar1.Maximum = (int)all;
            progressBar1.Minimum = 0;

            progressBar1.Step = 1;
            progressBar1.Value = (int)all - (int)free;

            label6.Text = ((float)all / 512).ToString() + " мб.";
            label8.Text = ((float)free / 512).ToString() + " мб.";
        }
    }
}
