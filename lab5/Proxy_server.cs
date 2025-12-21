using System.Net.Sockets;
using System.Net;

namespace SOCKCSProxy
{
    public class ProxyServer
    {
        private const int TransferBufferSize = 1024 * 8;
        private const int ProxyPort = 1080;  
        TcpListener listener;

        public ProxyServer()
        {
            listener = new TcpListener(IPAddress.Any, ProxyPort);
        }

        public async Task StartServer()
        {
            listener.Start();
            Console.WriteLine($"SOCKS5 proxy server started on port {ProxyPort}");

            while (true)
            {
                TcpClient client = await listener.AcceptTcpClientAsync();
                _ = HandleClientAsync(client);
            }
        }
        
        private async Task HandleClientAsync(TcpClient client)
        {
            var stream = client.GetStream();

            byte[] buffer = new byte[1024];
            int readBytes = await stream.ReadAsync(buffer, 0, buffer.Length);
            
            if (readBytes == 0 || buffer[0] != (byte)Bytes.Version) 
            {
                Console.WriteLine("It is not SOCKS5");
                return;
            }

            Console.WriteLine("Begin protocol");

            await Messages.SendWithoutAuthentication(stream);

            await stream.ReadAsync(buffer, 0, buffer.Length);

            if (readBytes == 0 || buffer[0] != (byte)Bytes.Version)
            {
                Console.WriteLine("It is not SOCKS5");
                return;
            }

            if (buffer[1] != (byte)Bytes.Connect) 
            {
                Console.WriteLine("Unknown or unrealized comand");
                return;
            }

            Console.WriteLine("Receive tcp connection");

            IPAddress? destinationIP = null;
            int destinationPort = 0;

            try
            {
                (destinationIP, destinationPort) = await AddressParser.ParseAddress(buffer);
            }
            catch (UnknownAddressException ex)
            {
                Console.WriteLine("Unknown address");
                await Messages.SendMessage(stream, Bytes.UnknownAddress);
                client.Close();
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DNS resolution failed {ex.Message}");
                await Messages.SendMessage(stream, Bytes.UnavailableHost);
                client.Close();
                return;
            }

            Console.WriteLine($"Get ip and port {destinationIP}");

            try
            {
                TcpClient remoteClient = new TcpClient();
                await remoteClient.ConnectAsync(destinationIP, destinationPort);
                
                await Messages.SendMessage(stream, Bytes.Accept);

                _ = Task.WhenAll(
                        TransferStream(client.GetStream(), remoteClient.GetStream()), 
                        TransferStream(remoteClient.GetStream(), client.GetStream()));
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unknown address type");
                Messages.SendMessage(stream, Bytes.UnknownAddress);
                client.Close();
                return;
            }
        }

        private static async Task TransferStream(NetworkStream input, NetworkStream output)
        {
            byte[] buffer = new byte[TransferBufferSize];
            int length = 0;
            while((length = await input.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                await output.WriteAsync(buffer, 0, length);
                Console.WriteLine("Transfer bytes");
            }
        }
    }
}