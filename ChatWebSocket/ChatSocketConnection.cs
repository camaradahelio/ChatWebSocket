using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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

        public async Task EntrarNaSala(string nickName, WebSocket socket)
        {
            await EnviarMensagemParaTodosAsync($"Usuário {nickName} entrou para o bate papo!");

            await this.Connectar(nickName, socket);

            await RecebendoMensagem(socket, async (result, buffer) =>
            {
                if (result.MessageType == WebSocketMessageType.Text)
                {
                    await this.RecebendoMensagemAsync(socket, result, buffer);
                    return;
                }

                else if (result.MessageType == WebSocketMessageType.Close)
                {
                    await this.Desconectar(socket);
                    return;
                }
            });
        }

        private async Task Connectar(string userId, WebSocket socket)
        {
            _chatSocketManager.AdicionarSocket(userId, socket);
        }

        private async Task Desconectar(WebSocket socket)
        {
            await _chatSocketManager.RemoverSocket(_chatSocketManager.PegarSocker(socket));
        }

        private async Task EnviandoMensagemAsync(WebSocket socket, string mensagem)
        {
            if (socket.State != WebSocketState.Open)
                return;

            await socket.SendAsync(buffer: new ArraySegment<byte>(
                    array: Encoding.UTF8.GetBytes(mensagem),
                    offset: 0,
                    count: mensagem.Length),
                    messageType: WebSocketMessageType.Text,
                    endOfMessage: true,
                    cancellationToken: CancellationToken.None);
        }

        private async Task EnviandoMensagemAsync(string nickName, string mensagem)
        {
            await EnviandoMensagemAsync(_chatSocketManager.PegarSockerPorNickName(nickName), mensagem);
        }

        private async Task EnviarMensagemParaTodosAsync(string mensagem)
        {
            foreach (var pair in _chatSocketManager.Todos())
            {
                if (pair.Value.State == WebSocketState.Open)
                    await EnviandoMensagemAsync(pair.Value, mensagem);
            }
        }

        private async Task RecebendoMensagemAsync(WebSocket socket, WebSocketReceiveResult result, byte[] buffer)
        {
            var nickname = _chatSocketManager.PegarSocker(socket);

            var stringMensagem = Encoding.UTF8.GetString(buffer, 0, result.Count);
            MensagemChatModel chatModel = JsonConvert.DeserializeObject<MensagemChatModel>(stringMensagem);

            string mensagem = string.Empty;

            if (string.IsNullOrEmpty(chatModel.Destinatario))
            {
                mensagem = $"{nickname} diz: {chatModel.Mensagem}";
                await EnviarMensagemParaTodosAsync(mensagem);
            }
            else
            {
                mensagem = $"{nickname} diz para {chatModel.Destinatario}: {chatModel.Mensagem} ";
                await EnviandoMensagemAsync(chatModel.Destinatario, mensagem);
            }
        }

        private async Task RecebendoMensagem(WebSocket socket, Action<WebSocketReceiveResult, byte[]> handleMessage)
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
