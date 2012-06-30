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

namespace GNIChatServer
{
    class Server : GNIServer
    {
        public bool running = true;

        static void Main(string[] args)
        {
            new Server().DoStuff();
        }

        public void DoStuff()
        {
            StartServer(5151, true, true, true);
            Console.WriteLine("Server started.");

            while (running)
            {
                System.Threading.Thread.Sleep(1000);
                BroadcastSignal(new GNIData() { keyType = GNIDataType.None, valueType = GNIDataType.None } );
            }
        }

        public override void OnClientConnected(GNIClientInformation client)
        {
            SMessage(client.name + " has joined. " + clients.Count + " people online.");
        }

        public override void OnDataReceived(GNIData data, uint source)
        {
            switch (data.keyString)
            {
                case "message":
                    Message("[" + DateTime.Now.ToString() + "] <" + GetClient(source).name + "> " + data.valueString);
                    break;
                case "nick":
                    string oldname = "";
                    for (int i = 0; i < clients.Count; i++)
                    {
                        if (clients[i].clientID == source)
                        {
                            GNIClientInformation client = clients[i];
                            oldname = client.name;
                            client.name = data.valueString;
                            clients[i] = client;
                        }
                    }
                    if(oldname != "") SMessage(oldname + " has changed nick to " + data.valueString);
                    break;
            }
        }

        public void Message(string s)
        {
            GNIData data = new GNIData("chatmessage", s);
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(s);
            BroadcastSignal(data);
        }

        public void SMessage(string s)
        {
            GNIData data = new GNIData("systemmessage", s);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(s);
            BroadcastSignal(data);
        }

        public override void OnClientDisconnected(GNIClientInformation client)
        {
            SMessage(client.name + " has left.");
        }
    }
}
