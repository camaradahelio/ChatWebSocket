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

        public ConcurrentDictionary<string, WebSocket> Todos()
        {
            return _sockets;
        }

        public string PegarSocker(WebSocket socket)
        {
            return _sockets.FirstOrDefault(p => p.Value == socket).Key;
        }

        public WebSocket PegarSockerPorNickName(string nickName)
        {
            return _sockets.FirstOrDefault(p => p.Key == nickName).Value;
        }

        public void AdicionarSocket(string nickName, WebSocket socket)
        {
            lock (_sockets)
            {
                _sockets.TryAdd(CriarConexao(nickName), socket);
            }           
        }

        public async Task RemoverSocket(string id)
        {
            WebSocket socket;
            _sockets.TryRemove(id, out socket);

            await socket.CloseAsync(closeStatus: WebSocketCloseStatus.NormalClosure,
                                    statusDescription: "Conexão fechada pelo ChatSocketManager",
                                    cancellationToken: CancellationToken.None);
        }

        private string CriarConexao(string nickName)
        {
            return $"@{nickName}";
        }

    }
}
