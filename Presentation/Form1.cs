﻿using CommunicationContracts;
using MaterialSkin;
using MaterialSkin.Controls;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Presentation
{
    public partial class Presentation : MaterialForm
    {
        public Presentation()
        {
            InitializeComponent();

            var materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.AddFormToManage(this);
            materialSkinManager.Theme = MaterialSkinManager.Themes.DARK;
            materialSkinManager.ColorScheme = new ColorScheme(Primary.BlueGrey800, Primary.BlueGrey900, Primary.BlueGrey500, Accent.LightBlue200, TextShade.WHITE);

            addressBox.Text = Environment.GetCommandLineArgs().Length >= 2 ? Environment.GetCommandLineArgs()[1] : "http://localhost:62752";
        }

        HubConnection connection = null;
        private string connectionAddress = string.Empty;

        private async void prompt_Click(object sender, EventArgs e)
        {
            await RefreshConnection();
            if (connection != null)
            {
                await connection.InvokeAsync("SubscribeAndSendStatistics");
            }
        }

        private async void addressBox_TextChanged(object sender, EventArgs e) => await RefreshConnection();

        private async Task RefreshConnection()
        {
            var connectionText = $"{addressBox.Text}/ua";
            {
                addressBox.BackColor = Color.OrangeRed;
                if (connection != null && connection.State == HubConnectionState.Connected)
                {
                    await connection.StopAsync();
                }
                connection = new HubConnectionBuilder()
                    .WithUrl(connectionText)
                    .AddMessagePackProtocol()
                    .Build();
                connection.On(
                    "ShowSimpleTextStatistics",
                    (ServerPresentationComputationSummary resultingTextStatistics) =>
                    {
                        Invoke(new Action(() =>
                        {
                            //logBox.Text = $"{resultingTextStatistics.finishedAutomata.Sum(s => s.solution.solvedB.Count)} total interesting.";
                            int toCompute = resultingTextStatistics.total - resultingTextStatistics.finishedAutomata.Count;
                            chart1.Series["UnaryFinishedSeries"].Points.Clear();
                            chart1.Series["UnaryFinishedSeries"].Points.AddXY("To compute", toCompute);
                            chart1.Series["UnaryFinishedSeries"].Points.AddXY("Computed", resultingTextStatistics.finishedAutomata.Count);


                            if (resultingTextStatistics.finishedAutomata.Count > 0)
                            {
                                var totalSeconds = (resultingTextStatistics.finishedAutomata.Max(r => r.finishTime) - resultingTextStatistics.finishedAutomata.Min(r => r.issueTime)).TotalSeconds;
                                int totalMili = (int)(totalSeconds * 1000) - (int)totalSeconds * 1000;
                                var avgSeconds = resultingTextStatistics.finishedAutomata.Count / totalSeconds;
                                double leftSeconds = toCompute * totalSeconds / resultingTextStatistics.finishedAutomata.Count;

                                //logBox.Text += resultingTextStatistics.description + $" Total speed: {avgSeconds:F2}";

                                materialLabel1.Text = "Total computation time: " + (new TimeSpan(0, 0, 0, (int)totalSeconds, totalMili)).ToString();
                                materialLabel3.Text = $"Average speed: {avgSeconds:F2} automata per second.";
                                materialLabel2.Text = "Expected end of computation at: " + DateTime.Now.AddSeconds(leftSeconds).ToString();
                            }

                            listOfAutomata.Items.Clear();
                            foreach (var a in resultingTextStatistics.finishedAutomata)
                            {
                                if (a.solution.solvedB.Count > 0)
                                {
                                    string b_tab,a_tab = byteTabToString(a.solution.unaryArray);
                                    foreach (var b in a.solution.solvedB)
                                    {
                                        b_tab = byteTabToString(b);
                                        listOfAutomata.Items.Add($"[{a_tab},{b_tab}]");
                                    }
                                }
                            }

                            labelAutomataCount.Text = $"There are {listOfAutomata.Items.Count} interesting automata.";
                        }));
                    }
                    );
                addressBox.BackColor = Color.YellowGreen;
                try
                {
                    await connection.StartAsync();
                    connectionAddress = connectionText;
                }
                catch (Exception e)
                {

                }
            }
        }

        private string byteTabToString (byte[] tab)
        {
            string s = tab[0].ToString();
            for(int i = 1; i < tab.Length; i++)
            {
                s += $",{tab[i]}";
            }
            return $"[{s}]";
        }

        private async void close_Click(object sender, EventArgs e)
        {
            if (connection != null)
            {
                await connection.StopAsync();
                addressBox.BackColor = Color.OrangeRed;
            }
        }

        private void runVisualisationButton_Click(object sender, EventArgs e)
        {
            if (listOfAutomata.SelectedItem != null)
            {
                string automaton = listOfAutomata.SelectedItem.ToString();
                System.Diagnostics.Process.Start($"{addressBox.Text}/?automaton={automaton}");
            }
            else
            {
                string message = "You did not selected any automaton. Want to see visualisation of Cerny automaton?\nPS.You can click NO, go back, and select some automaton this time.";
                string caption = "Not selected automaton";
                MessageBoxButtons buttons = MessageBoxButtons.YesNo;
                DialogResult result;
                result = MessageBox.Show(message, caption, buttons);
                if (result == System.Windows.Forms.DialogResult.Yes)
                {
                    System.Diagnostics.Process.Start($"{addressBox.Text}/");
                }
            }
        }
    }
}
