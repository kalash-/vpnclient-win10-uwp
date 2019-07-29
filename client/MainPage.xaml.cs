using System.Collections.Generic;
using System.IO;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Net;
using MPVPN;
using Windows.Networking.Vpn;
using Windows.Security.Credentials;
using System.Threading.Tasks;
using System;
using System.Net.NetworkInformation;
using Windows.UI.Core;
using Windows.ApplicationModel.Core;

namespace client
{
    public sealed partial class MainPage : Page
    {
        private List<Config.Server> serversConfigLists;

        VpnNativeProfile profile = new VpnNativeProfile()
        {
            ProfileName = ApplicationParameters.ConnectionName,
            NativeProtocolType = VpnNativeProtocolType.IpsecIkev2,
            AlwaysOn = true,
            UserAuthenticationMethod = VpnAuthenticationMethod.Eap,
            EapConfiguration = File.ReadAllText("profile.xml")
        };
        VpnManagementAgent manager = new VpnManagementAgent();
        CoreApplicationView CurrentView = CoreApplication.GetCurrentView();

        public MainPage()
        {
            this.InitializeComponent();
            Microsoft.IdentityModel.Logging.IdentityModelEventSource.ShowPII = true;
        }

        private async Task<VpnManagementConnectionStatus> getVPNStatus()
        {
            var profiles = await manager.GetProfilesAsync();

            foreach(var profile in profiles)
            {
                var nativeProfile = profile as VpnNativeProfile;
                if (nativeProfile != null && profile.ProfileName == ApplicationParameters.ConnectionName)
                {
                    return nativeProfile.ConnectionStatus;
                }
            }

            return VpnManagementConnectionStatus.Disconnected;
        }

        private List<string> getVpnIPs()
        {
            List<string> ips = new List<string>();

            var interfaces = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface networkInterface in interfaces)
            {
                if (networkInterface.Name == ApplicationParameters.ConnectionName)
                {
                    foreach (var address in networkInterface.GetIPProperties().UnicastAddresses)
                    {
                        ips.Add(address.Address.ToString());
                    }

                    break;
                }
            }

            return ips;
        }

        private async Task doDisconnect()
        {
            VpnManagementErrorStatus profileStatus = await manager.DisconnectProfileAsync(profile);
            profileStatus = await manager.DeleteProfileAsync(profile);
        }
        private async Task doConnect(Config.Server server)
        {
            profile.Servers.Add(server.serverAddress);

            VpnManagementErrorStatus profileStatus = await manager.AddProfileFromObjectAsync(profile);

            PasswordCredential credentials = new PasswordCredential
            {
                UserName = server.eap_name,
                Password = server.eap_secret,
            };

            VpnManagementErrorStatus connectStatus = await manager.ConnectProfileWithPasswordCredentialAsync(profile, credentials);
        }

        async void OnLoad(object sender, RoutedEventArgs e)
        {
            await updateStatusText();

            var config = await GetConfig(GenerateToken());
            var token = new MPVPN.Config(config);

            serversConfigLists = token.servers;

            updateServersList(serversConfigLists);
        }

        public static string GenerateToken()
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor 
            {
                Subject = new ClaimsIdentity(),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes("jVnPSec9iu76t4e4")), SecurityAlgorithms.HmacSha256),
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);

            //return tokenHandler.WriteToken(token);
            return "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.e30.yqr1EtbahXKJhiE70GtcLzJRFMxLwFEg-tB1R3H1MyY";
        }

        public async Task<string> GetConfig(string token)
        {
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(ApplicationParameters.ConfigurationURL);
            httpWebRequest.Method = "GET";
            httpWebRequest.Headers["Authorization"] = "Bearer " + token;

            var data = await httpWebRequest.GetResponseAsync();

            using (HttpWebResponse response = (HttpWebResponse) data)
            using (Stream stream = response.GetResponseStream())   
            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        private void updateServersList(List<Config.Server> servers)
        {
            foreach (Config.Server server in servers)
            {
                ComboBoxItem itm = new ComboBoxItem();
                itm.Content = server.remoteIdentifier;

                serversList.Items.Add(itm);
            }
        }

        private async Task updateStatusText()
        {
            var status = await getVPNStatus();

            //todo: handle connecting and disconnecting statuses

            if (status == VpnManagementConnectionStatus.Connected)
            {
                connectButton.Content = "Disconnect";
            }
            else if(status == VpnManagementConnectionStatus.Disconnected)
            {
                connectButton.Content = "Connect";
            }

            var message = "VPN is " + status.ToString() + "\n";

            foreach (var ip in getVpnIPs())
            {
                message += ip + "\n";
            }

            ipsList.Text = message;
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            var status = await getVPNStatus();

            switch (status)
            {
                case VpnManagementConnectionStatus.Connected:
                    await doDisconnect().ContinueWith(async (t1) =>
                    {
                        await CurrentView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                        {
                            await updateStatusText();
                        });
                    });
                    break;

                case VpnManagementConnectionStatus.Disconnected:
                    if (serversList.SelectedIndex != -1)
                    {
                        await doDisconnect();
                        await doConnect(serversConfigLists[serversList.SelectedIndex]);
                        await CurrentView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                        {
                            await updateStatusText();
                        });
                    }
                    break;
            }
        }
    }
}
