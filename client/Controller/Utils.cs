using JWT;
using JWT.Algorithms;
using JWT.Serializers;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace MPVPN
{
    class Utils
    {
        public static string GenerateToken()
        {
            IJwtEncoder encoder = new JwtEncoder(new HMACSHA256Algorithm(), new JsonNetSerializer(), new JwtBase64UrlEncoder());

            return encoder.Encode(new Dictionary<string, object>(), ApplicationParameters.Secret);
        }

        public static async Task<string> GetConfig(string token)
        {
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(ApplicationParameters.ConfigurationURL);
            httpWebRequest.Method = "GET";
            httpWebRequest.Headers["Authorization"] = "Bearer " + token;

            var data = await httpWebRequest.GetResponseAsync();

            using (HttpWebResponse response = (HttpWebResponse)data)
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                var config = reader.ReadToEnd();

                SecureStorage.ProtectAsync(config, ApplicationParameters.ConfigKey);

                return config;
            }
        }
    }
}
