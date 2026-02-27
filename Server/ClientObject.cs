using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class ClientObject
    {
        public Guid Id { get; } = Guid.NewGuid();
        public TcpClient Client { get; }
        public NetworkStream Stream { get; }
        public string? Nickname { get; set; }
        public GameRoom? CurrentRoom { get; set; } = null;
        public ClientObject(TcpClient client)
        { 
            Client = client;
            Stream = client.GetStream();
        }
        public async Task SendMessageAsync(string msg)
        {
            if (!Client.Connected)
            {
                return;
            }
            byte[] data = Encoding.UTF8.GetBytes(msg);
            await Stream.WriteAsync(data, 0, data.Length);
        }
    }
}
