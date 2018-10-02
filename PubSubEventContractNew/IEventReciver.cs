using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;

namespace SenApi.ServicesContract.PubSubContract
{
    [ServiceContract]
    public interface IEventReciver
    {
        [OperationContract(IsOneWay =true)]
        void ReciveEvent(string event_name,Uri info,EventData[] data);
    }
}
