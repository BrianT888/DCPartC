using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Windows.Documents;
using APIClasses;
namespace ClientDesktop
{
    public class Server
    {
        private List<Job> jobList = new List<Job>();//list of jobs in server
        public void StartServer(string ip, int port)
        {
            Console.WriteLine("Attempting to start Server...");
            ServiceHost host = new ServiceHost(typeof(ClientJobServer));
            NetTcpBinding tcp = new NetTcpBinding();

            //host.AddServiceEndpoint(typeof(IClientJobServer), tcp, "net.tcp://" + ip + ":" + port + "/ClientJobServer");//use 0.0.0.0 instead of localhost to accept external connections as its 'peer to peer'
            host.AddServiceEndpoint(typeof(IClientJobServer), tcp, "net.tcp://0.0.0.0:" + port + "/ClientJobServer");//using this as the line above only works if the ip is the current on the machine
            //it didn't accept any other ip for the tcp connection other than the one assigned to the machine through win + r -> cmd -> ipconfig

            host.Open();
            Console.WriteLine("Server started...");
            host.Close();
        }
    }
}
