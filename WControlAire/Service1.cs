using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Configuration;
using System.IO;
using System.ServiceModel;
using SenApi.ServicesContract.PubSubContract;
using SenApi.Services.PubSubServices;
using SenApi.ServicesContract.ParallelPort;

namespace WControlAire
{
    public partial class Service1 : ServiceBase
    {
         Timer timer;
         Thread init;
         EventReciverService reciver;
         DateTime lastPinch;
         int timer_interval;
         int minTimeTilNext;
         object syncro;

        public Service1()
        {
            syncro = new object();
            lastPinch = new DateTime(1, 1, 1);
            try
            {
                timer_interval = Convert.ToInt32(ConfigurationManager.AppSettings["period"]) * 60 * 1000;
                minTimeTilNext = Convert.ToInt32(ConfigurationManager.AppSettings["minSpan"]) * 60 * 1000;
            }
            catch (Exception e) {
                timer_interval = 120 * 60 * 1000;
                minTimeTilNext = 60 * 60 * 1000;
            }
            timer = new Timer(SwitchState, syncro, 0, timer_interval);
        }

        protected override void OnStart(string[] args)
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
                    CleanPort(syncro);    
                });
                init.Start();
            }
            catch (Exception exc)
            {

            }
        }

        void OnRecive(string event_name, Uri sender, EventData[] data)
        {
            lock (syncro)
            {
                if (Convert.ToInt32(data[1].Data_Val) == 63)
                {
                    SwitchState(syncro);
                    timer.Change(0, timer_interval);
                }
            }
        }

        void CleanPort(object o)
        {
            lock (o)
            {
                ChannelFactory<IPortServerContract> factory = new ChannelFactory<IPortServerContract>("IParallelPortAPI");
                IPortServerContract client = factory.CreateChannel();
                client.WritePortValue(888, 0);
                factory.Close();
            }
        }

        void SwitchState(object o) {
            lock (o)
            {
                TimeSpan span;
                span = DateTime.Now - lastPinch;

                if (span > new TimeSpan(0, 0, minTimeTilNext))
                {
                    lastPinch = DateTime.Now;
                    ChannelFactory<IPortServerContract> factory = new ChannelFactory<IPortServerContract>("IParallelPortAPI");
                    IPortServerContract client = factory.CreateChannel();
                    client.WritePortValue(888, 1);
                    Thread.Sleep(2000);
                    client.WritePortValue(888, 0);
                    factory.Close();
                    StreamWriter s = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "Log.txt", true);
                    s.WriteLine(DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + " (Cambio de Aire)");
                    s.WriteLine("----------------------------------------");
                    s.Flush();
                    s.Close();
                }
            }
        }

        protected override void OnStop()
        {
            timer.Dispose();
            timer = null;            
        }
    }
}
