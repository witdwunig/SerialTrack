using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace server.Services
{
    public class UdpBroadcastService : BackgroundService
    {
        private const int Port = 5000;
        private const string Message = "SERIAL-SERVER|5000";
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var updClient = new UdpClient();
            updClient.EnableBroadcast = true;

            IPEndPoint broadcastEndPoint = new IPEndPoint(IPAddress.Broadcast, Port);

            while (!stoppingToken.IsCancellationRequested)
            {
                byte[] messageBytes = Encoding.UTF8.GetBytes(Message);
                await updClient.SendAsync(messageBytes, messageBytes.Length, broadcastEndPoint);

                await Task.Delay(3000, stoppingToken);
            }
        }   
    }
}
