// Program.cs
//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Xml.Linq;

using System.ServiceModel;
using System.Configuration;
using SenApi.ServicesContract.PubSubContract;
using SenApi.Services.PubSubServices;

using SenApi.ServicesContract.ParallelPort;
using SenApi.BaseSensorsContract;

namespace DiscoClient
{
    public class SomeClass {
        int some_class_var;

        public SomeClass(int S) {
            some_class_var = S;
        }
        public void SomeMethod(string s,Uri u,EventData[] data){
            if(s=="PSC"){
            Console.WriteLine("port:{0}_value:{1}",data[0].Data_ID,data[0].Data_Val);
            Console.WriteLine("port:{0}_value:{1}", data[1].Data_ID, data[1].Data_Val);
            Console.WriteLine("port:{0}_value:{1}", data[2].Data_ID, data[2].Data_Val);
            }
            if(s=="DTMF")
            Console.WriteLine("dtmf code:{0}_duration:{1}", data[0].Data_Val, data[1].Data_Val);
            if (s == "NSTAT")
                Console.WriteLine("NSTAT from: {0}", u.ToString());
        }
    }
    class Client
    {
        public static  List<SensorGroupCtrl> ls ;
        public static void Main()
        {
            ls = new List<SensorGroupCtrl>();
            SomeClass A=new SomeClass(5);
            EventReciverService instance = new EventReciverService();
            
            instance.EventRecibed = A.SomeMethod;
            ServiceHost r_host = new ServiceHost(instance);            
            r_host.Open();
            BuildDevicesControls();
            ChannelFactory<IPubSubEventAPI> factory = new ChannelFactory<IPubSubEventAPI>("IPubSubEventAPI");
            IPubSubEventAPI client =  factory.CreateChannel();
            InitSensors();
            client.SubscribeForEvent("NSTAT", new ReciverEndpointInfo { Address = new Uri(ConfigurationManager.AppSettings["service_address"]), Binding = "KAKA" });
            client.SubscribeForEvent("DTMF", new ReciverEndpointInfo { Address = new Uri(ConfigurationManager.AppSettings["service_address"]), Binding = "KAKA" });
            client.SubscribeForEvent("PSC", new ReciverEndpointInfo { Address = new Uri(ConfigurationManager.AppSettings["service_address"]), Binding = "KAKA" });
            factory.Close();
            // Create a DiscoveryEndpoint that points to the DiscoveryProxy
            //Uri probeEndpointAddress = new Uri("net.tcp://localhost:8001/Probe");
            //DiscoveryEndpoint discoveryEndpoint = new DiscoveryEndpoint(new NetTcpBinding(), new EndpointAddress(probeEndpointAddress));
            //UdpDiscoveryEndpoint discoveryEndpoint = new UdpDiscoveryEndpoint();

           // DiscoveryClient discoveryClient = new DiscoveryClient(discoveryEndpoint);            

           // Console.WriteLine("Finding IParallelPortService endpoints");
           // Console.WriteLine();

            try
            {
                // Find ICalculatorService endpoints            
                //FindResponse findResponse = discoveryClient.Find(new FindCriteria(typeof(IPortServerContract)));

                //Console.WriteLine("Found {0} IParallelPortService endpoint(s).", findResponse.Endpoints.Count);
                //Console.WriteLine();
                // Check to see if endpoints were found, if so then invoke the service.
              
            }
            catch (TargetInvocationException)
            {
                Console.WriteLine("This client was unable to connect to and query the proxy. Ensure that the proxy is up and running.");
            }

            Console.WriteLine("Press <ENTER> to exit.");
            Console.ReadLine();
        }
        public static void InitSensors()
        {
            List<SensorGroupCtrl> sgs = ls.Where(g => g.isLocal == true).ToList();
            foreach (SensorGroupCtrl sg in sgs)
            {

                if (sg.hasDTMFS)
                {
                    try
                    {
                        ChannelFactory<IBaseSensorsContract> factory = new ChannelFactory<IBaseSensorsContract>("DTMF", new EndpointAddress(sg.dtmf_Address));
                        IBaseSensorsContract client = factory.CreateChannel();
                        if (!client.IsDetecting())
                            client.StartDetect();
                        factory.Close();
                       
                        sg.IsDTMFActive = true;
                    }
                    catch (Exception e)
                    {
                        sg.IsDTMFActive = false;
                        Console.WriteLine("No se pudo establecer comunicacion con el servicio: " + sg.dtmf_Address);
                    }
                }
                if (sg.hasPPort)
                {
                    try
                    {
                        ChannelFactory<IPortServerContract> factory = new ChannelFactory<IPortServerContract>("PPort", new EndpointAddress(sg.pport_Address));
                        IBaseSensorsContract client = factory.CreateChannel();
                        if (!client.IsDetecting())
                            client.StartDetect();
                        factory.Close();                      
                        sg.IsPPortActive = true;
                    }
                    catch (Exception e)
                    {
                        sg.IsPPortActive = false;
                        Console.WriteLine("No se pudo establecer comunicacion con el servicio: " + sg.pport_Address);
                    }
                }
            }
        }
        public static void BuildDevicesControls()
        {
            string xmlpath = ConfigurationManager.AppSettings["XMLConfigName"];
            var d = XDocument.Load(xmlpath);
            IEnumerable<XElement> l = d.Element("Devices").Elements("DeviceGroup");
            foreach (XElement dg in l)
            {
                string pagekey = dg.Attribute("code").Value;
                string rname = dg.Attribute("name").Value;
                string dname = dg.Attribute("desciption").Value;
                bool local = Convert.ToBoolean(dg.Attribute("local").Value);
                bool dtmf = Convert.ToBoolean(dg.Attribute("dtmf").Value);
                bool pport = Convert.ToBoolean(dg.Attribute("pport").Value);
                Uri dtmf_add = null;
                Uri pport_add = null;
                try
                {
                    if (local)
                    {
                        dtmf_add = new Uri(dg.Attribute("dtmf_address").Value);
                        pport_add = new Uri(dg.Attribute("pport_address").Value);
                    }
                }
                catch (Exception)
                {
                    
                }
                var sg = new SensorGroupCtrl(pagekey, rname, dname, local, dtmf, pport, pport_add, dtmf_add);
                sg.lastContact = DateTime.Now;   
                ls.Add(sg); 
            }
        }
    }
}