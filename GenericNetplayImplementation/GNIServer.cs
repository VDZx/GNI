using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using System.IO;

/*
 * GenericNetplayImplementation - A networking library for C# (version 2)

    Written in 2012 by Vincent de Zwaan

    To the extent possible under law, the author(s) have dedicated all copyright and related and neighboring rights to this software to the public domain worldwide. This software is distributed without any warranty. 

    You should have received a copy of the CC0 Public Domain Dedication along with this software. If not, see <http://creativecommons.org/publicdomain/zero/1.0/>. 
  
 * Any modifications I make to this software in the future will likely be published at <https://github.com/VDZx/GNI>.
 */

namespace GenericNetplayImplementation
{
    public abstract class GNIServer : GNIObject
    {
        //Public
        public List<GNIClientInformation> clients;  //Information on all connected clients
        public bool convertTypes = false;           //Whether or not to automatically fill in non-applicable types per signal
        public bool enableLog = true;               //Whether or not to log information
        public int listenInterval = 100;           //How often the server checks for new connections
        public int port;                            //The port on which the server listens

        //Private
        private uint currentID = 0;             //The next ID to assign to a client
        private bool stopListening = false;     //If enabled, stops listening for new connections
        private bool clientLock = false;        //If enabled, waits with certain actions until it's safe to execute them

        //-----------------Startup----------------
        public GNIServer() { }

        public GNIServer(int port, bool autoPoll = false, bool startListening = true, bool enableLog = true)
        {
            StartServer(port, autoPoll, startListening, enableLog);
        }

        public void StartServer(int port, bool autoPoll = false, bool startListening = true, bool enableLog = true)
        {
            clients = new List<GNIClientInformation>();
            this.port = port;

            //Set default listening behavior
            if (!startListening) stopListening = true;
            else stopListening = false;

            if (autoPoll) stopAutopoll = false;
            else stopAutopoll = true;

            if (startListening)
                ThreadPool.QueueUserWorkItem(new WaitCallback(Listen));

            if (autoPoll)
                AutoPoll();
        }

        //--------------Listening------------
        //Listens for incoming connections
        private void Listen(object o)
        {
            if (!stopListening)
            {
                TcpListener listener = new TcpListener(System.Net.IPAddress.Any, port);
                listener.Start();

                while (!stopListening)
                {
                    //Add any incoming cients to the list
                    if (listener.Pending())
                    {
                        bool succesful = false;
                        try
                        {
                            while (clientLock) { Thread.Sleep(50); }
                            clientLock = true;
                            TcpClient tcpClient = listener.AcceptTcpClient();
                            GNIClientInformation client = new GNIClientInformation(currentID, tcpClient);
                            clients.Add(client);
                            succesful = true;
                            ThreadPool.QueueUserWorkItem(new WaitCallback(delegate { OnClientConnected(client); }));
                            clientLock = false;
                        }
                        catch (Exception) { clientLock = false; }
                        if (succesful)
                        {
                            currentID++;
                        }
                    }

                    Thread.Sleep(listenInterval);
                }

                listener.Stop();
            }
        }

        //User-friendly methods for this
        public void StartListening()
        {
            stopListening = false;
            ThreadPool.QueueUserWorkItem(new WaitCallback(Listen));
        }

        public void StopListening()
        {
            stopListening = true;
        }

        public bool GetIsListening() { if (stopListening) return false; else return true; }

        //------------Connection handling---------
        public void RemoveClient(uint clientID)
        {
            RemoveClient(GetClient(clientID));
        }

        public void RemoveClient(GNIClientInformation client)
        {
            OnClientDisconnected(client);

            try
            {
                client.tcpClient.Close();
            }
            catch (Exception) { }

            clients.Remove(client);
        }

        //------------Update-----------
        public override void Update()
        {
            while (clientLock) Thread.Sleep(50);
            //clientLock = true;

            try
            {
                //For every client
                for (int i = 0; i < clients.Count; i++)
                {
                    GNIClientInformation client = clients[i];

                    //Check if connection still exists
                    if (client.tcpClient == null) RemoveClient(client);
                    //And is still connected
                    if (!client.tcpClient.Connected) RemoveClient(client);

                    //If it's not currently loading any data...
                    if (client.dataBeingTransferred.started == false)
                    {
                        //Start loading if data length is available
                        if (client.tcpClient.Available > 3)
                        {
                            byte[] buffer = new byte[4];
                            client.tcpClient.GetStream().Read(buffer, 0, 4);
                            int dataLength = BitConverter.ToInt32(buffer, 0);
                            client.dataBeingTransferred = new GNIPendingData(dataLength);
                        }
                    }

                    //Now, if it's already started loading data...
                    if (client.dataBeingTransferred.started)
                    {
                        int bytesToRead = client.tcpClient.Available;
                        if (bytesToRead > 0)
                        {
                            if (bytesToRead > client.dataBeingTransferred.datalength - client.dataBeingTransferred.dataread)
                                bytesToRead = client.dataBeingTransferred.datalength - client.dataBeingTransferred.dataread;
                            byte[] buffer = new byte[bytesToRead];
                            client.tcpClient.GetStream().Read(buffer, 0, bytesToRead);
                            client.dataBeingTransferred.AddData(buffer);

                            if (client.dataBeingTransferred.datalength == client.dataBeingTransferred.dataread)
                            {
                                HandleSignal(client.dataBeingTransferred.data, clients[i].clientID);
                                client.dataBeingTransferred = new GNIPendingData(false);
                            }
                        }
                    }

                    clients[i] = client;
                }
            }
            catch (Exception ex) { }

            //clientLock = false;
        }



        //------------SendSignal------------
        public void SendSignal(uint recipient, GNIData data)
        {
            while (clientLock) { Thread.Sleep(50); }
            clientLock = true;
            try
            {
                for (int i = 0; i < clients.Count; i++)
                {
                    if (clients[i].clientID == recipient) SendSignal(clients[i], data);
                }
            }
            catch (Exception) { }
            clientLock = false;
        }

        public void SendSignal(GNIClientInformation recipient, GNIData data)
        {
            SendSignal(recipient.tcpClient, data);
        }

        public void BroadcastSignal(GNIData data)
        {
            while (clientLock) { Thread.Sleep(50); }
            clientLock = true;
            try
            {
                for (int i = 0; i < clients.Count; i++)
                {
                    SendSignal(clients[i], data);
                }
            }
            catch (Exception) { }
            clientLock = false;
        }

        //------------Events----------------
        public virtual void OnClientConnected(GNIClientInformation client) { }
        public virtual void OnClientDisconnected(GNIClientInformation client) { }


        //------------Miscellaneous----------
        public GNIClientInformation GetClient(uint clientID)
        {
            for (int i = 0; i < clients.Count; i++)
            {
                if (clients[i].clientID == clientID) return clients[i];
            }
            return new GNIClientInformation(uint.MaxValue, null);
        }

        
    }
}
