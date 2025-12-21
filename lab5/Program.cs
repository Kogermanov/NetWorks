
namespace SOCKCSProxy
{

    class Program
    {   
        public static async Task Main(string[] args) 
        {
            ProxyServer server = new ProxyServer();
            await server.StartServer();
        }
    }
}