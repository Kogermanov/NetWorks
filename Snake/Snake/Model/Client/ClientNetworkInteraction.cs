using Snakes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using Google.Protobuf;
using System.Threading.Tasks;
using static Snakes.GameMessage.Types;
using System.Threading;
using Snake.Model.Client;
using System.Windows.Forms;
using System.Runtime.Remoting.Messaging;
using System.Security.Cryptography;

namespace Snake.Model.Client
{
    internal class ClientNetworkInteraction
    {
        private UdpClient udpSocket;
        private IPEndPoint serverEndPoint;
        private int playerId;
        private int MsgSeq = 0;
        private int delay = 300;

        // Поле для хранения сообщений
        private Dictionary<long, GameMessage> messageStorage = new Dictionary<long, GameMessage>();

        private GameModelOfClient gameModel;

        // Для синхронизации доступа к общим ресурсам messageStorage
        private readonly object _lock = new object();

        // Ограничение количества попыток подключения
        private int joinAttempts = 0;
        private const int maxJoinAttempts = 3;

        public ClientNetworkInteraction(GameModelOfClient gameModel, IPEndPoint serverEndPoint)
        {
            this.gameModel = gameModel;
            delay = gameModel.gameParameters.Delay;
            this.serverEndPoint = serverEndPoint;
        }

        public void SetNewServerEndPoint(IPEndPoint serverEndPoint)
        {
            this.serverEndPoint = serverEndPoint;
        }

        public IPEndPoint GetServerEndPoint()
        {
            return this.serverEndPoint;
        }
        public async Task StartAsync()
        {
            udpSocket = new UdpClient();

            udpSocket.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

            await SendJoinMessageAsync();

            _ = ReceiveMessagesAsync();

            _ = CheckingNeedPing();

            _ = CheckingMasterLife();
        }

        public void StartAsyncWithoutJoining(IPEndPoint iPEndPoint)
        {
            udpSocket = new UdpClient(iPEndPoint);

            udpSocket.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

            _ = ReceiveMessagesAsync();

            _ = CheckingNeedPing();

            _ = CheckingMasterLife();
        }

        public UdpClient GetSocket() { 
            return udpSocket;
        }

        private async Task CheckingMasterLife()
        {
            while (true)
            {
                int sizeMessageStorage = messageStorage.Count();
                DateTime startTime = DateTime.Now;
                while ((DateTime.Now - startTime).TotalMilliseconds < delay * 2)
                {
                    await Task.Delay(10);
                }
                if (sizeMessageStorage == messageStorage.Count())
                {
                    gameModel.MasterIsDead();
                    //break;
                }
                if (gameModel.IsGameOver)
                {
                    udpSocket.Close();
                    break;
                }

            }
        }
        private async Task CheckingNeedPing()
        {

            while (true)
            {

                DateTime startTime = DateTime.Now;
                while ((DateTime.Now - startTime).TotalMilliseconds < delay * 0.8)
                {
                    await Task.Delay(10);
                }
                if (gameModel.IsGameOver)
                {
                    udpSocket.Close();
                    break;
                }
                // Отправляем Ping сообщение, если не было отправлено других сообщений
                await SendPingMessageAsync();
            }
        }

        private async Task SendPingMessageAsync()
        {
            int newMsgSeq = this.MsgSeq++;

            var pingMSG = new PingMsg();
            var gameMessage = new GameMessage
            {
                Ping = pingMSG,
                MsgSeq = newMsgSeq,
            };

            byte[] data = gameMessage.ToByteArray();
            await udpSocket.SendAsync(data, data.Length, serverEndPoint);
        }

