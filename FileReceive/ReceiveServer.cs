using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using GenericNetplayImplementation;

/*
 * GenericNetplayImplementation - A networking library for C# (version 2)

    Written in 2012 by Vincent de Zwaan

    To the extent possible under law, the author(s) have dedicated all copyright and related and neighboring rights to this software to the public domain worldwide. This software is distributed without any warranty. 

    You should have received a copy of the CC0 Public Domain Dedication along with this software. If not, see <http://creativecommons.org/publicdomain/zero/1.0/>. 
  
 * Any modifications I make to this software in the future will likely be published at <https://github.com/VDZx/GNI>.
 */

namespace FileReceive
{
    class ReceiveServer : GNIServer
    {
        static void Main(string[] args)
        {
            new ReceiveServer().DoStuff();
        }

        public Dictionary<uint, string> filenames;

        public void DoStuff()
        {
            filenames = new Dictionary<uint, string>();
            Console.WriteLine("Starting server...");
            StartServer(5151);
            Console.WriteLine("Server started.");
            while (true)
            {
                System.Threading.Thread.Sleep(100);
                Update();
            }
        }

        public override void OnDataReceived(GNIData data, uint source = 0)
        {
            Console.WriteLine("Received signal: " + data.keyString);
            switch (data.keyString)
            {
                case "filename":
                    this.filenames[source] = data.valueString;
                    Console.WriteLine("Filename for " + source + " set to " + data.valueString);
                    break;
                case "filedata":
                    string filename = filenames[source];
                    Console.WriteLine("Receiving file " + filename + " from " + source);
                    try
                    {
                        BinaryWriter writer = new BinaryWriter(new FileStream(filename, FileMode.Create));
                        writer.Write(data.valueBytes);
                        writer.Close();
                        SendSignal(GetClient(source), new GNIData().SetData("finish", 0));
                        Console.WriteLine("Succesfully received file " + filename + " from " + source);
                    }
                    catch (Exception ex) { Console.WriteLine("Error while writing " + filename + ": " + ex.Message); }
                    break;
            }
        }
    }
}
