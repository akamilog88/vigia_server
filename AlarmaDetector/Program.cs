using System;
using System.Collections.Generic;
using  System.ComponentModel;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Threading;
using System.Speech.Synthesis;
using System.ServiceModel;
using SenApi.ServicesContract.PubSubContract;
using SenApi.Services.PubSubServices;


namespace AlarmaDetector
{
    
    class Program
    {
        private static BindingList<EventData[]> detected;

        static void Main(string[] args)
        {
            detected = new BindingList<EventData[]>();
            EventReciverService reciver = new EventReciverService();
            ServiceHost host = new ServiceHost(reciver);
            reciver.EventRecibed = AlarmEventHandler;
            host.Open();
            SubscribeForEvent();
            Console.WriteLine("Press Enter to Exit");
            Console.ReadKey();
        }

        private static void SubscribeForEvent() {
            ChannelFactory<IPubSubEventAPI> factory = new ChannelFactory<IPubSubEventAPI>("IPubSubEventAPI");
            IPubSubEventAPI client =  factory.CreateChannel();
            client.SubscribeForEvent("PSC", new ReciverEndpointInfo { Address = new Uri(ConfigurationManager.AppSettings["service_address"].ToString()),Binding="" });
            factory.Close();
        }

        private static void AlarmEventHandler(string evt,Uri sender,EventData[] data){
            EventData[] ocurrencias = detected.Single(d => (d[0].Data_Val == data[0].Data_Val && d[1].Data_Val == data[1].Data_Val && d[2].Data_Val == data[2].Data_Val));
            if (ocurrencias == null)
                detected.Add(data);
        }

        private static void FireCodeAlarm() {
            int count = 0;            
                while (count++ < 5)
                {
                    SpeechSynthesizer speaker = new SpeechSynthesizer();
                    speaker.Speak("Alarma intrusa en el departamento");
                    Thread.Sleep(1000);
                }           
        }
    }
}
