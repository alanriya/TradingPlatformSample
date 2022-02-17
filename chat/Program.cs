using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Text;
using System.Timers;


namespace chat
{
    public class Program
    {
        static void Main(string[] args)
        {
            string filePath = @"C:\Users\Alan_\source\CSharpLessons\chat\dataset\SCH.log";
            int seqNumber = 1;
            List<string> lines = File.ReadAllLines(filePath).ToList();
            MessageParser parser = new MessageParser();
            OrderBook orderbook = new OrderBook();
            foreach (string line in lines)
            {
                OrderInfo test = parser.processMessage($"{line} {seqNumber}");
                orderbook.processOrder(ref test);
                seqNumber++;
            }
        }
    }

    public class udper {
        static void Main(string[] args) {
            // UDP server
            int recv;
            byte[] data = new byte[1024];
            IPEndPoint ipep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 5000);
            Socket newsock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            newsock.Bind(ipep);
            IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
            EndPoint Remote = (EndPoint)(sender);
            // message parser and orderbook
            Dictionary<String, OrderBook> orderbooks = new Dictionary<String, OrderBook> ();
            Dictionary<String, Strategy> strategies = new Dictionary<String, Strategy>();
            loadStrategyConfig(ref strategies);
            MessageParser parser = new MessageParser();
            ExchangeConnection exchangeConnection = new ExchangeConnection();
            TimeSpan endTime = new TimeSpan(11, 49, 59);
            while (true)
            {
                data = new byte[1024];
                recv = newsock.ReceiveFrom(data, ref Remote);
                
                String stringData = System.Text.Encoding.UTF8.GetString(data);
                // record the tick-to-trade time.
                if (stringData == "exit") {
                    newsock.SendTo(data, recv, SocketFlags.None, Remote);
                    break;
                }
                OrderInfo marketData = parser.processMessage(stringData);
                if (!orderbooks.ContainsKey(marketData.symbol))
                {
                    orderbooks[marketData.symbol] = new OrderBook();
                }
                orderbooks[marketData.symbol].processOrder(ref marketData);
                // update strategy
                Strategy currentStrategy = strategies[marketData.symbol];
                currentStrategy.updateData(ref marketData);

                // run the strategy for signal, should return the tradeRecord.
                if (currentStrategy.triggerCondition(ref marketData))
                {
                    OrderBook orderbook = orderbooks[marketData.symbol];
                    tradeRecord t = currentStrategy.enterTrade(ref orderbook, ref marketData);
                    exchangeConnection.submitOrder(ref t);
                }
                // udp socket reply
                newsock.SendTo(data, recv, SocketFlags.None, Remote);
                if (marketData.epochTime.TimeOfDay > endTime) {
                    break;
                }

            }

            foreach (KeyValuePair<String , Strategy> entry in strategies) {
                if (orderbooks.ContainsKey(entry.Key)){
                    Strategy currentStrategy = strategies[entry.Key];
                    OrderBook currentBook = orderbooks[entry.Key];
                    tradeRecord t = currentStrategy.exitTrade(ref currentBook, entry.Key);
                    exchangeConnection.submitOrder(ref t);
                }
            }

            exchangeConnection.closeExchangeConnection();
            // generate pnl report.
            foreach (KeyValuePair<String, Strategy> entry in strategies)
            {
                if (orderbooks.ContainsKey(entry.Key))
                {
                    Strategy currentStrategy = strategies[entry.Key];
                    printReport(ref currentStrategy, entry.Key);
                }
            }
        }

        static void printReport(ref Strategy strategy, string symbol) {
            Console.WriteLine($"--- PnL report for {symbol} ---");
            double pnl = 0.0;
            int totalCnt = 0;
            foreach (tradeRecord tr in strategy.records) {
                Console.WriteLine($"{tr.orderNumber}, {tr.price}, {tr.volume}, {tr.side}");
                int multiplier = tr.side == 0 ? -1 : 1;
                pnl += multiplier * tr.volume * tr.price;
                totalCnt++;
            }

            Console.WriteLine($"profit and loss: {pnl}, count: {totalCnt}"); 
        }

        static void loadStrategyConfig( ref Dictionary<String, Strategy> strategies) {
            // set the parameter.
            strategies.Add("SCH", new Strategy(106.70));
            strategies.Add("SCS", new Strategy(272.20));
        }
    }

    public class tcper {
        static void Main(string[] args) {
            byte[] data = new byte[1024];
            string input, stringData;
            TcpClient server;
            try
            {
                server = new TcpClient("127.0.0.1", 9050);
            }
            catch (SocketException)
            {
                Console.WriteLine("Unable to connect to server");
                return;
            }
            NetworkStream ns = server.GetStream();
            int recv = ns.Read(data, 0, data.Length);
            stringData = Encoding.ASCII.GetString(data, 0, recv);
            Console.WriteLine(stringData);
            while (true)
            {
                input = Console.ReadLine();
                if (input == "exit")
                    break;
                ns.Write(Encoding.ASCII.GetBytes(input), 0, input.Length);
                ns.Flush();
                data = new byte[1024];
                recv = ns.Read(data, 0, data.Length);
                stringData = Encoding.ASCII.GetString(data, 0, recv);
                Console.WriteLine(stringData);
            }
            Console.WriteLine("Disconnecting from server...");
            ns.Close();
            server.Close();
        }
    }
}
