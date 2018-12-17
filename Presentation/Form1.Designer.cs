namespace Presentation
{
    partial class Form1
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
            this.prompt = new System.Windows.Forms.Button();
            this.logBox = new System.Windows.Forms.RichTextBox();
            this.addressBox = new System.Windows.Forms.TextBox();
            this.close = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // prompt
            // 
            this.prompt.Location = new System.Drawing.Point(30, 107);
            this.prompt.Name = "prompt";
            this.prompt.Size = new System.Drawing.Size(185, 52);
            this.prompt.TabIndex = 0;
            this.prompt.Text = "Prompt for data";
            this.prompt.UseVisualStyleBackColor = true;
            this.prompt.Click += new System.EventHandler(this.prompt_Click);
            // 
            // logBox
            // 
            this.logBox.Location = new System.Drawing.Point(242, 107);
            this.logBox.Name = "logBox";
            this.logBox.Size = new System.Drawing.Size(501, 287);
            this.logBox.TabIndex = 1;
            this.logBox.Text = "";
            // 
            // addressBox
            // 
            this.addressBox.BackColor = System.Drawing.Color.Yellow;
            this.addressBox.Location = new System.Drawing.Point(30, 45);
            this.addressBox.Name = "addressBox";
            this.addressBox.Size = new System.Drawing.Size(713, 29);
            this.addressBox.TabIndex = 2;
            this.addressBox.Text = "http://localhost:62752/s";
            this.addressBox.TextChanged += new System.EventHandler(this.addressBox_TextChanged);
            // 
            // close
            // 
            this.close.Location = new System.Drawing.Point(30, 192);
            this.close.Name = "close";
            this.close.Size = new System.Drawing.Size(185, 50);
            this.close.TabIndex = 3;
            this.close.Text = "Close connection";
            this.close.UseVisualStyleBackColor = true;
            this.close.Click += new System.EventHandler(this.close_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 24F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.close);
            this.Controls.Add(this.addressBox);
            this.Controls.Add(this.logBox);
            this.Controls.Add(this.prompt);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button prompt;
        private System.Windows.Forms.RichTextBox logBox;
        private System.Windows.Forms.TextBox addressBox;
        private System.Windows.Forms.Button close;
    }
}

