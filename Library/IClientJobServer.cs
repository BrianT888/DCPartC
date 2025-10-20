using System.ServiceModel;

namespace APIClasses
{
    [ServiceContract]
    public interface IClientJobServer
    {
        [OperationContract]
        void Post(Job job);//upload answer
        [OperationContract]
        Job GetJob();//download job
    }
}
