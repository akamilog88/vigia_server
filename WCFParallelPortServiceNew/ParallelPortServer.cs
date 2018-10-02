using System;
using System.ServiceModel;

using System.Threading;
using System.Configuration;
using SenApi.ServicesContract.ParallelPort;
using SenApi.ServicesContract.PubSubContract;

using SenApi.SensorInterfaces.ParallelPort;


namespace SenApi.Services.ParallelPort
{
    // NOTA: puede usar el comando "Rename" del menú "Refactorizar" para cambiar el nombre de clase "Service1" en el código y en el archivo de configuración a la vez.
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class ParallelPortServer : IPortServerContract
    {
        private PortAccessAPI port_api;
        private Timer reader;
        Timer self_timer;
        private object _lock_state;
        private object _publish_sync;
        private ParallelPortInfo current_state;
        private bool detecting;
        int interval;

        public ParallelPortServer() {
            port_api = PortAccessAPI.GetInstance();
            _lock_state = new object();
            _publish_sync = new object();
            current_state = ReadAllPortValue();
            reader = new Timer(CheckForStateChanged, null, 10, 10);
            detecting = false;
           
            try
            {
                interval = Convert.ToInt16(ConfigurationManager.AppSettings["NetChekPeriod"]);
                var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                System.Configuration.AppSettingsSection section = config.AppSettings;
                detecting = Convert.ToBoolean(section.Settings["detecting"].Value);
                Console.WriteLine("***Recovering Detection State:...{0}***",detecting);
            }
            catch (Exception e)
            {
                interval = 1;
            }
            if (detecting)
                StartDetect();
        }
       
        public int ReadPortValue(int port)
        {
            lock (port_api)
            {
                return port_api.Input(port);
            }
        }
        public void StartDetect() {
            reader = new Timer(CheckForStateChanged, _lock_state, 10, 10);
            
            self_timer = new Timer(SelfNotification, null, 0, interval * 60000);
            Console.WriteLine("***Start Detecting***");
            detecting = true;
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            System.Configuration.AppSettingsSection section = config.AppSettings;            
            section.Settings.Remove("detecting");
            section.Settings.Add("detecting", "true");
            config.Save(ConfigurationSaveMode.Modified);
        }

        public void StopDetect() {
            detecting = false;
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            System.Configuration.AppSettingsSection section = config.AppSettings;           
            section.Settings.Remove("detecting");
            section.Settings.Add("detecting", "true");
            config.Save(ConfigurationSaveMode.Modified);
            
            Console.WriteLine("***Stop Detecting***");
            reader.Dispose();
            self_timer.Dispose();
        }

        public bool IsDetecting() {
            return detecting;
        }

        public int[] ReadPortsValueArray() { 
            int[] result= new int[3];
            var r= new ParallelPortInfo(port_api.Input(ParallelPortInfo.P378),port_api.Input(ParallelPortInfo.P379),port_api.Input(ParallelPortInfo.P37A));
            result[0]=r.getPortState(ParallelPortInfo.P378);
            result[1]=r.getPortState(ParallelPortInfo.P379);
            result[2]=r.getPortState(ParallelPortInfo.P37A);
            return result;
        }

        public void WritePortValue(int port,int val) {
            lock (port_api)
            {
                port_api.Output(port, val);
            }
        }

        private void CheckForStateChanged(object _lock_state)
        {        
            ParallelPortInfo _read;
            if (detecting)
            {
                lock (this._lock_state)
                {
                    _read = ReadAllPortValue();
                    if (!_read.IQualTo(current_state))
                    {
                        current_state = _read;
                        new Thread(PublishEvent).Start(_read);
                    }
                }
            }
        }

        public ParallelPortInfo ReadAllPortValue()
        {
            return new ParallelPortInfo(port_api.Input(ParallelPortInfo.P378),port_api.Input(ParallelPortInfo.P379),port_api.Input(ParallelPortInfo.P37A));
        }

        private void PublishEvent(object inf)
        {
            if (detecting)
            {
                lock (_publish_sync)
                {
                    ParallelPortInfo info = inf as ParallelPortInfo;
                    ChannelFactory<IPubSubEventAPI> factory = new ChannelFactory<IPubSubEventAPI>("IPubSubEventAPI");
                    try
                    {
                        IPubSubEventAPI client = factory.CreateChannel();
                        client.PublishEvent("PSC", new Uri(ConfigurationManager.AppSettings["service_address"]), new EventData[]{
                            new EventData{Data_ID="TIME",Data_Val=DateTime.Now },
                            new EventData{Data_ID="P378",Data_Val=info.getPortState(ParallelPortInfo.P378)},
                            new EventData{Data_ID="P379",Data_Val=info.getPortState(ParallelPortInfo.P379)},
                            new EventData{Data_ID="P37A",Data_Val=info.getPortState(ParallelPortInfo.P37A)}
                    });
                        Console.WriteLine("***Event Publihed***" + "Event Type:" + "PSC");
                        Console.WriteLine("TIME:{0}",DateTime.Now );
                        Console.WriteLine("port:{0}_value:{1}", "378", info.getPortState(ParallelPortInfo.P378));
                        Console.WriteLine("port:{0}_value:{1}", "379", info.getPortState(ParallelPortInfo.P379));
                        Console.WriteLine("port:{0}_value:{1}", "37A", info.getPortState(ParallelPortInfo.P37A));
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("**No se pudo establecer el canal de comunicacion con el subsistema Pub-Sub de eventos***, Evento PSC***");
                        Console.WriteLine("**Detalle de la excepcion:{0}**" + e.Message);
                    }
                }
            }
        }
        
      
        
        private void SelfNotification(object o)
        {
            if (detecting)
            {
                ChannelFactory<IPubSubEventAPI> factory = new ChannelFactory<IPubSubEventAPI>("IPubSubEventAPI");
                try
                {
                    IPubSubEventAPI client = factory.CreateChannel();
                    client.PublishEvent("NSTAT", new Uri(ConfigurationManager.AppSettings["service_address"]), new EventData[]{
                    new EventData{Data_ID="DETECT",Data_Val = true}  
                });
                }
                catch (Exception e)
                {
                    Console.WriteLine("**No se pudo establecer el canal de comunicacion con el subsistema Pub-Sub de eventos**, Evento NSTAT***" );
                    Console.WriteLine("**Detalle de la excepcion:{0}**" + e.Message);
                }
            }
        }
        /*public CompositeType GetDataUsingDataContract(CompositeType composite)
        {
            if (composite == null)
            {
                throw new ArgumentNullException("composite");
            }
            if (composite.BoolValue)
            {
                composite.StringValue += "Suffix";
            }
            return composite;
        }*/
    }
}
