using Snakes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using Google.Protobuf;
using static Snakes.GameMessage.Types;
using System.Threading;
using Snake.Converters;
using Snake.GameObjects;


namespace Snake.Model.Server
{
    internal class ServerNetworkInteraction
    {
        private UdpClient udpSocket;
        private IPEndPoint serverEndPoint;
        private IPEndPoint multicastEndPoint;

        private Dictionary<int, IPEndPoint> clients;

        private int nextPlayerId = 1;

        private GameModelOfServer gameModel;

        private GameParameters gameParameters;

        private Thread sendThread;

        public ServerNetworkInteraction(GameModelOfServer gameModel, GameParameters gameParameters)
        {
            clients = new Dictionary<int, IPEndPoint>();

            this.gameModel = gameModel;
            this.gameParameters = gameParameters;

            udpSocket = new UdpClient();

            serverEndPoint = new IPEndPoint(IPAddress.Any, 0);

            multicastEndPoint = new IPEndPoint(IPAddress.Parse("239.192.0.4"), 9192);

            udpSocket.ExclusiveAddressUse = false;
            udpSocket.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            udpSocket.Client.Bind(new IPEndPoint(IPAddress.Any, 0));
            udpSocket.JoinMulticastGroup(multicastEndPoint.Address);

            sendThread = new Thread(SendAnnouncementPeriodically);
            sendThread.Start();

        }

        public ServerNetworkInteraction(GameModelOfServer gameModel, GameParameters gameParameters, UdpClient udpSocket, Dictionary<int, IPEndPoint> allClients, int nextPlayerId)
        {
            this.clients = allClients;

            this.nextPlayerId = nextPlayerId;

            this.gameModel = gameModel;

            this.gameParameters = gameParameters;

            this.udpSocket = udpSocket;

            //nextPlayerId

            if (udpSocket == null || udpSocket.Client == null || !udpSocket.Client.IsBound)
            {
                throw new InvalidOperationException("Сокет не готов для использования сервером.");
            }

            Console.WriteLine($"Сокет привязан к: {udpSocket.Client.LocalEndPoint}");

            serverEndPoint = new IPEndPoint(IPAddress.Any, 0);

            multicastEndPoint = new IPEndPoint(IPAddress.Parse("239.192.0.4"), 9192);

            udpSocket.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            //udpSocket.Client.Bind(new IPEndPoint(IPAddress.Any, 0));
            udpSocket.JoinMulticastGroup(multicastEndPoint.Address);

            sendThread = new Thread(SendAnnouncementPeriodically);
            sendThread.Start();

        }

        public void Start()
        {
            while (true)
            {
                try
                {
                    byte[] data = udpSocket.Receive(ref serverEndPoint);
                    Console.WriteLine("--------------------------------------------------------");
                    HandleMessage(data, serverEndPoint);
                }
                catch (SocketException ex)
                {
                    Console.WriteLine($"SocketException: {ex.Message}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception: {ex.Message}");
                }

                if (gameModel.IsGameOver)
                {
                    break;
                }
            }
        }

        public void CloseSocket() {
            udpSocket.Close();
        }

        public IPEndPoint GetServerEndPoint() {
            return (IPEndPoint) udpSocket.Client.LocalEndPoint;

        }
        private void HandleMessage(byte[] data, IPEndPoint clientEndPoint)
        {
            try
            {
                var message = GameMessage.Parser.ParseFrom(data);

                switch (message.TypeCase)
                {
                    case GameMessage.TypeOneofCase.Join:
                        Console.WriteLine("Join received from client " + clientEndPoint.ToString());

                        HandleJoinMessage(message, clientEndPoint);
                        break;
                    case GameMessage.TypeOneofCase.Ping:
                        Console.WriteLine("Ping received from client " + clientEndPoint.ToString());

                        HandlePingMessage(message, clientEndPoint);
                        break;
                    case GameMessage.TypeOneofCase.Steer:
                        Console.WriteLine("Steer received from client " + clientEndPoint.ToString());

                        int playerId = -1;

                        foreach (var kvp in clients)
                        {
                            if (kvp.Value.Equals(clientEndPoint))
                            {
                                playerId = kvp.Key;
                                break;
                            }
                        }

                        if (playerId == -1)
                        {
                            Console.WriteLine("Unknown client endpoint");
                            return;
                        }

                        gameModel.MakePlayerAlive(FindIdByEndpoint(clientEndPoint));//тут ошибка не забудь

                        HandleSteerMessage(message.Steer, playerId);
                        break;
                    case GameMessage.TypeOneofCase.Discover:
                        HandleDiscoverMessage(data, clientEndPoint);
                        break;
                }
            }
            catch (SocketException ex)
            {
                Console.WriteLine($"SocketException in HandleMessage: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in HandleMessage: {ex.Message}");
            }
        }

