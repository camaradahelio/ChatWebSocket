using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChatWebSocket
{
    public class ChatSocketConnection : IChatSocketConnection
    {
        protected ChatSocketManager _chatSocketManager = new ChatSocketManager();

        public async Task Connect(string nickName, WebSocket socket)
        {            
            await SendMessageToAllAsync($"User with id <b>{nickName}</b> has joined the chat");

            await this.OnConnected(nickName, socket);

            await Receive(socket, async (result, buffer) =>
            {
                if (result.MessageType == WebSocketMessageType.Text)
                {
                    await this.ReceiveAsync(socket, result, buffer);
                    return;
                }

                else if (result.MessageType == WebSocketMessageType.Close)
                {
                    await this.OnDisconnected(socket);
                    return;
                }
            });     
        }

        private async Task OnConnected(string userId, WebSocket socket)
        {
            _chatSocketManager.AddSocket(userId, socket);
        }

        private async Task OnDisconnected(WebSocket socket)
        {
            await _chatSocketManager.RemoveSocket(_chatSocketManager.GetId(socket));
        }

        private async Task SendMessageAsync(WebSocket socket, string message)
        {
            if (socket.State != WebSocketState.Open)
                return;

            await socket.SendAsync(buffer: new ArraySegment<byte>(
                    array: Encoding.ASCII.GetBytes(message),
                    offset: 0,
                    count: message.Length),
                    messageType: WebSocketMessageType.Text,
                    endOfMessage: true,
                    cancellationToken: CancellationToken.None);
        }

        private async Task SendMessageAsync(string socketId, string message)
        {
            await SendMessageAsync(_chatSocketManager.GetSocketById(socketId), message);
        }

        private async Task SendMessageToAllAsync(string message)
        {
            foreach (var pair in _chatSocketManager.GetAll())
            {
                if (pair.Value.State == WebSocketState.Open)
                    await SendMessageAsync(pair.Value, message);
            }
        }

        private async Task ReceiveAsync(WebSocket socket, WebSocketReceiveResult result, byte[] buffer)
        {
            var socketId = _chatSocketManager.GetId(socket);
            var message = $"{socketId} said: {Encoding.UTF8.GetString(buffer, 0, result.Count)}";

            await SendMessageToAllAsync(message);
        }

        private async Task Receive(WebSocket socket, Action<WebSocketReceiveResult, byte[]> handleMessage)
        {
            var buffer = new byte[1024 * 4];

            while (socket.State == WebSocketState.Open)
            {
                var result = await socket.ReceiveAsync(buffer: new ArraySegment<byte>(buffer),
                  cancellationToken: CancellationToken.None);

                handleMessage(result, buffer);
            }
        }
    }
}
