using System.Net;
using System.Net.Sockets;

namespace UdpMulticastProgram
{
    public class Printer
    {
        static TimeSpan expirationTime = TimeSpan.FromSeconds(5);
        public static void PrintNewIP (Dictionary<IPEndPoint, DateTime> activeCopies, IPEndPoint newIP) {
            Console.WriteLine($"Обнаружена новая копия: {newIP.ToString()}");
            lock (activeCopies)
            {
                if (activeCopies.Count > 0)
                {
                    Console.WriteLine("Активные копии:");
                    foreach (var copy in activeCopies)
                    {
                        Console.WriteLine($"{copy.Key}");
                    }
                    Console.WriteLine();
                }
            }
        }

        public static void PrintRemoveIP (Dictionary<IPEndPoint, DateTime> activeCopies) {
            lock (activeCopies) {
                DateTime now = DateTime.Now;
                List<string> expiredCopies = new List<string>();
                foreach (var copy in activeCopies) {
                    if (now - copy.Value > expirationTime) {
                        activeCopies.Remove(copy.Key);
                        Console.WriteLine($"Копия {copy.Key} отключилась.");
                        if (activeCopies.Count > 0) {
                            Console.WriteLine("Активные копии:");
                            foreach (var nCopy in activeCopies) {
                                Console.WriteLine($"{nCopy.Key}");
                            }
                            Console.WriteLine();
                        }
                    }
                }
            }
        } 
    }  
}
