using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace SenApi.ServicesContract.PubSubContract
{
    [DataContract]
    [Serializable()]
    public class ReciverEndpointInfo
    {
        [DataMember]
        public String Binding{get;set;}
        [DataMember]
        public Uri Address { get; set; }
    }
}
