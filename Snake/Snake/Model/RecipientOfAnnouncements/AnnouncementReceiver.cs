using Snakes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Snakes.GameMessage.Types;

namespace Snake.Model.RecipientOfAnnouncements
{

    public class AnnouncementReceivedEventArgs : EventArgs
    {
        public IPEndPoint EndPoint { get; }
        public AnnouncementMsg AnnouncementMsg { get; }

        public AnnouncementReceivedEventArgs(IPEndPoint endPoint, AnnouncementMsg announcementMsg)
        {
            EndPoint = endPoint;
            AnnouncementMsg = announcementMsg;
        }
    }

    public class AnnouncementReceiver
    {
        public event EventHandler<AnnouncementReceivedEventArgs> AnnouncementReceived;

        private UdpClient udpClient;
        private IPEndPoint multicastEndPoint;
        private Thread receiveThread;

        private bool IsPortAvailable(int port)
        {
            try
            {
                udpClient.Client.Bind(new IPEndPoint(IPAddress.Any, port));
                return true;
            }
            catch (SocketException)
            {
                return false;
            }
        }

        public AnnouncementReceiver()
        {
            udpClient = new UdpClient();
            multicastEndPoint = new IPEndPoint(IPAddress.Parse("239.192.0.4"), 9192);//ВЕРНУТЬ
            udpClient.ExclusiveAddressUse = false;
            udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            udpClient.Client.Bind(new IPEndPoint(IPAddress.Any, 9192));//ВЕРНУТЬ

            //if (!IsPortAvailable(12345))
            //{
            //    if (!IsPortAvailable(1234))
            //    {
            //        if (!IsPortAvailable(12346))
            //        {
            //            //ok
            //        }
            //    }
            //}
            udpClient.JoinMulticastGroup(multicastEndPoint.Address);//ВЕРНУТЬ

            receiveThread = new Thread(ReceiveAnnouncements);
            receiveThread.Start();
        }

        private void ReceiveAnnouncements()
        {
            while (true)
            {
                try
                {
                    byte[] data = udpClient.Receive(ref multicastEndPoint);
                    HandleAnnouncementMessage(data, multicastEndPoint);
                }
                catch (SocketException ex)
                {
                    Console.WriteLine($"SocketException in ReceiveAnnouncements: {ex.Message}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception in ReceiveAnnouncements: {ex.Message}");
                }
            }
        }

        private void HandleAnnouncementMessage(byte[] data, IPEndPoint endPoint)
        {
            var message = GameMessage.Parser.ParseFrom(data);

            if (message.TypeCase == GameMessage.TypeOneofCase.Announcement)
            {
                AnnouncementReceived?.Invoke(this, new AnnouncementReceivedEventArgs(endPoint, message.Announcement));
            }
        }
    }
}
