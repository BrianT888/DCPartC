using System.ServiceModel;

namespace APIClasses
{
    [ServiceContract]
    public interface IClientJobServer
    {
        [OperationContract]
        void Execute(Job job);//upload answer
        [OperationContract]
        bool GetStatus();
        [OperationContract]
        Job GetJob();//download job
    }
}
