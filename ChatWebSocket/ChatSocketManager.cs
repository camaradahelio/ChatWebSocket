using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace ChatWebSocket
{
    public class ChatSocketManager
    {
        private ConcurrentDictionary<string, WebSocket> _sockets = new ConcurrentDictionary<string, WebSocket>();

        public ConcurrentDictionary<string, WebSocket> GetAll()
        {
            return _sockets;
        }

        public string GetId(WebSocket socket)
        {
            return _sockets.FirstOrDefault(p => p.Value == socket).Key;
        }

        public WebSocket GetSocketById(string id)
        {
            return _sockets.FirstOrDefault(p => p.Key == id).Value;
        }

        public void AddSocket(string userId, WebSocket socket)
        {
            lock (_sockets)
            {
                _sockets.TryAdd(CreateConnectionId(userId), socket);
            }           
        }

        public async Task RemoveSocket(string id)
        {
            WebSocket socket;
            _sockets.TryRemove(id, out socket);

            await socket.CloseAsync(closeStatus: WebSocketCloseStatus.NormalClosure,
                                    statusDescription: "Closed by the ChatSocketManager",
                                    cancellationToken: CancellationToken.None);
        }

        private string CreateConnectionId(string userId)
        {
            return $"@{userId}";
        }

    }
}
