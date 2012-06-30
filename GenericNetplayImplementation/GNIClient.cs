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
    public abstract class GNIClient : GNIObject
    {
        public bool connected = false;
        public int port;                            //The port on which the server listens
        public string serverURL;
        public TcpClient tcpClient;

        private GNIPendingData dataBeingTransferred;

        public GNIClient() { }
        public GNIClient(string url, int port) { StartClient(url, port); }

        public void StartClient(string url, int port)
        {
            this.serverURL = url;
            this.port = port;
            this.tcpClient = new TcpClient(serverURL, port);
            this.connected = true;
            this.dataBeingTransferred = new GNIPendingData(false);
        }

        public override void Update()
        {
            if (connected)
            {
                //Check if connection still exists
                if (tcpClient == null) { connected = false; OnConnectionLost(); }
                //And is still connected
                if (!tcpClient.Connected) { connected = false; OnConnectionLost(); }

                //If it's not currently loading any data...
                if (dataBeingTransferred.started == false)
                {
                    //Start loading if data length is available
                    if (tcpClient.Available > 3)
                    {
                        byte[] buffer = new byte[4];
                        tcpClient.GetStream().Read(buffer, 0, 4);
                        int dataLength = BitConverter.ToInt32(buffer, 0);
                        dataBeingTransferred = new GNIPendingData(dataLength);
                    }
                }

                //Now, if it's already started loading data...
                if (dataBeingTransferred.started)
                {
                    int bytesToRead = tcpClient.Available;
                    if (bytesToRead > 0)
                    {
                        if (bytesToRead > dataBeingTransferred.datalength - dataBeingTransferred.dataread)
                            bytesToRead = dataBeingTransferred.datalength - dataBeingTransferred.dataread;
                        byte[] buffer = new byte[bytesToRead];
                        tcpClient.GetStream().Read(buffer, 0, bytesToRead);
                        dataBeingTransferred.AddData(buffer);

                        if (dataBeingTransferred.datalength == dataBeingTransferred.dataread)
                        {
                            HandleSignal(dataBeingTransferred.data);
                            dataBeingTransferred = new GNIPendingData(false);
                        }
                    }
                }
            }
        }

        public virtual void OnConnectionLost() { }
    }
}
