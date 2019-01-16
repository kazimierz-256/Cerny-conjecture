using CommunicationContracts;
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
            if (connection != null && connection.State == HubConnectionState.Connected)
            {
                addressBox.BackColor = Color.YellowGreen;
                await connection.InvokeAsync("SubscribeAndSendStatistics");
            }
        }

        private async void addressBox_TextChanged(object sender, EventArgs e) => await RefreshConnection();
        private List<string> automataToLaunch = new List<string>();
        private async Task RefreshConnection()
        {
            var connectionText = $"{addressBox.Text}/ua";
            {
                //addressBox.BackColor = Color.OrangeRed;
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
                            int toCompute = resultingTextStatistics.total - resultingTextStatistics.finishedAutomata.Count;
                            chart1.Series["UnaryFinishedSeries"].Points.Clear();
                            chart1.Series["UnaryFinishedSeries"].Points.AddXY("To compute", toCompute);
                            chart1.Series["UnaryFinishedSeries"].Points.AddXY("Computed", resultingTextStatistics.finishedAutomata.Count);

                            if (resultingTextStatistics.finishedAutomata.Count > 0)
                            {
                                chart1.Titles[0].Text = $"Unary Automata with n = {resultingTextStatistics.finishedAutomata[0].solution.unaryArray.Length}";
                                var totalComputingTime = GetTotalComputationTime(resultingTextStatistics.finishedAutomata);
                                var totalSpeed = GetAverageSpeed(resultingTextStatistics.finishedAutomata);
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
                                materialLabel1.Text = "Total computation time: " + totalComputingTime.ToString();
                                materialLabel3.Text = $"Total speed: {totalSpeed:F2} automata per second.";
                                materialLabel2.Text = "Expected end of computation at: " + DateTime.Now.AddSeconds(leftSeconds).ToString();
                            }

                            var sortedLengths = new List<int>();
                            var sortedResults = new List<string>();
                            //var results = 
                            foreach (var a in resultingTextStatistics.finishedAutomata)
                            {
                                if (a.solution.solvedB.Count > 0)
                                {
                                    string b_tab, a_tab = byteTabToString(a.solution.unaryArray);
                                    for (int i = 0; i < a.solution.solvedB.Count; i++)
                                    {
                                        var b = a.solution.solvedB[i];
                                        b_tab = byteTabToString(b);
                                        sortedLengths.Add(-a.solution.solvedSyncLength[i]);
                                        sortedResults.Add($"[{a_tab},{b_tab}]");
                                    }
                                }
                            }

                            listOfAutomata.Items.Clear();
                            automataToLaunch.Clear();
                            var resultingArray = sortedResults.ToArray();
                            var resultingLengths = sortedLengths.ToArray();
                            Array.Sort(resultingLengths, resultingArray);
                            for (int i = 0; i < resultingArray.Length; i++)
                            {
                                automataToLaunch.Add(resultingArray[i]);
                                listOfAutomata.Items.Add($"{resultingArray[i]} - synchronizing length {-resultingLengths[i]}");
                            }
                            labelAutomataCount.Text = $"There are {listOfAutomata.Items.Count} interesting automata.";
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
                //addressBox.BackColor = Color.OrangeRed;
            }
        }

        private void runVisualisationButton_Click(object sender, EventArgs e)
        {
            if (listOfAutomata.SelectedItem != null)
            {
                var automatonIndex = listOfAutomata.SelectedIndex;
                string automaton = automataToLaunch[automatonIndex];
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
