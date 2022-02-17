## Trading Platform Project

## Introduction
This is a trading platform set up using C#. it consists of the market data injector, simulated exchange, orderbook, strategy component.
The data is first read from the txt file and pass the message down using multicast UDP. The orderbook object will process the message received from the UDP channel and builds the book.
The Strategy object is a simple strategy that buys when instrument price is higher than a certain price. When the condition is met, the strategy will place an buy order to the simulated exchange at the best ask price. The exchange will reply with the filled order.
At the end of the trading session, the all the position in the strategy will be reversed and the pnl will be computed.

C# .NET 6.0 is used for this project and the internal UDP and TCP sockets are used for this project.

## Project Installation
1) Install .NET CORE (at least 6.0 and above).
2) Go to the root folder where the .sln is located. 1 solution can consist of multiple project.
3) Open command line, run "dotnet restore" to install external packages.
4) "dotnet run" to launch the solution.
5) "dotnet build" to generate the dll and exe.

## Technology used
1) C# .NET 6.0

