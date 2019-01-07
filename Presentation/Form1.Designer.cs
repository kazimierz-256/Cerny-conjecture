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
            this.materialLabel1 = new MaterialSkin.Controls.MaterialLabel();
            this.materialLabel3 = new MaterialSkin.Controls.MaterialLabel();
            this.materialTabControl1 = new MaterialSkin.Controls.MaterialTabControl();
            this.tabSettings = new System.Windows.Forms.TabPage();
            this.tabStatistics = new System.Windows.Forms.TabPage();
            this.materialTabSelector1 = new MaterialSkin.Controls.MaterialTabSelector();
            this.materialTabControl1.SuspendLayout();
            this.tabSettings.SuspendLayout();
            this.tabStatistics.SuspendLayout();
            this.SuspendLayout();
            // 
            // logBox
            // 
            this.logBox.Location = new System.Drawing.Point(17, 282);
            this.logBox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.logBox.Name = "logBox";
            this.logBox.Size = new System.Drawing.Size(303, 90);
            this.logBox.TabIndex = 1;
            this.logBox.Text = "";
            // 
            // prompt
            // 
            this.prompt.Depth = 0;
            this.prompt.Location = new System.Drawing.Point(124, 93);
            this.prompt.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.prompt.MouseState = MaterialSkin.MouseState.HOVER;
            this.prompt.Name = "prompt";
            this.prompt.Primary = true;
            this.prompt.Size = new System.Drawing.Size(258, 53);
            this.prompt.TabIndex = 4;
            this.prompt.Text = "Prompt for data";
            this.prompt.UseVisualStyleBackColor = true;
            this.prompt.Click += new System.EventHandler(this.prompt_Click);
            // 
            // close
            // 
            this.close.Depth = 0;
            this.close.Location = new System.Drawing.Point(124, 169);
            this.close.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.close.MouseState = MaterialSkin.MouseState.HOVER;
            this.close.Name = "close";
            this.close.Primary = true;
            this.close.Size = new System.Drawing.Size(258, 53);
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
            this.addressBox.Location = new System.Drawing.Point(124, 49);
            this.addressBox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.addressBox.MouseState = MaterialSkin.MouseState.HOVER;
            this.addressBox.Name = "addressBox";
            this.addressBox.PasswordChar = '\0';
            this.addressBox.SelectedText = "";
            this.addressBox.SelectionLength = 0;
            this.addressBox.SelectionStart = 0;
            this.addressBox.Size = new System.Drawing.Size(258, 28);
            this.addressBox.TabIndex = 6;
            this.addressBox.Text = "http://localhost:62752/ua";
            this.addressBox.UseSystemPasswordChar = false;
            // 
            // materialLabel1
            // 
            this.materialLabel1.AutoSize = true;
            this.materialLabel1.Depth = 0;
            this.materialLabel1.Font = new System.Drawing.Font("Roboto", 11F);
            this.materialLabel1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(222)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.materialLabel1.Location = new System.Drawing.Point(13, 81);
            this.materialLabel1.MouseState = MaterialSkin.MouseState.HOVER;
            this.materialLabel1.Name = "materialLabel1";
            this.materialLabel1.Size = new System.Drawing.Size(222, 24);
            this.materialLabel1.TabIndex = 7;
            this.materialLabel1.Text = "Całkowity czas obliczeń: ";
            // 
            // materialLabel3
            // 
            this.materialLabel3.AutoSize = true;
            this.materialLabel3.Depth = 0;
            this.materialLabel3.Font = new System.Drawing.Font("Roboto", 11F);
            this.materialLabel3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(222)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.materialLabel3.Location = new System.Drawing.Point(13, 147);
            this.materialLabel3.MouseState = MaterialSkin.MouseState.HOVER;
            this.materialLabel3.Name = "materialLabel3";
            this.materialLabel3.Size = new System.Drawing.Size(421, 24);
            this.materialLabel3.TabIndex = 9;
            this.materialLabel3.Text = "Średni czas analizy jednego automatu unarnego: ";
            // 
            // materialTabControl1
            // 
            this.materialTabControl1.Controls.Add(this.tabSettings);
            this.materialTabControl1.Controls.Add(this.tabStatistics);
            this.materialTabControl1.Depth = 0;
            this.materialTabControl1.Location = new System.Drawing.Point(2, 125);
            this.materialTabControl1.MouseState = MaterialSkin.MouseState.HOVER;
            this.materialTabControl1.Name = "materialTabControl1";
            this.materialTabControl1.SelectedIndex = 0;
            this.materialTabControl1.Size = new System.Drawing.Size(1052, 578);
            this.materialTabControl1.TabIndex = 10;
            // 
            // tabSettings
            // 
            this.tabSettings.Controls.Add(this.prompt);
            this.tabSettings.Controls.Add(this.addressBox);
            this.tabSettings.Controls.Add(this.close);
            this.tabSettings.Location = new System.Drawing.Point(4, 25);
            this.tabSettings.Name = "tabSettings";
            this.tabSettings.Padding = new System.Windows.Forms.Padding(3);
            this.tabSettings.Size = new System.Drawing.Size(1044, 549);
            this.tabSettings.TabIndex = 0;
            this.tabSettings.Text = "USTAWIENIA";
            this.tabSettings.UseVisualStyleBackColor = true;
            // 
            // tabStatistics
            // 
            this.tabStatistics.Controls.Add(this.materialLabel1);
            this.tabStatistics.Controls.Add(this.logBox);
            this.tabStatistics.Controls.Add(this.materialLabel3);
            this.tabStatistics.Location = new System.Drawing.Point(4, 25);
            this.tabStatistics.Name = "tabStatistics";
            this.tabStatistics.Padding = new System.Windows.Forms.Padding(3);
            this.tabStatistics.Size = new System.Drawing.Size(1044, 537);
            this.tabStatistics.TabIndex = 1;
            this.tabStatistics.Text = "STATYSTYKI";
            this.tabStatistics.UseVisualStyleBackColor = true;
            // 
            // materialTabSelector1
            // 
            this.materialTabSelector1.BaseTabControl = this.materialTabControl1;
            this.materialTabSelector1.Depth = 0;
            this.materialTabSelector1.Location = new System.Drawing.Point(-6, 63);
            this.materialTabSelector1.MouseState = MaterialSkin.MouseState.HOVER;
            this.materialTabSelector1.Name = "materialTabSelector1";
            this.materialTabSelector1.Size = new System.Drawing.Size(1060, 56);
            this.materialTabSelector1.TabIndex = 11;
            this.materialTabSelector1.Text = "materialTabSelector1";
            // 
            // Presentation
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1054, 702);
            this.Controls.Add(this.materialTabSelector1);
            this.Controls.Add(this.materialTabControl1);
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Name = "Presentation";
            this.Text = "Presentation module";
            this.materialTabControl1.ResumeLayout(false);
            this.tabSettings.ResumeLayout(false);
            this.tabStatistics.ResumeLayout(false);
            this.tabStatistics.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.RichTextBox logBox;
        private MaterialSkin.Controls.MaterialRaisedButton prompt;
        private MaterialSkin.Controls.MaterialRaisedButton close;
        private MaterialSkin.Controls.MaterialSingleLineTextField addressBox;
        private MaterialSkin.Controls.MaterialLabel materialLabel1;
        private MaterialSkin.Controls.MaterialLabel materialLabel3;
        private MaterialSkin.Controls.MaterialTabControl materialTabControl1;
        private System.Windows.Forms.TabPage tabSettings;
        private System.Windows.Forms.TabPage tabStatistics;
        private MaterialSkin.Controls.MaterialTabSelector materialTabSelector1;
    }
}

