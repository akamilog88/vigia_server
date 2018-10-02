using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DiscoClient
{
    public class SensorGroupCtrl
    {
        public string Code { private set; get; }
        public string CodeName { private set; get; }
        public string DesciptiveName { private set; get; }
        public bool isLocal { private set; get; }
        public bool hasDTMFS { private set; get; }
        public bool hasPPort { private set; get; }
        public Uri dtmf_Address { private set; get; }
        public Uri pport_Address { private set; get; }
        public bool IsConnected { get; set; }
        public bool IsDTMFActive { get; set; }
        public bool IsPPortActive { get; set; }


        public SensorGroupCtrl(string code, string pname, string dname, bool local, bool dtmf, bool pport, Uri pport_address = null, Uri dtmf_address = null)
        {
            this.Code = code;
            CodeName = pname;
            DesciptiveName = dname;
            isLocal = local;
            hasDTMFS = dtmf;
            hasPPort = pport;
            IsConnected = true;
            this.pport_Address = pport_address;
            this.dtmf_Address = dtmf_address;
            IsDTMFActive = false;
            IsPPortActive = false;
        }

        public void Reset()
        {
            this.IsConnected = true;
            lastContact = DateTime.Now;         
        }
        public DateTime lastContact { get; set; }
    }
}
