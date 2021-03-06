using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;

namespace eagle.tunnel.dotnet.core
{
    public class Client : Server
    {
        protected IPEndPoint[] remoteAddresses;
        protected int indexOfRemoteAddresses;

        public Client(
            IPEndPoint[] remoteaddresses,
            IPEndPoint localaddress
        ) : base(localaddress)
        {
            remoteAddresses = remoteaddresses;
            indexOfRemoteAddresses = 0;
        }

        public new void Start()
        {
            Console.WriteLine("Find Remote Server(s): {0}", remoteAddresses.Length);
            base.Start();
        }

        protected override void HandleClient(object clientObj)
        {
            TcpClient client2Client = clientObj as TcpClient;
            TcpClient client2Server = new TcpClient();
            try
            {
                int tmpIndex = indexOfRemoteAddresses++;
                if (tmpIndex >= remoteAddresses.Length)
                {
                    tmpIndex %= remoteAddresses.Length;
                }
                client2Server.Connect(remoteAddresses[tmpIndex]);
            }
            catch (SocketException se)
            {
                Console.WriteLine(se.Message);
            }
            

            Pipe pipe0 = new Pipe(
                client2Client,
                client2Server
            );
            pipe0.EncryptTo = true;
            Pipe pipe1 = new Pipe(
                client2Server,
                client2Client
                );
            pipe0.EncryptTo = true;
            pipe1.EncryptFrom = true;

            pipe0.Flow();
            pipe1.Flow();
        }

        public void Stop()
        {
            Running = false;
            Console.WriteLine("quitting...");
        }
    }
}