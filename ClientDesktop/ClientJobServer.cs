using APIClasses;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;

namespace ClientDesktop
{
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, UseSynchronizationContext = false)]
    public class ClientJobServer : IClientJobServer
    {
        public static ConcurrentQueue<Job> jobList = new ConcurrentQueue<Job>();//list of jobs in server in concurrent q for thread safety if multiple access the data at once
        public void Post(Job job)
        {
            job.jobStatus = Job.Status.Finished;
            jobList.Enqueue(job);//put it back into the list
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
            //0 = not started, 1=progress, 2=finished
            //foreach (Job job in jobList)
            //{
            //    if (job.jobStatus == 0)
            //    {
            //        return job;
            //    }
            //}
            //return null;
        }

        public bool GetStatus()//do i need to get status?
        {
            throw new System.NotImplementedException();
        }

        public void SubmitJob(Job job)
        {
            jobList.Enqueue(job);//add job to the back of queue
        }

        public int GetNumberOfJobs()
        {
            return jobList.Count;
        }
    }
}
