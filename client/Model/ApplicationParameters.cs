using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPVPN
{
    static public class ApplicationParameters
    {
        static public string ConfigKey => "mpvpnconfig";
        static public string ConnectionName = "mpvpn";
        static public string ConfigurationURL = "http://159.65.72.139.sslip.io/api/list.json";
    }
}
