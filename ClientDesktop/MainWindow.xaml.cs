using RestSharp;
using System.Collections.Generic;
using System.Windows;
using APIClasses;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.ServiceModel;
using Microsoft.Scripting.Hosting;
using IronPython.Hosting;

namespace ClientDesktop
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Client user;
        private Server server;
        public MainWindow()
        {
            InitializeComponent();
            server = new Server();//new server thread
        }

        private void RegisterButton_OnClick(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Register Button Clicked");

            try
            {
                string ip = IPInput.Text;
                int port = Int32.Parse(PortInput.Text);
                if (string.IsNullOrEmpty(ip))
                {
                    MessageBox.Show("IP must be provided");
                    return;
                }
                
                //might need an ip address validation for it to be only numbers and dots

                if (port < 1 || port > 65535)//port validation
                {
                    MessageBox.Show("Port number must be between 1 and 65535.");
                    return;
                }

                //server and network threads might need to be a if one succeeds then start the other
                //rn its more like starting up the server, but not networking because idk registration failed?
                //maybe network first before starting up server?

                server.StartServer(ip, port);//start server thread with ip + port provided

                Networking(ip, port);
                //disable register button
                RegisterClientButton.Visibility = Visibility.Hidden;

            }catch (FormatException ex)
            {
                MessageBox.Show("Invalid Port number.");
                return;
            }
        }
        private async void Networking(string ip, int port)
        {
            //Network only thread that connects to server in spec
            string URL = "http://localhost:5199";

            RestClient client = new RestClient(URL);
            RestRequest registerRequest = new RestRequest($"/api/clients/Register/{ip}/{port}", Method.Post);
            var registerResponse = await client.ExecuteAsync(registerRequest);
            if (!registerResponse.IsSuccessful)
            {
                MessageBox.Show("Failed to register client: " + registerResponse.ErrorMessage);
                return;
            }
            MessageBox.Show("Client registered successfully!");
            user = JsonConvert.DeserializeObject<Client>(registerResponse.Content);
            //maybe a check to make sure user isn't null after?
            //tho it might not be able to be null if the registration is successful

            while (true)
            {
                RestRequest request = new RestRequest("/api/clients/GetOtherClients/" + user.clientId, Method.Get);//in while loop to get new clients constantly after finishing scanning everyone
                var response = await client.ExecuteAsync(request);

                if (response.IsSuccessful)
                {
                    
                    var otherClients = JsonConvert.DeserializeObject<List<Client>>(response.Content);

                    // Process the list of other clients as needed
                    Console.WriteLine("Amount of other clients: " + otherClients.Count);

                    foreach (var otherClient in otherClients)
                    {
                        //might need to make a channelfactory here for each client to go to their server.cs and interact with their ClientJobServer.cs
                        ChannelFactory<IClientJobServer> foobFactory;
                        NetTcpBinding tcp = new NetTcpBinding();
                        string otherClientURL = "net.tcp://127.0.0.1:" + otherClient.port + "/ClientJobServer";//use loopback 127.0.0.1 for connection to self machine
                        foobFactory = new ChannelFactory<IClientJobServer>(tcp, otherClientURL);
                        IClientJobServer foob = foobFactory.CreateChannel();

                        Console.WriteLine($"Current Client ID: " + user.clientId +  $" Client ID: {otherClient.clientId}, IP: {otherClient.ip}, Port: {otherClient.port}");//console output to show which client is being looked at right now by which user
                        //ONLY WORKS IF CLIENT IS ALSO OPEN, IF CLOSE REDO 
                        //got to remove old clients from DB with DELETE somehow
                        //rn doing it manually on swagger very tiring

                        Job job = foob.GetJob();//call GetJob from the other client's server

                        if (job != null)
                        {
                            //do stuff here
                            Console.WriteLine("Client " + user.clientId + " obtained job from client ID " + otherClient.clientId);
                            Console.WriteLine($"Job ID: {job.jobId}, Client ID: {job.clientId}, Job Description: {job.jobDescription}, Job Status: {job.jobStatus}");
                        }
                        else
                        {
                            Console.WriteLine("No job available from client ID " + otherClient.clientId);
                        }

                        //RestRequest jobRequest = new RestRequest($"/api/jobs/GetJobsByClientId/{otherClient.clientId}", Method.Get);
                        //var jobResponse = await client.ExecuteAsync(jobRequest);
                        //if (jobResponse.IsSuccessful)
                        //{
                        //    var jobs = JsonConvert.DeserializeObject<List<Job>>(jobResponse.Content);
                        //    foreach (var job in jobs)
                        //    {
                        //        if (job.jobStatus) continue; // Skip already finished jobs

                        //        Console.WriteLine($"Job ID: {job.jobId}, Client ID: {job.clientId}, Job Description: {job.jobDescription}, Job Status: {job.jobStatus}");

                        //        Task.Delay(4000).Wait(); // Simulate job processing time

                        //        // Mark the job as finished
                        //        RestRequest finishJobRequest = new RestRequest($"/api/jobs/FinishedJob/{job.jobId}", Method.Put);
                        //        var finishJobResponse = await client.ExecuteAsync(finishJobRequest);
                        //        if (finishJobResponse.IsSuccessful)
                        //        {
                        //            Console.WriteLine($"Job ID: {job.jobId} marked as finished.");
                        //        }
                        //        else
                        //        {
                        //            Console.WriteLine($"Failed to mark Job ID: {job.jobId} as finished. Error: {finishJobResponse.ErrorMessage}");
                        //        }
                        //    }
                        //}
                        //else
                        //{
                        //    Console.WriteLine("Failed to retrieve jobs for client ID " + otherClient.clientId + ": " + jobResponse.ErrorMessage);
                        //    Task.Delay(5000).Wait(); // Wait before retrying
                        //}
                    }
                }
                else
                {
                    MessageBox.Show("Failed to retrieve other clients: " + response.ErrorMessage);
                }
            }
        }

        //string python code = job.jobDescription;
        private void RunPythonJobDescription(string pythonCode)
        {
            ScriptEngine engine = Python.CreateEngine();
            ScriptScope scope = engine.CreateScope();
            engine.Execute(pythonCode, scope);
            dynamic mainFunc = scope.GetVariable("main");//i assume the codes always gonna be def main():
            var result = mainFunc();//result after main() is called
        }
    }
}
