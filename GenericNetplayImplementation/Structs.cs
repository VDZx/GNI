using System;
using System.Collections.Generic;
using System.Text;

/*
 * GenericNetplayImplementation - A networking library for C# (version 2)

    Written in 2012 by Vincent de Zwaan

    To the extent possible under law, the author(s) have dedicated all copyright and related and neighboring rights to this software to the public domain worldwide. This software is distributed without any warranty. 

    You should have received a copy of the CC0 Public Domain Dedication along with this software. If not, see <http://creativecommons.org/publicdomain/zero/1.0/>. 
  
 * Any modifications I make to this software in the future will likely be published at <https://github.com/VDZx/GNI>.
 */

namespace GenericNetplayImplementation
{
    public struct GNIClientInformation
    {
        public GNIClientInformation(uint clientID, System.Net.Sockets.TcpClient tcpClient)
        {
            this.clientID = clientID;
            this.tcpClient = tcpClient;
            this.name = "Anonymous";
            this.data = new GNIDataSet(true);
            this.dataBeingTransferred = new GNIPendingData(false);
        }

        public uint clientID;
        public System.Net.Sockets.TcpClient tcpClient;
        public string name;
        public GNIDataSet data;
        public GNIPendingData dataBeingTransferred;
    }

    public struct GNIData
    {
        public GNIData(bool empty)
        {
            keyType = GNIDataType.Short;
            valueType = GNIDataType.Short;
            encoding = GNIEncoding.ASCII;
            keyInt = -1;
            keyString = "";
            keyBytes = new byte[0];
            valueInt = 0;
            valueString = "";
            valueBytes = new byte[0];
        }

        public GNIData(string key, string value)
        {
            keyType = GNIDataType.String;
            valueType = GNIDataType.String;
            encoding = GNIEncoding.ASCII;
            keyInt = -1;
            keyString = key;
            keyBytes = new byte[0];
            valueInt = 0;
            valueString = value;
            valueBytes = new byte[0];
        }

        public GNIData(string key, int value)
        {
            keyType = GNIDataType.String;
            valueType = GNIDataType.Short;
            encoding = GNIEncoding.ASCII;
            keyInt = -1;
            keyString = key;
            keyBytes = new byte[0];
            valueInt = value;
            valueString = Convert.ToString(value);
            valueBytes = new byte[0];
        }

        public GNIDataType keyType;
        public GNIDataType valueType;
        public GNIEncoding encoding;
        public int keyInt;
        public string keyString;
        public byte[] keyBytes;
        public int valueInt;
        public string valueString;
        public byte[] valueBytes;

        public void EnterKeyNull() { keyInt = -1; keyString = ""; keyBytes = new byte[0]; }
        public void EnterKeyInt(int key) { keyInt = key; keyString = ""; keyBytes = new byte[0]; }
        public void EnterKeyString(string key) { keyInt = -1; keyString = key; keyBytes = new byte[0]; }
        public void EnterKeyByteArray(byte[] key) { keyInt = -1; keyString = ""; keyBytes = key; }

        public void EnterValueNull() { valueInt = 0; valueString = ""; valueBytes = new byte[0]; }
        public void EnterValueInt(int value) { valueInt = value; valueString = ""; valueBytes = new byte[0]; }
        public void EnterValueString(string value) { valueInt = 0; valueString = value; valueBytes = new byte[0]; }
        public void EnterValueByteArray(byte[] value) { valueInt = -1; valueString = ""; valueBytes = value; }

        public void Initialize()
        {
            keyType = GNIDataType.Short;
            valueType = GNIDataType.Short;
            encoding = GNIEncoding.ASCII;
            keyInt = -1;
            keyString = "";
            keyBytes = new byte[0];
            valueInt = 0;
            valueString = "";
            valueBytes = new byte[0];
        }

        //Set functions
        public GNIData SetData(int key, int value)
        {
            this.Initialize();
            this.keyType = GNIDataType.Short;
            this.keyInt = key;
            this.valueType = GNIDataType.Short;
            this.valueInt = value;
            return this;
        }

        public GNIData SetData(int key, string value)
        {
            this.Initialize();
            this.keyType = GNIDataType.Short;
            this.keyInt = key;
            this.valueType = GNIDataType.String;
            this.valueString = value;
            return this;
        }

        public GNIData SetData(int key, byte[] value)
        {
            this.Initialize();
            this.keyType = GNIDataType.Short;
            this.keyInt = key;
            this.valueType = GNIDataType.ByteArray;
            this.valueBytes = value;
            return this;
        }

