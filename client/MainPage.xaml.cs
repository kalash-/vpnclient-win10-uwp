using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using MPVPN;
using Windows.Networking.Vpn;
using System.Threading.Tasks;
using System;
using Windows.UI.Core;
using Windows.ApplicationModel.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml.Media.Imaging;

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
#if DEBUG
            Microsoft.IdentityModel.Logging.IdentityModelEventSource.ShowPII = true; //set extender exception informations
#endif
        }

        async void OnLoad(object sender, RoutedEventArgs e)
        {
            await UpdateStatusText();

            try
            {
                var configText = await Utils.GetConfig(Utils.GenerateToken());
                var config = new MPVPN.Config(configText);

                serversConfigLists = config.servers;

                UpdateServersList(serversConfigLists);
            }

            catch(System.Exception ex)
            {
                await UpdateStatusText(ex.Message);
            }
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

        private async Task UpdateStatusText(string text = "")
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

                var message = text + "\nVPN is " + status.ToString() + "\n";

                foreach (var ip in vpnManager.GetVpnIPs())
                {
                    message += ip + "\n";
                }

                ipsList.Text = message;
            });
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                progress.IsActive = true;

                var status = await vpnManager.GetVPNStatus();

                switch (status)
                {
                    case VpnManagementConnectionStatus.Connected:
                        await vpnManager.DoDisconnect();
                        await UpdateStatusText();

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
            catch (Exception ex)
            {
                await UpdateStatusText(ex.Message);
            }
            finally
            {
                progress.IsActive = false;
            }
        }

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                var storedData = await SecureStorage.UnprotectDataAsync(ApplicationParameters.ConfigKey);
                var messageDialog = new MessageDialog(storedData);

                messageDialog.Commands.Add(new UICommand("Close"));
                await messageDialog.ShowAsync();
            }

            catch(Exception ex)
            {
                await UpdateStatusText(ex.Message);
            }
        }

        private void ServersList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var server = serversConfigLists[serversList.SelectedIndex];

            BitmapImage image = new BitmapImage(new Uri("https://www.countryflags.io/" + server.country.Substring(0, 2) + "/flat/64.png"));

            flag_image.Source = image;
        }
    }
}
