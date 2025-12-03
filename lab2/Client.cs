using System.Net.Sockets;
using System;
using System.Text;

namespace SendingFiles 
{
    public class Client : NetworkWorker
    {
        string host;
        int port;
        TcpClient client;
        public Client (string host, int port)
        {
            this.host = host;
            this.port = port;
            client = new TcpClient();
        }

        public void SendFile (string file) 
        {
            try
            {
                Console.WriteLine ($"connect to {host}:{port}");
                client.Connect(host, port);
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.Message);
                return;
            }
            NetworkStream stream = client.GetStream();

            byte[] message = Encoding.UTF8.GetBytes(file);

            stream.Write(message, 0, message.Length);
            
            byte[] answer = new byte[ACCEPT];
            stream.Read(answer);

            FileInfo fileInfo = new FileInfo(file);
            byte[] len = Encoding.UTF8.GetBytes(fileInfo.Length.ToString());
            Console.WriteLine(fileInfo.Length.ToString());

            stream.Write(len, 0, len.Length);
            stream.Read(answer, 0, answer.Length);
            
            int file_size = (int)fileInfo.Length;
            int count = 0;

            FileStream fileStream = new FileStream(file, FileMode.Open);
            byte[] buffer = new byte[SIZE_DATA];
            while (file_size > 0) 
            {
                count = fileStream.Read(buffer, 0, buffer.Length);
                stream.Write(buffer, 0, count);
                file_size -= count;
            }
            
            fileStream.Close();
            stream.Close();
            client.Close();

            Console.WriteLine("flash " + fileInfo.Length.ToString() + " bytes");
        }
    }
}