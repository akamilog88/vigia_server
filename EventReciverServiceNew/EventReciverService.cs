using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using SenApi.ServicesContract.PubSubContract;

namespace SenApi.Services.PubSubServices
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Single)]
    public class EventReciverService : IEventReciver
    {
        public Action<string, Uri, EventData[]> EventRecibed;
        public void ReciveEvent(string event_name, Uri sender, EventData[] data) {
            Console.WriteLine("Event {0} recived, from {1} ", event_name, sender.ToString());
            if (EventRecibed != null) {
                    Console.WriteLine("***Event Handler INVOKED***");
                try
                {
                    EventRecibed.BeginInvoke(event_name, sender, data, null, null);
                }catch(Exception e){
                    Console.WriteLine("***Exception Invoking the EventReciver***");
                }
                //EventRecibed(event_name, sender, data);
            }           
        }
    }
}
