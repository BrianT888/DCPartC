using APIClasses;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows.Documents;
namespace ClientDesktop
{
    public class Server
    {
        private ServiceHost host;
        public void StartServer(string ip, int port)
        {
            Task.Run(() =>//have it run on a background thread and not stop since host.close is in diff method, the server will stay open after host.open
            {
                try
                {
                    host = new ServiceHost(typeof(ClientJobServer));
                    NetTcpBinding tcp = new NetTcpBinding();

                    host.AddServiceEndpoint(typeof(IClientJobServer), tcp, $"net.tcp://0.0.0.0:{port}/ClientJobServer");//have to use 0.0.0.0 or 127.0.0.1 instead of ip var because nettcp doesn't allow unavail ips
                    //only other ip address that can be hosted is the machines own ipv4 address through win + r -> cmd -> ipconfig -> IPv4 Address...
                    host.Open();
                    Console.WriteLine($"Server started on {ip}:{port}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Failed to start server: " + ex.Message);
                }
            });
        }

        public void submitJob(Job job)
        {
            ClientJobServer.jobList.Enqueue(job);
        }

        public void StopServer()
        {
            if (host != null)
            {
                host.Close();
                Console.WriteLine("Server stopped.");
            }
        }
    }
}
