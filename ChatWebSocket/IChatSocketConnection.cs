using System;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace ChatWebSocket
{
    public interface IChatSocketConnection
    {
        Task EntrarNaSala(string nickName, WebSocket socket);
    }
}