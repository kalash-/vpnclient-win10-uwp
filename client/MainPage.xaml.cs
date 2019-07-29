﻿using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using MPVPN;
using Windows.Networking.Vpn;
using System.Threading.Tasks;
using System;
using Windows.UI.Core;
using Windows.ApplicationModel.Core;
using Windows.UI.Popups;

namespace client
{
    public sealed partial class MainPage : Page
    {
        List<Config.Server> serversConfigLists;
        readonly CoreApplicationView CurrentView = CoreApplication.GetCurrentView();
        readonly VpnManager vpnManager = new VpnManager();

        public MainPage()
        {
            this.InitializeComponent();
            Microsoft.IdentityModel.Logging.IdentityModelEventSource.ShowPII = true;
        }

        async void OnLoad(object sender, RoutedEventArgs e)
        {
            await UpdateStatusText();

            var configText = await Utils.GetConfig(Utils.GenerateToken());
            var config = new MPVPN.Config(configText);

            serversConfigLists = config.servers;

            UpdateServersList(serversConfigLists);
        }

        private void UpdateServersList(List<Config.Server> servers)
        {
            foreach (Config.Server server in servers)
            {
                ComboBoxItem itm = new ComboBoxItem
                {
                    Content = server.remoteIdentifier
                };

                serversList.Items.Add(itm);
            }
        }

        private async Task UpdateStatusText()
        {
            await CurrentView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                var status = await vpnManager.GetVPNStatus();

                //todo: handle connecting and disconnecting statuses

                if (status == VpnManagementConnectionStatus.Connected)
                {
                    serversList.IsEnabled = false;
                    connectButton.Content = "Disconnect";
                }
                else if (status == VpnManagementConnectionStatus.Disconnected)
                {
                    serversList.IsEnabled = true;
                    connectButton.Content = "Connect";
                }

                var message = "VPN is " + status.ToString() + "\n";

                foreach (var ip in vpnManager.GetVpnIPs())
                {
                    message += ip + "\n";
                }

                ipsList.Text = message;
            });
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            var status = await vpnManager.GetVPNStatus();

            switch (status)
            {
                case VpnManagementConnectionStatus.Connected:
                    await vpnManager.DoDisconnect().ContinueWith(async (t1) =>
                    {
                        await UpdateStatusText();
                    });
                    break;

                case VpnManagementConnectionStatus.Disconnected:
                    if (serversList.SelectedIndex != -1)
                    {
                        await vpnManager.DoDisconnect();
                        await vpnManager.DoConnect(serversConfigLists[serversList.SelectedIndex]);
                        await UpdateStatusText();
                    }
                    break;
            }
        }

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var storedData = await SecureStorage.UnprotectDataAsync(ApplicationParameters.ConfigKey);
            var messageDialog = new MessageDialog(storedData);

            messageDialog.Commands.Add(new UICommand("Close"));
            await messageDialog.ShowAsync();            
        }
    }
}
