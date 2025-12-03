using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Net.NetworkInformation;

namespace org.cchmc.{{cookiecutter.namespace}}.auth
{
    public static class IpAddressHelper
    {
        public static string IpAddress(HttpContext context)
        {
            // Taken from https://jasonwatmore.com/post/2020/05/25/aspnet-core-3-api-jwt-authentication-with-refresh-tokens#refresh-token-cs
            //  but its unclear if the x-forwarded-for is really necessary
            if (!context.Request.Headers.TryGetValue("X-Forwarded-For", out var result))
            {
                result = context.Connection.RemoteIpAddress.MapToIPv4().ToString();
                if (result == "0.0.0.1" || result == "127.0.0.1")
                    result = GetVPNConnectionIPAddress() ?? result;
            }
            return result;
        }

        private static string GetVPNConnectionIPAddress()
        {
            try
            {
                //** Get all network adapters
                var adapters = NetworkInterface.GetAllNetworkInterfaces().ToList();
                //** Get our cicso anyconnect adapter (VPN) - "cisco" for windows, "utun3" for mac, "vmxnet3" for remote connections
                var adapterMatches = new[] { "cisco", "utun3", "vmxnet3" };
                var adapter = adapters.FirstOrDefault(x => adapterMatches.Any(a => x.Description.Contains(a, StringComparison.CurrentCultureIgnoreCase)));

                var matchingIps = adapter.GetIPProperties().UnicastAddresses.Where(p => p.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork).ToList();
                if (matchingIps.Count != 0) return matchingIps.First().Address.ToString();
            }
            catch (Exception)
            {

            }
            return "";
        }
    }
}
