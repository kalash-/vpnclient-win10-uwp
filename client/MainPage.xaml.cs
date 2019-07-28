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

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace client
{
    public sealed partial class MainPage : Page
    {
        private List<Config.Server> serversConfigLists;

        public MainPage()
        {
            this.InitializeComponent();
            Microsoft.IdentityModel.Logging.IdentityModelEventSource.ShowPII = true;
        }

        private async Task yo(Config.Server server)
        {
            VpnManagementAgent mgr = new VpnManagementAgent();

            VpnNativeProfile profile = new VpnNativeProfile()
            {
                ProfileName = server.eap_name,
                NativeProtocolType = VpnNativeProtocolType.IpsecIkev2,
                AlwaysOn = true,
                UserAuthenticationMethod = VpnAuthenticationMethod.Eap,
                EapConfiguration = File.ReadAllText("profile.xml")
        };

            profile.Servers.Add(server.serverAddress);

            VpnManagementErrorStatus profileStatus = await mgr.AddProfileFromObjectAsync(profile);

            PasswordCredential credentials = new PasswordCredential
            {
                UserName = server.eap_name,
                Password = server.eap_secret,
            };

            VpnManagementErrorStatus connectStatus = await mgr.ConnectProfileWithPasswordCredentialAsync(profile, credentials);
        }

        void OnLoad(object sender, RoutedEventArgs e)
        {
            var t = new MPVPN.Config(GetConfig(GenerateToken()));

            serversConfigLists = t.servers;

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

        public string GetConfig(string token)
        {
            var httpWebRequest = (HttpWebRequest)WebRequest.Create("http://159.65.72.139.sslip.io/api/list.json");
            httpWebRequest.Method = "GET";
            httpWebRequest.Headers["Authorization"] = "Bearer " + token;

            var responce = httpWebRequest.GetResponseAsync();
            responce.Wait();

            using (HttpWebResponse response = (HttpWebResponse) responce.Result)
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
                itm.Content = server.eap_name;

                serversList.Items.Add(itm);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (serversList.SelectedIndex != -1)
            {
                yo(serversConfigLists[serversList.SelectedIndex]);
            }
        }
    }
}
