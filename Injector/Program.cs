using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Injector
{
    class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length == 0) {
                Console.WriteLine("Please provide the filename to process.");
                return;
            }

            if (args.Length==1)
            {
                int recv;
                byte[] data = new byte[1024];
                Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                IPEndPoint sender = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 5000);
                EndPoint Remote = (EndPoint)sender;
                data = new byte[1024];
                string filePath = $"{args[0]}";
                int seqNumber = 1;
                List<String> lines = File.ReadAllLines(filePath).ToList();
                Console.WriteLine("Reading and sending messages...");
                foreach (string line in lines) {
                    string newLine = $"{line} {seqNumber}";
                    server.SendTo(Encoding.ASCII.GetBytes(newLine), Remote);
                    data = new byte[1024];
                    recv = server.ReceiveFrom(data, ref Remote);
                    seqNumber++;
                }
                Console.WriteLine("Stopping injector client");
                server.Close();
            }

        }
    }
}