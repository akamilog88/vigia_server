using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.ServiceModel;
using System.Configuration;
using System.IO;

using SenApi.ServicesContract.PubSubContract;

namespace SenApi.Services.PubSubServices
{
    // NOTA: puede usar el comando "Rename" del menú "Refactorizar" para cambiar el nombre de clase "Service1" en el código y en el archivo de configuración a la vez.
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class PubSubService : IPubSubEventAPI
    {
        Dictionary<String, List<ReciverEndpointInfo>> _registrations;
        
        bool foreward=false;
        public PubSubService() {
            DeserializeSubscripciones();
            if(_registrations==null)
                this._registrations = new Dictionary<string, List<ReciverEndpointInfo>>();            
            try{
            this.foreward = Convert.ToBoolean( ConfigurationManager.AppSettings["FOREWARD"]);
            }catch(Exception){
            
            }
        }

        private void SerializeSubscripciones() 
        {
            FileStream fs = new FileStream("Subscripciones.dat", FileMode.Create);

            // Construct a BinaryFormatter and use it to serialize the data to the stream.
            BinaryFormatter formatter = new BinaryFormatter();
            try
            {
                formatter.Serialize(fs, _registrations);
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to serialize. Reason: " + e.Message);
                throw;
            }
            finally
            {
                fs.Close();
            }

        }
        private void DeserializeSubscripciones()
        {
            FileStream fs=null;
            if (File.Exists("Subscripciones.dat"))
            {
                fs = new FileStream("Subscripciones.dat", FileMode.Open);

                try
                {
                    BinaryFormatter formatter = new BinaryFormatter();

                    // Deserialize the hashtable from the file and 
                    // assign the reference to the local variable.
                    _registrations = (Dictionary<String, List<ReciverEndpointInfo>>)formatter.Deserialize(fs);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Failed to deserialize. Reason: " + e.Message);
                    throw;
                }
                finally
                {
                    fs.Close();
                }
            }
        }
        public void PublishEvent(String event_name,Uri sender_address, EventData[] data) {
            Console.WriteLine("***Event Published, event_identifier:{0}***",event_name);
            Console.WriteLine("***Publisher Service address{0}***", sender_address.ToString());
            List<ReciverEndpointInfo> uris;
            lock (this._registrations)
            {
                 uris = _registrations.SingleOrDefault(o => o.Key == event_name).Value;
            }
            Console.WriteLine("***Detected {0} subscribciones***", uris==null?0:uris.Count);
            if (foreward)
            {
                Action<String, Uri, EventData[]> forewardFunc = (s, u, d) =>
                {                      
                ChannelFactory<IPubSubEventAPI> factory = new ChannelFactory<IPubSubEventAPI>("IPubSub_Foreward");
                try
                {
                    IPubSubEventAPI client = factory.CreateChannel();
                    client.PublishEvent(event_name, sender_address, data);
                    Console.WriteLine("**Event Forewarded**");
                }
                catch (Exception e)
                {
                    Console.WriteLine("**No se pudo establecer el canal de comunicacion con el Servicio PubSub de nivel superior**");
                    Console.WriteLine("**Detalle de la excepcion:{0}**" + e.Message);
                }
            };
            forewardFunc.BeginInvoke(event_name, sender_address, data, null, null);                   
            }
            if (uris != null)
            {              
                foreach (ReciverEndpointInfo r in uris)
                {
                    Action<String, Uri, EventData[]> notify = (s, u, d) =>
                    {                      
                        ChannelFactory<IEventReciver> factory = new ChannelFactory<IEventReciver>("IEventReciver", new EndpointAddress(r.Address));
                        try
                        {
                            IEventReciver client = factory.CreateChannel();
                            client.ReciveEvent(s, u, d);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("**No se pudo establecer el canal de comunicacion con el Cliente Subscrito{0}**, Detalle:{1}", r.Address , " "+e.Message);
                            Console.WriteLine("**Detalle de la excepcion:{0}**" + e.Message);
                        }                     
                    };                    
                        notify.BeginInvoke(event_name, sender_address, data, null, r);                   
                    }
            }
            else
                Console.WriteLine("***No existen subscripciones para el evento***");
        }
      

        public void SubscribeForEvent(String event_name, ReciverEndpointInfo listener)
        {
            lock (_registrations) {
                
                string result = _registrations.SingleOrDefault(o => o.Key == event_name).Key;

                if (result != null)
                {
                    bool u = _registrations[result].Exists(o => o.Address == listener.Address);                    

                    if (!u)
                    {
                        Console.WriteLine("***Subscribtion Added for {0} ", event_name);
                        _registrations[result].Add(listener);
                        SerializeSubscripciones();
                    }                   
                }
                else{
                    _registrations.Add(event_name, (new List<ReciverEndpointInfo> { listener }));
                    Console.WriteLine("***Subscribtion Added for {0} ",event_name);
                    SerializeSubscripciones();
                }
            }
        }

        public void UnSubscribe(string event_name, ReciverEndpointInfo listener)
        {
            lock (_registrations)
            {

                string result = _registrations.SingleOrDefault(o => o.Key == event_name).Key;

                if (result != null)
                {
                    bool u = _registrations[result].Exists(o => o.Address == listener.Address);

                    if (u)
                    {
                        ReciverEndpointInfo r = _registrations[result].Single(o => o.Address == listener.Address);
                        _registrations[result].Remove(r);
                        SerializeSubscripciones();
                        Console.WriteLine("***Subscribtion Removed for {0} ", event_name);
                    }
                }               
            }
        }
    }
}
