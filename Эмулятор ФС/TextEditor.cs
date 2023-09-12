using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Эмулятор_ФС
{
    public partial class TextEditor : Form
    {
        FATEmul FATEmul;

        private bool isChange = false;
        private string text;

        public TextEditor(string name, string text, FATEmul fATEmul, bool change)
        {
            InitializeComponent();

            FileNameLabel.Text = name;
            TextPanel.Text = text;

            this.text = text;
            FATEmul = fATEmul;

            label2.Text = 0.ToString();
            label4.Text = TextPanel.Text.Length.ToString();

            TextPanel.ReadOnly = !change;

            if (name[0] == '%') TextPanel.ReadOnly = true;
        }

        private void TextPanel_TextChanged(object sender, EventArgs e)
        {
            if (TextPanel.Text == text) isChange = false;
            else isChange = true;

            label2.Text = TextPanel.SelectionStart.ToString();
            label4.Text = TextPanel.Text.Length.ToString();
        }

        private void TextEditor_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (isChange) // Если текст изменен
            {
                if (MessageBox.Show("Сохранить внесенные изменения?", "Сохранение", MessageBoxButtons.YesNo) == DialogResult.Yes) // Выбор пользователя
                {
                    FATEmul.tempString = TextPanel.Text;
                }
                else
                {
                    FATEmul.tempString = text;
                }
            }
            else FATEmul.tempString = text;
        }

        private void TextPanel_Click(object sender, EventArgs e)
        {
            label2.Text = TextPanel.SelectionStart.ToString();
        }
    }
}
