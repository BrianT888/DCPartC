using APIClasses;
using IronPython.Hosting;
using Microsoft.Scripting.Hosting;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Policy;
using System.ServiceModel;
using System.Text;
using System.Text.Encodings.Web;
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
        private Client user;
        private Server server;
        private RestClient client;
        private int jobsDone = 0;
        private enum clientStatus
        {
            Idle,
            FindingJob,
            Working
        }
        private clientStatus status = clientStatus.Idle;
        private bool loop;
        private int i;
        public MainWindow()
        {
            InitializeComponent();
            server = new Server();//new server thread
        }

        private async void RegisterButton_OnClick(object sender, RoutedEventArgs e)
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
                
                if (port < 1 || port > 65535)//port validation
                {
                    MessageBox.Show("Port number must be between 1 and 65535.");
                    return;
                }

                string URL = "http://localhost:5199";

                client = new RestClient(URL);

                RestRequest registerRequest = new RestRequest($"/api/clients/Register/{ip}/{port}", Method.Post);
                RestResponse registerResponse = await client.ExecuteAsync(registerRequest);
                if (!registerResponse.IsSuccessful)
                {
                    MessageBox.Show("Failed to register client: Duplicate Port Found");
                    return;
                }

                MessageBox.Show("Client registered successfully!");
                user = JsonConvert.DeserializeObject<Client>(registerResponse.Content);

                loop = true;

                server.StartServer(ip, port);//start server thread with ip + port provided

                StartNetworkThread();
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
                //api call + client job server call to save jobs
                string pythonName = PythonInput.Text;// = PythonNameInput.Text;
                if (string.IsNullOrEmpty(pythonName) )
                {
                    MessageBox.Show("Please Enter a python script");
                    return;
                }
                string pythonJob = PythonInput.Text;

                //Encoding and Hashing
                byte[] encodedArray = Encoding.UTF8.GetBytes(pythonJob);
                string encoded = Convert.ToBase64String(encodedArray);
                SHA256 sHA256 = SHA256.Create();
                byte[] pythonHash = sHA256.ComputeHash(encodedArray);

                Job newJob = new Job();
                newJob.clientId = user.clientId;
                newJob.jobName = pythonName;
                newJob.jobDescription = encoded;
                newJob.hash = pythonHash;
                newJob.jobStatus = Job.Status.NotStarted;
                //leave job Id as db will auto set it 

                RestRequest request = new RestRequest($"/api/jobs", Method.Post);
                request.AddJsonBody(newJob);

                RestResponse pythonJobResponse = await client.ExecuteAsync(request);
                if (!pythonJobResponse.IsSuccessful)
                {
                    MessageBox.Show("Failed to submit job to DB");
                    return;
                } else
                {
                    Job job = JsonConvert.DeserializeObject<Job>(pythonJobResponse.Content);
                    Console.WriteLine($"{user.clientId} : {job.jobId}: {job.jobDescription} ({job.jobName})");
                    server.submitJob(job);
                    Console.WriteLine($"Entered job into {user.clientId}'s server: {job.jobId} {job.jobDescription}");
                    MessageBox.Show("Submitted Job");
                }
            }
            else
            {
                MessageBox.Show("Please Register first");
                return;
            }
        }
        private async void Networking()
        {
            i = 0;
            while (loop)
            {
                i++;

                RestRequest request = new RestRequest("/api/clients/GetOtherClients/" + user.clientId, Method.Get);//in while loop to get new clients constantly after finishing scanning everyone
                RestResponse response = await client.ExecuteAsync(request);

                if (response.IsSuccessful)
                {

                    var otherClients = JsonConvert.DeserializeObject<List<Client>>(response.Content);

                    // Process the list of other clients as needed
                    Console.WriteLine($"{user.clientId} sees Amount of other clients: {otherClients.Count}");
                    Console.WriteLine($"ye{i}");

                    status = clientStatus.FindingJob;

                    foreach (var otherClient in otherClients)
                    {
                        if (!loop) { break; }
                        try
                        {
                            ChannelFactory<IClientJobServer> foobFactory;
                            NetTcpBinding tcp = new NetTcpBinding();
                            string otherClientURL = $"net.tcp://127.0.0.1:{otherClient.port}/ClientJobServer";//use loopback 127.0.0.1 for connection to self machine
                            foobFactory = new ChannelFactory<IClientJobServer>(tcp, otherClientURL);
                            IClientJobServer foob = foobFactory.CreateChannel();

                            Console.WriteLine($"Current Client ID: {user.clientId} -> Client ID: {otherClient.clientId}, IP: {otherClient.ip}, Port: {otherClient.port}");//console output to show which client is being looked at right now by which user

                            Job job = foob.GetJob();//call GetJob from the other client's server

                            if (job != null)
                            {
                                if (job.jobStatus == Job.Status.Finished)
                                {
                                    Console.WriteLine("Jobs already done");
                                    foob.Post(job);
                                    continue;//go look at diff client
                                }
                                if (!loop) { break; }

                                //do stuff here
                                Console.WriteLine("Client " + user.clientId + " obtained job from client ID " + otherClient.clientId);//
                                Console.WriteLine($"Job ID: {job.jobId}, Client ID: {job.clientId}, Job Description: {job.jobDescription}, Job Status: {job.jobStatus}");//

                                //Check encoded
                                byte[] pythonEncodedArray = Convert.FromBase64String(job.jobDescription);

                                //Check Hash
                                SHA256 sha256 = SHA256.Create();
                                byte[] hashCheck = sha256.ComputeHash(pythonEncodedArray);

                                if (hashCheck != null)
                                {
                                    if (hashCheck.SequenceEqual(job.hash))
                                    {
                                        if (!loop) { break; }
                                        status = clientStatus.Working;

                                        Console.WriteLine($"Client {user.clientId} has found Hashes are equal for {job.jobId}");
                                        //Since hashes are the same, can get string from encoded array as it isn't corrupted
                                        string python = Encoding.UTF8.GetString(pythonEncodedArray);
                                        Console.WriteLine($"Client {user.clientId} picked up job with python script :({python}) from {otherClient.clientId}");

                                        //Using python string which is the python script, let the iron python run it
                                        dynamic result = RunPythonJobDescription(python);
                                        await Task.Delay(2000);
                                        string res = Convert.ToString(result);
                                        if (result != null)
                                        {
                                            if (!loop) { break; }
                                            Console.WriteLine($"{user.clientId} executed code {job.jobId} with result: {res}");
                                            job.Result = res;
                                            foob.Post(job);
                                            job.jobStatus = Job.Status.Finished;
                                            jobsDone++;

                                            if (!loop) { break; }
                                            RestRequest request1 = new RestRequest($"/api/jobs/FinishedJob/{res}", Method.Put);
                                            request1.AddJsonBody(job);
                                            RestResponse response1 = await client.ExecuteAsync(request1);
                                            if (!response1.IsSuccessful)
                                            {
                                                Console.WriteLine($"Not successful in updating Finished Job {job.jobId}");
                                            }

                                            if(!loop) { break; }
                                            RestRequest request2 = new RestRequest($"/api/clients/FinishedJob/{user.clientId}", Method.Put);
                                            RestResponse response2 = await client.ExecuteAsync(request2);
                                            if (!response2.IsSuccessful)
                                            {
                                                Console.WriteLine($"Could not increment user {user.clientId} job counter");
                                            }
                                            status = clientStatus.FindingJob;
                                        }
                                    } else
                                    {
                                        Console.WriteLine($"Hashes do not match, something went wrong");
                                    }
                                }
                            }
                            else
                            {
                                Console.WriteLine("No job available from client ID " + otherClient.clientId);
                            } 
                        } catch(EndpointNotFoundException ex)//exception when catching client with dead server
                        {
                            Console.WriteLine("Caught Endpoint exception: " + ex.Message);//clears clients that are dead when they are found in list
                            RestRequest deleteRequest = new RestRequest($"/api/clients/UsePort/{otherClient.port}", Method.Delete);
                            RestResponse deleteResponse = await client.ExecuteAsync(deleteRequest);
                            if(!deleteResponse.IsSuccessful)
                            {
                                Console.WriteLine($"{user.clientId} failed to find and remove {otherClient.port}");
                            }
                        } catch (Exception ex)
                        {
                            //more generic
                            Console.WriteLine("Caught Exception: " + ex.Message);
                        }
                    }
                }
                else
                {
                    Console.WriteLine($"{user.clientId} can not see anyone");
                }
                await Task.Delay(1000);
            }
        }

        //make the networking run on a different thread
        private void StartNetworkThread()
        {
            Task.Run(() => Networking());
        }

        private dynamic RunPythonJobDescription(string pythonCode)
        {
            dynamic result = null;
            if (pythonCode.Contains("print(") )//parses and gets the middle value of the print()
            {
                pythonCode = pythonCode.Split('(', ')')[1];
                Console.WriteLine("Just split " + pythonCode);
            }

            try
            {
                ScriptEngine engine = Python.CreateEngine();
                ScriptScope scope = engine.CreateScope();
                result = engine.Execute(pythonCode, scope);
                Console.WriteLine("Inside python " + result);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Execution Error: " + ex);
            }
            return result;
        }

        //When closing window api call to delete itself from server
        protected override async void OnClosing(CancelEventArgs e)
        {
            this.Title = "Closing client threads, Please wait";

            loop = false;//stops networking loop

            if (user != null)//to prevent opening and closing a client without registering leading to an api call
            {
                RestRequest deleteRequest = new RestRequest($"/api/clients/UseID/{user.clientId}", Method.Delete);
                RestResponse response = await client.ExecuteAsync(deleteRequest);
                server.StopServer();

                if (!response.IsSuccessful)
                {
                    Console.WriteLine($"Could not delete from database client {user.clientId}");
                }

            }
        }

        //To see if client is working on a job or not
        private void Refresh_OnClick(object sender, RoutedEventArgs e)
        {
            JobCountTextBlock.Text = $"Jobs Finished: {jobsDone}";
            ClientWorkingTextBlock.Text = status.ToString();
        }
    }
}
