using System.ServiceModel;

namespace APIClasses
{
    [ServiceContract]
    public interface IClientJobServer
    {
        [OperationContract]
        string Execute(string python);//upload answer
        [OperationContract]
        bool GetStatus();
        [OperationContract]
        Job GetJob();//download job
        [OperationContract]
        void SubmitJob(Job job);//submit job back to server
    }
}
