using Microsoft.AspNetCore.Http;
using System.Net;
using System.Net.Sockets;

namespace StudentProj.Common
{
    public static class IpHelper
    {
        public static string GetClientIpAddress(HttpContext context)
        {
            var ip = context.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
            if (ip == "::1" || ip == "127.0.0.1")
            {
                try
                {
                    var host = Dns.GetHostEntry(Dns.GetHostName());
                    foreach (var address in host.AddressList)
                    {
                        if (address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            return address.ToString();
                        }
                    }
                }
                catch { }
            }
            return ip;
        }
    }
}
