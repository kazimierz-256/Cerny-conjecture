using System;

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
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Title title1 = new System.Windows.Forms.DataVisualization.Charting.Title();
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
            this.materialLabelAdres = new MaterialSkin.Controls.MaterialLabel();
            this.materialDivider1 = new MaterialSkin.Controls.MaterialDivider();
            this.chart1 = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.materialTabControl1.SuspendLayout();
            this.tabSettings.SuspendLayout();
            this.tabStatistics.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chart1)).BeginInit();
            this.SuspendLayout();
            // 
            // logBox
            // 
            this.logBox.Location = new System.Drawing.Point(782, 417);
            this.logBox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.logBox.Name = "logBox";
            this.logBox.Size = new System.Drawing.Size(303, 90);
            this.logBox.TabIndex = 1;
            this.logBox.Text = "";
            // 
            // prompt
            // 
            this.prompt.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.prompt.Depth = 0;
            this.prompt.Location = new System.Drawing.Point(351, 90);
            this.prompt.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.prompt.MouseState = MaterialSkin.MouseState.HOVER;
            this.prompt.Name = "prompt";
            this.prompt.Primary = true;
            this.prompt.Size = new System.Drawing.Size(342, 53);
            this.prompt.TabIndex = 4;
            this.prompt.Text = "Prompt for data";
            this.prompt.UseVisualStyleBackColor = true;
            this.prompt.Click += new System.EventHandler(this.prompt_Click);
            // 
            // close
            // 
            this.close.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.close.Depth = 0;
            this.close.Location = new System.Drawing.Point(351, 166);
            this.close.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.close.MouseState = MaterialSkin.MouseState.HOVER;
            this.close.Name = "close";
            this.close.Primary = true;
            this.close.Size = new System.Drawing.Size(342, 53);
            this.close.TabIndex = 5;
            this.close.Text = "Close connection";
            this.close.UseVisualStyleBackColor = true;
            this.close.Click += new System.EventHandler(this.close_Click);
            // 
            // addressBox
            // 
            this.addressBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.addressBox.BackColor = System.Drawing.Color.White;
            this.addressBox.Depth = 0;
            this.addressBox.Hint = "";
            this.addressBox.Location = new System.Drawing.Point(351, 46);
            this.addressBox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.addressBox.MouseState = MaterialSkin.MouseState.HOVER;
            this.addressBox.Name = "addressBox";
            this.addressBox.PasswordChar = '\0';
            this.addressBox.SelectedText = "";
            this.addressBox.SelectionLength = 0;
            this.addressBox.SelectionStart = 0;
            this.addressBox.Size = new System.Drawing.Size(342, 28);
            this.addressBox.TabIndex = 6;
            this.addressBox.Text = Environment.GetCommandLineArgs().Length > 1 ? Environment.GetCommandLineArgs()[0] : "http://localhost:62752";
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
            this.materialTabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.materialTabControl1.Controls.Add(this.tabSettings);
            this.materialTabControl1.Controls.Add(this.tabStatistics);
            this.materialTabControl1.Depth = 0;
            this.materialTabControl1.Location = new System.Drawing.Point(2, 125);
            this.materialTabControl1.MouseState = MaterialSkin.MouseState.HOVER;
            this.materialTabControl1.Name = "materialTabControl1";
            this.materialTabControl1.SelectedIndex = 0;
            this.materialTabControl1.Size = new System.Drawing.Size(1239, 578);
            this.materialTabControl1.TabIndex = 10;
            // 
            // tabSettings
            // 
            this.tabSettings.Controls.Add(this.materialLabelAdres);
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
            this.tabStatistics.Controls.Add(this.chart1);
            this.tabStatistics.Controls.Add(this.materialDivider1);
            this.tabStatistics.Controls.Add(this.materialLabel1);
            this.tabStatistics.Controls.Add(this.logBox);
            this.tabStatistics.Controls.Add(this.materialLabel3);
            this.tabStatistics.Location = new System.Drawing.Point(4, 25);
            this.tabStatistics.Name = "tabStatistics";
            this.tabStatistics.Padding = new System.Windows.Forms.Padding(3);
            this.tabStatistics.Size = new System.Drawing.Size(1231, 549);
            this.tabStatistics.TabIndex = 1;
            this.tabStatistics.Text = "STATYSTYKI";
            this.tabStatistics.UseVisualStyleBackColor = true;
            // 
            // materialTabSelector1
            // 
            this.materialTabSelector1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.materialTabSelector1.BaseTabControl = this.materialTabControl1;
            this.materialTabSelector1.Depth = 0;
            this.materialTabSelector1.Location = new System.Drawing.Point(-6, 63);
            this.materialTabSelector1.MouseState = MaterialSkin.MouseState.HOVER;
            this.materialTabSelector1.Name = "materialTabSelector1";
            this.materialTabSelector1.Size = new System.Drawing.Size(1247, 56);
            this.materialTabSelector1.TabIndex = 11;
            this.materialTabSelector1.Text = "materialTabSelector1";
            // 
            // materialLabelAdres
            // 
            this.materialLabelAdres.AutoSize = true;
            this.materialLabelAdres.Depth = 0;
            this.materialLabelAdres.Font = new System.Drawing.Font("Roboto", 11F);
            this.materialLabelAdres.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(222)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.materialLabelAdres.Location = new System.Drawing.Point(347, 20);
            this.materialLabelAdres.MouseState = MaterialSkin.MouseState.HOVER;
            this.materialLabelAdres.Name = "materialLabelAdres";
            this.materialLabelAdres.Size = new System.Drawing.Size(64, 24);
            this.materialLabelAdres.TabIndex = 7;
            this.materialLabelAdres.Text = "Adres:";
            // 
            // materialDivider1
            // 
            this.materialDivider1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.materialDivider1.Depth = 0;
            this.materialDivider1.Location = new System.Drawing.Point(6, 238);
            this.materialDivider1.MouseState = MaterialSkin.MouseState.HOVER;
            this.materialDivider1.Name = "materialDivider1";
            this.materialDivider1.Size = new System.Drawing.Size(341, 305);
            this.materialDivider1.TabIndex = 10;
            this.materialDivider1.Text = "materialDivider1";
            // 
            // chart1
            // 
            chartArea1.Name = "ChartArea1";
            this.chart1.ChartAreas.Add(chartArea1);
            legend1.Name = "Legend1";
            this.chart1.Legends.Add(legend1);
            this.chart1.Location = new System.Drawing.Point(28, 253);
            this.chart1.Name = "chart1";
            series1.ChartArea = "ChartArea1";
            series1.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Doughnut;
            series1.IsValueShownAsLabel = true;
            series1.Legend = "Legend1";
            series1.Name = "UnaryFinishedSeries";
            series1.XValueType = System.Windows.Forms.DataVisualization.Charting.ChartValueType.String;
            this.chart1.Series.Add(series1);
            this.chart1.Size = new System.Drawing.Size(300, 275);
            this.chart1.TabIndex = 11;
            this.chart1.Text = "chart1";
            title1.Name = "Przeanalizowane automaty unarne";
            title1.Text = "Przeanalizowane automaty unarne";
            this.chart1.Titles.Add(title1);
            // 
            // Presentation
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1238, 702);
            this.Controls.Add(this.materialTabSelector1);
            this.Controls.Add(this.materialTabControl1);
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.MaximumSize = new System.Drawing.Size(1500, 900);
            this.MinimumSize = new System.Drawing.Size(900, 700);
            this.Name = "Presentation";
            this.Text = "Presentation module";
            this.materialTabControl1.ResumeLayout(false);
            this.tabSettings.ResumeLayout(false);
            this.tabSettings.PerformLayout();
            this.tabStatistics.ResumeLayout(false);
            this.tabStatistics.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chart1)).EndInit();
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
        private MaterialSkin.Controls.MaterialLabel materialLabelAdres;
        private System.Windows.Forms.DataVisualization.Charting.Chart chart1;
        private MaterialSkin.Controls.MaterialDivider materialDivider1;
    }
}

