using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIClasses
{
    public class Job
    {
        public int jobId { get; set; }

        public int clientId { get; set; }

        public string jobName { get; set; }
        public string jobDescription { get; set; }//python script
        public byte[] hash { get; set; }//for sha and checking python script didn't lost anything

        public enum Status {
            NotStarted,
            Progressing,
            Finished
        };

        public Status jobStatus { get; set; }
        public string Result { get; set; }//to store back whatever is the result of the python code
    }
}
