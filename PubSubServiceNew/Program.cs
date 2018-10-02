// program.cs
//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

using System;
using System.ServiceModel;

namespace SenApi.Services.PubSubServices
{
    class Program
    {
        public static void Main()
        {

            // Host the DiscoveryProxy service
            ServiceHost proxyServiceHost = new ServiceHost(new PubSubService());

            try
            {
                // Add DiscoveryEndpoint to receive Probe and Resolve messages
                //DiscoveryEndpoint discoveryEndpoint = new DiscoveryEndpoint(new NetTcpBinding(), new EndpointAddress(probeEndpointAddress));
                //discoveryEndpoint.IsSystemEndpoint = false;

                // Add AnnouncementEndpoint to receive Hello and Bye announcement messages
                //AnnouncementEndpoint announcementEndpoint = new AnnouncementEndpoint(new NetTcpBinding(), new EndpointAddress(announcementEndpointAddress));

                //proxyServiceHost.AddServiceEndpoint(discoveryEndpoint);
                //proxyServiceHost.AddServiceEndpoint(announcementEndpoint);

                proxyServiceHost.Open();

                Console.WriteLine("PubSub Service started.");
                Console.WriteLine();
                Console.WriteLine("Press <ENTER> to terminate the service.");
                Console.WriteLine();
                Console.ReadLine();

                proxyServiceHost.Close();
            }
            catch (CommunicationException e)
            {
                Console.WriteLine(e.Message);
            }
            catch (TimeoutException e)
            {
                Console.WriteLine(e.Message);
            }

            if (proxyServiceHost.State != CommunicationState.Closed)
            {
                Console.WriteLine("Aborting the service...");
                proxyServiceHost.Abort();
            }
        }
    }
}