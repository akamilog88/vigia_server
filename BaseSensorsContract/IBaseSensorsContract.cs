using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;

namespace SenApi.BaseSensorsContract
{
    [ServiceContract]
    public interface IBaseSensorsContract
    {
        [OperationContract]
        void StartDetect();
        [OperationContract]
        void StopDetect();
        [OperationContract]
        bool IsDetecting();
    }
}
