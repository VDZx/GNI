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

namespace FileSend
{
    class SendClient : GNIClient
    {
        static void Main(string[] args)
        {
            new SendClient().DoStuff();
        }

        public bool transferring = false;

        public void DoStuff()
        {
            Console.WriteLine("");
            Console.WriteLine("Enter IP Address:");
            string ip = Console.ReadLine();
            //try
            {
                StartClient(ip, 5151);
                Console.Write("Send which file?>");
                string filename = Console.ReadLine();
                BinaryReader br = null;
                try
                {
                    br = new BinaryReader(new FileStream(filename, FileMode.Open));
                }
                catch (Exception ex) { Console.WriteLine("Error while opening file: " + ex.Message); goto EndProgram; }

                if (filename.Contains("\\"))
                {
                    filename = filename.Substring(filename.LastIndexOf("\\") + 1);
                }

                if (filename.Contains("/"))
                {
                    filename = filename.Substring(filename.LastIndexOf("/") + 1);
                }

                
                //Send file name
                Console.WriteLine("Sending file name...");
                SendSignal(tcpClient, new GNIData().SetData("filename", filename));

                //Send file data
                Console.WriteLine("Preparing to send...");
                SendSignal(tcpClient, new GNIData().SetData("filedata", br.ReadBytes(Convert.ToInt32(br.BaseStream.Length))));
                transferring = true;

                Console.WriteLine("Sending file...");
                Console.WriteLine("");

                br.Close();

                while (transferring)
                {
                    this.Update();
                    System.Threading.Thread.Sleep(100);
                }

                Console.WriteLine("File sent!");
                goto EndProgram;
            }
            //catch (Exception ex) { Console.WriteLine("Connection error! " + ex.Message); goto EndProgram; }
            
            EndProgram:
                Console.ReadKey();
        }

        public override void OnDataReceived(GNIData data, uint source = 0)
        {
            switch (data.keyString)
            {
                case "finish":
                    transferring = false;
                    break;
            }
        }
    }
}
