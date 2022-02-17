using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace chat
{
    public class MessageParser
    {
        public OrderInfo processMessage(string message) {
            int pos = 0;
            List<String> vect = new List<String>();
            String current = "";

            while (pos < message.Length) {
                char c = message[pos];
                if (c == '\t' || c == ' ')
                {
                    if (current != "")
                    {
                        vect.Add(current);
                        current = "";
                    }
                }
                else {
                    current += c;
                }

                if (pos == message.Length - 1)
                {
                    vect.Add(current);
                }
                pos++;
            }

            return new OrderInfo( vect[0], vect[1], vect[2], vect[3], vect[4], Convert.ToDouble(vect[5]), Convert.ToInt32(vect[6]), Convert.ToInt32(vect[7]));
        }

    }
}
