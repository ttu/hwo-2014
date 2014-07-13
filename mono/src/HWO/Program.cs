using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace HWO
{
    public class Program
    {
        public static void Main(string[] args)
        {
            string host = args[0];
            int port = int.Parse(args[1]);
            string botName = args[2];
            string botKey = args[3];

            Console.WriteLine("Connecting to " + host + ":" + port + " as " + botName + "/" + botKey);

            using (var client = new TcpClient(host, port))
            {
                NetworkStream stream = client.GetStream();
                var reader = new StreamReader(stream);
                var writer = new StreamWriter(stream);
                writer.AutoFlush = true;

                var bot = new SimpleBot(reader, writer, botName, botKey);
                bot.Start();

                //System.Diagnostics.Debugger.Break();
            }
        }
    }
}
