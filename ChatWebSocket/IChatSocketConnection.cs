using System;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace ChatWebSocket
{
    public interface IChatSocketConnection
    {
        Task Connect(string nickName, WebSocket socket);
    }
}