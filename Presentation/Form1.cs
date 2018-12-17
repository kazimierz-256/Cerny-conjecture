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
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            RefreshConnection();
        }

        HubConnection connection = null;
        private string connectionAddress = string.Empty;

        private async void prompt_Click(object sender, EventArgs e)
        {
            if (connection != null)
            {
                await RefreshConnection();
                await connection.InvokeAsync("SendStatistics");
            }
        }

        private async void addressBox_TextChanged(object sender, EventArgs e) => await RefreshConnection();

        private async Task RefreshConnection()
        {
            if (!addressBox.Text.Equals(connectionAddress))
            {
                addressBox.BackColor = Color.Yellow;
                if (connection != null && connection.State == HubConnectionState.Connected)
                {
                    await connection.StopAsync();
                }
                connection = new HubConnectionBuilder()
                    .WithUrl(addressBox.Text)
                    .Build();
                await connection.StartAsync();
                connection.On(
                    "ShowSimpleTextStatistics",
                    (string resultingTextStatistics) =>
                    {
                        Invoke(new Action(() =>
                        {
                            logBox.Text = resultingTextStatistics;
                        }));
                    }
                    );
                addressBox.BackColor = Color.LawnGreen;
            }
        }

        private async void close_Click(object sender, EventArgs e)
        {
            await connection.StopAsync();
            addressBox.BackColor = Color.Yellow;
        }
    }
}
