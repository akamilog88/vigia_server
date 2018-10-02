using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using SenApi.ServicesContract.NetworkTesting;

namespace SenApi.Services.NetworkTestingService
{
    // NOTA: puede usar el comando "Rename" del menú "Refactorizar" para cambiar el nombre de clase "Service1" en el código y en el archivo de configuración a la vez.
    
    public class NetworkTestingService : INetworkTesting
    {
        public string GetComunicationToken(string key)
        {
            return key;
        }
    }
}
