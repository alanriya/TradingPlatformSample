using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace chat
{
    public class OrderBook
    {
        public SortedDictionary<double, Limit> buyTree = new SortedDictionary<double, Limit>();
        public SortedDictionary<double, Limit> sellTree = new SortedDictionary<double, Limit>();
        public Limit highestBuy = new Limit();
        public Limit lowestSell = new Limit();
        public Dictionary<string, Order> ordersInBook = new Dictionary<string, Order>();

        public void processOrder(ref OrderInfo marketData)
        {
            if (marketData.orderCategory == "NEW" && marketData.side == "SELL")
            {
                addOrder(ref marketData, ref sellTree, ref lowestSell);
            }
            else if (marketData.orderCategory == "CANCEL" && marketData.side == "SELL")
            {
                cancelOrder(ref marketData, ref sellTree, ref lowestSell);
            }
            else if (marketData.orderCategory == "TRADE" && marketData.side == "SELL")
            {
                tradeOrder(ref marketData, ref sellTree, ref lowestSell);
            }
            else if (marketData.orderCategory == "NEW" && marketData.side == "BUY")
            {
                addOrder(ref marketData, ref buyTree, ref highestBuy);
            }
            else if (marketData.orderCategory == "CANCEL" && marketData.side == "BUY")
            {
                cancelOrder(ref marketData, ref buyTree, ref highestBuy);
            }
            else if (marketData.orderCategory == "TRADE" && marketData.side == "BUY")
            {
                tradeOrder(ref marketData, ref buyTree, ref highestBuy);
            }
            //Console.WriteLine("Buy Side: ");
            //printTree(ref buyTree);
            //Console.WriteLine("Sell Side: ");
            //printTree(ref sellTree);
            //Console.WriteLine($"{marketData.epochTime}: {highestBuy.limitPrice},{highestBuy.totalVolume} | {lowestSell.limitPrice}, {lowestSell.totalVolume}");

            
        }

        public void printTree(ref SortedDictionary<double, Limit> sideTree) {
            foreach (KeyValuePair<double, Limit> pair in sideTree) {
                Console.Write($"{pair.Key} ");
                Order order = pair.Value.headOrder;
                while (order!= null) {
                    Console.Write($"{order.idNumber} ");
                    order = order.nextOrder;
                    
                }
                Console.WriteLine();
            }
        }

        public void addOrder(ref OrderInfo marketData, ref SortedDictionary<double, Limit> sideTree, ref Limit bboTree)
        {

            Order currentOrder = new Order(
                    marketData.orderId,
                    marketData.side == "BUY" ? 0 : 1,
                    marketData.volume,
                    marketData.price,
                    marketData.epochTime
                );

            if (sideTree.ContainsKey(currentOrder.price))
            {
                // key is in the SortedDict
                sideTree[currentOrder.price].size++;
                sideTree[currentOrder.price].totalVolume += currentOrder.volume;
                sideTree[currentOrder.price].setTailTo(ref currentOrder);
            }
            else
            {
                // key is not in the SortedDict.
                Limit newLimit = new Limit(currentOrder.price, 1, currentOrder.volume);
                newLimit.setTailTo(ref currentOrder);
                sideTree.Add(currentOrder.price, newLimit);
            }
            if (marketData.side == "BUY")
            {
                getMaxValue(ref sideTree, ref bboTree);
            }
            else {
                getMinValue(ref sideTree, ref bboTree);
            }
            ordersInBook[marketData.orderId] = currentOrder;
        }

        public void cancelOrder(ref OrderInfo marketData, ref SortedDictionary<double, Limit> sideTree, ref Limit bboTree) {
            bool keyInDictionary = ordersInBook.ContainsKey(marketData.orderId);
            if (keyInDictionary) {
                Order orderbookOrder = ordersInBook[marketData.orderId];
                Limit currentLimit = sideTree[marketData.price];
                currentLimit.size--;
                // deleting the order from the tree.
                if (currentLimit.size == 0)
                {
                    currentLimit.headOrder = null;
                    currentLimit.tailOrder = null;
                    sideTree.Remove(marketData.price);
                }
                else {
                    if (currentLimit.headOrder == orderbookOrder)
                    {
                        // Console.WriteLine("order to be deleted is head order and size greater than 0");
                        orderbookOrder.nextOrder.previousOrder = null;
                        currentLimit.headOrder = orderbookOrder.nextOrder;
                        orderbookOrder.nextOrder = null;
                        currentLimit.totalVolume -= orderbookOrder.volume;
                    }
                    else if (currentLimit.tailOrder == orderbookOrder)
                    {
                        // Console.WriteLine("order to be deleted is tail order and size greater than 0");
                        orderbookOrder.previousOrder.nextOrder = null;
                        currentLimit.tailOrder = orderbookOrder.previousOrder;
                        orderbookOrder.previousOrder = null;
                        currentLimit.totalVolume -= orderbookOrder.volume;
                    }
                    else
                    {
                        // Console.WriteLine("order to be deleted is in-between order and size greater 0");
                        orderbookOrder.previousOrder.nextOrder = orderbookOrder.nextOrder;
                        orderbookOrder.nextOrder.previousOrder = orderbookOrder.previousOrder;
                        orderbookOrder.nextOrder = null;
                        orderbookOrder.previousOrder = null;
                        currentLimit.totalVolume -= orderbookOrder.volume;
                    }
                }
                // update the bbo. 
                if (marketData.side == "BUY")
                {
                    getMaxValue(ref sideTree, ref bboTree);
                }
                else
                {
                    getMinValue(ref sideTree, ref bboTree);
                }
                // remove the order in the orderbook.
                ordersInBook.Remove(orderbookOrder.idNumber);

            }
        }

        public void tradeOrder(ref OrderInfo marketData, ref SortedDictionary<double, Limit> sideTree, ref Limit bboTree)
        {
            if (ordersInBook.ContainsKey(marketData.orderId))
            {
                if (marketData.volume == ordersInBook[marketData.orderId].volume)
                {
                    cancelOrder(ref marketData, ref sideTree, ref bboTree);
                }
                else
                {
                    ordersInBook[marketData.orderId].volume -= marketData.volume;
                    sideTree[marketData.price].totalVolume -= marketData.volume;
                    Order currentOrder = sideTree[marketData.price].headOrder;

                    if (marketData.side == "BUY")
                    {
                        getMaxValue(ref sideTree, ref bboTree);
                    }
                    else
                    {
                        getMinValue(ref sideTree, ref bboTree);
                    }
                }
            }
        }

        public void getMaxValue(ref SortedDictionary<double, Limit> sideTree, ref Limit bboTree) {
            double highestLevel = sideTree.Keys.Max();
            bboTree = sideTree[highestLevel];
        }

        public void getMinValue(ref SortedDictionary<double, Limit> sideTree, ref Limit bboTree)
        {
            double lowestLevel = sideTree.Keys.Min();
            bboTree = sideTree[lowestLevel];
        }

    }
    public class Limit {

        public double limitPrice;
        public int size;
        public int totalVolume;
        public Order? headOrder = null;
        public Order? tailOrder = null;

        public Limit() {
            size = 0;
        }
        public Limit(double _limitPrice, int _size, int _totalVolume) {
            limitPrice = _limitPrice;
            size = _size;
            totalVolume = _totalVolume; 
        }

        public void setTailTo(ref Order order) {
            if (tailOrder == order)
            {
                return;
            }
            else if (tailOrder == null)
            {
                headOrder = order;
                tailOrder = order;
            }
            else if (headOrder == tailOrder)
            {
                headOrder.nextOrder = order;
                tailOrder = order;
                tailOrder.previousOrder = headOrder;
            }
            else {
                tailOrder.nextOrder = order;
                order.previousOrder = tailOrder;
                tailOrder = order;
            }
        }
    }

    

    public class tradeRecord {
        public string symbol;
        public int orderNumber;
        public double price;
        public int volume;
        public int side;

        public tradeRecord(string _symbol, int _orderNumber, double _price, int _volume, int _side)
        {
            symbol = _symbol;
            orderNumber = _orderNumber;
            price = _price;
            side = _side;
            volume = _volume;
        }
    
    }
    public class Order
    {
        public String idNumber;
        public DateTime entryTime;
        public double price;
        public int side;
        public int volume;
        public Order? nextOrder;
        public Order? previousOrder;

        public Order(String _idNumber, int _side, int _volume, double _price, DateTime _entryTime)
        {
            idNumber = _idNumber;
            side = _side;
            volume = _volume;
            price = _price;
            entryTime = _entryTime;
            nextOrder = null;
            previousOrder = null;
        }
    }
}
