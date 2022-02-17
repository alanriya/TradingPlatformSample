using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace chat
{
    public class ExchangeConnection
    {
        public byte[] data = new byte[1024];
        public string input, stringData;
        public TcpClient client;
        public int recv;
        public NetworkStream ns;

        public ExchangeConnection()
        {
            try
            {
                client = new TcpClient("127.0.0.1", 9050);
                Console.WriteLine("Connected to exchange via TCP connection.");
            }
            catch (SocketException)
            {
                Console.WriteLine("Unable to connect to server");
                return;
            }
            ns = client.GetStream();
            recv = ns.Read(data, 0, data.Length);
            stringData = Encoding.ASCII.GetString(data, 0, recv);
            Console.WriteLine(stringData);
        }

        public void submitOrder(ref tradeRecord _tradeRecord)
        {
            string orderString = $"{_tradeRecord.orderNumber},{_tradeRecord.price},{_tradeRecord.volume},{_tradeRecord.side}";
            ns.Write(Encoding.ASCII.GetBytes(orderString), 0, orderString.Length);
            ns.Flush();
            data = new byte[1024];
            recv = ns.Read(data, 0, data.Length);
            stringData = Encoding.ASCII.GetString(data, 0, recv);
            Console.WriteLine($"exchange replied with order filled: {stringData}");

        }

        public void closeExchangeConnection()
        {
            ns.Close();
            client.Close();
            Console.WriteLine("Connection to Exchange has closed.");
        }
    }
}
