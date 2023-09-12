namespace Эмулятор_ФС
{
    partial class StartMenu
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.UserInput = new System.Windows.Forms.Button();
            this.FileFormatting = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // UserInput
            // 
            this.UserInput.FlatAppearance.BorderColor = System.Drawing.Color.White;
            this.UserInput.FlatAppearance.BorderSize = 0;
            this.UserInput.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.UserInput.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.UserInput.Location = new System.Drawing.Point(12, 12);
            this.UserInput.Name = "UserInput";
            this.UserInput.Size = new System.Drawing.Size(188, 49);
            this.UserInput.TabIndex = 0;
            this.UserInput.Text = "Войти в систему";
            this.UserInput.UseVisualStyleBackColor = true;
            this.UserInput.Click += new System.EventHandler(this.UserInput_Click);
            // 
            // FileFormatting
            // 
            this.FileFormatting.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.FileFormatting.FlatAppearance.BorderColor = System.Drawing.Color.White;
            this.FileFormatting.FlatAppearance.BorderSize = 0;
            this.FileFormatting.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.FileFormatting.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.FileFormatting.Location = new System.Drawing.Point(12, 67);
            this.FileFormatting.Name = "FileFormatting";
            this.FileFormatting.Size = new System.Drawing.Size(188, 49);
            this.FileFormatting.TabIndex = 1;
            this.FileFormatting.Text = "Создать диск";
            this.FileFormatting.UseVisualStyleBackColor = true;
            this.FileFormatting.Click += new System.EventHandler(this.FileFormatting_Click);
            // 
            // button1
            // 
            this.button1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.button1.FlatAppearance.BorderColor = System.Drawing.Color.White;
            this.button1.FlatAppearance.BorderSize = 0;
            this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.button1.Location = new System.Drawing.Point(12, 122);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(188, 26);
            this.button1.TabIndex = 2;
            this.button1.Text = "Ввод ключа активации";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // StartMenu
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(212, 160);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.FileFormatting);
            this.Controls.Add(this.UserInput);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "StartMenu";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Вход";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button UserInput;
        private System.Windows.Forms.Button FileFormatting;
        private System.Windows.Forms.Button button1;
    }
}