using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace chat
{
    public struct OrderInfo
    {
        public DateTime epochTime;
        public String orderId;
        public String symbol;
        public String side;
        public String orderCategory;
        public double price;
        public int volume;
        public int sequenceNumber;
        public DateTime d1 = new DateTime(1970, 1, 1, 0, 0, 0);
        public OrderInfo(String _epochTime, String _orderId, String _symbol, String _side, String _orderCategory, double _price, int _volume, int _seqNo) { 
            DateTime d2 = d1.AddSeconds(Convert.ToInt64(_epochTime.Substring(0,10))).AddHours(8).AddTicks(Convert.ToInt64(_epochTime.Substring(10, 9)) / 100);
            epochTime = d2;
            orderId = _orderId;
            symbol = _symbol;
            side = _side;
            orderCategory = _orderCategory;
            price = _price;
            volume = _volume;
            sequenceNumber = _seqNo;
        }   
    }
}
