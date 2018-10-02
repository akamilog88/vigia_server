using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using SenApi.BaseSensorsContract;

namespace SenApi.Services.DTMF
{
    // NOTA: puede usar el comando "Rename" del menú "Refactorizar" para cambiar el nombre de clase "Service1" en el código y en el archivo de configuración a la vez.
   [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class DTMFServiceCtrl : IBaseSensorsContract
    {
        private bool detecting;
        
        public event EventHandler<EventArgs>  onStartDetect;
        public event EventHandler<EventArgs> onStopDetect;

        public DTMFServiceCtrl() {
            detecting = false;          
        }
        public void StartDetect()
        {        
            onStartDetect.BeginInvoke(this, new EventArgs(), null, null);
        }

        public void StopDetect()
        {          
            onStopDetect.BeginInvoke(this, new EventArgs(), null, null);
        }

        public bool IsDetecting()
        {
            return detecting;
        }
    }
}
