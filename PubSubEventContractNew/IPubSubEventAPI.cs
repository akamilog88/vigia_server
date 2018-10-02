using System;
using System.ServiceModel;


namespace SenApi.ServicesContract.PubSubContract
{
    [ServiceContract]
    public interface IPubSubEventAPI
    {
        [OperationContract(IsOneWay =true)]
        void PublishEvent(String event_name, Uri sender, EventData[] data);
        [OperationContract(IsOneWay = true)]
        void SubscribeForEvent(String event_name, ReciverEndpointInfo listener);
        [OperationContract(IsOneWay = true)]
        void UnSubscribe(String event_name, ReciverEndpointInfo listener);
    }
}
