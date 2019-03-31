using System.Net;

namespace SimpleIpc.Shared
{
    /// <summary>
    /// A static class that provides default network configuration.
    /// </summary>
    internal static class NetworkDefaults
    {
        internal const int DefaultPort = 13001;

        internal static IPAddress LoopbackIPAddress
        {
            get
            {
                IPHostEntry ipHostInfo = Dns.GetHostEntry("localhost");
                IPAddress ipAddress = ipHostInfo.AddressList[0];
                return ipAddress;
            }
        }
    }
}