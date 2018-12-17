namespace Presentation
{
    partial class Presentation
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
            this.logBox = new System.Windows.Forms.RichTextBox();
            this.prompt = new MaterialSkin.Controls.MaterialRaisedButton();
            this.close = new MaterialSkin.Controls.MaterialRaisedButton();
            this.addressBox = new MaterialSkin.Controls.MaterialSingleLineTextField();
            this.SuspendLayout();
            // 
            // logBox
            // 
            this.logBox.Location = new System.Drawing.Point(373, 185);
            this.logBox.Name = "logBox";
            this.logBox.Size = new System.Drawing.Size(415, 183);
            this.logBox.TabIndex = 1;
            this.logBox.Text = "";
            // 
            // prompt
            // 
            this.prompt.Depth = 0;
            this.prompt.Location = new System.Drawing.Point(12, 185);
            this.prompt.MouseState = MaterialSkin.MouseState.HOVER;
            this.prompt.Name = "prompt";
            this.prompt.Primary = true;
            this.prompt.Size = new System.Drawing.Size(355, 80);
            this.prompt.TabIndex = 4;
            this.prompt.Text = "Prompt for data";
            this.prompt.UseVisualStyleBackColor = true;
            this.prompt.Click += new System.EventHandler(this.prompt_Click);
            // 
            // close
            // 
            this.close.Depth = 0;
            this.close.Location = new System.Drawing.Point(12, 288);
            this.close.MouseState = MaterialSkin.MouseState.HOVER;
            this.close.Name = "close";
            this.close.Primary = true;
            this.close.Size = new System.Drawing.Size(355, 80);
            this.close.TabIndex = 5;
            this.close.Text = "Close connection";
            this.close.UseVisualStyleBackColor = true;
            this.close.Click += new System.EventHandler(this.close_Click);
            // 
            // addressBox
            // 
            this.addressBox.BackColor = System.Drawing.Color.White;
            this.addressBox.Depth = 0;
            this.addressBox.Hint = "";
            this.addressBox.Location = new System.Drawing.Point(12, 119);
            this.addressBox.MouseState = MaterialSkin.MouseState.HOVER;
            this.addressBox.Name = "addressBox";
            this.addressBox.PasswordChar = '\0';
            this.addressBox.SelectedText = "";
            this.addressBox.SelectionLength = 0;
            this.addressBox.SelectionStart = 0;
            this.addressBox.Size = new System.Drawing.Size(776, 36);
            this.addressBox.TabIndex = 6;
            this.addressBox.Text = "http://localhost:62752/s";
            this.addressBox.UseSystemPasswordChar = false;
            // 
            // Presentation
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 24F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1099, 872);
            this.Controls.Add(this.logBox);
            this.Controls.Add(this.addressBox);
            this.Controls.Add(this.close);
            this.Controls.Add(this.prompt);
            this.Name = "Presentation";
            this.Text = "Presentation module";
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.RichTextBox logBox;
        private MaterialSkin.Controls.MaterialRaisedButton prompt;
        private MaterialSkin.Controls.MaterialRaisedButton close;
        private MaterialSkin.Controls.MaterialSingleLineTextField addressBox;
    }
}

