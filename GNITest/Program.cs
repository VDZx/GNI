using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using GenericNetplayImplementation;

namespace GNITest
{
    class Program : GNIServer
    {
        static void Main(string[] args)
        {
            Program program = new Program();

            program.StartServer(5151, true, true, true);

            System.Threading.Thread.Sleep(1000);

            bool success = false;

            TcpClient tcp = null;

            while (!success)
            {
                try
                {
                    tcp = new TcpClient("127.0.0.1", 5151);
                    success = true;
                }
                catch (Exception ex) { Console.WriteLine(ex.Message); System.Threading.Thread.Sleep(100); }
            }

            bool running = true;
            while (running)
            {
                program.SendSignal(tcp, new GNIData(true) { valueType = GNIDataType.String, valueString = "Hello there!", encoding = GNIEncoding.ASCII });
                System.Threading.Thread.Sleep(1000);
            }
        }

        public override void OnDataReceived(GNIData data, uint source = 0)
        {
            //base.DataReceived(data);
            Console.WriteLine(data.valueString);
        }

        public override void OnClientConnected(GNIClientInformation client)
        {
            Console.WriteLine("Client connected: " + client.clientID);
        }

        /*public override void SendSignal(TcpClient recipient, GNIData data)
        {
            byte keyType = 0;
            switch (data.keyType)
            {
                case GNIDataType.Short: keyType = 1; break;
                case GNIDataType.String: keyType = 2; break;
            }

            byte valueType = 0;
            switch (data.valueType)
            {
                case GNIDataType.Short: valueType = 1; break;
                case GNIDataType.String: valueType = 2; break;
            }

            byte encoding = 0;
            if (data.valueType == GNIDataType.String)
            {
                switch (data.encoding)
                {
                    case GNIEncoding.ASCII: encoding = 0; break;
                }
            }

            byte[] keyBytes = new byte[0];
            switch (data.keyType)
            {
                case GNIDataType.Short: keyBytes = BitConverter.GetBytes(Convert.ToInt16(data.keyInt)); break;
                case GNIDataType.String:
                    switch (data.encoding)
                    {
                        case GNIEncoding.ASCII: keyBytes = Encoding.ASCII.GetBytes(data.keyString); break;
                    }
                    break;
            }

            byte[] valueBytes = new byte[0];
            switch (data.valueType)
            {
                case GNIDataType.Short: valueBytes = BitConverter.GetBytes(Convert.ToInt16(data.valueInt)); break;
                case GNIDataType.String:
                    switch (data.encoding)
                    {
                        case GNIEncoding.ASCII: valueBytes = Encoding.ASCII.GetBytes(data.valueString); break;
                    }
                    break;
            }

            int encodingAddition = 0;
            if (data.keyType == GNIDataType.String || data.valueType == GNIDataType.String) encodingAddition = 1;

            int keyLength = GNIGeneral.GetLengthLength(data.keyType);
            int valueLength = GNIGeneral.GetLengthLength(data.valueType);

            byte[] toSend = new byte[
                4 //Data length
                + 1 //Key type
                + 1 //Value type
                + keyLength //Key length
                + valueLength //Value length
                + encodingAddition
                + keyBytes.Length
                + valueBytes.Length
                ];

            int currentposition = 0;

            //Write data length
            byte[] buffer = BitConverter.GetBytes(toSend.Length - 4);
            for (int i = 0; i < 4; i++) { toSend[currentposition] = buffer[i]; currentposition++; }
            //Write key type
            toSend[currentposition] = keyType; currentposition++;
            //Write value type
            toSend[currentposition] = valueType; currentposition++;
            //Write key length
            buffer = BitConverter.GetBytes(Convert.ToInt16(keyBytes.Length));
            for (int i = 0; i < keyLength; i++) { toSend[currentposition] = buffer[i]; currentposition++; }
            //Write value length
            buffer = BitConverter.GetBytes(Convert.ToInt16(valueBytes.Length));
            for (int i = 0; i < valueLength; i++) { toSend[currentposition] = buffer[i]; currentposition++; }
            //Write encoding if applicable
            if (data.keyType == GNIDataType.String || data.valueType == GNIDataType.String)
            {
                toSend[currentposition] = encoding; currentposition++;
            }
            //Write key
            for (int i = 0; i < keyBytes.Length; i++) { toSend[currentposition] = keyBytes[i]; currentposition++; }
            //Write value
            for (int i = 0; i < valueBytes.Length; i++) { toSend[currentposition] = valueBytes[i]; currentposition++; }

            //Send the data!
            recipient.GetStream().Write(toSend, 0, toSend.Length);
        }*/
    }
}
