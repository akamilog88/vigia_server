using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using SenApi.BaseSensorsContract;
using SenApi.Services.DTMF;
using SenApi.SensorInterfaces.DTMFDetector;
using System.Threading;
using SenApi.ServicesContract.PubSubContract;
using System.Configuration;

namespace DtmfCtrlServiceHost
{
    class Program
    {
        static DTMFDetector detector;
        static Timer timer;
        static bool detecting;
        static int interval = 0;
        static ServiceHost host;

        static DTMFServiceCtrl wcfInstance;

        static void Main(string[] args)
        {
          

            wcfInstance = new DTMFServiceCtrl();
            host = new ServiceHost(wcfInstance);
            host.Open();

            detector = new DTMFDetector();
            wcfInstance.onStopDetect += wcfInstance_onStopDetect;
            wcfInstance.onStartDetect += wcfInstance_onStartDetect;
            

            detector.ToneDetected += detector_ToneDetected;
            detector.EndRecording += detector_EndRecording;
            detector.DetectSilence = false;

            detecting = false;
            try
            {
                interval = Convert.ToInt16(ConfigurationManager.AppSettings["service_address"]);
            }
            catch (Exception e)
            {
                interval = 1;
            }
            timer = new Timer(SelfNotification, null, 0, interval * 60000);

            Console.WriteLine("***DTMFServiceCtrl Started***");
            Console.WriteLine("***Press <ENTER> To EXIT***");
            Console.ReadKey();
        }

        static  void wcfInstance_onStartDetect(object sender, EventArgs e)
        {
            detecting = true;
            detector.StartDetect();
        }

        static void wcfInstance_onStopDetect(object sender, EventArgs e)
        {
            detecting = false;
            detector.Stop();
        }

        static void detector_EndRecording(object sender, EventArgs e)
        {
            if (wcfInstance.IsDetecting())
            {
                ChannelFactory<IPubSubEventAPI> factory = new ChannelFactory<IPubSubEventAPI>("IPubSubEventAPI");
                try
                {
                    IPubSubEventAPI client = factory.CreateChannel();
                    client.PublishEvent("ENDOR", new Uri(ConfigurationManager.AppSettings["service_address"]), new EventData[]{
                    new EventData{Data_ID="Alarm",Data_Val = detecting}  
                    });
                }
                catch (Exception excp)
                {
                    Console.WriteLine("**No se pudo establecer el canal de comunicacion con el subsistema Pub-Sub de eventos**, Detalle: " + excp.Message);
                    Console.WriteLine("**Detalle de la excepcion:{0}**" + excp.Message);
                }
            }
        }
        static void SelfNotification(object o)
        {
            if (wcfInstance.IsDetecting())
            {
                ChannelFactory<IPubSubEventAPI> factory = new ChannelFactory<IPubSubEventAPI>("IPubSubEventAPI");
                try
                {
                    IPubSubEventAPI client = factory.CreateChannel();
                    client.PublishEvent("NSTAT", new Uri(ConfigurationManager.AppSettings["service_address"]), new EventData[]{
                new EventData{Data_ID="DETECT",Data_Val = detecting}  
                });
                }
                catch (Exception e)
                {
                    Console.WriteLine("**No se pudo establecer el canal de comunicacion con el subsistema Pub-Sub de eventos**, Detalle: " + e.Message);
                    Console.WriteLine("**Detalle de la excepcion:{0}**" + e.Message);
                }
            }
        }
        static void detector_ToneDetected(object sender, DTMFDetectedArgs e)
        {
            if (wcfInstance.IsDetecting())
                PublishEvent(e.ToneAscii, e.Duration);
        }

        static void PublishEvent(string ascii, double duration)
        {
            ChannelFactory<IPubSubEventAPI> factory = new ChannelFactory<IPubSubEventAPI>("IPubSubEventAPI");
            try
            {
                IPubSubEventAPI client = factory.CreateChannel();
                client.PublishEvent("DTMF", new Uri(ConfigurationManager.AppSettings["service_address"]), new EventData[]{
                    new EventData{Data_ID="TIME",Data_Val =DateTime.Now},
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
