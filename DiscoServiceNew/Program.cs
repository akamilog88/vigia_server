// Program.cs
//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

using System;
using System.ServiceModel;
using System.ServiceModel.Description;



namespace DiscoveryProxyModel
{
    class Program
    {
        public static void Main()
        {
           // Uri baseAddress = new Uri("net.tcp://localhost:9002/CalculatorService/" + Guid.NewGuid().ToString());
           // Uri announcementEndpointAddress = new Uri("net.tcp://localhost:9021/Announcement");

           // ServiceHost serviceHost = new ServiceHost(typeof(CalculatorService), baseAddress);
            ServiceHost serviceHost = new ServiceHost(typeof(CalculatorService));
            try
            {
                //ServiceEndpoint netTcpEndpoint = serviceHost.AddServiceEndpoint(typeof(ICalculatorService), new NetTcpBinding(), string.Empty);
                //serviceHost.AddServiceEndpoint(new DiscoveryEndpoint());

                // Create an announcement endpoint, which points to the Announcement Endpoint hosted by the proxy service.
                //AnnouncementEndpoint announcementEndpoint = new AnnouncementEndpoint(new NetTcpBinding(), new EndpointAddress(announcementEndpointAddress));
                //ServiceDiscoveryBehavior serviceDiscoveryBehavior = new ServiceDiscoveryBehavior();
                //serviceDiscoveryBehavior.AnnouncementEndpoints.Add(announcementEndpoint);

                // Make the service discoverable
                //serviceHost.Description.Behaviors.Add(serviceDiscoveryBehavior);

                serviceHost.Open();

                Console.WriteLine("Calculator Service started ");
                Console.WriteLine();
                Console.WriteLine("Press <ENTER> to terminate the service.");
                Console.WriteLine();
                Console.ReadLine();

                serviceHost.Close();
            }
            catch (CommunicationException e)
            {
                Console.WriteLine(e.Message);
                Console.ReadLine();
            }
            catch (TimeoutException e)
            {
                Console.WriteLine(e.Message);
                Console.ReadLine();
            }

            if (serviceHost.State != CommunicationState.Closed)
            {
                Console.WriteLine("Aborting the service...");
                serviceHost.Abort();
            }
        }

    }
}