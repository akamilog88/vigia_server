using System;

using System.Configuration;
using SenApi.ServicesContract.PubSubContract;
using SenApi.SensorInterfaces.DTMFDetector;
using System.ServiceModel;

namespace DtmfDetector
{
    class Program
    {
        static void Main(string[] args)
        {
            var d = new DTMFDetector();
            d.ToneDetected += d_ToneDetected;
            d.StartDetect();
            Console.WriteLine("Press <Enter> to exit");
            ConsoleKeyInfo c;
            while ((c = Console.ReadKey()).Key != ConsoleKey.Enter) {
                Console.WriteLine("Press <Enter> to exit");
            }            
        }

        static void d_ToneDetected(object sender, DTMFDetectedArgs e)
        {
            PublishEvent(e.ToneAscii, e.Duration);
        }
        static void PublishEvent(string ascii, double duration)
        {
            ChannelFactory<IPubSubEventAPI> factory = new ChannelFactory<IPubSubEventAPI>("IPubSubEventAPI");
            try
            {
                IPubSubEventAPI client = factory.CreateChannel();
                client.PublishEvent("DTMF", new Uri(ConfigurationManager.AppSettings["service_address"]), new EventData[]{
                new EventData{Data_ID="ASCII",Data_Val =ascii},  
                new EventData{Data_ID="Duration",Data_Val =duration}
                });
            }
            catch (Exception e)
            {
                Console.WriteLine("**No se pudo establecer el canal de comunicacion con el subsistema Pub-Sub de eventos**, Detalle: " + e.Message);
                Console.WriteLine("**Detalle de la excepcion:{0}**" + e.Message);
            }
        }
    }
}
