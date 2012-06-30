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

    public abstract class GNIObject
    {
        public int autoPollInterval = 100;           //How often it checks for new messages
        protected bool stopAutopoll = false;      //If enabled, stops autoPolling

        //Overridable methods
        public virtual void Update() { }
        public virtual void OnDataReceived(GNIData data, uint source = 0) { }
        public virtual void LoseConnection(TcpClient connection) { }


        //Actual methods
        public void AutoPoll()
        {
            ThreadPool.QueueUserWorkItem(delegate
            {
                while (!stopAutopoll)
                {
                    Update();

                    Thread.Sleep(autoPollInterval);
                }
            });
        }

        //------------HandleSignal----------
        protected void HandleSignal(byte[] rawSignal, uint source = 0)
        {
            GNIData data = new GNIData();

            System.IO.MemoryStream ms = new System.IO.MemoryStream(rawSignal);

            //Read types
            int keyTypeByte = ReadVariableLengthInt(ms, 1);
            int valueTypeByte = ReadVariableLengthInt(ms, 1);

            //Enter types
            switch (keyTypeByte)
            {
                case 0: data.keyType = GNIDataType.None; break;
                case 1: data.keyType = GNIDataType.Short; break;
                case 2: data.keyType = GNIDataType.String; break;
                case 3: data.keyType = GNIDataType.ByteArray; break;
            }
            switch (valueTypeByte)
            {
                case 0: data.valueType = GNIDataType.None; break;
                case 1: data.valueType = GNIDataType.Short; break;
                case 2: data.valueType = GNIDataType.String; break;
                case 3: data.valueType = GNIDataType.ByteArray; break;
            }

            int keyLength = ReadVariableLengthInt(ms, GNIGeneral.GetLengthLength(data.keyType));
            int valueLength = ReadVariableLengthInt(ms, GNIGeneral.GetLengthLength(data.valueType));

            //If string, read encoding byte
            int encoding = 0;
            if (data.keyType == GNIDataType.String || data.valueType == GNIDataType.String)
            {
                encoding = ReadVariableLengthInt(ms, 1);
            }

            //Enter encoding
            switch (encoding)
            {
                case 0: data.encoding = GNIEncoding.ASCII; break;
            }

            //Read and enter key
            switch (data.keyType)
            {
                case GNIDataType.None: data.EnterKeyNull(); break;
                case GNIDataType.Short: data.EnterKeyInt(ReadShort(ms)); break;
                case GNIDataType.String: data.EnterKeyString(ReadString(ms, keyLength, data.encoding)); break;
                case GNIDataType.ByteArray: byte[] buffer = new byte[keyLength]; ms.Read(buffer, 0, keyLength); data.EnterKeyByteArray(buffer); break;
            }

            //Read and enter value
            switch (data.valueType)
            {
                case GNIDataType.None: data.EnterValueNull(); break;
                case GNIDataType.Short: data.EnterValueInt(ReadShort(ms)); break;
                case GNIDataType.String: data.EnterValueString(ReadString(ms, valueLength, data.encoding)); break;
                case GNIDataType.ByteArray: byte[] buffer = new byte[valueLength]; ms.Read(buffer, 0, valueLength); data.EnterValueByteArray(buffer); break;
            }

            OnDataReceived(data, source);
        }

        public void SendSignal(TcpClient recipient, GNIData data)
        {
            byte keyType = 0;
            switch (data.keyType)
            {
                case GNIDataType.Short: keyType = 1; break;
                case GNIDataType.String: keyType = 2; break;
                case GNIDataType.ByteArray: keyType = 3; break;
            }

            byte valueType = 0;
            switch (data.valueType)
            {
                case GNIDataType.Short: valueType = 1; break;
                case GNIDataType.String: valueType = 2; break;
                case GNIDataType.ByteArray: valueType = 3; break;
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
                case GNIDataType.ByteArray:
                    keyBytes = data.keyBytes;
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
                case GNIDataType.ByteArray:
                    valueBytes = data.valueBytes;
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
            if (keyLength == 4) buffer = BitConverter.GetBytes(Convert.ToInt32(keyBytes.Length));
            else buffer = BitConverter.GetBytes(Convert.ToInt16(keyBytes.Length));
            for (int i = 0; i < keyLength; i++) { toSend[currentposition] = buffer[i]; currentposition++; }
            //Write value length
            if (valueLength == 4) buffer = BitConverter.GetBytes(Convert.ToInt32(valueBytes.Length));
            else buffer = BitConverter.GetBytes(Convert.ToInt16(valueBytes.Length));
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
            if (recipient.Connected)
            {
                try
                {
                    recipient.GetStream().Write(toSend, 0, toSend.Length);
                }
                catch (Exception) { LoseConnection(recipient); }
            }
            else LoseConnection(recipient);
        }

        //--------------

        protected byte[] ReadBytes(Stream stream, int number)
        {
            byte[] buffer = new byte[number];
            stream.Read(buffer, 0, number);
            return buffer;
        }

        protected int ReadShort(byte[] bytes) { return BitConverter.ToInt16(bytes, 0); }
        protected int ReadShort(Stream stream) { return ReadShort(ReadBytes(stream, 2)); }
        protected int ReadInt(byte[] bytes) { return BitConverter.ToInt32(bytes, 0); }
        protected int ReadInt(Stream stream) { return ReadInt(ReadBytes(stream, 4)); }
        protected int ReadVariableLengthInt(Stream stream, int length)
        {
            switch (length)
            {
                case 0: return 0;
                case 1: return stream.ReadByte();
                case 2: return ReadShort(stream);
                case 4: return ReadInt(stream);
            }
            return 0;
        }

        protected string ReadString(byte[] bytes, GNIEncoding encoding)
        {
            switch (encoding)
            {
                case GNIEncoding.ASCII:
                    return Encoding.ASCII.GetString(bytes);
            }
            return "";
        }
        protected string ReadString(Stream stream, int number, GNIEncoding encoding) { return ReadString(ReadBytes(stream, number), encoding); }
    }

}
