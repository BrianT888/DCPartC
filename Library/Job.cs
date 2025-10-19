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

        public bool jobStatus { get; set; } = false;//lowk might have to change this into smt that can be posted for more description?
    }
}