        private async Task SendJoinMessageAsync()
        {
            int newMsgSeq = this.MsgSeq++;
            var joinMsg = new JoinMsg
            {
                PlayerType = PlayerType.Human,
                PlayerName = "Player" + (new Random()).Next(1, 101),
                GameName = "Game",
                RequestedRole = NodeRole.Normal
            };

            var gameMessage = new GameMessage
            {
                Join = joinMsg,
                MsgSeq = newMsgSeq,
            };

            byte[] data = gameMessage.ToByteArray();
            await udpSocket.SendAsync(data, data.Length, serverEndPoint);

            WaitForJoinMessageWithTimeout(newMsgSeq);
        }

        public async void WaitForJoinMessageWithTimeout(int newMsgSeq)
        {
            DateTime startTime = DateTime.Now;

            while ((DateTime.Now - startTime).TotalMilliseconds < delay)
            {
                var msg = await FindMessageBySeq(newMsgSeq);
                if (msg != null)
                {
                    Console.WriteLine("Get Ack message");
                    gameModel.SetYourSnakeId(msg.ReceiverId);
                    return;
                }
                await Task.Delay(25);
            }

            if (joinAttempts < maxJoinAttempts)
            {
                joinAttempts++;
                await SendJoinMessageAsync();
            }
            else
            {
                Console.WriteLine("Failed to join after multiple attempts.");
            }
        }

        private async Task ReceiveMessagesAsync()
        {
            while (true)
            {
                if (gameModel.IsGameOver)
                {
                    udpSocket.Close();
                    break;
                }
                try
                {
                    UdpReceiveResult result = await udpSocket.ReceiveAsync();
                    byte[] data = result.Buffer;
                    Console.WriteLine($"Received message from {result.RemoteEndPoint.Address}:{result.RemoteEndPoint.Port}");

                    var message = GameMessage.Parser.ParseFrom(data);

                    switch (message.TypeCase)
                    {
                        case GameMessage.TypeOneofCase.State:
                            Console.WriteLine("Received State message");
                            HandleStateMessage(message.State);
                            break;
                        case GameMessage.TypeOneofCase.Ack:
                            AddMessageToStorage(message);
                            Console.WriteLine("Received Ack message");
                            break;
                        case GameMessage.TypeOneofCase.Error:
                            AddMessageToStorage(message);
                            Console.WriteLine("Received Error message");
                            break;
                        default:
                            Console.WriteLine($"Received unknown message type: {message.TypeCase}");
                            break;
                    }
                }
                catch (SocketException ex)
                {
                    Console.WriteLine($"SocketException in ReceiveMessagesAsync: {ex.Message}");
                    await Task.Delay(delay); // Добавляем задержку перед повторной попыткой
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception in ReceiveMessagesAsync: {ex.Message}");
                    await Task.Delay(delay); // Добавляем задержку перед повторной попыткой
                }
            }
        }

        public async Task SendSteerMessageAsync(Direction direction)
        {
            try
            {
                var steerMsg = new SteerMsg
                {
                    Direction = direction
                };

                var gameMessage = new GameMessage
                {
                    Steer = steerMsg,
                    MsgSeq = this.MsgSeq++,
                    SenderId = playerId
                };

                byte[] data = gameMessage.ToByteArray();
                await udpSocket.SendAsync(data, data.Length, serverEndPoint);
            }
            catch (SocketException ex)
            {
                Console.WriteLine($"SocketException in SendSteerMessageAsync: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in SendSteerMessageAsync: {ex.Message}");
            }
        }

        private void HandleStateMessage(StateMsg stateMsg)
        {
            gameModel.UpdateClientModel(GameStateParser.ParseGameState(stateMsg.State));
        }

        private void AddMessageToStorage(GameMessage message)
        {
            lock (_lock)
            {
                messageStorage[message.MsgSeq] = message;
            }
        }

        private async Task<GameMessage> FindMessageBySeq(long msgSeq)
        {
            lock (_lock)
            {
                if (messageStorage.TryGetValue(msgSeq, out var message))
                {
                    return message;
                }
                return null;
            }
        }

        private void RemoveMessageBySeq(long msgSeq)
        {
            lock (_lock)
            {
                messageStorage.Remove(msgSeq);
            }
        }
    }
}