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
        private MaterialSkinManager materialSkinManager;
        public Presentation()
        {
            InitializeComponent();

            materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.AddFormToManage(this);
            materialSkinManager.Theme = MaterialSkinManager.Themes.DARK;
            materialSkinManager.ColorScheme = new ColorScheme(Primary.BlueGrey800, Primary.BlueGrey900, Primary.BlueGrey500, Accent.LightBlue200, TextShade.WHITE);
            listOfAutomata.BackColor = materialSkinManager.ColorScheme.PrimaryColor;
            listOfAutomata.ForeColor = materialSkinManager.ColorScheme.TextColor;
            chart1.BackColor = materialSkinManager.ColorScheme.PrimaryColor;
            chart1.BackSecondaryColor = materialSkinManager.ColorScheme.PrimaryColor;
            chart1.BorderlineColor = materialSkinManager.ColorScheme.PrimaryColor;
            chart1.ForeColor = materialSkinManager.ColorScheme.TextColor;
            foreach (var area in chart1.ChartAreas)
            {
                area.BackColor = materialSkinManager.ColorScheme.PrimaryColor;
                area.BackSecondaryColor = materialSkinManager.ColorScheme.PrimaryColor;
                area.BorderColor = materialSkinManager.ColorScheme.TextColor;
            }

            foreach (var title in chart1.Titles)
            {
                title.ForeColor = materialSkinManager.ColorScheme.TextColor;
            }

            foreach (var series in chart1.Legends)
            {
                series.BackColor = materialSkinManager.ColorScheme.PrimaryColor;
                series.ForeColor = materialSkinManager.ColorScheme.TextColor;
            }

            addressBox.Text = Environment.GetCommandLineArgs().Length >= 2 ? Environment.GetCommandLineArgs()[1] : "http://localhost:62752";

        }

        HubConnection connection = null;
        private string connectionAddress = string.Empty;

        private async void prompt_Click(object sender, EventArgs e)
        {
            await RefreshConnection();
            if (connection != null && connection.State == HubConnectionState.Connected)
            {
                addressBox.BackColor = materialSkinManager.ColorScheme.AccentColor;
                await connection.InvokeAsync("SubscribeAndSendStatistics");
            }
        }

        private async void addressBox_TextChanged(object sender, EventArgs e) => await RefreshConnection();
        private List<string> automataToLaunch = new List<string>();
        private bool timerLaunched = false;
        private TimeSpan totalComputingTime;
        private DateTime startCountingTime;
        private async Task RefreshConnection()
        {
            var connectionText = $"{addressBox.Text}/ua";
            if (connection != null && connection.State == HubConnectionState.Connected)
            {
                await connection.StopAsync();
            }

            var connectionBuilder = new HubConnectionBuilder()
                .WithUrl(connectionText);

            connectionBuilder.AddMessagePackProtocol();
            connection = connectionBuilder.Build();
            connection.On(
                "ShowSimpleTextStatistics",
                (ServerPresentationComputationSummary summary) =>
                {
                    Invoke(new Action(() =>
                    {
                        int toCompute = summary.total - summary.finishedAutomata.Count;
                        chart1.Series["UnaryFinishedSeries"].Points.Clear();
                        chart1.Series["UnaryFinishedSeries"].Points.AddXY("To compute", toCompute);
                        chart1.Series["UnaryFinishedSeries"].Points.AddXY("Computed", summary.finishedAutomata.Count);

                        if (summary.finishedAutomata.Count > 0)
                        {
                            chart1.Titles[0].Text = $"Unary Automata with n = {summary.n}";
                            totalComputingTime = GetTotalComputationTime(summary.finishedAutomata);
                            var totalSpeed = GetAverageSpeed(summary.finishedAutomata);
                            double leftSeconds;
                            if (totalSpeed == 0)
                            {
                                if (toCompute == 0)
                                    leftSeconds = 0;
                                else
                                    leftSeconds = Double.MaxValue;
                            }
                            else
                            {
                                leftSeconds = toCompute / totalSpeed;
                            }

                            startCountingTime = DateTime.Now;
                            if (!timerLaunched && summary.finishedAutomata.Count == summary.total)
                            {
                                materialLabel1.Text = "Total computation time: " + totalComputingTime.ToString();
                            }
                            else if (!timerLaunched && summary.finishedAutomata.Count < summary.total)
                            {
                                timer1.Tick += (o, e) =>
                                {
                                    materialLabel1.Text = "Total computation time: " + (totalComputingTime + (DateTime.Now - startCountingTime)).ToString();
                                };
                                timer1.Start();
                                timerLaunched = true;
                            }
                            else if (summary.finishedAutomata.Count == summary.total)
                            {
                                timer1.Stop();
                                timerLaunched = false;
                            }
                            materialLabel3.Text = $"Total speed: {totalSpeed:F2} automata per second.";
                            materialLabel2.Text = "Expected end of computation at: " + DateTime.Now.AddSeconds(leftSeconds).ToString();
                        }

                        var sortedLengths = new List<int>();
                        var sortedResults = new List<Tuple<int, string>>();

                        foreach (var a in summary.finishedAutomata)
                        {
                            if (a.solution.solvedB.Count > 0)
                            {
                                string b_tab, a_tab = byteTabToString(a.solution.unaryArray);
                                for (int i = 0; i < a.solution.solvedB.Count; i++)
                                {
                                    var b = a.solution.solvedB[i];
                                    b_tab = byteTabToString(b);
                                    sortedLengths.Add(-a.solution.solvedSyncLength[i]);
                                    sortedResults.Add(new Tuple<int, string>(a.solution.unaryIndex, $"[{a_tab},{b_tab}]"));
                                }
                            }
                        }

                        listOfAutomata.Items.Clear();
                        automataToLaunch.Clear();
                        var resultingArray = sortedResults.ToArray();
                        var resultingLengthsNegated = sortedLengths.ToArray();
                        Array.Sort(resultingLengthsNegated, resultingArray);
                        for (int i = 0; i < resultingArray.Length; i++)
                        {
                            automataToLaunch.Add(resultingArray[i].Item2);
                            listOfAutomata.Items.Add($"{resultingArray[i].Item2}, index = {resultingArray[i].Item1} - synchronizing length {-1 * resultingLengthsNegated[i]}");
                        }
                        var cernyLength = (summary.n - 1) * (summary.n - 1);
                        var cernyConjectureViolated = resultingLengthsNegated.Length > 0 && Math.Abs(resultingLengthsNegated[0]) > cernyLength;
                        var yetText = summary.finishedAutomata.Count < summary.total ? "yet " : string.Empty;
                        var cernyConjectureViolatedText = cernyConjectureViolated ? $"VIOLATED for n = {summary.n}!" : $"not {yetText}violated for n = {summary.n}";
                        labelAutomataCount.Text = $"There are {listOfAutomata.Items.Count} interesting automata.\nCerny conjecture is {cernyConjectureViolatedText}";
                    }));
                }
                );

            try
            {
                await connection.StartAsync();
                connectionAddress = connectionText;
            }
            catch (Exception e)
            {
            }
        }

        private double marginalTime = 1d / TimeSpan.FromDays(365).TotalSeconds;
        private double GetAverageSpeed(List<FinishedStatistics> statistics)
        {
            ExtractSpeeds(statistics, out var times, out var aps);
            var totalUsefulTime = TimeSpan.Zero;
            var totalUseful = 0d;
            for (int i = 0; i < aps.Count; i++)
            {
                if (aps[i] > marginalTime)
                {
                    totalUsefulTime += times[i];
                    totalUseful += times[i].TotalMilliseconds * aps[i];
                }
            }
            return totalUseful / totalUsefulTime.TotalMilliseconds;
        }

        private void ExtractSpeeds(List<FinishedStatistics> statistics, out List<TimeSpan> times, out List<double> automataPerSecond)
        {
            var timeEvents = new SortedDictionary<DateTime, double>();
            foreach (var statistic in statistics)
            {
                var start = statistic.solution.startTime;
                var finish = statistic.solution.finishTime;
                if (finish <= start)
                    continue;
                var durationSedonds = (finish - start).TotalSeconds;
                var aps = 1d / durationSedonds;

                if (timeEvents.ContainsKey(start))
                    timeEvents[start] += aps;
                else
                    timeEvents.Add(start, aps);

                if (timeEvents.ContainsKey(finish))
                    timeEvents[finish] -= aps;
                else
                    timeEvents.Add(finish, -aps);
            }

            times = new List<TimeSpan>();
            automataPerSecond = new List<double>();
            var lastTime = DateTime.MinValue;
            var lastSpeed = 0d;
            foreach (var timeEvent in timeEvents)
            {
                if (lastTime == DateTime.MinValue)
                {
                    lastTime = timeEvent.Key;
                    lastSpeed = timeEvent.Value;
                }
                else
                {
                    times.Add(timeEvent.Key - lastTime);
                    lastTime = timeEvent.Key;
                    automataPerSecond.Add(lastSpeed);
                    lastSpeed += timeEvent.Value;
                }
            }
        }

        private TimeSpan GetTotalComputationTime(List<FinishedStatistics> statistics)
        {
            ExtractSpeeds(statistics, out var times, out var aps);
            var totalTime = TimeSpan.Zero;
            for (int i = 0; i < aps.Count; i++)
            {
                if (aps[i] > marginalTime)
                    totalTime += times[i];
            }
            return totalTime;
        }

        private string byteTabToString(byte[] tab)
        {
            string s = tab[0].ToString();
            for (int i = 1; i < tab.Length; i++)
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
                addressBox.BackColor = Color.Empty;
                addressBox.ForeColor = Color.White;
            }
        }

        private void Visualize(bool powerGraph)
        {
            if (listOfAutomata.SelectedItem != null)
            {
                var automatonIndex = listOfAutomata.SelectedIndex;
                string automaton = automataToLaunch[automatonIndex];
                var powerGraphString = powerGraph ? '1' : '0';
                System.Diagnostics.Process.Start($"{addressBox.Text}/?automaton={automaton}&powerGraph={powerGraphString}");
            }
            else
            {
                string message = "You did not selected an automaton. Would you like to see the Cerny automaton?";
                string caption = "No automaton selected";
                MessageBoxButtons buttons = MessageBoxButtons.YesNo;
                DialogResult result;
                result = MessageBox.Show(message, caption, buttons);
                if (result == System.Windows.Forms.DialogResult.Yes)
                {
                    System.Diagnostics.Process.Start($"{addressBox.Text}/");
                }
            }
        }

        private void runVisualisationButton_Click(object sender, EventArgs e)
        {
            Visualize(false);
        }

        private void materialRaisedButton1_Click(object sender, EventArgs e)
        {
            Visualize(true);
        }
       
    }
}
