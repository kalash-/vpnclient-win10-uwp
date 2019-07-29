using System;
using System.Threading.Tasks;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.DataProtection;
using Windows.Storage.Streams;

namespace MPVPN
{
    class SecureStorage
    {
        static readonly private Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
        static public async void ProtectAsync(String strMsg)
        {
            DataProtectionProvider Provider = new DataProtectionProvider("LOCAL=user");

            var buffMsg = CryptographicBuffer.ConvertStringToBinary(strMsg, BinaryStringEncoding.Utf8);

            var buffProtected = await Provider.ProtectAsync(buffMsg);

            var result = CryptographicBuffer.EncodeToBase64String(buffProtected);

            localSettings.Values[ApplicationParameters.ConfigKey] = result;
        }

        static public async Task<String> UnprotectDataAsync()
        {
            DataProtectionProvider Provider = new DataProtectionProvider();

            var data = localSettings.Values[ApplicationParameters.ConfigKey];

            var buffer = CryptographicBuffer.DecodeFromBase64String(data as string);

            IBuffer buffUnprotected = await Provider.UnprotectAsync(buffer);

            return CryptographicBuffer.ConvertBinaryToString(BinaryStringEncoding.Utf8, buffUnprotected);
        }
    }
}
