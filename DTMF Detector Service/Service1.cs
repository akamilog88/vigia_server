using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.ServiceModel;
using System.ServiceModel.Web;

using System.Configuration;
using SenApi.ServicesContract.PubSubContract;
using SenApi.SensorInterfaces.DTMFDetector;
using SenApi.Services.DTMF;

namespace DTMF_Detector_Service
{
    public partial class Service1 : ServiceBase
    {
        DTMFDetector detector;
        Timer timer;
        bool detecting;
        int interval;
        private ServiceHost host;

        DTMFServiceCtrl wcfInstance;

        public Service1()
        {
            InitializeComponent();
            detector = new DTMFDetector();
       
            wcfInstance = new DTMFServiceCtrl();
            wcfInstance.onStopDetect += wcfInstance_onStopDetect;
            wcfInstance.onStartDetect += wcfInstance_onStartDetect;
            host = new ServiceHost(wcfInstance);
           
            detector.ToneDetected += detector_ToneDetected;
            detector.EndRecording += detector_EndRecording;
            detector.DetectSilence = false;
           
            detecting = false;
            try
            {
                interval = Convert.ToInt16(ConfigurationManager.AppSettings["service_address"]);
                var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                System.Configuration.AppSettingsSection section = config.AppSettings;
                detecting = Convert.ToBoolean(section.Settings["detecting"].Value);
            }
            catch (Exception e) {
                interval = 1;
            }
            if (detecting)
            {
                detector.StartDetect();
                wcfInstance.StartDetect();
            }
            timer = new Timer(SelfNotification, null, 0, interval * 60000);
        }

        void wcfInstance_onStartDetect(object sender, EventArgs e)
        {
            detecting = true;
            detector.StartDetect();
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            System.Configuration.AppSettingsSection section = config.AppSettings;
            section.Settings.Remove("detecting");
            section.Settings.Add("detecting", "true");
            config.Save(ConfigurationSaveMode.Modified);
        }

        void wcfInstance_onStopDetect(object sender, EventArgs e)
        {
            detecting = false;
            detector.Stop();
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            System.Configuration.AppSettingsSection section = config.AppSettings;
            section.Settings.Remove("detecting");
            section.Settings.Add("detecting", "false");
            config.Save(ConfigurationSaveMode.Modified);
        }

        void detector_EndRecording(object sender, EventArgs e)
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
        private void SelfNotification(object o) {
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
        void detector_ToneDetected(object sender, DTMFDetectedArgs e)
        {
            if(wcfInstance.IsDetecting())
                PublishEvent(e.ToneAscii, e.Duration);
        }

        private void PublishEvent(string ascii,double duration)
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

        protected override void OnStart(string[] args)
        {
            try
            {
                host.Open();
              
            }
            catch {
               
            }
        }
        
        protected override void OnStop()
        {
            detector.Stop();
            host.Close();
            timer.Dispose();
            timer = null;
        }
    }
}
