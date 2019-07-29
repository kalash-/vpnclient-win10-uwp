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
        static public async void ProtectAsync(string value, string name)
        {
            DataProtectionProvider Provider = new DataProtectionProvider("LOCAL=user");

            var buffMsg = CryptographicBuffer.ConvertStringToBinary(value, BinaryStringEncoding.Utf8);

            var buffProtected = await Provider.ProtectAsync(buffMsg);

            var result = CryptographicBuffer.EncodeToBase64String(buffProtected);

            localSettings.Values[name] = result;
        }

        static public async Task<string> UnprotectDataAsync(string name)
        {
            DataProtectionProvider Provider = new DataProtectionProvider();

            var value = localSettings.Values[name];

            var buffer = CryptographicBuffer.DecodeFromBase64String(value as string);

            IBuffer buffUnprotected = await Provider.UnprotectAsync(buffer);

            return CryptographicBuffer.ConvertBinaryToString(BinaryStringEncoding.Utf8, buffUnprotected);
        }
    }
}
