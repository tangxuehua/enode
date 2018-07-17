using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace ENode.EQueue.Utils
{
    public class SocketUtils
    {
        public static IPEndPoint GetIPEndPointFromHostName(string hostName, int port, AddressFamily? addressFamily = AddressFamily.InterNetwork, bool throwIfMoreThanOneIP = true)
        {
            var ipAddresses = Dns.GetHostAddresses(hostName);

            if (addressFamily.HasValue)
            {
                ipAddresses = ipAddresses
                    .Where(a => a.AddressFamily == addressFamily.Value)
                    .ToArray();
            }

            if (ipAddresses.Length == 0)
            {
                throw new ArgumentException(
                    "Unable to retrieve address from specified host name.",
                    "hostName"
                );
            }
            else if (throwIfMoreThanOneIP && ipAddresses.Length > 1)
            {
                throw new ArgumentException(
                   "There is more that one IP address to the specified host.",
                   "hostName"
               );
            }

            return new IPEndPoint(ipAddresses.FirstOrDefault(), port);
        }

        public static IPAddress GetLocalIPV4()
        {
            var networkTypes = new List<NetworkInterfaceType>()
            {
                NetworkInterfaceType.Ethernet,
                NetworkInterfaceType.Wireless80211
            };
            var hasGatewayNetworkInterfaces = NetworkInterface.GetAllNetworkInterfaces()
                .Where(ni => ni.GetIPProperties().GatewayAddresses.Count > 0);
            var ethernetNetworkInterfaces = hasGatewayNetworkInterfaces
                .Where(ni => networkTypes.Contains(ni.NetworkInterfaceType));

            return ethernetNetworkInterfaces
                .Select(ni => ni.GetIPProperties().UnicastAddresses.First(x => x.Address.AddressFamily == AddressFamily.InterNetwork).Address)
                .First();
        }
    }
}