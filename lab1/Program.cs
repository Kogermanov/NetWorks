using System.Net;
using System;

namespace UdpMulticastProgram
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Start program");

            string? multicastAddress = Console.ReadLine();

            IPAddress? multicastIP;
            if (!IPAddress.TryParse(multicastAddress, out multicastIP))
            {
                Console.WriteLine("bad format multicast address.");
                return;
            }

            UdpMulticast socket = new UdpMulticast(multicastIP);
            socket.SearchMulticastCopies();
        }
    }

}
