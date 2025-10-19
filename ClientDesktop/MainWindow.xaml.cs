using APIClasses;
using IronPython.Hosting;
using Microsoft.Scripting.Hosting;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Security.Policy;
using System.ServiceModel;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace ClientDesktop
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ClientJobServer foobServer = new ClientJobServer();//Clients own job server//ok this is like mega local no work
        private Client user;
        private Server server;
        private RestClient client;
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

                StartNetworkThread(ip, port);
                //disable register button
                RegisterClientButton.Visibility = Visibility.Hidden;

            }catch (FormatException ex)
            {
                MessageBox.Show("Invalid Port number.");
                return;
            }
        }

        private async void PythonJobSubmitButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (user != null)//make sure you can't submit without being registered first
            {
                Console.WriteLine($"{user.clientId} is Submitting Supposed Python Job");
                Console.WriteLine($"{user.clientId} job list has: {foobServer.GetNumberOfJobs()}");
                //api call + client job server call to save jobs
                string pythonName = "name";// = PythonNameInput.Text;
                string pythonJob = PythonInput.Text;//lowk need another input box for the name of the python job this right here just the description part

                RestRequest request = new RestRequest($"/api/jobs/CreateJob/{user.clientId}/{pythonName}/{pythonJob}", Method.Post);
                var pythonJobResponse = await client.ExecuteAsync(request);
                if (!pythonJobResponse.IsSuccessful)
                {
                    MessageBox.Show("Failed to submit job to DB");
                    return;
                } else
                {
                    Job job = JsonConvert.DeserializeObject<Job>(pythonJobResponse.Content);
                    //foobServer.SubmitJob(job);//submit python job object to clients own server 
                    //server.submittingJob(job);
                    //ClientJobServer.jobList.Enqueue(job);
                    Console.WriteLine($"{user.clientId} : {job.jobDescription} ({job.jobName})");
                    server.submitJob(job);
                }
            }
            else
            {
                MessageBox.Show("Please Register first");
                return;
            }
        }
        private async Task Networking(string ip, int port)
        {
            //Network only thread that connects to server in spec
            string URL = "http://localhost:5199";

            client = new RestClient(URL);

            RestRequest registerRequest = new RestRequest($"/api/clients/Register/{ip}/{port}", Method.Post);
            var registerResponse = await client.ExecuteAsync(registerRequest);
            if (!registerResponse.IsSuccessful)
            {
                //lowk could fail here once i do error handling people can't use idk the same ip or smt same ip + port combo 
                Dispatcher.Invoke(() =>
                {
                    MessageBox.Show("Failed to register client: " + registerResponse.ErrorMessage);
                });
                return;
            }

            Dispatcher.Invoke(() =>
            {
                MessageBox.Show("Client registered successfully!");
            });
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
                    Console.WriteLine($"{user.clientId} sees Amount of other clients: {otherClients.Count}");

                    foreach (var otherClient in otherClients)
                    {
                        //might need to make a channelfactory here for each client to go to their server.cs and interact with their ClientJobServer.cs
                        ChannelFactory<IClientJobServer> foobFactory;
                        NetTcpBinding tcp = new NetTcpBinding();
                        string otherClientURL = $"net.tcp://127.0.0.1:{otherClient.port}/ClientJobServer";//use loopback 127.0.0.1 for connection to self machine
                        foobFactory = new ChannelFactory<IClientJobServer>(tcp, otherClientURL);
                        IClientJobServer foob = foobFactory.CreateChannel();

                        Console.WriteLine($"Current Client ID: {user.clientId} -> Client ID: {otherClient.clientId}, IP: {otherClient.ip}, Port: {otherClient.port}");//console output to show which client is being looked at right now by which user
                        //ONLY WORKS IF CLIENT IS ALSO OPEN, IF CLOSE REDO 

                        Job job = foob.GetJob();//call GetJob from the other client's server

                        if (job != null)
                        {
                            //do stuff here
                            Console.WriteLine("Client " + user.clientId + " obtained job from client ID " + otherClient.clientId);//
                            Console.WriteLine($"Job ID: {job.jobId}, Client ID: {job.clientId}, Job Description: {job.jobDescription}, Job Status: {job.jobStatus}");//
                        }
                        else
                        {
                            Console.WriteLine("No job available from client ID " + otherClient.clientId);
                        }

                        Task.Delay(5000).Wait();
                    }

                    Task.Delay(3000).Wait();
                }
                else
                {
                    Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show("Failed to retrieve other clients: " + response.ErrorMessage);
                    });

                }
            }
        }

        //make the networking run on a different thread
        private void StartNetworkThread(string ip, int port)
        {
            Task.Run(() => Networking(ip, port));
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

        //When closing window api call to delete itself from server
        protected override void OnClosed(EventArgs e)
        {
            if (user != null)//to prevent opening and closing a client without registering leading to an api call
            {
                RestRequest deleteRequest = new RestRequest($"/api/clients/{user.clientId}", Method.Delete);
                var response = client.Execute(deleteRequest);

                if (!response.IsSuccessful)
                {
                    Console.WriteLine($"Could not delete from database client {user.clientId}");
                }
            }

        }
    }
}
