using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace SenApi.ServicesContract.PubSubContract
{
    [DataContract]
    public class EventData
    {
        string data_identifier;
        Object data_Value;

        [DataMember]
        public string Data_ID
        {
            get { return data_identifier; }
            set { data_identifier = value; }
        }

        [DataMember]
        public Object Data_Val
        {
            get { return data_Value; }
            set { data_Value = value; }
        }
    }
}
