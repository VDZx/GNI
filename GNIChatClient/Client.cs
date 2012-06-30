using System;
using System.Collections.Generic;
using System.Text;
using GenericNetplayImplementation;

/*
 * GenericNetplayImplementation - A networking library for C# (version 2)

    Written in 2012 by Vincent de Zwaan

    To the extent possible under law, the author(s) have dedicated all copyright and related and neighboring rights to this software to the public domain worldwide. This software is distributed without any warranty. 

    You should have received a copy of the CC0 Public Domain Dedication along with this software. If not, see <http://creativecommons.org/publicdomain/zero/1.0/>. 
  
 * Any modifications I make to this software in the future will likely be published at <https://github.com/VDZx/GNI>.
 */

namespace GNIChatClient
{
    class Client : GNIClient
    {
        public bool running = true;

        static void Main(string[] args)
        {
            new Client().DoStuff();
        }

        public void DoStuff()
        {
            Console.WriteLine("Enter IP Address:");
            string ip = Console.ReadLine();
            StartClient(ip, 5151);
            AutoPoll();

            while (running)
            {
                SendMessage(Console.ReadLine());
                System.Threading.Thread.Sleep(100);
            }
        }

        public void SendMessage(string message)
        {
            if (message.StartsWith("/"))
            {
                if (message.StartsWith("/nick"))
                {
                    string[] split = message.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                    if (split.Length > 1)
                    {
                        GNIData data = new GNIData("nick", split[1]);
                        SendSignal(tcpClient, data);
                    }
                }
            }
            else
            {
                GNIData data = new GNIData("message", message);
                SendSignal(tcpClient, data);

            }
        }

        public override void OnDataReceived(GNIData data, uint source = 0)
        {
            switch (data.keyString)
            {
                case "chatmessage":
                    Message(data.valueString);
                    break;
                case "systemmessage":
                    SMessage(data.valueString);
                    break;
            }
        }

        public void Message(string s)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(s);
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        public void SMessage(string s)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(s);
            Console.ForegroundColor = ConsoleColor.Gray;
        }
    }
}
