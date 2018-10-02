using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using SenApi.Services.PubSubServices;

namespace PubSubServiceHost
{
    class Program
    {
        static void Main(string[] args)
        {
            ServiceHost host =  new ServiceHost(typeof(PubSubService));
            host.Open();
            var port = host.Description.Endpoints.Single(o => o.Contract.ContractType.ToString().Contains("IPubSubEventAPI")).Address.Uri.Port;
            Console.WriteLine("***PubSub Service Started on port:{0}***", port);
            Console.WriteLine("***Press <ENTER> To EXIT***");
            Console.ReadKey();
        }
    }
}
