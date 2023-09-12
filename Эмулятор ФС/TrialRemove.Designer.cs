namespace Эмулятор_ФС
{
    partial class TrialRemove
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
            this.checkCode = new System.Windows.Forms.Button();
            this.codeBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // checkCode
            // 
            this.checkCode.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.checkCode.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.checkCode.Location = new System.Drawing.Point(162, 64);
            this.checkCode.Name = "checkCode";
            this.checkCode.Size = new System.Drawing.Size(110, 34);
            this.checkCode.TabIndex = 12;
            this.checkCode.Text = "Активировать";
            this.checkCode.UseVisualStyleBackColor = true;
            this.checkCode.Click += new System.EventHandler(this.checkCode_Click);
            // 
            // codeBox
            // 
            this.codeBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.codeBox.Location = new System.Drawing.Point(79, 20);
            this.codeBox.Name = "codeBox";
            this.codeBox.Size = new System.Drawing.Size(193, 22);
            this.codeBox.TabIndex = 11;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label1.Location = new System.Drawing.Point(12, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(43, 20);
            this.label1.TabIndex = 10;
            this.label1.Text = "Код:";
            // 
            // TrialRemove
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(284, 110);
            this.Controls.Add(this.checkCode);
            this.Controls.Add(this.codeBox);
            this.Controls.Add(this.label1);
            this.Name = "TrialRemove";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "TrialRemove";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button checkCode;
        private System.Windows.Forms.TextBox codeBox;
        private System.Windows.Forms.Label label1;
    }
}