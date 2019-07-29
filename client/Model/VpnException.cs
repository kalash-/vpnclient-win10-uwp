using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPVPN
{
    class VpnException : Exception
    {
        public VpnException(string message) : base(message)
        {
        }
    }
}
