using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;

namespace eagle.tunnel.dotnet.core
{
    public class AuthenticationServer : Server
    {
        public AuthenticationServer(IPEndPoint ipep) : base(ipep) { }

        public AuthenticationServer(string url, int port) : base(url, port) { }

        protected override void Listen(TcpListener server)
        {
            Running = true;
            while(Running)
            {
                Connect newConnect;
                try
                {
                    TcpClient client = server.AcceptTcpClient();
                    newConnect = Authenticate(client);
                    if (newConnect == null) { continue;}
                }
                catch (SocketException se)
                {
                    Console.WriteLine(se.Message);
                    continue;
                }
                
                Thread handleClientThread = new Thread(HandleClient);
                handleClientThread.IsBackground = true;
                handleClientThread.Start(newConnect);
            }
        }

        private static Connect Authenticate(TcpClient client)
        {
            if (Conf.allConf.ContainsKey("users"))
            {
                string req = ReadStr(client);
                string[] reqs = req.Split(':');
                if (reqs.Length != 2)
                {
                    return null;
                }
                string id = reqs[0];
                string pswd = reqs[1];
                if (Authenticate(id, pswd))
                {
                    WriteStr(client, "valid");
                    return new Connect(client, id);
                }
                else
                {
                    WriteStr(client, "invalid");
                    client.Close();
                    return null;
                }
            }
            else
            {
                return new Connect(client, null);
            }
        }

        public static bool Authenticate(string id, string pswd)
        {
            string rightPswd;
            try
            {
                rightPswd = Conf.Users[id].Password;
            }
            catch(KeyNotFoundException)
            {
                return false;
            }

            return rightPswd == pswd;
        }
    }
}