        private void HandleJoinMessage(GameMessage msg, IPEndPoint clientEndPoint)
        {
            var joinMsg = msg.Join;
            int playerId = nextPlayerId++;
            clients.Add(playerId, clientEndPoint);
            var gamePlayer = new GamePlayer
            {
                Name = joinMsg.PlayerName,
                Id = playerId,
                Role = joinMsg.RequestedRole,
                Type = joinMsg.PlayerType,
                Score = 0
            };

            gameModel.AddNewSnake( gamePlayer.Id, gamePlayer.Name, gamePlayer.Role, clientEndPoint);

            SendAckMessage(clientEndPoint, msg.MsgSeq, playerId);
        }

        private void HandlePingMessage(GameMessage msg, IPEndPoint clientEndPoint)
        {

            gameModel.MakePlayerAlive(FindIdByEndpoint(clientEndPoint));

            SendAckMessage(clientEndPoint, msg.MsgSeq, -1);
        }

        private int FindIdByEndpoint(IPEndPoint endpoint)
        {
            // Поиск ключа по значению
            var pair = clients.FirstOrDefault(x => x.Value.Equals(endpoint));
            return pair.Key;
        }

        private void HandleSteerMessage(SteerMsg steerMsg, int id)
        {
            Console.WriteLine($"Received steer message with direction {steerMsg.Direction}");
            gameModel.ChangeDirectionSnakeById(steerMsg.Direction, id);
        }

        private void SendAckMessage(IPEndPoint clientEndPoint, long msgSeq, int playerId)
        {
            if (playerId != -1)
            {
                var ackMsg = new GameMessage
                {
                    Ack = new AckMsg(),
                    MsgSeq = msgSeq, // Используем тот же порядковый номер, что и в исходном сообщении
                    ReceiverId = playerId
                };

                byte[] data = ackMsg.ToByteArray();
                udpSocket.Send(data, data.Length, clientEndPoint);
            }
            else
            {
                var ackMsg = new GameMessage
                {
                    Ack = new AckMsg(),
                    MsgSeq = msgSeq,
                };

                byte[] data = ackMsg.ToByteArray();
                udpSocket.Send(data, data.Length, clientEndPoint);
            }
        }

        public void SendGameStatePeriodically(GameStateData data)
        {
            Thread thread = new Thread(() => 
            BroadcastMessage(GameStateCreator.CreateGameState(data)));
            thread.Start(); 
        }

        private void BroadcastMessage(GameState message)
        {
            var gameMessage = new GameMessage
            {
                State = new StateMsg
                {
                    State = message
                },
                MsgSeq = 1
            };
            byte[] data = gameMessage.ToByteArray();

            foreach (var client in clients.ToList())
            {
                try
                {
                    if (gameModel.Snakes[gameModel.Snakes.FindIndex(snake => snake.Id == client.Key)].SnakeState == GameState.Types.Snake.Types.SnakeState.Alive )
                    {
                        Console.WriteLine($"Sending message to {client.Value.Address}:{client.Value.Port}");
                        udpSocket.Send(data, data.Length, client.Value);
                    }
                }
                catch (SocketException ex)
                {
                    Console.WriteLine($"SocketException in BroadcastMessage: {ex.Message}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception in BroadcastMessage: {ex.Message}");
                }
            }
        }

        private void SendAnnouncementPeriodically()
        {
            while (true)
            {
                SendAnnouncement();
                Thread.Sleep(1000);
                if (gameModel.IsGameOver)
                {
                    break;
                }
            }
        }

        private void SendAnnouncement()
        {

            var gameMessage = new GameMessage
            {
                Announcement = AnnouncementBuilder.BuildAnnouncementMsg(gameParameters),
                MsgSeq = 1
            };

            byte[] data = gameMessage.ToByteArray();

            udpSocket.Send(data, data.Length, multicastEndPoint);
        }

        public void HandleDiscoverMessage(byte[] data, IPEndPoint clientEndPoint)
        {
            var message = GameMessage.Parser.ParseFrom(data);

            if (message.TypeCase == GameMessage.TypeOneofCase.Discover)
            {
                SendAnnouncement();
            }
        }
    }
}
