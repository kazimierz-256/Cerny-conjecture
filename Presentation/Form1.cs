using CommunicationContracts;
using MaterialSkin;
using MaterialSkin.Controls;
using Microsoft.AspNetCore.SignalR.Client;
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
            if (!addressBox.Text.Equals(connectionAddress))
            {
                addressBox.BackColor = Color.OrangeRed;
                if (connection != null && connection.State == HubConnectionState.Connected)
                {
                    await connection.StopAsync();
                }
                connection = new HubConnectionBuilder()
                    .WithUrl(addressBox.Text)
                    .Build();
                connection.On(
                    "ShowSimpleTextStatistics",
                    (ServerPresentationComputationSummary resultingTextStatistics) =>
                    {
                        Invoke(new Action(() =>
                        {
                            logBox.Text = $"{resultingTextStatistics.finishedAutomata.Sum(s => s.solution.solvedB.Count)} total interesting.";
                            chart1.Series["UnaryFinishedSeries"].Points.Clear();
                            chart1.Series["UnaryFinishedSeries"].Points.AddXY("Pozostało", resultingTextStatistics.total - resultingTextStatistics.finishedAutomata.Count);
                            chart1.Series["UnaryFinishedSeries"].Points.AddXY("Wykonano", resultingTextStatistics.finishedAutomata.Count);


                            if (resultingTextStatistics.finishedAutomata.Count > 0)
                            {
                                var totalSeconds = (resultingTextStatistics.finishedAutomata.Max(r => r.finishTime) - resultingTextStatistics.finishedAutomata.Min(r => r.issueTime)).TotalSeconds;
                                int totalMili = (int)(totalSeconds * 1000) - (int)totalSeconds*1000;
                                var avgSeconds = resultingTextStatistics.finishedAutomata.Count / totalSeconds;
                                int avgMili = (int)(avgSeconds * 1000) - (int)avgSeconds*1000;

                                logBox.Text += resultingTextStatistics.description + $" Total speed: {avgSeconds:F2}";

                                materialLabel1.Text = "Całkowity czas obliczeń: " + (new TimeSpan(0, 0, 0, (int)totalSeconds, totalMili)).ToString();
                                materialLabel3.Text = "Średni czas analizy jednego automatu unarnego: " + (new TimeSpan(0, 0, 0, (int)avgSeconds, avgMili)).ToString();
                            }
                        }));
                    }
                    );
                addressBox.BackColor = Color.YellowGreen;
                try
                {
                    await connection.StartAsync();
                }
                catch (Exception e)
                {

                }
            }
        }

        private async void close_Click(object sender, EventArgs e)
        {
            if (connection != null)
            {
                await connection.StopAsync();
                addressBox.BackColor = Color.OrangeRed;
            }
        }

    }
}
