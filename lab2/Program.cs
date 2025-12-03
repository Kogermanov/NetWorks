using System;

namespace SendingFiles
{
    class Program
    {
        static void Main(string[] args) 
        {
            if (args.Length == 0) 
            {
                Console.WriteLine ("We must choose mode");
                return;
            }
            else
            {
                if (args[0] == "server")
                {
                    Server server = new Server(int.Parse(args[1]));
                    server.Start();
                }
                else 
                {
                    Client client = new Client(args[2], int.Parse(args[3]));
                    client.SendFile(args[1]);
                }
            }
        }
    }
}
