using System;
using System.Threading.Tasks;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.DataProtection;
using Windows.Storage.Streams;

namespace MPVPN
{
    class SecureStorage
    {
        readonly private Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

        public async void ProtectAsync(String strMsg)
        {
            DataProtectionProvider Provider = new DataProtectionProvider("LOCAL=user");

            IBuffer buffMsg = CryptographicBuffer.ConvertStringToBinary(strMsg, BinaryStringEncoding.Utf8);

            IBuffer buffProtected = await Provider.ProtectAsync(buffMsg);

            localSettings.Values[ApplicationParameters.ConfigKey] = buffProtected;
        }

        public async Task<String> UnprotectDataAsync()
        {
            DataProtectionProvider Provider = new DataProtectionProvider();

            IBuffer buffUnprotected = await Provider.UnprotectAsync(localSettings.Values[ApplicationParameters.ConfigKey] as IBuffer);

            return CryptographicBuffer.ConvertBinaryToString(BinaryStringEncoding.Utf8, buffUnprotected);
        }
    }
}
