using RestSharp;
using System.Collections.Generic;
using System.Windows;
using APIClasses;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace ClientDesktop
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Client user;
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void RegisterButton_OnClick(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Register Button Clicked");

            string ip = IPInput.Text;
            string port = PortInput.Text;

            if (string.IsNullOrEmpty(ip) || string.IsNullOrEmpty(port))
            {
                MessageBox.Show("IP and Port must be provided");
                return;
            }

            Networking(ip, port);
            //disable register button and inputs
            RegisterClientButton.Visibility = Visibility.Hidden;
        }
        private async void Networking(string ip, string port)
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

            while (true)
            {
                RestRequest request = new RestRequest("/api/clients/GetOtherClients/" + user.clientId, Method.Get);
                var response = await client.ExecuteAsync(request);

                if (response.IsSuccessful)
                {
                    var otherClients = JsonConvert.DeserializeObject<List<Client>>(response.Content);

                    // Process the list of other clients as needed
                    Console.WriteLine("Amount of other clients: " + otherClients.Count);

                    foreach (var otherClient in otherClients)
                    {
                        Console.WriteLine($"Client ID: {otherClient.clientId}, IP: {otherClient.ip}, Port: {otherClient.port}");
                        RestRequest jobRequest = new RestRequest($"/api/jobs/GetJobsByClientId/{otherClient.clientId}", Method.Get);
                        var jobResponse = await client.ExecuteAsync(jobRequest);
                        if (jobResponse.IsSuccessful)
                        {
                            var jobs = JsonConvert.DeserializeObject<List<Job>>(jobResponse.Content);
                            foreach (var job in jobs)
                            {
                                if (job.jobStatus) continue; // Skip already finished jobs

                                Console.WriteLine($"Job ID: {job.jobId}, Client ID: {job.clientId}, Job Description: {job.jobDescription}, Job Status: {job.jobStatus}");

                                await Task.Delay(5000); // Simulate job processing time //idk how necessary delaying is tho tbh
                                // Mark the job as finished
                                RestRequest finishJobRequest = new RestRequest($"/api/jobs/FinishedJob/{job.jobId}", Method.Put);
                                var finishJobResponse = await client.ExecuteAsync(finishJobRequest);
                                if (finishJobResponse.IsSuccessful)
                                {
                                    Console.WriteLine($"Job ID: {job.jobId} marked as finished.");
                                }
                                else
                                {
                                    Console.WriteLine($"Failed to mark Job ID: {job.jobId} as finished. Error: {finishJobResponse.ErrorMessage}");
                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine("Failed to retrieve jobs for client ID " + otherClient.clientId + ": " + jobResponse.ErrorMessage);
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Failed to retrieve other clients: " + response.ErrorMessage);
                }
            }
        }
    }
}
