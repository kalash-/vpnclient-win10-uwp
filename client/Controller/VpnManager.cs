using System;
using System.Collections.Generic;
using System.IO;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Windows.Networking.Vpn;
using Windows.Security.Credentials;

namespace MPVPN
{
    class VpnManager
    {
        readonly VpnNativeProfile profile = new VpnNativeProfile()
        {
            ProfileName = ApplicationParameters.ConnectionName,
            NativeProtocolType = VpnNativeProtocolType.IpsecIkev2,
            AlwaysOn = true,
            UserAuthenticationMethod = VpnAuthenticationMethod.Eap,
            EapConfiguration = File.ReadAllText(Windows.ApplicationModel.Package.Current.InstalledLocation.Path + @"\Assets\profile.xml")
        };
        readonly VpnManagementAgent manager = new VpnManagementAgent();

        public async Task<VpnManagementConnectionStatus> GetVPNStatus()
        {
            var profiles = await manager.GetProfilesAsync();

            foreach (var profile in profiles)
            {
                if (profile is VpnNativeProfile nativeProfile && profile.ProfileName == ApplicationParameters.ConnectionName)
                {
                    return nativeProfile.ConnectionStatus;
                }
            }

            return VpnManagementConnectionStatus.Disconnected;
        }

        public List<string> GetVpnIPs()
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

        public async Task DoDisconnect()
        {
            VpnManagementErrorStatus profileStatus = await manager.DisconnectProfileAsync(profile);
            profileStatus = await manager.DeleteProfileAsync(profile);
        }

        public async Task DoConnect(Config.Server server)
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

    }
}
