using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;

namespace numeryseryjneWPF.Services
{
    internal class ServerDiscoveryService
    {
        public async Task<string?> DiscoverServerAsync(int timeoutMs = 3000)
        {
            UdpClient? udpClient = null;
            try
            {
                udpClient = new UdpClient(5000);
                udpClient.Client.ReceiveTimeout = timeoutMs;
                var result = await udpClient.ReceiveAsync();

                string message = Encoding.UTF8.GetString(result.Buffer);

                if (message.StartsWith("SERIAL-SERVER"))
                {
                    string[] parts = message.Split("|");
                    string serverIp = result.RemoteEndPoint.Address.ToString();
                    string port = parts.Length > 1 ? parts[1] : "5000";
                    Debug.WriteLine($"{serverIp} {port}");
                    return $"http://{serverIp}:{port}";
                }
            } catch (SocketException ex)
            {
                Debug.WriteLine($"UDP PORT 5000 unaviaiable {ex.Message}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error podczas szukania servera: {ex.Message}");
            } finally
            {
                udpClient?.Close();
            }
            return null;
        }
    }
}
