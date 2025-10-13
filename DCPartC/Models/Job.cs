namespace DCPartC.Models
{
    public class Job
    {
        public int jobId { get; set; }
        
        public int clientId { get; set; }

        public string jobName { get; set; }
        public string jobDescription { get; set; }
    }
}
