using APIClasses;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;

namespace ClientDesktop
{
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, UseSynchronizationContext = false)]
    internal class ClientJobServer : IClientJobServer
    {
        private ConcurrentQueue<Job> jobList = new ConcurrentQueue<Job>();//list of jobs in server in concurrent q for thread safety if multiple access the data at once
        public string Execute(string python)//run the python code tho idk how to do that yet
        {
            throw new System.NotImplementedException();
        }

        public Job GetJob()
        {
            if (jobList.TryDequeue(out Job job))
            {
                return job;//if able to dequeue job then return the job
            }
            else
            {
                return null; // No job available for null check
            }
        }

        public bool GetStatus()//do i need to get status?
        {
            throw new System.NotImplementedException();
        }

        public void SubmitJob(Job job)
        {
            jobList.Enqueue(job);//add job to the back of queue
        }
    }
}