        public GNIData SetData(string key, int value)
        {
            this.Initialize();
            this.keyType = GNIDataType.String;
            this.keyString = key;
            this.valueType = GNIDataType.Short;
            this.valueInt = value;
            return this;
        }

        public GNIData SetData(string key, string value)
        {
            this.Initialize();
            this.keyType = GNIDataType.String;
            this.keyString = key;
            this.valueType = GNIDataType.String;
            this.valueString = value;
            return this;
        }

        public GNIData SetData(string key, byte[] value)
        {
            this.Initialize();
            this.keyType = GNIDataType.String;
            this.keyString = key;
            this.valueType = GNIDataType.ByteArray;
            this.valueBytes = value;
            return this;
        }

        public GNIData SetData(byte[] key, int value)
        {
            this.Initialize();
            this.keyType = GNIDataType.ByteArray;
            this.keyBytes = key;
            this.valueType = GNIDataType.Short;
            this.valueInt = value;
            return this;
        }

        public GNIData SetData(byte[] key, string value)
        {
            this.Initialize();
            this.keyType = GNIDataType.ByteArray;
            this.keyBytes = key;
            this.valueType = GNIDataType.String;
            this.valueString = value;
            return this;
        }

        public GNIData SetData(byte[] key, byte[] value)
        {
            this.Initialize();
            this.keyType = GNIDataType.ByteArray;
            this.keyBytes = key;
            this.valueType = GNIDataType.ByteArray;
            this.valueBytes = value;
            return this;
        }
    }

    public struct GNIDataSet
    {
        public GNIDataSet(bool empty)
        {
            allData = new GNIData[0];
        }

        public GNIData[] allData;

        //Public methods
        public void Add(GNIData data)
        {
            GNIData[] newData = new GNIData[allData.Length + 1];
            for (int i = 0; i < allData.Length; i++)
            {
                newData[i] = allData[i];
            }
            newData[newData.Length - 1] = data;
            allData = newData;
        }

        public void Remove(string key)
        {
            Remove(KeyPosition(key), false);
        }

        public void Remove(int key)
        {
            Remove(KeyPosition(key), false);
        }

        public void Overwrite(GNIData data)
        {
            int keyPosition = -1;
            switch (data.keyType)
            {
                case GNIDataType.Short:
                    keyPosition = KeyPosition(data.keyInt);
                    break;
                case GNIDataType.String:
                    keyPosition = KeyPosition(data.keyString);
                    break;
            }
            if (keyPosition == -1) Add(data);
            else allData[keyPosition] = data;
        }

        public GNIData GetData(string key)
        {
            int keyPosition = KeyPosition(key);
            GNIData returnData;
            if (keyPosition == -1) returnData = new GNIData(true);
            else returnData = allData[keyPosition];
            return returnData;
        }

        public GNIData GetData(int key)
        {
            int keyPosition = KeyPosition(key);
            GNIData returnData;
            if (keyPosition == -1) returnData = new GNIData(true);
            else returnData = allData[keyPosition];
            return returnData;
        }

        //Private methods
        private void Remove(int keyPosition, bool disambiguation)
        {
            if(keyPosition == -1) return;
            GNIData[] newData = new GNIData[allData.Length - 1];
            for (int i = 0; i < allData.Length; i++)
            {
                if(i < keyPosition)
                    newData[i] = allData[i];
                else if (i > keyPosition)
                    newData[i - 1] = allData[i];
            }
            allData = newData;
        }

        private int KeyPosition(string key)
        {
            int foundAt = -1;
            for (int i = 0; i < allData.Length; i++)
            {
                if (allData[i].keyString == key) foundAt = i;
            }
            return foundAt;
        }

        private int KeyPosition(int key)
        {
            int foundAt = -1;
            for (int i = 0; i < allData.Length; i++)
            {
                if (allData[i].keyInt == key) foundAt = i;
            }
            return foundAt;
        }
    }

    public struct GNIPendingData
    {
        public GNIPendingData(bool started)
        {
            this.started = started;
            this.datalength = 0;
            this.dataread = 0;
            this.data = new byte[0];
        }

        public GNIPendingData(int datalength)
        {
            this.datalength = datalength;
            dataread = 0;
            data = new byte[datalength];
            started = true;
        }

        public bool started;
        public int datalength;
        public int dataread;
        public byte[] data;

        public void AddData(byte[] bytes)
        {
            for (int i = 0; i < bytes.Length; i++)
            {
                data[dataread + i] = bytes[i];
            }
            dataread += bytes.Length;
        }
    }
}
