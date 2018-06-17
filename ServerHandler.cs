using System.Dynamic;
using System;
using System.Collections.Generic;

namespace PR2_Client
{
    class ServerHandler
    {
        public Server[] servers;
        public ServerHandler()
        {
            dynamic m_Data = HttpHandler.getServers().servers;
            Newtonsoft.Json.Linq.JArray r = m_Data;
            int k = r.Count;
            servers = new Server[k];
            for(int i = 0; i < k; i++)
            {
                Server m_Server = new Server();
                m_Server.name = m_Data[i].server_name;
                m_Server.server_id = m_Data[i].server_id;
                m_Server.address = m_Data[i].address;
                m_Server.pop = m_Data[i].population;
                m_Server.port = m_Data[i].port;
                servers[i] = m_Server;
            }
        }
        public int promptServer()
        {
            Console.WriteLine("Select server:");
            for(int i = 0; i < servers.Length; i++)
            {
                Console.WriteLine("[" + (i+1) + "] " + servers[i].name + " " + servers[i].pop);
            }
            int j = Console.Read();
            Console.ReadLine();
            return j-48;
        }
    }
    class Server
    {
        public string name;
        public int server_id;
        public string address;
        public int port;
        public int pop;
    }
}