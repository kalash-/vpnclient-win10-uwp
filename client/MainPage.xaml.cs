using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using MPVPN;
using Windows.Networking.Vpn;
using System.Threading.Tasks;
using System;
using Windows.UI.Core;
using Windows.ApplicationModel.Core;

namespace client
{
    public sealed partial class MainPage : Page
    {
        private List<Config.Server> serversConfigLists;
        readonly CoreApplicationView CurrentView = CoreApplication.GetCurrentView();
        VpnManager vpnManager = new VpnManager();

        public MainPage()
        {
            this.InitializeComponent();
            Microsoft.IdentityModel.Logging.IdentityModelEventSource.ShowPII = true;
        }

        async void OnLoad(object sender, RoutedEventArgs e)
        {
            await updateStatusText();

            var config = await Utils.GetConfig(Utils.GenerateToken());
            var token = new MPVPN.Config(config);

            serversConfigLists = token.servers;

            updateServersList(serversConfigLists);
        }

        private void updateServersList(List<Config.Server> servers)
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

        private async Task updateStatusText()
        {
            await CurrentView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                var status = await vpnManager.GetVPNStatus();

                //todo: handle connecting and disconnecting statuses

                if (status == VpnManagementConnectionStatus.Connected)
                {
                    connectButton.Content = "Disconnect";
                }
                else if (status == VpnManagementConnectionStatus.Disconnected)
                {
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
                        await updateStatusText();
                    });
                    break;

                case VpnManagementConnectionStatus.Disconnected:
                    if (serversList.SelectedIndex != -1)
                    {
                        await vpnManager.DoDisconnect();
                        await vpnManager.DoConnect(serversConfigLists[serversList.SelectedIndex]);                        
                        await updateStatusText();
                    }
                    break;
            }
        }

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var yo = await SecureStorage.UnprotectDataAsync();
        }
    }
}
