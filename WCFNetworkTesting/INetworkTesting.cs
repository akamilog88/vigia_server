using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace SenApi.ServicesContract.NetworkTesting
{
    // NOTA: puede usar el comando "Rename" del menú "Refactorizar" para cambiar el nombre de interfaz "IService1" en el código y en el archivo de configuración a la vez.
    [ServiceContract]
    public interface INetworkTesting
    {
        [OperationContract]
        string GetComunicationToken(string key);
        // TODO: agregue aquí sus operaciones de servicio
    }   
}
