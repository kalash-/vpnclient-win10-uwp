using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace MPVPN
{
    class Utils
    {
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
                return reader.ReadToEnd();
            }
        }
    }
}
