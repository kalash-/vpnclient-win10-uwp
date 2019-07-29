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
            VpnManagementErrorStatus status = await manager.DisconnectProfileAsync(profile);
            if (status != VpnManagementErrorStatus.Ok)
            {
                throw new Exception("Disconnect failed. Status is " + status);
            }

            status = await manager.DeleteProfileAsync(profile);
            if (status != VpnManagementErrorStatus.Ok)
            {
                throw new Exception("VPN profile delete failed. Status is " + status);
            }
        }

        public async Task DoConnect(Config.Server server)
        {
            profile.Servers.Add(server.serverAddress);

            VpnManagementErrorStatus status = await manager.AddProfileFromObjectAsync(profile);
            if (status != VpnManagementErrorStatus.Ok)
            {
                throw new Exception("VPN profile delete failed. Status is " + status);
            }

            PasswordCredential credentials = new PasswordCredential
            {
                UserName = server.eap_name,
                Password = server.eap_secret,
            };

            status = await manager.ConnectProfileWithPasswordCredentialAsync(profile, credentials);
            if (status != VpnManagementErrorStatus.Ok)
            {
                throw new Exception("VPN profile delete failed. Status is " + status);
            }
        }
    }
}
