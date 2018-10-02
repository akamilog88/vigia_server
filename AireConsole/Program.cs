using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using SenApi.ServicesContract.ParallelPort;
using SenApi.ServicesContract.PubSubContract;
using SenApi.Services.PubSubServices;
using System.Configuration;
using System.ServiceModel;
using System.IO;

namespace AireConsole
{
    class Program
    {
        static Timer timer;
        static Thread init;
        static EventReciverService reciver;
        static DateTime lastPinch;
        static int timer_interval;
        static int minTimeTilNext;
        static object syncro;

        static void Main(string[] args)
        {
            syncro = new object();
            lastPinch = new DateTime(1, 1, 1);
            timer_interval = Convert.ToInt32(ConfigurationManager.AppSettings["period"]) * 60 * 1000;
            minTimeTilNext = Convert.ToInt32(ConfigurationManager.AppSettings["minSpan"]);
            timer = new Timer(SwitchState, syncro, 0, timer_interval);
            String[] s = {""};
            OnStart(s);
            CleanPort(syncro);
            Console.WriteLine("***Press Any KEy to Exit***");
            Console.ReadKey();
        }
        static void OnStart(string[] args)
        {
            try
            {
                init = new Thread(o =>
                {
                    ChannelFactory<IPubSubEventAPI> factory = new ChannelFactory<IPubSubEventAPI>("IPubSubEventAPI");
                    IPubSubEventAPI client = factory.CreateChannel();
                    var r = new ReciverEndpointInfo { Address = new Uri(ConfigurationSettings.AppSettings["service_address"]), Binding = "KAKA" };
                    client.SubscribeForEvent("PSC", r);
                    factory.Close();

                    reciver = new EventReciverService();
                    reciver.EventRecibed = OnRecive;
                    ServiceHost h = new ServiceHost(reciver);
                    h.Open();
                });
                init.Start();
            }
            catch (Exception exc)
            {

            }
        }
        static void OnRecive(string event_name, Uri sender, EventData[] data)
        {          
            lock(syncro){  
                if (Convert.ToInt32(data[1].Data_Val) == 63)
                {                
                    SwitchState(syncro);
                    timer.Change(0, timer_interval);
                }
            }
        }
        static void CleanPort(object o) {
            lock (o)
            {
                ChannelFactory<IPortServerContract> factory = new ChannelFactory<IPortServerContract>("IParallelPortAPI");
                IPortServerContract client = factory.CreateChannel();
                client.WritePortValue(888, 0);
                factory.Close();
            }
        }

        static void SwitchState(object o)
        {
            lock (o)
            {
                TimeSpan span;
                span = DateTime.Now - lastPinch;
                
                if (span > new TimeSpan(0, minTimeTilNext, 0))
                {
                    lastPinch = DateTime.Now;
                    ChannelFactory<IPortServerContract> factory = new ChannelFactory<IPortServerContract>("IParallelPortAPI");
                    IPortServerContract client = factory.CreateChannel();
                    client.WritePortValue(888, 1);
                    Thread.Sleep(2000);
                    client.WritePortValue(888, 0);
                    factory.Close();
                    StreamWriter s = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "Log.txt",true);
                    s.WriteLine(DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() +" (Cambio de Aire)");
                    s.WriteLine("----------------------------------------");
                    s.Flush();
                    s.Close();
                }
            }
        }
    }
}
