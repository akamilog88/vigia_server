using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using SenApi.Services.ParallelPort;

namespace ParallelPortServiceHost
{
    class Program
    {
        static void Main(string[] args)
        {
            ServiceHost host = new ServiceHost(new ParallelPortServer());
            host.Open();
            var port = host.Description.Endpoints.Single(o => o.Contract.ContractType.ToString().Contains("IPortServerContract")).Address.Uri.Port;
            Console.WriteLine("***ParallelPort Service Started on port:{0}***",port);
            Console.WriteLine("***Press <ENTER> To EXIT***");
            Console.ReadKey();
            host.Close();
        }
    }
}
