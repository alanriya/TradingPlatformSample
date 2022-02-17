using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace chat
{
    internal class Strategy
    {
        // simple sniper strategy, buy when trade is above certain price.
        DateTime strategyTime;
        DateTime previousTime;
        public List<double> dataset = new List<double>();
        public List<tradeRecord> records = new List<tradeRecord>();
        public double sniperLimit;
        public int seqNumber = 1;

        public Strategy(double _sniperLimit) {
            sniperLimit = _sniperLimit;
        }
        public void updateData(ref OrderInfo dataReceived) {
            // aggregate by minute and update the dataset.
            if (strategyTime.Minute != dataReceived.epochTime.Minute && dataReceived.orderCategory == "TRADE") {
                dataset.Add(dataReceived.price);
                strategyTime = dataReceived.epochTime;
            }

        }

        public bool triggerCondition(ref OrderInfo dataReceived) {
            if (previousTime != strategyTime && dataset.Count != 0) {
                previousTime = strategyTime;
                double lastTradePrice = dataset[dataset.Count - 1];
                Console.WriteLine($"Condition checked at {dataReceived.epochTime} ");
                return lastTradePrice > sniperLimit;
            }
            return false;            
        }

        public tradeRecord enterTrade(ref OrderBook orderbook, ref OrderInfo dataReceived) {
            tradeRecord t = new tradeRecord(dataReceived.symbol, seqNumber , orderbook.lowestSell.limitPrice, 5, 0);
            seqNumber++;
            records.Add(t);
            return t;
        }

        public tradeRecord exitTrade(ref OrderBook orderbook, String symbol)
        {
            int volume = 0;
            foreach (tradeRecord tradesEntered in records) {
                volume += tradesEntered.volume;
            }
            tradeRecord t = new tradeRecord(symbol, seqNumber, orderbook.highestBuy.limitPrice, volume, 1);
            seqNumber++;
            records.Add(t);
            return t;
        }
    }
}
